using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Converters
{
    public class ArrayConverter : IBinaryConverter
    {
        public Type TargetArrayElementType { get; }

        public IBinaryConverter InnerConverter { get; }

        public ArrayConverter(Type arrayElementType, IBinaryConverter innerConverter)
        {
            TargetArrayElementType = arrayElementType ?? throw new ArgumentNullException(nameof(arrayElementType));
            InnerConverter = innerConverter ?? throw new ArgumentNullException(nameof(innerConverter));
        }

        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            Array array = (Array)value;

            if (array is null)
            {
                if (member.GetCustomAttribute<RequiredAttribute>() is not null)
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

            Array array = Array.CreateInstance(TargetArrayElementType, length);

            for (int i = 0; i < length; i++)
            {
                array.SetValue(InnerConverter.ToValue(member, stream, parameter), i);
            }

            return array;
        }
    }
}
