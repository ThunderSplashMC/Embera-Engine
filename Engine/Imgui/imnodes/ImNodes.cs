using System;
using System.Collections.Generic;
using System.Numerics;
using DevoidEngine.Engine.Imgui.imnodes;

namespace DevoidEngine.Engine.Imgui.imnodes
{
    struct CubicBezier
    {
        public Vector2 P0, P1, P2, P3;
        public int NumSegments;
    }

    class ImNodes
    {
        static GImNodes GImNodes;

        public static Vector2 EvalCubicBezier
        (
            float t,
            ref Vector2 P0,
            ref Vector2 P1,
            ref Vector2 P2,
            ref Vector2 P3
        )
        {
            float u = 1.0f - t;
            float b0 = u * u * u;
            float b1 = 3 * u * u * t;
            float b2 = 3 * u * t * t;
            float b3 = t * t * t;
            return new Vector2(b0 * P0.X + b1 * P1.X + b2 * P2.X + b3 * P3.X, b0 * P0.Y + b1 * P1.Y + b2 * P2.Y + b3 * P3.Y);
        }

        public static Vector2 GetClosestPointOnCubicBezier(int num_segments, ref Vector2 p, ref CubicBezier cb)
        {
            ASSERT(num_segments > 0);
            Vector2 p_last = cb.P0;
            Vector2 p_closest = new Vector2();
            float p_closest_dist = float.MaxValue;
            float t_step = 1.0f / (float)num_segments;
            for (int i = 1; i <= num_segments; ++i)
            {
                Vector2 p_current = EvalCubicBezier(t_step * i, ref cb.P0, ref cb.P1, ref cb.P2, ref cb.P3);
                Vector2 p_line = ImLineClosestPoint(p_last, p_current, p);
                float dist = ImLengthSqr(p - p_line);
                if (dist < p_closest_dist)
                {
                    p_closest = p_line;
                    p_closest_dist = dist;
                }
                p_last = p_current;
            }
            return p_closest;
        }

        public static float ImLengthSqr(Vector2 lhs)
        {
            return lhs.X * lhs.X + lhs.Y * lhs.Y;
        }

        static Vector2 ImMax(ref Vector2 lhs, ref Vector2 rhs) { return new Vector2(lhs.X >= rhs.X? lhs.X : rhs.X, lhs.Y >= rhs.Y? lhs.Y : rhs.Y); }
        static Vector2 ImMin(Vector2 lhs, Vector2 rhs) { return new Vector2(lhs.X<rhs.X? lhs.X : rhs.X, lhs.Y<rhs.Y? lhs.Y : rhs.Y); }
        static float ImMin(float X1, float X2) { return X1 < X2 ? X1 : X2; }
        static float ImMax(float X1, float X2) { return X1 > X2 ? X1 : X2; }

        static float GetDistanceToCubicBezier(
            ref Vector2 pos,
            ref CubicBezier cubic_bezier,
            int num_segments
        )
        {
            Vector2 point_on_curve = GetClosestPointOnCubicBezier(num_segments, ref pos, ref cubic_bezier);

            Vector2 to_curve = point_on_curve - pos;
            
            return (float)Math.Sqrt(ImLengthSqr(to_curve));
        }

        static ImRect GetContainingRectForCubicBezier(ref CubicBezier cb)
        {
            Vector2 min = new Vector2(ImMin(cb.P0.X, cb.P3.X), ImMin(cb.P0.Y, cb.P3.Y));
            Vector2 max = new Vector2(ImMax(cb.P0.X, cb.P3.X), ImMax(cb.P0.Y, cb.P3.Y));

            float hover_distance = GImNodes.Style.LinkHoverDistance;

            ImRect rect = new ImRect() { Min = min, Max = max };
            rect.Add(cb.P1);
            rect.Add(cb.P2);
            rect.Expand(new Vector2(hover_distance, hover_distance));

            return rect;
        }

    public static Vector2 ImLineClosestPoint(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ap = p - a;
            Vector2 ab_dir = b - a;
            float ab_len = (float)Math.Sqrt(ab_dir.X * ab_dir.X + ab_dir.Y * ab_dir.Y);
            ab_dir *= 1.0f / ab_len;
            float dot = ap.X * ab_dir.X + ap.Y * ab_dir.Y;
            float ab_len_sqr = 0;
            if (dot < 0.0f)
            {
                return a;
            }
            if (dot > ab_len)
            {
                ab_len_sqr = ab_dir.X * ab_dir.X + ab_dir.Y * ab_dir.Y;
            }
            if (dot > ab_len_sqr)
            {
                return b;
            }
            return a + ab_dir * dot;
            return a + ab_dir * dot / ab_len_sqr;
        }

        public static void ASSERT(bool x)
        {
            if (!x)
            {
                throw new Exception();
            }
        }
    }
}
