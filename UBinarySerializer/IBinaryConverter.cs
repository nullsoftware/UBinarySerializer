using System;
using System.IO;
using System.Reflection;

namespace NullSoftware.Serialization
{
    /// <summary>
    /// Represents a methods which needed for binary serialization or deserialization.
    /// </summary>
    public interface IBinaryConverter
    {
        /// <summary>
        /// Converts value to bytes using <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="member">Field or property member info.</param>
        /// <param name="stream">Instance of <see cref="BinaryWriter"/> that will be used to write bytes.</param>
        /// <param name="value">Value to convert into bytes.</param>
        /// <param name="parameter">Additional parameter.</param>
        void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter);

        /// <summary>
        /// Converts bytes to value using <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="member">Field or property member info.</param>
        /// <param name="stream">Instance of <see cref="BinaryReader"/> that will be used to read bytes.</param>
        /// <param name="parameter">Additional parameter.</param>
        /// <returns>The value converted from bytes.</returns>
        object ToValue(MemberInfo member, BinaryReader stream, object parameter);
    }
}