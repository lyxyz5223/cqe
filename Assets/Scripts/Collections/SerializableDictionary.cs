using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace Assets.Editor
{

    // 自定义的序列化字典
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [System.Serializable]
        public class Pair
        {
            public TKey key;
            public TValue value;
        }

        [SerializeField]
        private List<Pair> pairs = new List<Pair>();

        private Dictionary<TKey, TValue> runtimeDict;

        public Dictionary<TKey, TValue> ToDictionary()
        {
            if (runtimeDict == null)
            {
                runtimeDict = new Dictionary<TKey, TValue>();
            }
            if (runtimeDict.Count != pairs.Count)
            {
                runtimeDict.Clear();
                foreach (var pair in pairs)
                {
                    if (pair.key != null && !runtimeDict.ContainsKey(pair.key))
                        runtimeDict.Add(pair.key, pair.value);
                }
            }
            return runtimeDict;
        }

        public TValue Get(TKey key)
        {
            ToDictionary();
            runtimeDict.TryGetValue(key, out TValue value);
            return value;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            ToDictionary();
            return runtimeDict.TryGetValue(key, out value);
        }

        // 允许编辑器访问内部列表
        public List<Pair> Pairs => pairs;
    }

}
