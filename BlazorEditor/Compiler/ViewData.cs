using System;
using System.Collections.Generic;

namespace BlazorEditor.Compiler
{
    public class ViewData
    {
        private readonly Dictionary<string, dynamic> _dictionary;
        public Action<Dictionary<string, dynamic>> Callback;

        public ViewData()
        {
            _dictionary = new Dictionary<string, dynamic>();
        }

        public int Count => _dictionary.Count;

        public Dictionary<string, dynamic>.KeyCollection Keys => _dictionary.Keys;

        public Dictionary<string, dynamic>.ValueCollection Values => _dictionary.Values;

        public dynamic this[string key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                _dictionary[key] = value;
                Callback.Invoke(_dictionary);
            }
        }    
        
        public void Add(string key, dynamic value)
        {
            _dictionary.Add(key, value);
            Callback.Invoke(_dictionary);
        }

        public bool Remove(string key)
        {
            bool result = _dictionary.Remove(key);
            Callback.Invoke(_dictionary);
            return result;
        }
    }
}