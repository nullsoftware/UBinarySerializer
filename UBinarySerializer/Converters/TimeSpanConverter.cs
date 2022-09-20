using System;
using System.IO;
using System.Reflection;

namespace NullSoftware.Serialization.Converters
{
    /// <summary>
    /// Converter for <see cref="TimeSpan"/> values.
    /// </summary>
    public class TimeSpanConverter : IBinaryConverter
    {
        /// <inheritdoc/>
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            stream.Write(((TimeSpan)value).Ticks);
        }

        /// <inheritdoc/>
        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            return new TimeSpan(stream.ReadInt64());
        }
    }
}