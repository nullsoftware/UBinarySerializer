using System;
using System.IO;
using System.Reflection;

namespace NullSoftware.Serialization.Converters
{
    /// <summary>
    /// Converter for <see cref="Version"/> values.
    /// </summary>
    public class VersionConverter : IBinaryConverter
    {
        /// <inheritdoc/>
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            Version version = (Version)value;

            stream.Write(version.Major);
            stream.Write(version.Minor);
            stream.Write(version.Revision);
            stream.Write(version.Build);
        }

        /// <inheritdoc/>
        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            int major = stream.ReadInt32();
            int minor = stream.ReadInt32();
            int build = stream.ReadInt32();
            int revision = stream.ReadInt32();

            if (build < 0)
                return new Version(major, minor);
            else if (revision < 0)
                return new Version(major, minor, build);
            else
                return new Version(major, minor, build, revision);
        }
    }
}
