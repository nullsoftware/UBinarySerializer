[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner-direct-single.svg)](https://stand-with-ukraine.pp.ua)

[![](https://img.shields.io/nuget/vpre/UBinarySerializer)](https://www.nuget.org/packages/UBinarySerializer/)
[![](https://img.shields.io/nuget/dt/UBinarySerializer)](https://www.nuget.org/packages/UBinarySerializer/)

# UBinarySerializer
Framework for data binary serialization.

## Getting started.
Use one of the follwing methods to install and use this library:

- **Package Manager:**

    ```batch
    PM> Install-Package UBinarySerializer
    ```

- **.NET CLI:**

    ```batch
    > dotnet add package UBinarySerializer
    ```
----
To create serializer for specific object type, 
you need to create `BinarySerializer<>` instance:
```C#
BinarySerializer<MyObject> serializer = new BinarySerializer<MyObject>();
```
Or to create unsafe serializer:
```C#
BinaryUnsafeSerializer<MyObject> serializer = new BinaryUnsafeSerializer<MyObject>();
```

Then to serialize object to binary data use `Serialize` or `SerializeObject` methods.  

Difference between unsafe and safe serializers, is that **safe** serializer serializes the data safely, 
allows to serialize null-reference objects and saves object data version (generation) for backward-compatibility.  
**Unsafe** serializer optimized for fast serialization, and disallows null-references.  

To control fields and properties during serialization use `BinIndexAttribute`:

```C#
using System;
using NullSoftware.Serialization;

public class Player
{
    [BinIndex(0)]
    public int Health { get; set; }

    [BinIndex(1, Generation = 2)]
    public int Hunger { get; set; }

    [BinIndex(2)]
    public Vector3 Postion { get; set; }

    [BinIndex(3)]
    public GameMode GameMode { get; set; }

    [BinIndex(4)]
    public Texture Skin { get; set; }

    [BinIndex(5)]
    public List<Item> Items { get; set; }
        = new List<Item>();
    
    public int DeathsCount { get; set; } // will not be serialized.
}
```
`Generation` allows to add new fields/properties for already serialized object without breaking serialization (in case if **safe** serializer was used).
If there is no `BinIndexAttribute` specified in object will be serialized all not-readonly fields/properties.  

Also can be used `RequiredAttribute` from `System.ComponentModel.DataAnnotations` to specify that current field/property can not have null-reference even in **safe** serialization.

Also there is possible to craete custom binary converter using `IBinaryConverter` interface, and `BinaryConverterAttribute` for target object.

Converter:
```C#
public class FourCharacterCodeConverter : IBinaryConverter
{
    public void ToBytes(MemberInfo member, BinaryWriter stream, object value, object parameter)
    {
        stream.Write(((FourCharacterCode)value).Value);
    }

    public object ToValue(MemberInfo member, BinaryReader stream, object parameter)
    {
        return new FourCharacterCode(stream.ReadBytes(4));
    }
}
```

Converter target:
```C#
[BinaryConverter(typeof(FourCharacterCodeConverter)/*,  SerializerType = typeof(BinaryUnsafeSerializer) */)]
public struct FourCharacterCode : IEquatable<FourCharacterCode>
{
    public byte[] Value { get; } // must be 4-bytes length

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
```
