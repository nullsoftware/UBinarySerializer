using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NullSoftware.Serialization.Converters
{
    /// <summary>
    /// Converter for lists.
    /// </summary>
    public class ListConverter : IBinaryConverter
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
        /// Initializes a new instance of the <see cref="ListConverter"/> 
        /// class to the specified list type and inner converter.
        /// </summary>
        /// <param name="listType">Type of target list.</param>
        /// <param name="innerConverter">Converter for inner list elements.</param>
        public ListConverter(Type listType, IBinaryConverter innerConverter)
        {
            TargetListType = listType ?? throw new ArgumentNullException(nameof(listType));
            InnerConverter = innerConverter ?? throw new ArgumentNullException(nameof(innerConverter));
        }

        /// <inheritdoc/>
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            IList list = (IList)value;

            if (list is null)
            {
                if (member.GetCustomAttribute<RequiredAttribute>() != null)
                    throw new ArgumentNullException(nameof(value), $"Member {member.Name} can not have null value.");

                stream.Write(-1); // means that list is null

                return;
            }

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

            if (count == -1)
                return null;

            /*// list can be readonly, so we need to try to get it from the parameter
            IList list = (parameter as IDefaultValueProvider)?.GetDefaultValue<IList>() ?? (IList)Activator.CreateInstance(TargetListType);*/

            IList list = (IList)Activator.CreateInstance(TargetListType);

            for (int i = 0; i < count; i++)
            {
                list.Add(InnerConverter.ToValue(member, stream, parameter));
            }

            return list;
        }
    }
}
