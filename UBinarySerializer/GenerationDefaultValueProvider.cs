using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullSoftware.Serialization
{
    internal class GenerationDefaultValueProvider : IGenerationProvider, IDefaultValueProvider
    {
        public ushort Generation { get; }

        public object DefaultValue { get; }

        public GenerationDefaultValueProvider(ushort generation)
        {
            Generation = generation;
        }

        public GenerationDefaultValueProvider(ushort generation, object defaultValue) : this(generation)
        {
            DefaultValue = defaultValue;
        }

        public static implicit operator ushort(GenerationDefaultValueProvider provider) => provider.Generation;

        public T GetDefaultValue<T>()
        {
            return (T)DefaultValue;
        }

        public override string ToString()
        {
            return $"{Generation} {DefaultValue}";
        }
    }
}
