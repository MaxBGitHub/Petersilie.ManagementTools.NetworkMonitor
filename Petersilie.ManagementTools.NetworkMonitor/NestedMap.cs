using System;
using System.Collections.Generic;


namespace Petersilie.ManagementTools.NetworkMonitor
{
    /* Wrapper class for a nested dictionary.
    ** Used for the mapping of ICMP type, code and description.
    ** Other data structures that could have achieved this
    ** where overkill so I went with the nested dictionary.
    ** I know it's not pretty... but it does the job. */
    internal class NestedMap<TKey, TSubKey, TValue>
    {
        // Internal dictionary.
        Dictionary<TKey, Dictionary<TSubKey, TValue>> _keyStore;


        /* Indexer to get value from key and subkey combination.
        ** Returns a Tuple<T1, T2, T3>. */
        public Tuple<TKey, TSubKey, TValue> this[TKey key, TSubKey subKey]
        {
            get
            {
                if (_keyStore.ContainsKey(key)) {
                    if (_keyStore[key].ContainsKey(subKey)) {
                        return Tuple.Create(key, subKey, _keyStore[key][subKey]);
                    } /* Second level storage contains subkey. */
                } /* First level storage contains key. */
                return null;
            }
        }


        /* Indexer to get all subkey/value combinations from
        ** first level key storage. */
        public Dictionary<TSubKey, TValue> this[TKey key]
        {
            get
            {
                if (_keyStore.ContainsKey(key)) {
                    return _keyStore[key];
                } /* First level storage has key. */
                return null;
            }
        }


        // Adds a new key/subkey/value pair 
        public void Add(TKey key, TSubKey subKey, TValue value)
        {
            if (!(_keyStore.ContainsKey(key))) {
                _keyStore.Add(key, new Dictionary<TSubKey, TValue>() {
                    { subKey, value } // init with values.
                });
            } /* First level storage does not have primary key. */
            else {
                if (!(_keyStore[key].ContainsKey(subKey))) {
                    _keyStore[key].Add(subKey, value);
                } /* Seconds level storage does not have sub key. */
            } /* First level storage already has primary key. */
        }


        public NestedMap()
        {
            _keyStore = new Dictionary<TKey, Dictionary<TSubKey, TValue>>();
        }
    }
}
