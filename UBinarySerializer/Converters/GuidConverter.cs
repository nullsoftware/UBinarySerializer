using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Converters
{
    public class GuidConverter : IBinaryConverter
    {
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            Guid guid = (Guid)value;

            stream.Write(guid.ToByteArray());
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            return new Guid(stream.ReadBytes(16));
        }
    }
}
