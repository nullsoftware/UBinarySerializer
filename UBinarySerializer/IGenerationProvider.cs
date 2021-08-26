using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization
{
    internal interface IGenerationProvider
    {
        ushort Generation { get; }
    }
}
