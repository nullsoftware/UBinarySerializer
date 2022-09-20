using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;

namespace NullSoftware.Serialization.Converters
{
    /// <summary>
    /// Converter for arrays.
    /// </summary>
    public class ArrayConverter : IBinaryConverter
    {
        /// <summary>
        /// Gets type of array inner elements.
        /// </summary>
        public Type ArrayElementType { get; }

        /// <summary>
        /// Gets converter for array inner elements.
        /// </summary>
        public IBinaryConverter InnerConverter { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayConverter"/> 
        /// class to the specified array element type and inner converter.
        /// </summary>
        /// <param name="arrayElementType">Type of array inner elements.</param>
        /// <param name="innerConverter">Converter for array inner elements.</param>
        public ArrayConverter(Type arrayElementType, IBinaryConverter innerConverter)
        {
            ArrayElementType = arrayElementType ?? throw new ArgumentNullException(nameof(arrayElementType));
            InnerConverter = innerConverter ?? throw new ArgumentNullException(nameof(innerConverter));
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
