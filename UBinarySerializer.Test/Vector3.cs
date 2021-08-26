using System;
using System.Globalization;

namespace NullSoftware.Serialization.Test
{
    public struct Vector3 : IEquatable<Vector3>, ICloneable, IFormattable
    {
        [BinIndex(0)]
        public float X { get; set; }

        [BinIndex(1)]
        public float Y { get; set; }

        [BinIndex(2)]
        public float Z { get; set; }

        public Vector3(float x, float y)
        {
            this.X = x;
            this.Y = y;
            this.Z = default;
        }

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3 vector)
                return Equals(vector);
            else
                return false;
        }

        public bool Equals(Vector3 other)
        {
            return this.X == other.X
                && this.Y == other.Y
                && this.Z == other.Z;
        }

        public override string ToString()
        {
            return ToString("X, Y, Z", CultureInfo.InvariantCulture);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            if (format is null)
                return format;

            string result = format;
            result = result.Replace("X", X.ToString(provider));
            result = result.Replace("Y", Y.ToString(provider));
            result = result.Replace("Z", Z.ToString(provider));

            return result;
        }

        public string ToString(string format)
        {
            if (format is null)
                return format;

            string result = format;
            result = result.Replace("X", X.ToString());
            result = result.Replace("Y", Y.ToString());
            result = result.Replace("Z", Z.ToString());

            return result;
        }

        public object Clone()
        {
            return new Vector3(X, Y, Z);
        }

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !left.Equals(right);
        }

        public static Vector3 operator *(Vector3 vector, float factor)
        {
            return new Vector3(vector.X * factor, vector.Y * factor, vector.Z * factor);
        }
    }
}
