namespace MetricsDefinition
{
    sealed class Token
    {
        public TokenType Type { get; set; }

        public int StartPosition { get; set; } // index of first character in original expression string

        public int EndPosition { get; set; } // index of last character in orginal expression string

        public string Value { get; set; }

        public Token(TokenType type, int startPosition, int endPostion, string value)
        {
            Type = type;
            StartPosition = startPosition;
            EndPosition = endPostion;
            Value = value;
        }
    }
}
