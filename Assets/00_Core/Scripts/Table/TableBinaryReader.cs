using System;
using System.IO;

namespace Base.Data
{
    public class TableBinaryReader : BinaryReader
    {
        public TableBinaryReader(Stream input) : base(input) { }

        public void Read(out int value) => value = ReadInt32();
        public void Read(out bool value) => value = ReadBoolean();
        public void Read(out float value) => value = ReadSingle();
        public void Read(out string value) => value = ReadString();
        public void Read(out TblIndex value) => value = new TblIndex(ReadInt32());
        
        public void Read<TEnum>(out TEnum value) where TEnum : Enum 
            => value = (TEnum)Enum.ToObject(typeof(TEnum), ReadInt32());
        
        public object ReadField(Type type)
        {
            if (type == typeof(int)) return ReadInt32();
            if (type == typeof(float)) return ReadSingle();
            if (type == typeof(string)) return ReadString();
            if (type == typeof(bool)) return ReadBoolean();
            if (type == typeof(TblIndex)) return new TblIndex(ReadInt32());
            if (type.IsEnum) return Enum.ToObject(type, ReadInt32());

            return null;
        }
    }
}