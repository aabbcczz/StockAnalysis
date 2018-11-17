namespace StockAnalysis.Common.SymbolName
{
    using System;
    using Exchange;

    /// <summary>
    /// class represents security symbol
    /// </summary>
    public sealed class SecuritySymbol : IComparable, IEquatable<SecuritySymbol>, IComparable<SecuritySymbol>
    {
        /// <summary>
        /// Raw symbol, does not include the modifier of security exchange. 
        /// For example, 000001 is raw symbol, SH.000001 is the normalized symbol
        /// </summary>
        public string RawSymbol { get; private set; }
        /// <summary>
        /// Normalized symbole, which includes the modifier of security exchange.
        /// For example, 000001 is raw symbol, SH.000001 is the normalized symbol
        /// </summary>
        public string NormalizedSymbol { get; private set; }

        /// <summary>
        /// The id of security exchange to which the symbol belongs
        /// </summary>
        public ExchangeId ExchangeId { get; private set; }


        public SecuritySymbol(string rawSymbol, string normalizedSymbol, ExchangeId exchangeId)
        {
            if (string.IsNullOrWhiteSpace(rawSymbol) || string.IsNullOrWhiteSpace(normalizedSymbol))
            {
                throw new ArgumentNullException();
            }

            RawSymbol = rawSymbol;
            NormalizedSymbol = normalizedSymbol;
            ExchangeId = exchangeId;
        }

        #region IComparable member
        public int CompareTo(object obj)
        {
            return CompareTo(obj as SecuritySymbol);
        }

        #endregion

        #region IComparable<Symbol> member
        public int CompareTo(SecuritySymbol other)
        {
            if (ReferenceEquals(other, null))
            {
                throw new ArgumentNullException();
            }

            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            int result = string.Compare(NormalizedSymbol, other.NormalizedSymbol, StringComparison.OrdinalIgnoreCase);
            if (result != 0)
            {
                return result;
            }
            else
            {
                return string.Compare(RawSymbol, other.RawSymbol, StringComparison.OrdinalIgnoreCase);
            }
        }

        public static bool operator <(SecuritySymbol left, SecuritySymbol right)
        {
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                throw new ArgumentNullException();
            }

            return left.CompareTo(right) < 0;
        }

        public static bool operator >(SecuritySymbol left, SecuritySymbol right)
        {
            return right < left;
        }

        public static bool operator <=(SecuritySymbol left, SecuritySymbol right)
        {
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                throw new ArgumentNullException();
            }

            return left.CompareTo(right) <= 0;

        }

        public static bool operator >=(SecuritySymbol left, SecuritySymbol right)
        {
            return right <= left;
        }

        #endregion

        #region IEquatable<Symbol> members

        /// <summary>
        /// Checks whether this object is equal to the specified one.
        /// </summary>
        /// 
        /// <param name="obj">The object to compare with.</param>
        /// 
        /// <returns><c>true</c> if this object is equal to <paramref name="obj"/>.</returns>
        /// 
        /// <remarks>
        /// This object is equal to <paramref name="obj"/> if <paramref name="obj"/> is not
        /// <c>null</c> and is of the same type as this object and ...
        /// </remarks>
        /// 
        public override bool Equals(object obj)
        {
            return Equals(obj as SecuritySymbol);
        }

        /// <summary>
        /// Generates a hash code for this object.
        /// </summary>
        /// 
        /// <returns>The hash code of this object.</returns>
        public override int GetHashCode()
        {
            return (RawSymbol.ToUpperInvariant().GetHashCode() << 16) ^ (NormalizedSymbol.ToUpperInvariant().GetHashCode());
        }

        /// <summary>
        /// Checks whether this object is equal to the specified one.
        /// </summary>
        /// 
        /// <param name="other">The object to compare with.</param>
        /// 
        /// <returns><c>true</c> if this object is equal to <paramref name="other"/>.</returns>
        /// 
        /// <remarks>
        /// This object is equal to <paramref name="other"/> if <paramref name="other"/>
        /// is not <c>null</c> and ...
        /// </remarks>
        public bool Equals(SecuritySymbol other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            else
            {
                if (RawSymbol.Equals(other.RawSymbol, StringComparison.InvariantCultureIgnoreCase)
                    && NormalizedSymbol.Equals(other.NormalizedSymbol, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Checks whether the specified objects are equal.
        /// </summary>
        /// 
        /// <param name="left">The object to compare with.</param>
        /// <param name="right">The object to compare.</param>
        /// 
        /// <returns><c>true</c> if <paramref name="left"/> is equal to <paramref name="right"/>.</returns>
        /// 
        /// <remarks>
        /// The two objects are equal if both are <c>null</c> or ...
        /// </remarks>
        /// 
        public static bool operator ==(SecuritySymbol left, SecuritySymbol right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (ReferenceEquals(left, null))
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Checks whether the specified objects are not equal.
        /// </summary>
        /// 
        /// <param name="left">The object to compare with.</param>
        /// <param name="right">The object to compare.</param>
        /// 
        /// <returns><c>true</c> if <paramref name="left"/> is not equal to <paramref name="right"/>.</returns>
        /// 
        /// <remarks>
        /// The two objects are not equal if only one of them is <c>null</c> or ...
        /// </remarks>
        /// 
        public static bool operator !=(SecuritySymbol left, SecuritySymbol right)
        {
            if (ReferenceEquals(left, right))
                return false;

            if (ReferenceEquals(left, null))
                return true;

            return !left.Equals(right);
        }

        #endregion
    }
}
