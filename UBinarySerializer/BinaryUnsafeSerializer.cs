using NullSoftware.Serialization.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace NullSoftware.Serialization
{
    /// <summary>
    /// Base class for unsafe binary serializer.
    /// </summary>
    public abstract class BinaryUnsafeSerializer
    {
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
        protected BinaryUnsafeSerializer(IDictionary<Type, IBinaryConverter> converters)
        {
            Converters = converters;
        }

        /// <summary>
        /// Serializes specified object to binary data using specified stream.
        /// </summary>
        /// <param name="stream">The stream used to write the binary data.</param>
        /// <param name="data">The object to serialize.</param>
        public abstract void SerializeObject(Stream stream, object data);

        /// <summary>
        /// Serializes specified object to binary data and returns the result.
        /// </summary>
        /// <param name="data">The object to serialize.</param>
        /// <returns>The serialized data.</returns>
        public abstract byte[] SerializeObject(object data);

        /// <summary>
        /// Deserializes object using specified stream.
        /// </summary>
        /// <param name="stream">The stream used to read the binary data.</param>
        /// <returns>The deserialized object.</returns>
        public abstract object DeserializeObject(Stream stream);

        /// <summary>
        /// Deserializes object from byte array.
        /// </summary>
        /// <param name="data">The byte array to deserialize data.</param>
        /// <returns>The deserialized object.</returns>
        public abstract object DeserializeObject(byte[] data);

        /// <summary>
        /// Initializes default converters for current serializer.
        /// Must contain all primitive type converters.
        /// </summary>
        /// <returns>Default converters that will be used by current serializer.</returns>
        protected static IDictionary<Type, IBinaryConverter> InitializeDefaultConverters()
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
                [typeof(String)] = new StringUnsafeConverter(),
                [typeof(DateTime)] = new DateTimeConverter(),
                [typeof(TimeSpan)] = new TimeSpanConverter(),
                [typeof(DateTimeOffset)] = new DateTimeOffsetConverter(),
                [typeof(Guid)] = new GuidConverter(),
                [typeof(Version)] = new VersionConverter(),
                [typeof(Uri)] = new UriUnsafeConverter(),
            };
        }

        internal abstract void ContinueSerialization(BinaryWriter stream, object value); // unsafe serialization

        internal abstract object ContinueDeserialization(BinaryReader stream); // unsafe deserialization
    }

    /// <summary>
    /// Serializes and deserializes an object, or an entire graph of connected objects, in binary format.
    /// It uses unsafe method for serialization.
    /// </summary>
    /// <typeparam name="T">Type of object to serialize/deserialize.</typeparam>
    public class BinaryUnsafeSerializer<T> : BinaryUnsafeSerializer where T : new()
    {
        private readonly Dictionary<MemberInfoProxy, BinIndexAttribute> _bindings;

        /// <summary>
        /// Gets or sets default encoding for current instance of serializer.
        /// </summary>
        public Encoding DefaultEncoding { get; set; } = Encoding.ASCII;


        protected BinaryUnsafeSerializer(IDictionary<Type, IBinaryConverter> converters) : base(converters)
        {
            Type targetType = typeof(T);

            if (targetType.IsPrimitive || targetType.IsEnum || targetType.IsArray)
                throw new ArgumentException("Can not create serializer for current type.");

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

            _bindings = bindingsTmp.OrderBy(t => t.Value.Index).ToDictionary(t => t.Key, t => t.Value);
            List<BinaryUnsafeSerializer> serializersTmp = new List<BinaryUnsafeSerializer>();

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

                if (memberType.CheckIfListType())
                {
                    enumerableTypes.Add(memberType);
                    memberType = memberType.GetListItemType();

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
                                Converters.Add(memberType, Converters[nullableInnerType]);

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
                        IBinaryConverter converter;

                        if (memberType.GetCustomAttribute(typeof(BinaryConverterAttribute)) is BinaryConverterAttribute customConverterAtt &&
                            (customConverterAtt.SerializerType is null || customConverterAtt.SerializerType == typeof(BinaryUnsafeSerializer)))
                        {
                            converter = (IBinaryConverter)Activator.CreateInstance(
                                customConverterAtt.ConverterType);
                        }
                        else
                        {
                            BinaryUnsafeSerializer serializer = (BinaryUnsafeSerializer)Activator.CreateInstance(
                                typeof(BinaryUnsafeSerializer<>).MakeGenericType(memberType),
                                BindingFlags.Instance | BindingFlags.NonPublic,
                                null,
                                new object[] { Converters },
                                null);

                            serializersTmp.Add(serializer);
                            converter = new BinaryUnsafeSerializerConverter(serializer);
                        }

                        Converters.Add(memberType, converter);

                        if (nullableType != null)
                        {
                            Converters.Add(nullableType, Converters[memberType]);
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
                                currentConverter = new ArrayUnsafeConverter(t.GetElementType(), currentConverter);
                            else if (ReflectionOperations.CheckIfListType(t))
                                currentConverter = new ListUnsafeConverter(t, currentConverter);

                            if (!Converters.ContainsKey(t))
                                Converters.Add(t, currentConverter);
                        }
                    }
                }
            }
        }

        public BinaryUnsafeSerializer() : this(InitializeDefaultConverters())
        {

        }

        #region Serialize

        /// <summary>
        /// Serializes specified object to binary data using specified stream and encoding.
        /// </summary>
        /// <param name="stream">The stream used to write the binary data.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="encoding">The encoding for string serialization.</param>
        public void Serialize(Stream stream, T value, Encoding encoding)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, encoding, true))
            {
                ContinueSerialization(writer, value);
            }
        }

        public void Serialize(Stream stream, T value) => Serialize(stream, value, DefaultEncoding);

        public byte[] Serialize(T value, Encoding encoding)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream, value, encoding);

                return stream.GetBuffer();
            }
        }

        public byte[] Serialize(T value) => Serialize(value, DefaultEncoding);

        public override void SerializeObject(Stream stream, object data)
        {
            Serialize(stream, (T)data);
        }

        public override byte[] SerializeObject(object data)
        {
            return Serialize((T)data);
        }

        #endregion

        #region Deserialize

        public T Deserialize(Stream stream, Encoding encoding)
        {
            using (BinaryReader reader = new BinaryReader(stream, encoding, true))
            {
                return (T)ContinueDeserialization(reader);
            }
        }

        public T Deserialize(Stream stream) => Deserialize(stream, DefaultEncoding);

        public T Deserialize(byte[] buffer, Encoding encoding)
        {
            using (MemoryStream stream = new MemoryStream(buffer, false))
            {
                return Deserialize(stream, encoding);
            }
        }

        public T Deserialize(byte[] buffer) => Deserialize(buffer, DefaultEncoding);

        public override object DeserializeObject(Stream stream)
        {
            return Deserialize(stream);
        }

        public override object DeserializeObject(byte[] data)
        {
            return Deserialize(data);
        }

        #endregion

        #region Internal

        internal override void ContinueSerialization(BinaryWriter stream, object value)
        {
            foreach (MemberInfoProxy member in _bindings.Select(t => t.Key))
            {
                SerializeMember(member, stream, ref value, null);
            }
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

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(BinarySerializer)} for {typeof(T).FullName}";
        }
    }
}
