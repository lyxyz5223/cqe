using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Utils
{
    public class ObjectPool<Key, ObjType>
    {
        private Dictionary<Key, Queue<ObjType>> pool = new Dictionary<Key, Queue<ObjType>>();
        public static readonly bool IsDisposable = typeof(IDisposable).IsAssignableFrom(typeof(ObjType));
        public readonly int MaxSize;
        public int Count => pool.Count;
        /// <summary>
        /// 构造一个对象池
        /// </summary>
        /// <param name="maxSize">对象池中每个key的最大对象数量，-1表示不限制</param>
        public ObjectPool(int maxSize = 100)
        {
            this.MaxSize = maxSize;
        }
        public ObjType Get(Key key)
        {
            if (pool.TryGetValue(key, out var queue) && queue.Count > 0)
                return queue.Dequeue();
            return default; // 返回默认值
        }
        public ObjType Get(Key key, Func<ObjType> creator)
        {
            if (pool.TryGetValue(key, out var queue) && queue.Count > 0)
                return queue.Dequeue();
            return creator != null ? creator() : default;
        }

        public void Return(Key key, ObjType item)
        {
            if (!pool.TryGetValue(key, out var queue))
            {
                queue = new Queue<ObjType>();
                pool[key] = queue;
            }
            if (MaxSize > 0 && queue.Count >= MaxSize)
            {
                if (IsDisposable && item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                return;
            }
            queue.Enqueue(item);
        }
        public void Clear()
        {
            if (IsDisposable)
            {
                foreach (var queue in pool.Values)
                {
                    while (queue.Count > 0)
                    {
                        if (queue.Dequeue() is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }
            pool.Clear();
        }

    }

    public class ObjectPool<ObjType>
    {
        private Queue<ObjType> pool = new();
        public static readonly bool IsDisposable = typeof(IDisposable).IsAssignableFrom(typeof(ObjType));
        public readonly int MaxSize;

        public int Count => pool.Count;

        /// <summary>
        /// 构造一个对象池
        /// </summary>
        /// <param name="maxSize">对象池中最大对象数量，-1表示不限制</param>
        public ObjectPool(int maxSize = 100)
        {
            this.MaxSize = maxSize;
        }

        public ObjType Get()
        {
            if (pool.Count > 0)
                return pool.Dequeue();
            return default; // 返回默认值
        }

        public ObjType Get(Func<ObjType> creator)
        {
            if (pool.Count > 0)
                return pool.Dequeue();
            return creator != null ? creator() : default;
        }

        public void Return(ObjType item)
        {
            if (MaxSize > 0 && pool.Count >= MaxSize)
            {
                if (IsDisposable && item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                return;
            }
            pool.Enqueue(item);
        }
        public void Clear()
        {
            if (IsDisposable)
            {
                while (pool.Count > 0)
                {
                    if (pool.Dequeue() is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
            pool.Clear();
        }
    }
}
