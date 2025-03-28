using System;

namespace NaoRemote.Json
{
    internal struct ArrayMetadata
    {
        private Type _elementType;

        public bool IsArray { get; set; }
        public bool IsList { get; set; }

        public Type ElementType
        {
            get
            {
                if (_elementType == null)
                    return typeof(JsonData);

                return _elementType;
            }

            set { _elementType = value; }
        }
    }
}
