using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization.Test
{
    public class Texture
    {
        public string Path { get; set; }

        public Texture()
        {

        }

        public Texture(string path)
        {
            this.Path = path;
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
