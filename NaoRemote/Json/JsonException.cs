//
// This autonomous intelligent system software is the property of Cartheur Research, Copyright 2018 - 2025, all rights reserved.
//
using System;

namespace NaoRemote.Json
{
    /// <summary>
    /// Base class throwed by LitJSON when a parsing error occurs.
    /// </summary>
    public class JsonException : ApplicationException
    {
        public JsonException()
        {
        }

        internal JsonException(ParserToken token) : base(string.Format(
                    "Invalid token '{0}' in input string", token))
        {
        }

        internal JsonException(ParserToken token, Exception innerException) : base(string.Format("Invalid token '{0}' in input string", token), innerException)
        {
        }

        internal JsonException(int c) : base(string.Format("Invalid character '{0}' in input string", (char)c))
        {
        }

        internal JsonException(int c, Exception innerException) : base(string.Format("Invalid character '{0}' in input string", (char)c), innerException)
        {
        }

        public JsonException(string message) : base(message)
        {
        }

        public JsonException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
