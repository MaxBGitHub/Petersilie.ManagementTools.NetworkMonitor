using System;
using System.Collections.Generic;


namespace Petersilie.ManagementTools.NetworkMonitor
{
    internal class NestedMap<TKey, TSubKey, TValue>
    {
        Dictionary<TKey, Dictionary<TSubKey, TValue>> _keyStore;


        public Tuple<TKey, TSubKey, TValue> this[TKey key, TSubKey subKey]
        {
            get
            {
                if (_keyStore.ContainsKey(key)) {
                    if (_keyStore[key].ContainsKey(subKey)) {
                        return Tuple.Create(key, subKey, _keyStore[key][subKey]);
                    }
                }
                return null;
            }
        }


        public Dictionary<TSubKey, TValue> this[TKey key]
        {
            get
            {
                if (_keyStore.ContainsKey(key)) {
                    return _keyStore[key];
                }
                return null;
            }
        }


        public void Add(TKey key, TSubKey subKey, TValue value)
        {
            if (!(_keyStore.ContainsKey(key))) {
                _keyStore.Add(key, new Dictionary<TSubKey, TValue>() { { subKey, value } });
            } else {
                if (!(_keyStore[key].ContainsKey(subKey))) {
                    _keyStore[key].Add(subKey, value);
                }
            }
        }


        public NestedMap()
        {
            _keyStore = new Dictionary<TKey, Dictionary<TSubKey, TValue>>();
        }
    }
}
