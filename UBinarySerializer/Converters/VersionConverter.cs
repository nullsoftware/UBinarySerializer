using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Converters
{
    public class VersionConverter : IBinaryConverter
    {
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            Version version = (Version)value;

            stream.Write(version.Major);
            stream.Write(version.Minor);
            stream.Write(version.Revision);
            stream.Write(version.Build);
        }

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
