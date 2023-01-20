using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Test
{
    public class FourCharCodeConverter : IBinaryConverter
    {
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            stream.Write(((FourCharCode)value).Value);
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            return new FourCharCode(stream.ReadBytes(4));
        }
    }
}
