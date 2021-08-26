using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Converters
{
    internal class NullableConverter<T> : IBinaryConverter where T : struct
    {
        public IBinaryConverter InnerConverter { get; }

        public NullableConverter(IBinaryConverter innerTypeConverter)
        {
            InnerConverter = innerTypeConverter ?? throw new ArgumentNullException(nameof(innerTypeConverter));
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
                nullableValue = (T)ToValue(member, stream, parameter);

            return nullableValue;
        }
    }
}
