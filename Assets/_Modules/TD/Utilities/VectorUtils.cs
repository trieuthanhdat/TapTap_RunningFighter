using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD.Utilities
{
    public static class VectorUtils
    {
        public static Vector2[] CalculateBezierCurvePoints(Vector2 startPoint, Vector2 endPoint, Vector2 controlPoint, int segments)
        {
            Vector2[] points = new Vector2[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                points[i] = CalculateBezierPoint(startPoint, endPoint, controlPoint, t);
            }

            return points;
        }
        public static Vector3 CalculateMidpoint(Vector3 A, Vector3 B)
        {
            return (A + B) / 2;
        }
        public static Vector2 CalculateBezierPoint(Vector2 startPoint, Vector2 endPoint, Vector2 controlPoint, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * startPoint;
            p += 3 * uu * t * controlPoint;
            p += 3 * u * tt * endPoint;
            p += ttt * endPoint;

            return p;
        }

        public static Vector3[] CalculateBezierCurvePoints(Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint, int segments)
        {
            Vector3[] points = new Vector3[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                points[i] = CalculateBezierPoint(startPoint, endPoint, controlPoint, t);
            }

            return points;
        }

        public static Vector3 CalculateBezierPoint(Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * startPoint;
            p += 3 * uu * t * controlPoint;
            p += 3 * u * tt * endPoint;
            p += ttt * endPoint;

            return p;
        }
        /// <summary>
        /// Converts the anchoredPosition of the first RectTransform to the second RectTransform,
        /// taking into consideration offset, anchors and pivot, and returns the new anchoredPosition
        /// </summary>
        public static Vector2 ConvertToRectTransform(RectTransform from, RectTransform to)
        {
            Vector2 localPoint;
            Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * 0.5f + from.rect.xMin, from.rect.height * 0.5f + from.rect.yMin);
            Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);
            screenP += fromPivotDerivedOffset;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);
            Vector2 pivotDerivedOffset = new Vector2(to.rect.width * 0.5f + to.rect.xMin, to.rect.height * 0.5f + to.rect.yMin);
            return to.anchoredPosition + localPoint - pivotDerivedOffset;
        }


        public static bool IsInRange(float pmin, float pmax, float value)
        {
            // Determine the minimum and maximum values of A and B
            float min = Mathf.Min(pmin, pmax);
            float max = Mathf.Max(pmin, pmax);

            // Check if C falls within the range of A and B
            return (value >= min && value <= max);
        }
        public static bool IsPointInRange(Vector2 A, Vector2 B, Vector2 C)
        {
            // Calculate vectors AB, AC, and BC
            Vector2 AB = B - A;
            Vector2 AC = C - A;
            Vector2 BC = C - B;

            // Calculate dot products
            float dotProductABAC = Vector2.Dot(AB, AC);
            float dotProductABBC = Vector2.Dot(AB, BC);

            // If dot products have the same sign, then C is within the range of AB
            return (dotProductABAC >= 0 && dotProductABBC <= 0);
        }
    }

}

