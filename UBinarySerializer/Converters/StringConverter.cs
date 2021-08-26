using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text;

namespace NullSoftware.Serialization.Converters
{
    public class StringConverter : IBinaryConverter
    {
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            string str = (string)value;

            if (member.GetCustomAttribute<RequiredAttribute>() is not null || parameter is null)
            {
                if (str is null)
                    throw new ArgumentNullException(nameof(value), $"Member {member.Name} can not have null value.");
            }
            else
            {
                stream.Write(str is not null);
            }

            stream.Write(str);
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            if (member.GetCustomAttribute<RequiredAttribute>() is not null || parameter is null)
            {
                return stream.ReadString();
            }
            else
            {
                return stream.ReadBoolean() ? stream.ReadString() : null;
            }
        }
    }
}
