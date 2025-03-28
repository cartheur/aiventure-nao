using System;
using System.Collections.Generic;

namespace NaoRemote.Json
{
    internal struct ObjectMetadata
    {
        private Type _elementType;

        public bool IsDictionary { get; set; }
        public IDictionary<string, PropertyMetadata> Properties { get; set; }

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