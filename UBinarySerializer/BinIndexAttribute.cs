using System;

namespace NullSoftware.Serialization
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BinIndexAttribute : Attribute
    {
        public int Index { get; }

        public ushort Generation { get; set; }

        public BinIndexAttribute(int index)
        {
            Index = index;
        }

        public override string ToString()
        {
            return $"{Index} ({nameof(Generation)}: {Generation})";
        }
    }
}
