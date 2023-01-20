using System;
using System.Collections.Generic;
using System.Text;

namespace NullSoftware.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class BinaryConverterAttribute : Attribute
    {
        public Type ConverterType { get; }

        public Type SerializerType { get; set; }

        public BinaryConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }
    }
}
