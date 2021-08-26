using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Test
{
    public struct Item : IEquatable<Item>
    {
        [BinIndex(0)]
        public int Id { get; set; }

        [BinIndex(1)]
        public byte Amount { get; set; }

        public Item(int id)
        {
            Id = id;
            Amount = 1;
        }

        public Item(int id, byte amount)
        {
            Id = id;
            Amount = amount;
        }

        public override int GetHashCode()
        {
            return Id ^ Amount;
        }

        public bool Equals(Item other)
        {
            return this.Id == other.Id
                && this.Amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            if (obj is Item item)
                return Equals(item);
            else
                return false;
        }

        public override string ToString()
        {
            return $"{Id} x{Amount}";
        }
    }
}
