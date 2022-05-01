using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text;

namespace NullSoftware.Serialization.Converters
{
    public class EncdoingConverter : IBinaryConverter
    {
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            Encoding enc = (Encoding)value;

            if (member.GetCustomAttribute<RequiredAttribute>() != null || parameter is null)
            {
                if (enc is null)
                    throw new ArgumentNullException(nameof(value), $"Member {member.Name} can not have null value.");

                stream.Write(enc.CodePage);
            }
            else
            {
                stream.Write(enc != null);

                if (enc != null) stream.Write(enc.CodePage);
            }
        }

        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            if (member.GetCustomAttribute<RequiredAttribute>() != null || parameter is null)
            {
                return Encoding.GetEncoding(stream.ReadInt32());
            }
            else
            {
                return stream.ReadBoolean() ? Encoding.GetEncoding(stream.ReadInt32()) : null;
            }
        }
    }
}
