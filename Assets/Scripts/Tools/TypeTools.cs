using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    // 全局类型缓存工具
    public static class TypeCache<T>
    {
        // 静态字段只初始化一次，线程安全
        public static readonly Type Type = typeof(T);
        public static readonly RuntimeTypeHandle TypeHandle = Type.TypeHandle;
    }

    /// <summary>
    /// 基于类型和实例的复合缓存Key
    /// </summary>
    public struct TypeInstanceKey : IEquatable<TypeInstanceKey>
    {
        public readonly RuntimeTypeHandle TypeHandle;
        public readonly int InstanceID;
        public TypeInstanceKey(Type type, int instanceID)
        {
            TypeHandle = type.TypeHandle;
            InstanceID = instanceID;
        }
        public TypeInstanceKey(GameObject go)
        {
            TypeHandle = go.GetType().TypeHandle;
            InstanceID = go.GetInstanceID();
        }

        // 为Component使用
        public TypeInstanceKey(Component component) : this(component.gameObject) { }
        public bool Equals(TypeInstanceKey other)
        {
            return TypeHandle.Equals(other.TypeHandle) && InstanceID == other.InstanceID;
        }
        public override bool Equals(object obj)
        {
            return obj is TypeInstanceKey other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (TypeHandle.GetHashCode() * 397) ^ InstanceID;
            }
        }
    }

}
