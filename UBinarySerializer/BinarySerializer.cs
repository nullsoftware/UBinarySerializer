using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NullSoftware.Serialization.Converters;

namespace NullSoftware.Serialization
{
    /// <summary>
    /// Base class for binary serializer.
    /// </summary>
    public abstract class BinarySerializer
    {
        internal abstract ushort LatestGeneration { get; } // latest generation of target type

        /// <summary>
        /// Gets converters collection that used in current serializer.
        /// </summary>
        /// <remarks>
        /// Each converter is bonded to object type.
        /// </remarks>
        protected IDictionary<Type, IBinaryConverter> Converters { get; }

        /// <summary>
        /// Initializes default properties.
        /// </summary>
        /// <param name="converters">
        /// Instance of converters dictionary. 
        /// If argument is null will be used <see cref="InitializeDefaultConverters"/> method.
        /// </param>
        protected BinarySerializer(IDictionary<Type, IBinaryConverter> converters)
        {
            Converters = converters ?? InitializeDefaultConverters();
        }

        /// <summary>
        /// Initializes default converters for current serializer.
        /// Must contain all primitive type converters.
        /// </summary>
        /// <returns>Default converters that will be used by current serializer.</returns>
        protected virtual IDictionary<Type, IBinaryConverter> InitializeDefaultConverters()
        {
            return new Dictionary<Type, IBinaryConverter>()
            {
                [typeof(Byte)] = new ByteConverter(),
                [typeof(SByte)] = new SByteConverter(),
                [typeof(Boolean)] = new BooleanConverter(),
                [typeof(Int16)] = new Int16Converter(),
                [typeof(UInt16)] = new UInt16Converter(),
                [typeof(Int32)] = new Int32Converter(),
                [typeof(UInt32)] = new UInt32Converter(),
                [typeof(Int64)] = new Int64Converter(),
                [typeof(UInt64)] = new UInt64Converter(),
                [typeof(Single)] = new SingleConverter(),
                [typeof(Double)] = new DoubleConverter(),
                [typeof(Decimal)] = new DecimalConverter(),
                [typeof(Char)] = new CharConverter(),
                [typeof(String)] = new StringConverter(),
                [typeof(DateTime)] = new DateTimeConverter(),
                [typeof(TimeSpan)] = new TimeSpanConverter(),
                [typeof(DateTimeOffset)] = new DateTimeOffsetConverter(),
                [typeof(Guid)] = new GuidConverter(),
                [typeof(Version)] = new VersionConverter(),
                [typeof(Uri)] = new UriConverter(),
            };
        }

        internal abstract void ContinueSerialization(BinaryWriter stream, object value); // unsafe serialization
        internal abstract void ContinueSerialization(BinaryWriter stream, object value, bool allowNullValue, ushort generation); // safe serialization
        internal abstract object ContinueDeserialization(BinaryReader stream); // unsafe deserialization
        internal abstract object ContinueDeserialization(BinaryReader stream, bool allowNullValue, ushort generation); // safe deserialization
    }

    /// <summary>
    /// Serializes and deserializes an specified object in binary format.
    /// </summary>
    /// <typeparam name="T">Type of object to serialize/deserialize.</typeparam>
    public class BinarySerializer<T> : BinarySerializer where T : new()
    {
        private readonly Dictionary<MemberInfoProxy, BinIndexAttribute> _bindings;

        /// <summary>
        /// Gets default encoding for current instance of serializer.
        /// </summary>
        public Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets a latest generation of field or property
        /// from current and child serializers using <see cref="BinIndexAttribute"/>.
        /// </summary>
        internal override ushort LatestGeneration { get; }

        /// <summary>
        /// Gets a value indicating whether the current 
        /// serialization target is reference type.
        /// </summary>
        private bool IsClass { get; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySerializer{T}"/>
        /// with specified main converters and custom converters.
        /// </summary>
        /// <param name="converters">Main converters dictionary, which must contains serializer converters.</param>
        /// <param name="customConverters">Custom converters, that will be merged with <paramref name="converters"/>.</param>
        protected BinarySerializer(IDictionary<Type, IBinaryConverter> converters,
            ICollection<KeyValuePair<Type, IBinaryConverter>> customConverters) : base(converters)
        {
            Type targetType = typeof(T);

            if (targetType.IsPrimitive || targetType.IsEnum || targetType.IsArray)
                throw new ArgumentException("Can not create serializer for current type.");

            if (customConverters != null)
            {
                // merge custom converters with main converters
                foreach (var pair in customConverters)
                {
                    if (Converters.ContainsKey(pair.Key))
                        Converters[pair.Key] = pair.Value;
                    else
                        Converters.Add(pair.Key, pair.Value);
                }
            }

            Dictionary<MemberInfoProxy, BinIndexAttribute> bindingsTmp = new Dictionary<MemberInfoProxy, BinIndexAttribute>();

            foreach (FieldInfo field in targetType.GetFields())
            {
                BinIndexAttribute binIndex = field.GetCustomAttribute<BinIndexAttribute>();

                if (field.IsInitOnly)
                {
                    if (binIndex != null)
                        throw new InvalidOperationException($"Field '{field.Name}' in '{targetType}' should not be readonly.");
                    else
                        continue;
                }

                bindingsTmp.Add(new MemberInfoProxy(field), binIndex);
            }

            foreach (PropertyInfo prop in targetType.GetProperties())
            {
                BinIndexAttribute binIndex = prop.GetCustomAttribute<BinIndexAttribute>();

                if (!prop.CanRead || !prop.CanWrite)
                {
                    if (binIndex != null)
                        throw new InvalidOperationException($"Property '{prop.Name}' in '{targetType}' should have get and set methods.");
                    else
                        continue;
                }

                bindingsTmp.Add(new MemberInfoProxy(prop), binIndex);
            }

            if (bindingsTmp.Values.Any(t => t != null))
            {
                // remove all memebers without 'BinIndexAttribute'
                bindingsTmp.Where(t => t.Value is null)
                    .Select(t => t.Key)
                    .ToList()
                    .ForEach(t => bindingsTmp.Remove(t));

                int[] indexes = bindingsTmp.Values.Select(t => t.Index).ToArray();

                if (indexes.Length != indexes.Distinct().Count())
                    throw new InvalidOperationException($"'{targetType}' contains members with same index.");
            }
            else
            {
                // if there is no members with 'BinIndexAttribute'
                // need to create indexes for all elements
                int i = 0;
                foreach (MemberInfoProxy member in bindingsTmp.Keys.ToArray())
                {
                    bindingsTmp[member] = new BinIndexAttribute(i++);
                }
            }

            if (bindingsTmp.Any() == false)
                throw new InvalidOperationException($"No members in '{targetType}' to serialize.");

            _bindings = bindingsTmp.OrderBy(t => t.Value.Index).ToDictionary(t => t.Key, t => t.Value);
            List<BinarySerializer> serializersTmp = new List<BinarySerializer>();

            foreach (MemberInfoProxy member in _bindings.Keys)
            {
                Type memberType = member.MemberTargetType;
                List<Type> enumerableTypes = new List<Type>();

            MemberCheck:
                if (memberType.IsArray)
                {
                    enumerableTypes.Add(memberType);
                    memberType = memberType.GetElementType();

                    goto MemberCheck;
                }

                if (CheckIfListType(memberType))
                {
                    enumerableTypes.Add(memberType);
                    memberType = GetListItemType(memberType);

                    goto MemberCheck;
                }

                if (memberType.IsAbstract || memberType.IsInterface || memberType == typeof(object))
                {
                    if (Converters.ContainsKey(memberType))
                        continue;

                    throw new InvalidOperationException($"Can not serialize abstract or interface types, like '{memberType}'.");
                }

                try
                {
                    if (memberType.IsEnum)
                        memberType = memberType.GetEnumUnderlyingType();

                    if (memberType.IsPrimitive || Converters.ContainsKey(memberType))
                        continue;

                    Type nullableType = null;

                    if (memberType.IsValueType)
                    {
                        Type nullableInnerType = Nullable.GetUnderlyingType(memberType);

                        if (nullableInnerType != null)
                        {
                            if (nullableInnerType.IsEnum)
                            {
                                nullableInnerType = Enum.GetUnderlyingType(nullableInnerType);
                            }

                            if (Converters.ContainsKey(nullableInnerType))
                            {
                                IBinaryConverter converter = (IBinaryConverter)Activator.CreateInstance(
                                    typeof(NullableConverter<>).MakeGenericType(nullableInnerType),
                                    Converters[nullableInnerType]);

                                Converters.Add(memberType, converter);

                                continue;
                            }
                            else
                            {
                                nullableType = memberType;
                                memberType = nullableInnerType;
                            }
                        }
                    }

                    if (memberType.IsClass || memberType.IsValueType)
                    {
                        BinarySerializer serializer = (BinarySerializer)Activator.CreateInstance(
                            typeof(BinarySerializer<>).MakeGenericType(memberType),
                            BindingFlags.Instance | BindingFlags.NonPublic,
                            null,
                            new object[] { Converters, null },
                            null);

                        serializersTmp.Add(serializer);
                        Converters.Add(memberType, new BinarySerializerConverter(serializer));

                        if (nullableType != null)
                        {
                            IBinaryConverter converter = (IBinaryConverter)Activator.CreateInstance(
                                typeof(NullableConverter<>).MakeGenericType(memberType),
                                Converters[memberType]);

                            Converters.Add(nullableType, converter);
                        }
                    }
                }
                finally
                {
                    if (enumerableTypes.Any() && !Converters.ContainsKey(enumerableTypes.First()))
                    {
                        IBinaryConverter currentConverter = Converters[memberType];
                        foreach (Type t in enumerableTypes.Reverse<Type>())
                        {
                            if (t.IsArray)
                                currentConverter = new ArrayConverter(t.GetElementType(), currentConverter);
                            else if (CheckIfListType(t))
                                currentConverter = new ListConverter(t, currentConverter);

                            if (!Converters.ContainsKey(t))
                                Converters.Add(t, currentConverter);
                        }
                    }
                }
            }

            LatestGeneration = GetLatestGeneration(serializersTmp);
            IsClass = targetType.IsClass;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySerializer{T}"/>
        /// using default converters.
        /// </summary>
        public BinarySerializer() : this(null, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinarySerializer{T}"/>
        /// using custom converters.
        /// </summary>
        /// <param name="customConverters">Custom serializer converters.</param>
        public BinarySerializer(ICollection<KeyValuePair<Type, IBinaryConverter>> customConverters) : this(null, customConverters)
        {

        }

        #endregion

        #region Serialize & Deserialize Public Methods

        /// <summary>
        /// Serializes specified object to binary data using specified stream and encoding.
        /// </summary>
        /// <param name="stream">The stream used to write the binary data.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="encoding">The encoding for string serialization.</param>
        public void Serialize(Stream stream, T value, Encoding encoding)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            if (value is null)
                throw new ArgumentNullException(nameof(value));

            using (BinaryWriter writer = new BinaryWriter(stream, encoding, true))
            {
                ushort latestGen = LatestGeneration;
                Converters[typeof(ushort)].ToBytes(null, writer, latestGen, null); // info about serializable object generation

                ContinueSerialization(writer, value, false, latestGen);
            }
        }

        /// <summary>
        /// Serializes specified object to binary data using specified stream.
        /// As encoding will be used <see cref="DefaultEncoding"/>.
        /// </summary>
        /// <param name="stream">The stream used to write the binary data.</param>
        /// <param name="value">The object to serialize.</param>
        public void Serialize(Stream stream, T value) => Serialize(stream, value, DefaultEncoding);

        /// <summary>
        /// Serializes specified object to binary data using specified encoding and returns the result.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="encoding">The encoding for string serialization.</param>
        /// <returns>The serialized object.</returns>
        public byte[] Serialize(T value, Encoding encoding)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream, value, encoding);

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Serializes specified object to binary data and returns the result.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>The serialized object.</returns>
        public byte[] Serialize(T value) => Serialize(value, DefaultEncoding);

        public void SerializeUnsafe(Stream stream, T value, Encoding encoding)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            if (value is null)
                throw new ArgumentNullException(nameof(value));

            using (BinaryWriter writer = new BinaryWriter(stream, encoding, true))
            {
                ContinueSerialization(writer, value);
            }
        }

        public void SerializeUnsafe(Stream stream, T value) => SerializeUnsafe(stream, value, DefaultEncoding);

        public byte[] SerializeUnsafe(T value, Encoding encoding)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                SerializeUnsafe(stream, value, encoding);

                return stream.ToArray();
            }
        }

        public byte[] SerializeUnsafe(T value) => SerializeUnsafe(value, DefaultEncoding);


        public T Deserialize(Stream stream, Encoding encoding)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            using (BinaryReader reader = new BinaryReader(stream, encoding, true))
            {
                ushort serializedGen = (ushort)Converters[typeof(ushort)].ToValue(null, reader, null);

                return (T)ContinueDeserialization(reader, false, serializedGen);
            }
        }

        public T Deserialize(Stream stream) => Deserialize(stream, DefaultEncoding);

        public T Deserialize(byte[] data, Encoding encoding)
        {
            using (MemoryStream stream = new MemoryStream(data, false))
            {
                return Deserialize(stream, encoding);
            }
        }

        public T Deserialize(byte[] data) => Deserialize(data, DefaultEncoding);


        public T DeserializeUnsafe(Stream stream, Encoding encoding)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            using (BinaryReader reader = new BinaryReader(stream, encoding, true))
            {
                return (T)ContinueDeserialization(reader);
            }
        }

        public T DeserializeUnsafe(Stream stream) => DeserializeUnsafe(stream, DefaultEncoding);

        public T DeserializeUnsafe(byte[] data, Encoding encoding)
        {
            using (MemoryStream stream = new MemoryStream(data, false))
            {
                return DeserializeUnsafe(stream, encoding);
            }
        }

        public T DeserializeUnsafe(byte[] data) => DeserializeUnsafe(data, DefaultEncoding);

        #endregion

        internal override void ContinueSerialization(BinaryWriter stream, object value, bool allowNullValue, ushort generation)
        {
            if (IsClass)
            {
                if (allowNullValue)
                {
                    stream.Write(value != null);

                    if (value is null)
                        return;
                }
                else if (value is null)
                {
                    throw new ArgumentNullException(
                        nameof(value), "Null value not supported for current member.");
                }
            }

            foreach (MemberInfoProxy member in _bindings.Where(t => t.Value.Generation <= generation).Select(t => t.Key))
            {
                SerializeMember(member, stream, ref value, generation);
            }
        }

        internal override void ContinueSerialization(BinaryWriter stream, object value)
        {
            foreach (MemberInfoProxy member in _bindings.Select(t => t.Key))
            {
                SerializeMember(member, stream, ref value, null);
            }
        }

        internal override object ContinueDeserialization(BinaryReader stream, bool allowNullValue, ushort generation)
        {
            if (allowNullValue && IsClass && stream.ReadBoolean() == false)
                return null;

            object result = new T();

            foreach (MemberInfoProxy member in _bindings.Where(t => t.Value.Generation <= generation).Select(t => t.Key))
            {
                DeserializeMember(member, stream, ref result, generation);
            }

            return result;
        }

        internal override object ContinueDeserialization(BinaryReader stream)
        {
            object result = new T();

            foreach (MemberInfoProxy member in _bindings.Select(t => t.Key))
            {
                DeserializeMember(member, stream, ref result, null);
            }

            return result;
        }


        /// <summary>
        /// Determinants whether specified type is derived from list type.
        /// </summary>
        /// <param name="type">Type to check for list.</param>
        /// <returns>true is current type is derived from list type, otherwise false.</returns>
        protected virtual bool CheckIfListType(Type type)
        {
            return type.IsGenericType && type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));
        }

        /// <summary>
        /// Gets list element type from specified type.
        /// </summary>
        /// <param name="listType">Type that derived from <see cref="IList{T}"/>.</param>
        /// <returns>Element type of <see cref="IList{T}"/>.</returns>
        protected virtual Type GetListItemType(Type listType)
        {
            Type result = listType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>));

            if (result is null)
                throw new InvalidOperationException($"Failed to determinate collection element type. Collection type: {listType}");

            return result.GenericTypeArguments[0];
        }

        private ushort GetLatestGeneration(IList<BinarySerializer> serializers)
        {
            return serializers
                .Select(t => t.LatestGeneration)
                .Concat(_bindings.Values.Select(t => t.Generation))
                .OrderBy(t => t)
                .Last();
        }

        private void SerializeMember(MemberInfoProxy member, BinaryWriter stream, ref object obj, object parameter)
        {
            Type memberType = member.SafeMemberTargetType;

            Converters[memberType].ToBytes(
                    member.MemberInfo,
                    stream,
                    member.GetValue(obj),
                    parameter);
        }

        private void DeserializeMember(MemberInfoProxy member, BinaryReader stream, ref object obj, object parameter)
        {
            Type memberType = member.SafeMemberTargetType;

            member.SetValue(obj, Converters[memberType].ToValue(
                member.MemberInfo,
                stream,
                parameter));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(BinarySerializer)} for {typeof(T)}";
        }
    }
}
