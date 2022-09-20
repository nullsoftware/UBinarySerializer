using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace NullSoftware.Serialization.Converters
{
    /// <summary>
    /// Converter for <see cref="DateTimeOffset"/> values.
    /// </summary>
    public class DateTimeOffsetConverter : IBinaryConverter
    {
        /// <inheritdoc/>
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            DateTimeOffset offset = (DateTimeOffset)value;

            stream.Write(offset.Ticks);
            stream.Write(offset.Offset.Ticks);
        }

        /// <inheritdoc/>
        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            return new DateTimeOffset(stream.ReadInt64(), new TimeSpan(stream.ReadInt64()));
        }
    }
}
