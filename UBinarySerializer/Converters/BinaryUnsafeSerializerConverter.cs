using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace NullSoftware.Serialization.Converters
{
    internal class BinaryUnsafeSerializerConverter : IBinaryConverter
    {
        internal BinaryUnsafeSerializer Serializer { get; }

        public BinaryUnsafeSerializerConverter(BinaryUnsafeSerializer serializer)
        {
            Serializer = serializer;
        }

        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            Serializer.ContinueSerialization(stream, value);
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            return Serializer.ContinueDeserialization(stream);
        }
    }
}
