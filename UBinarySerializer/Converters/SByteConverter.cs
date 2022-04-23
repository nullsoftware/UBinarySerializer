using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace NullSoftware.Serialization.Converters
{
    public class SByteConverter : IBinaryConverter
    {
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            stream.Write((SByte)value);
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            return stream.ReadSByte();
        }
    }
}
