using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text;

namespace NullSoftware.Serialization.Converters
{
    public class ArrayUnsafeConverter : IBinaryConverter
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
        /// Initializes a new instance of the <see cref="ArrayUnsafeConverter"/> 
        /// class to the specified array element type and inner converter.
        /// </summary>
        /// <param name="arrayElementType">Type of array inner elements.</param>
        /// <param name="innerConverter">Converter for array inner elements.</param>
        public ArrayUnsafeConverter(Type arrayElementType, IBinaryConverter innerConverter)
        {
            ArrayElementType = arrayElementType ?? throw new ArgumentNullException(nameof(arrayElementType));
            InnerConverter = innerConverter ?? throw new ArgumentNullException(nameof(innerConverter));
        }

        /// <inheritdoc/>
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            Array array = (Array)value;

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

            Array array = Array.CreateInstance(ArrayElementType, length);

            for (int i = 0; i < length; i++)
            {
                array.SetValue(InnerConverter.ToValue(member, stream, parameter), i);
            }

            return array;
        }

    }
}
