using System;
using System.IO;
using System.Reflection;

namespace NullSoftware.Serialization.Converters
{
    /// <summary>
    /// Converter for <see cref="Guid"/> values.
    /// </summary>
    public class GuidConverter : IBinaryConverter
    {
        /// <inheritdoc/>
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            Guid guid = (Guid)value;

            stream.Write(guid.ToByteArray());
        }

        /// <inheritdoc/>
        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            return new Guid(stream.ReadBytes(16));
        }
    }
}
