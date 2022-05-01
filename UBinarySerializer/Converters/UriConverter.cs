using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text;

namespace NullSoftware.Serialization.Converters
{
    public class UriConverter : IBinaryConverter
    {
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            Uri uri = (Uri)value;

            if (member.GetCustomAttribute<RequiredAttribute>() != null || parameter is null)
            {
                if (uri is null)
                    throw new ArgumentNullException(nameof(value), $"Member {member.Name} can not have null value.");

                stream.Write(uri.ToString());
            }
            else
            {
                stream.Write(uri != null);

                if (uri != null) stream.Write(uri.ToString());
            }
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            if (member.GetCustomAttribute<RequiredAttribute>() != null || parameter is null)
            {
                return new Uri(stream.ReadString());
            }
            else
            {
                return stream.ReadBoolean() ? new Uri(stream.ReadString()) : null;
            }
        }
    }
}
