//
// This autonomous intelligent system software is the property of Cartheur Research, Copyright 2018 - 2025, all rights reserved.
//
using NaoRemote.Json;
using System.Collections.Generic;

namespace NaoRemote
{
    public class NaoEventData
    {
        private readonly string _data;
        private object _extracted;

        public NaoEventData(string data)
        {
            _data = data;
        }

        public T Extract<T>()
        {
            if (_extracted == null)
                _extracted = JsonMapper.ToObject<List<T>>(_data)[0];
            return (T)_extracted;
        }
    }
}
