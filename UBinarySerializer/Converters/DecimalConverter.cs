using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace NullSoftware.Serialization.Converters
{
    /// <summary>
    /// Converter for <see cref="Decimal"/> values.
    /// </summary>
    public class DecimalConverter : IBinaryConverter
    {
        /// <inheritdoc/>
        public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
        {
            stream.Write((Decimal)value);
        }

        /// <inheritdoc/>
        public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
        {
            return stream.ReadDecimal();
        }
    }
}
