using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Converters
{
    public class TimeSpanConverter : IBinaryConverter
    {
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            stream.Write(((TimeSpan)value).Ticks);
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            return new TimeSpan(stream.ReadInt64());
        }
    }
}