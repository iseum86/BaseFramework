using System;
using System.Collections.Generic;

namespace Base.Data
{
    [Serializable]
    public struct TblIndex : IEquatable<TblIndex>, IComparable<TblIndex>
    {
        private int _value;
        public int Value => _value;
        
        public static TblIndex Invalid => new TblIndex(-1);

        public TblIndex(int value) => _value = value;

        public bool Equals(TblIndex other) => _value == other._value;
        public override bool Equals(object obj) => obj is TblIndex other && Equals(other);
        public override int GetHashCode() => _value;
        public int CompareTo(TblIndex other) => _value.CompareTo(other._value);

        public static bool operator ==(TblIndex a, TblIndex b) => a.Equals(b);
        public static bool operator !=(TblIndex a, TblIndex b) => !a.Equals(b);
    }

    public class TblIndexComparer : IEqualityComparer<TblIndex>
    {
        public bool Equals(TblIndex a, TblIndex b) => a.Value == b.Value;
        public int GetHashCode(TblIndex obj) => obj.GetHashCode();
    }
}