using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Tools
{
    [Serializable]
    struct GenerationObjectInfo : IHasGameObject
    {
        public GameObject gameObject { get; set; } // 掉落物wrapper
        public int prefabIndex; // 生成掉落物时使用的预制体索引，-1代表生成空物体

        // 序列化与反序列化

        // 序列化为字符串
        public string Serialize()
        {
            string guid = "";
#if UNITY_EDITOR
            if (gameObject != null)
            {
                string path = AssetDatabase.GetAssetPath(gameObject);
                guid = AssetDatabase.AssetPathToGUID(path);
            }
#endif
            return $"{guid}|{prefabIndex}";
        }

        // 从字符串反序列化
        public static GenerationObjectInfo Deserialize(string data)
        {
            var parts = data.Split('|');
            string guid = parts[0];
            int index = int.Parse(parts[1]);

            GameObject obj = null;
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
#endif
            return new GenerationObjectInfo { gameObject = obj, prefabIndex = index };
        }

        // 序列化为二进制
        public void Serialize(BinaryWriter writer)
        {
            string guid = "";
#if UNITY_EDITOR
            if (gameObject != null)
            {
                string path = AssetDatabase.GetAssetPath(gameObject);
                guid = AssetDatabase.AssetPathToGUID(path);
            }
#endif
            writer.Write(guid);
            writer.Write(prefabIndex);
        }

        // 从二进制反序列化
        public static GenerationObjectInfo Deserialize(BinaryReader reader)
        {
            string guid = reader.ReadString();
            int index = reader.ReadInt32();

            GameObject obj = null;
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
#endif
            return new GenerationObjectInfo { gameObject = obj, prefabIndex = index };
        }
    }


    #if UNITY_EDITOR
    /// <summary>
    /// With bugs
    /// </summary>
    [CustomPropertyDrawer(typeof(GenerationObjectInfo))]
    public class DropInfoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty prefabIndexProp = property.FindPropertyRelative("prefabIndex");
            object boxed = property.boxedValue;
            GenerationObjectInfo dropInfo = boxed is GenerationObjectInfo info ? info : default;

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float padding = 5f;
            float availableWidth = position.width;

            // 简化布局计算
            float objFieldWidth = availableWidth * 0.6f - padding;
            float indexFieldWidth = availableWidth * 0.4f;

            Rect objRect = new Rect(position.x, position.y, objFieldWidth, position.height);
            Rect indexRect = new Rect(position.x + objFieldWidth + padding, position.y, indexFieldWidth, position.height);

            EditorGUI.BeginChangeCheck();

            dropInfo.gameObject = (GameObject)EditorGUI.ObjectField(
                objRect,
                GUIContent.none,
                dropInfo.gameObject,
                typeof(GameObject),
                false
            );

            GUIContent indexLabel = new GUIContent("Index");
            dropInfo.prefabIndex = EditorGUI.IntField(indexRect, indexLabel, dropInfo.prefabIndex);

            if (EditorGUI.EndChangeCheck())
            {
                prefabIndexProp.intValue = dropInfo.prefabIndex;
                property.boxedValue = dropInfo;
                property.serializedObject.ApplyModifiedProperties();

                if (property.serializedObject.targetObject != null)
                {
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
#endif

}
