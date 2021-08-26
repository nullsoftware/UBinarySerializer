using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;

namespace NullSoftware.Serialization.Converters
{
    internal class BinarySerializerConverter : IBinaryConverter
    {
        internal BinarySerializer Serializer { get; }

        public BinarySerializerConverter(BinarySerializer serializer)
        {
            Serializer = serializer;
        }

        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            if (parameter is ushort generation)
                Serializer.ContinueSerialization(stream, value, member.GetCustomAttribute<RequiredAttribute>() is null, generation);
            else
                Serializer.ContinueSerialization(stream, value);
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            if (parameter is ushort generation)
                return Serializer.ContinueDeserialization(stream, member.GetCustomAttribute<RequiredAttribute>() is null, generation);
            else
                return Serializer.ContinueDeserialization(stream);
        }
    }
}