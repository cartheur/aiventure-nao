//
// This autonomous intelligent system software is the property of Cartheur Research, Copyright 2018 - 2025, all rights reserved.
//
namespace NaoRemote.Json
{
    /// <summary>
    /// Internal representation of the tokens used by the lexer and the parser.
    /// </summary>
    internal enum ParserToken
    {
        // Lexer tokens (see section A.1.1. of the manual)
        None = System.Char.MaxValue + 1,
        Number,
        True,
        False,
        Null,
        CharSeq,
        // Single char
        Char,
        // Parser Rules (see section A.2.1 of the manual)
        Text,
        Object,
        ObjectPrime,
        Pair,
        PairRest,
        Array,
        ArrayPrime,
        Value,
        ValueRest,
        String,
        // End of input
        End,
        // The empty rule
        Epsilon
    }
}
