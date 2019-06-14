// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;

namespace Wmhelp.XPath2
{
    public struct Integer : IFormattable, IComparable, IConvertible, IComparable<Integer>, IEquatable<Integer>
    {
        private readonly decimal _value;

        internal Integer(Decimal value)
        {
            _value = value;
        }

        public Integer(Int32 value)
        {
            _value = new Decimal(value);
        }

        public Integer(UInt32 value)
        {
            _value = new Decimal(value);
        }

        public Integer(Int64 value)
        {
            _value = new Decimal(value);
        }

        public Integer(UInt64 value)
        {
            _value = new Decimal(value);
        }

        public Integer(Integer other)
        {
            _value = other._value;
        }

        public static implicit operator Integer(byte value)
        {
            return new Integer(value);
        }

        public static implicit operator Integer(sbyte value)
        {
            return new Integer(value);
        }

        public static implicit operator Integer(short value)
        {
            return new Integer(value);
        }

        public static implicit operator Integer(ushort value)
        {
            return new Integer(value);
        }

        public static implicit operator Integer(long value)
        {
            return new Integer(value);
        }

        public static implicit operator Integer(ulong value)
        {
            return new Integer(value);
        }

        public static implicit operator Integer(int value)
        {
            return new Integer(value);
        }

        public static implicit operator Integer(uint value)
        {
            return new Integer(value);
        }

        public static explicit operator SByte(Integer i1)
        {
            return (SByte)i1._value;
        }

        public static explicit operator Byte(Integer i1)
        {
            return (Byte)i1._value;
        }

        public static explicit operator Char(Integer i1)
        {
            return (Char)i1._value;
        }

        public static explicit operator Int16(Integer i1)
        {
            return (Int16)i1._value;
        }

        public static explicit operator UInt16(Integer i1)
        {
            return (UInt16)i1._value;
        }

        public static explicit operator Int32(Integer i1)
        {
            return (Int32)i1._value;
        }

        public static explicit operator UInt32(Integer i1)
        {
            return (UInt32)i1._value;
        }

        public static explicit operator Int64(Integer i1)
        {
            return (Int64)i1._value;
        }

        public static explicit operator UInt64(Integer i1)
        {
            return (UInt64)i1._value;
        }

        public static explicit operator Single(Integer i1)
        {
            return (Single)i1._value;
        }

        public static explicit operator Double(Integer i1)
        {
            return (Double)i1._value;
        }

        public static explicit operator Decimal(Integer i1)
        {
            return i1._value;
        }

        public static explicit operator Integer(Decimal value)
        {
            return new Integer(Decimal.Truncate(value));
        }

        public static explicit operator Integer(Single value)
        {
            return new Integer(Decimal.Truncate(new Decimal(value)));
        }

        public static explicit operator Integer(Double value)
        {
            return new Integer(Decimal.Truncate(new Decimal(value)));
        }

        public override bool Equals(object obj)
        {
            if (obj is Integer integer)
            {
                return _value.Equals(integer._value);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public static string ToString(Integer value)
        {
            return value.ToString();
        }

        public static Integer operator +(Integer i1, Integer i2)
        {
            return new Integer(i1._value + i2._value);
        }

        public static Integer operator ++(Integer i1)
        {
            return new Integer(i1._value + 1);
        }

        public static Integer operator -(Integer i1)
        {
            return new Integer(0 - i1._value);
        }

        public static Integer operator -(Integer i1, Integer i2)
        {
            return new Integer(i1._value - i2._value);
        }

        public static Integer operator --(Integer i1)
        {
            return new Integer(i1._value - 1);
        }

        public static Integer operator *(Integer i1, Integer i2)
        {
            return new Integer(i1._value * i2._value);
        }

        public static Integer operator /(Integer i1, Integer i2)
        {
            return new Integer(Decimal.Truncate(i1._value / i2._value));
        }

        public static Integer operator %(Integer i1, Integer i2)
        {
            return new Integer(i1._value % i2._value);
        }

        public static bool operator ==(Integer i1, Integer i2)
        {
            return i1._value.Equals(i2._value);
        }

        public static bool operator !=(Integer i1, Integer i2)
        {
            return !i1._value.Equals(i2._value);
        }

        public static bool operator >(Integer i1, Integer i2)
        {
            return i1._value > i2._value;
        }

        public static bool operator <(Integer i1, Integer i2)
        {
            return i1._value < i2._value;
        }

        public static bool operator >=(Integer i1, Integer i2)
        {
            return i1 == i2 || i1 > i2;
        }


        public static bool operator <=(Integer i1, Integer i2)
        {
            return i1 == i2 || i1 < i2;
        }

        public static bool IsDerivedSubtype(object value)
        {
            if (value is Integer)
            {
                return true;
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;

                default:
                    return false;
            }
        }

        public static Integer ToInteger(object value)
        {
            return (Integer)Convert.ToDecimal(value);
        }

        #region IFormattable Members

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return _value.ToString(format, formatProvider);
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (!(obj is Integer))
                throw new ArgumentException("Object type must be a DataEngine.CoreServices.Integer");
            return _value.CompareTo(((Integer)obj)._value);
        }

        #endregion

        #region IConvertible Members

        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(_value, provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(_value, provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(_value, provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(_value, provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(_value, provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(_value, provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(_value, provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(_value, provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(_value, provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(_value, provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(_value, provider);
        }

        public string ToString(IFormatProvider provider)
        {
            return Convert.ToString(_value, provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return Convert.ChangeType(_value, conversionType, provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(_value, provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(_value, provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(_value, provider);
        }

        #endregion

        #region IComparable<Integer> Members

        int IComparable<Integer>.CompareTo(Integer other)
        {
            return _value.CompareTo(other._value);
        }

        #endregion

        #region IEquatable<Integer> Members

        bool IEquatable<Integer>.Equals(Integer other)
        {
            return _value.Equals(other._value);
        }

        #endregion
    }
}