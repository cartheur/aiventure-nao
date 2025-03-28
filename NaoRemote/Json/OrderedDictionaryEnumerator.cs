using System.Collections;
using System.Collections.Generic;

namespace NaoRemote.Json
{
    internal class OrderedDictionaryEnumerator : IDictionaryEnumerator
    {
        readonly IEnumerator<KeyValuePair<string, JsonData>> _listEnumerator;

        public object Current
        {
            get { return Entry; }
        }

        public DictionaryEntry Entry
        {
            get
            {
                KeyValuePair<string, JsonData> curr = _listEnumerator.Current;
                return new DictionaryEntry(curr.Key, curr.Value);
            }
        }

        public object Key
        {
            get { return _listEnumerator.Current.Key; }
        }

        public object Value
        {
            get { return _listEnumerator.Current.Value; }
        }

        public OrderedDictionaryEnumerator(IEnumerator<KeyValuePair<string, JsonData>> enumerator)
        {
            _listEnumerator = enumerator;
        }

        public bool MoveNext()
        {
            return _listEnumerator.MoveNext();
        }

        public void Reset()
        {
            _listEnumerator.Reset();
        }
    }
}