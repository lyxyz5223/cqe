using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Splines;

namespace Assets.Scripts.Utils
{
    public static class SplinesUtils
    {

        public static float GetLengthAlongAxis(Spline spline, Axis axis)
        {
            var start = spline.EvaluatePosition(0f);
            var end = spline.EvaluatePosition(1f);
            switch (axis)
            {
                case Axis.X:
                    return Mathf.Abs(end.x - start.x);
                case Axis.Y:
                    return Mathf.Abs(end.y - start.y);
                case Axis.Z:
                    return Mathf.Abs(end.z - start.z);
                default:
                    throw new System.ArgumentException("Invalid axis specified.");
            }
        }
    }
}
