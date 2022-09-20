using System;
using System.IO;
using System.Reflection;

namespace NullSoftware.Serialization.Converters
{
    /// <summary>
    /// Converter for <see cref="DateTime"/> values.
    /// </summary>
    public class DateTimeConverter : IBinaryConverter
    {
        /// <inheritdoc/>
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            stream.Write(((DateTime)value).Ticks);
        }

        /// <inheritdoc/>
        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            return new DateTime(stream.ReadInt64());
        }
    }
}