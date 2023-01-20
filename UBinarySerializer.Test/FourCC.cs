using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Test
{
    [BinaryConverter(typeof(FourCCConverter))]
    public struct FourCC : IEquatable<FourCC>
    {
        public byte[] Value { get; }

        public FourCC(params byte[] value)
        {
            if (value == null) throw new ArgumentNullException();
            if (value.Length != 4) throw new ArgumentOutOfRangeException();

            Value = value;
        }

        public FourCC(string value)
        {
            if (value == null) throw new ArgumentNullException();
            if (value.Length != 4) throw new ArgumentOutOfRangeException();

            Value = Encoding.ASCII.GetBytes(value);
        }

        public override string ToString()
        {
            return Encoding.ASCII.GetString(Value);
        }

        public bool Equals(FourCC other)
        {
            return Value.SequenceEqual(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is FourCC fourCC)
                return Equals(fourCC);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(Value);
        }

        public static bool operator ==(FourCC left, FourCC right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FourCC left, FourCC right)
        {
            return !left.Equals(right);
        }
    }
}
