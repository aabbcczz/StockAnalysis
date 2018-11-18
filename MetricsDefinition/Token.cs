namespace StockAnalysis.MetricsDefinition
{
    sealed class Token
    {
        public TokenType Type { get; private set; }

        public int StartPosition { get; private set; } // index of first character in original expression string

        public int EndPosition { get; private set; } // index of last character in orginal expression string

        public string Value { get; private set; }

        public Token(TokenType type, int startPosition, int endPostion, string value)
        {
            Type = type;
            StartPosition = startPosition;
            EndPosition = endPostion;
            Value = value;
        }
    }
}
