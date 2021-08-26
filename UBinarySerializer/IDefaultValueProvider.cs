using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization
{
    internal interface IDefaultValueProvider
    {
        object DefaultValue { get; }

        T GetDefaultValue<T>();
    }
}
