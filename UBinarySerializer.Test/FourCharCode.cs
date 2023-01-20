using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Test
{
    [BinaryConverter(typeof(FourCharCodeConverter))]
    public struct FourCharCode : IEquatable<FourCharCode>
    {
        public byte[] Value { get; }

        public FourCharCode(params byte[] value)
        {
            if (value == null) throw new ArgumentNullException();
            if (value.Length != 4) throw new ArgumentOutOfRangeException();

            Value = value;
        }

        public FourCharCode(string value)
        {
            if (value == null) throw new ArgumentNullException();
            if (value.Length != 4) throw new ArgumentOutOfRangeException();

            Value = Encoding.ASCII.GetBytes(value);
        }

        public override string ToString()
        {
            return Encoding.ASCII.GetString(Value);
        }

        public bool Equals(FourCharCode other)
        {
            return Value.SequenceEqual(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is FourCharCode fourCC)
                return Equals(fourCC);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(Value);
        }

        public static bool operator ==(FourCharCode left, FourCharCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FourCharCode left, FourCharCode right)
        {
            return !left.Equals(right);
        }
    }
}
