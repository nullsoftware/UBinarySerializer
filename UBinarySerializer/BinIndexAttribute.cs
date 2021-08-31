using System;

namespace NullSoftware.Serialization
{
    /// <summary>
    /// Specifies the index for binary serializaion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BinIndexAttribute : Attribute
    {
        /// <summary>
        /// Gets index of current attribute.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets or sets member generation.
        /// </summary>
        public ushort Generation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinIndexAttribute"/>
        /// with specified binary index.
        /// </summary>
        /// <param name="index">
        /// Binary index of member that will be used during serilization 
        /// or deserialization to place members in correct order.
        /// </param>
        public BinIndexAttribute(int index)
        {
            Index = index;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Index} ({nameof(Generation)}: {Generation})";
        }
    }
}
