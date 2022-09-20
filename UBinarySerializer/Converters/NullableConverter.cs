using System;
using System.IO;
using System.Reflection;

namespace NullSoftware.Serialization.Converters
{
    /// <summary>
    /// Converter for <see cref="Nullable{T}"/> values.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying value type of the <see cref="NullableConverter{T}"/> generic type.
    /// </typeparam>
    internal class NullableConverter<T> : IBinaryConverter where T : struct
    {
        /// <summary>
        /// Gets inner converter that represents converter for generic value.
        /// </summary>
        public IBinaryConverter InnerConverter { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableConverter{T}"/> class to the specified value.
        /// </summary>
        /// <param name="innerConverter">
        /// A value type.
        /// </param>
        public NullableConverter(IBinaryConverter innerConverter)
        {
            InnerConverter = innerConverter ?? throw new ArgumentNullException(nameof(innerConverter));
        }

        /// <inheritdoc/>
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            T? nullableValue = (T?)value;

            stream.Write(nullableValue.HasValue);

            if (nullableValue.HasValue)
                InnerConverter.ToBytes(member, stream, nullableValue.Value, parameter);
        }

        /// <inheritdoc/>
        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            T? nullableValue = new T?();

            if (stream.ReadBoolean())
                nullableValue = (T)InnerConverter.ToValue(member, stream, parameter);

            return nullableValue;
        }
    }
}
