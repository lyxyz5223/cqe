using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Tools
{
    [Serializable]
    public class FloatRange
    {
        [Tooltip("Starting value of the range")]
        public float start = 0f;
        [Tooltip("If the starting value is relative to the right")]
        public bool startRelativeRight = false;

        [Space(10)]
        [Tooltip("Ending value of the range")]
        public float end = 0f;
        [Tooltip("If the ending value is relative to the right")]
        public bool endRelativeRight = true;


        public FloatRange() { }
        public FloatRange(float start, float end, bool startRelativeRight = false, bool endRelativeRight = true)
        {
            this.start = start;
            this.end = end;
            this.startRelativeRight = startRelativeRight;
            this.endRelativeRight = endRelativeRight;
        }

        public FloatRange GetAbsoluteRange(float len)
        {
            return new FloatRange(startRelativeRight ? len + start : start, endRelativeRight ? len + end : end, false, false);
        }
    }

    [Serializable]
    public class IntRange
    {
        // if this is negative, it is relative to the right, else it is relative to the left
        public int start = 0;
        // if this is negative, it is relative to the right, else it is relative to the left
        public int end = 0;

        public IntRange()
        {

        }
        public IntRange(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        /// <summary>
        /// 获取绝对范围，即将负数转换为相对于数组长度的正数
        /// </summary>
        /// <param name="arrayLen">数组长度</param>
        /// <returns>绝对范围</returns>
        public IntRange GetAbsoluteRange(int arrayLen)
        {
            return new IntRange(MathFns.Mod(start, arrayLen), MathFns.Mod(end, arrayLen));
            //return new IntRange(start >= 0 ? start : arrayLen + start, end >= 0 ? end : arrayLen + end);
        }
    }

#if UNITY_EDITOR
    //[CustomPropertyDrawer(typeof(FloatRange))]
    public class FloatRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 获取字段
            var startProp = property.FindPropertyRelative("start");
            var endProp = property.FindPropertyRelative("end");
            var startRelativeRightProp = property.FindPropertyRelative("startRelativeRight");
            var endRelativeRightProp = property.FindPropertyRelative("endRelativeRight");

            // 计算布局
            float labelWidth = EditorGUIUtility.labelWidth;
            float dashWidth = 15;
            float checkerWidth = 15; // 用于显示相对/绝对的标志
            float fieldWidth = (position.width - labelWidth - dashWidth - checkerWidth * 2) / 2;

            Rect labelRect = new (position.x, position.y, labelWidth, position.height);
            Rect startRect = new (position.x + labelWidth, position.y, fieldWidth, position.height);
            Rect checkerStartRect = new (position.x + labelWidth + fieldWidth, position.y, checkerWidth, position.height);
            Rect dashRect = new (position.x + labelWidth + fieldWidth + checkerWidth, position.y, dashWidth, position.height);
            Rect endRect = new (position.x + labelWidth + fieldWidth + checkerWidth + dashWidth, position.y, fieldWidth, position.height);
            Rect checkerEndRect = new (position.x + labelWidth + fieldWidth + checkerWidth + dashWidth + fieldWidth, position.y, checkerWidth, position.height);

            // 绘制
            EditorGUI.PrefixLabel(labelRect, label);
            EditorGUI.PropertyField(startRect, startProp, GUIContent.none);
            EditorGUI.PropertyField(endRect, endProp, GUIContent.none);
            startRelativeRightProp.boolValue = EditorGUI.Toggle(checkerStartRect, startRelativeRightProp.boolValue);
            endRelativeRightProp.boolValue = EditorGUI.Toggle(checkerEndRect, endRelativeRightProp.boolValue);
            GUIStyle centeredStyle = new(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUI.LabelField(dashRect, "~", centeredStyle);
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(IntRange))]
    public class IntRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 获取字段
            var startProp = property.FindPropertyRelative("start");
            var endProp = property.FindPropertyRelative("end");

            // 计算布局
            float labelWidth = EditorGUIUtility.labelWidth;
            float dashWidth = 15;
            float fieldWidth = (position.width - labelWidth - dashWidth) / 2;

            Rect labelRect = new (position.x, position.y, labelWidth, position.height);
            Rect startRect = new (position.x + labelWidth, position.y, fieldWidth, position.height);
            Rect dashRect = new (position.x + labelWidth + fieldWidth, position.y, dashWidth, position.height);
            Rect endRect = new (position.x + labelWidth + fieldWidth + dashWidth, position.y, fieldWidth, position.height);

            // 绘制
            EditorGUI.PrefixLabel(labelRect, label);
            EditorGUI.PropertyField(startRect, startProp, GUIContent.none);
            GUIStyle centeredStyle = new(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUI.LabelField(dashRect, "~", centeredStyle);
            EditorGUI.PropertyField(endRect, endProp, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
#endif

}
