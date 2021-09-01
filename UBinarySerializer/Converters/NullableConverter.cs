using System;
using System.IO;
using System.Reflection;

namespace NullSoftware.Serialization.Converters
{
    internal class NullableConverter<T> : IBinaryConverter where T : struct
    {
        public IBinaryConverter InnerConverter { get; }

        public NullableConverter(IBinaryConverter innerConverter)
        {
            InnerConverter = innerConverter ?? throw new ArgumentNullException(nameof(innerConverter));
        }

        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            T? nullableValue = (T?)value;

            stream.Write(nullableValue.HasValue);

            if (nullableValue.HasValue)
                InnerConverter.ToBytes(member, stream, nullableValue.Value, parameter);
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            T? nullableValue = new T?();

            if (stream.ReadBoolean())
                nullableValue = (T)InnerConverter.ToValue(member, stream, parameter);

            return nullableValue;
        }
    }
}
