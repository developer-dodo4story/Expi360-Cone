using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathHelper
{
    //wraps the angle to remain within <-PI, PI> range
    public static float WrapAngle(float a)
    {
        float angle = a;
        float twoPI = Mathf.PI * 2.0f;

        while (angle < -Mathf.PI)
            angle += twoPI;
        while (angle > Mathf.PI)
            angle -= twoPI;
        return angle;
    }

    //rotates a 2d vector, angle in radians
    public static Vector2 RotateVector2(Vector2 v, float angle)
    {
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        return new Vector2(v.x * cos + v.y * sin, -v.x * sin + v.y * cos);
    }

    //given the angles are from -PI to PI
    public static float LerpAngle(float a, float b, float t)
    {
        float twoPI = Mathf.PI * 2.0f;

        float diff = Mathf.Abs(a - b);
        float wrappedA = a;
        if(twoPI - diff < diff)
        {
            wrappedA = a > 0.0f ? a - twoPI : a + twoPI;
            diff = twoPI - diff;
        }

        return WrapAngle(wrappedA * (1.0f - t) + b * t);
    }

    public static float AngularDist(float a, float b)
    {
        float twoPI = Mathf.PI * 2.0f;
        float diff = Mathf.Abs(a - b);
        return diff > twoPI - diff ? twoPI - diff : diff;
    }

    //given the angles are from -PI to PI
    //1 clockwise, -1 anticlockwise, 0 - same angle
    public static float AngleSignedDirection(float a, float b)
    {
        float twoPI = 2.0f * Mathf.PI;

        float diff = a - b;
        while (diff < -Mathf.PI)
            diff += twoPI;
        while (diff > Mathf.PI)
            diff -= twoPI;
        return Mathf.Sign(diff);

    }

    public static Vector3 BezierCubic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float u = 1.0f - t;
        
        return p0 * Mathf.Pow(u, 3) +
               3 * p1 * Mathf.Pow(u, 2) * t +
               3 * p2 * u * Mathf.Pow(t, 2) +
               p3 * Mathf.Pow(t, 3);
    }

    public static Vector3 BezierCubicDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float u = 1.0f - t;

        return 3 *( (p1 - p0) * u * u +
                2 * (p2 - p1) * u * t +
                    (p3 - p2) * t * t);
    }

    public static bool AroundZero(float val, float eps)
    {
        return Mathf.Abs(val) < eps;
    }

    public static Vector3 LineLineIntersection(Vector3 p1, Vector3 p1dir, Vector3 p2, Vector3 p2dir)
    {
        float eps = 0.0001f;
        float t = 0.0f;
        if(AroundZero(p1dir.y, eps) && AroundZero(p2dir.y, eps))
            t = (p2.z * p2dir.x - p2.x * p2dir.z + p1.x * p2dir.z - p1.z * p2dir.x) /
                (p1dir.z * p2dir.x - p2dir.z * p1dir.x);
        else
            t = (p2.y * p2dir.x - p2.x * p2dir.y + p1.x * p2dir.y - p1.y * p2dir.x) /
                (p1dir.y * p2dir.x - p2dir.y * p1dir.x);
        return p1 + p1dir * t;
    }
}
