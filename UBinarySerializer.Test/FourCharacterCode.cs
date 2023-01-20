using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Test
{
    [BinaryConverter(typeof(FourCharacterCodeConverter))]
    public struct FourCharacterCode : IEquatable<FourCharacterCode>
    {
        public byte[] Value { get; }

        public FourCharacterCode(params byte[] value)
        {
            if (value == null) throw new ArgumentNullException();
            if (value.Length != 4) throw new ArgumentOutOfRangeException();

            Value = value;
        }

        public FourCharacterCode(string value)
        {
            if (value == null) throw new ArgumentNullException();
            if (value.Length != 4) throw new ArgumentOutOfRangeException();

            Value = Encoding.ASCII.GetBytes(value);
        }

        public override string ToString()
        {
            return Encoding.ASCII.GetString(Value);
        }

        public bool Equals(FourCharacterCode other)
        {
            return Value.SequenceEqual(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is FourCharacterCode fourCC)
                return Equals(fourCC);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(Value);
        }

        public static bool operator ==(FourCharacterCode left, FourCharacterCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FourCharacterCode left, FourCharacterCode right)
        {
            return !left.Equals(right);
        }
    }
}
