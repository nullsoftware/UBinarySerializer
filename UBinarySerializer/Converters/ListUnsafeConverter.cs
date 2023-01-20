using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text;

namespace NullSoftware.Serialization.Converters
{
    public class ListUnsafeConverter : IBinaryConverter
    {
        /// <summary>
        /// Gets a type of target list.
        /// </summary>
        public Type TargetListType { get; }

        /// <summary>
        /// Gets converter for inner list elements.
        /// </summary>
        public IBinaryConverter InnerConverter { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListUnsafeConverter"/> 
        /// class to the specified list type and inner converter.
        /// </summary>
        /// <param name="listType">Type of target list.</param>
        /// <param name="innerConverter">Converter for inner list elements.</param>
        public ListUnsafeConverter(Type listType, IBinaryConverter innerConverter)
        {
            TargetListType = listType ?? throw new ArgumentNullException(nameof(listType));
            InnerConverter = innerConverter ?? throw new ArgumentNullException(nameof(innerConverter));
        }

        /// <inheritdoc/>
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            IList list = (IList)value;

            stream.Write(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                InnerConverter.ToBytes(member, stream, list[i], parameter);
            }
        }

        /// <inheritdoc/>
        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            int count = stream.ReadInt32();

            IList list = (IList)Activator.CreateInstance(TargetListType);

            for (int i = 0; i < count; i++)
            {
                list.Add(InnerConverter.ToValue(member, stream, parameter));
            }

            return list;
        }
    }
}
