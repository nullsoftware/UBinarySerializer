using System;
using System.IO;
using System.Reflection;

namespace NullSoftware.Serialization
{
    public class BinaryConverter : IBinaryConverter
    {
        private Action<MemberInfo, BinaryWriter, object> _toBytesMethod;
        private Func<MemberInfo, BinaryReader, object> _toValueMethod;

        public BinaryConverter(
            Action<MemberInfo, BinaryWriter, object> toBytesMethod, 
            Func<MemberInfo, BinaryReader, object> toValueMethod)
        {
            _toBytesMethod = toBytesMethod ?? throw new ArgumentNullException(nameof(toBytesMethod));
            _toValueMethod = toValueMethod ?? throw new ArgumentNullException(nameof(toValueMethod));
        }

        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            _toBytesMethod(member, stream, value);
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            return _toValueMethod(member, stream);
        }
    }
}
