using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Test
{
    public class Player : IEquatable<Player>
    {
        [BinIndex(0)]
        public int Health { get; set; }

        [BinIndex(1, Generation = 2)]
        public int Hunger { get; set; }

        [BinIndex(2)]
        public Vector3 Position { get; set; }

        [BinIndex(3)]
        public GameMode GameMode { get; set; }

        [BinIndex(4)]
        public Texture Skin { get; set; }

        [BinIndex(5)]
        public List<Item> Items { get; set; }
            = new List<Item>();

        public override bool Equals(object obj)
        {
            return Equals(obj as Player);
        }

        public bool Equals(Player other)
        {
            if (object.ReferenceEquals(other, null))
                return false;

            return this.Health == other.Health
                && this.Hunger == other.Hunger
                && this.Position == other.Position
                && this.GameMode == other.GameMode
                && this.Skin?.Path == other.Skin?.Path
                && this.Items.Count == other.Items.Count
                && this.Items.SequenceEqual(other.Items);
        }
    }
}