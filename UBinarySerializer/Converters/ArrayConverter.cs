using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;

namespace NullSoftware.Serialization.Converters
{
    public class ArrayConverter : IBinaryConverter
    {
        public Type ArrayElementType { get; }

        public IBinaryConverter InnerConverter { get; }

        public ArrayConverter(Type arrayElementType, IBinaryConverter innerConverter)
        {
            ArrayElementType = arrayElementType ?? throw new ArgumentNullException(nameof(arrayElementType));
            InnerConverter = innerConverter ?? throw new ArgumentNullException(nameof(innerConverter));
        }

        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            Array array = (Array)value;

            if (array is null)
            {
                if (member.GetCustomAttribute<RequiredAttribute>() != null)
                    throw new ArgumentNullException(nameof(value), $"Member {member.Name} can not have null value.");

                stream.Write(-1); // means that array is null

                return;
            }

            if (array.Length != array.GetLength(0))
                throw new ArgumentException($"Can not serialize multi-dimension array. Member '{member.Name}'.");

            stream.Write(array.Length);

            for (int i = 0; i < array.Length; i++)
            {
                InnerConverter.ToBytes(member, stream, array.GetValue(i), parameter);
            }
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            int length = stream.ReadInt32();

            if (length == -1)
                return null;

            Array array = Array.CreateInstance(ArrayElementType, length);

            for (int i = 0; i < length; i++)
            {
                array.SetValue(InnerConverter.ToValue(member, stream, parameter), i);
            }

            return array;
        }
    }
}
