using System;
using System.Reflection;

namespace NaoRemote.Json
{
    internal struct PropertyMetadata
    {
        public MemberInfo Info;
        public bool IsField;
        public Type Type;
    }
}
