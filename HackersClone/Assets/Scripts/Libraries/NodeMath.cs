using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NodeMath
{
    static public (float x, float z) FindPointFromAngle(float angle, float radius)
    {
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);
        return (x, y);
    }

    static public (float x, float z) FindPointFromAngle(float angle, float radius, (float x, float y) offset)
    {
        (float x, float y) baseCoordinate = FindPointFromAngle(angle, radius);
        return (baseCoordinate.x + offset.x, baseCoordinate.y + offset.y);
    }

    static public (float x, float y, float z) MidpointFormula(float x1, float x2, float y1, float y2, float z1, float z2)
    {
        (float x, float y, float z) midpoint;
        midpoint.x = (x1 + x2) / 2;
        midpoint.z = (z1 + z2) / 2;
        midpoint.y = (y1 + y2) / 2;
        return midpoint;

    }

    static public float DistanceFormula(float x1, float y1, float x2, float y2)
    {
        float x = Mathf.Pow((x2 - x1), 2);
        float y = Mathf.Pow((y2 - y1), 2);
        return Mathf.Sqrt(x + y);
    }

    static public (float x, float z) FindThirdPointOfTriangle(float x1, float y1, float x2, float y2, bool BToLeft)
    {
        float x = 0, z = 0;
        if (BToLeft)
        {
            x = ((x1 + x2) - Mathf.Sqrt(3.0f) * (y2 - y1)) / 2.0f;
            z = ((y1 + y2) + Mathf.Sqrt(3.0f) * (x2 - x1)) / 2.0f;
        }
        else if (!BToLeft)
        {
            x = ((x1 + x2) + Mathf.Sqrt(3.0f) * (y2 - y1)) / 2.0f;
            z = ((y1 + y2) - Mathf.Sqrt(3.0f) * (x2 - x1)) / 2.0f;
        }
        return (x, z);
    }


    //So I'm not that smart so everything for finding the AngleBetweenNodes is adapted from this script https://unitycoder.com/blog/2015/12/17/get-angle-between-2-gameobjects-in-degrees-0-360/
    //By adapted I mean changed it to return an angle instead of be in a Update() loop. So really its the same script. Anyway. Carry on.
    static public float AngleBetweenNodes(Vector3 positionA, Vector3 positionB)
    {
        var myPos = positionA;
        myPos.y = 0;

        var targetPos = positionB;
        targetPos.y = 0;

        Vector3 toOther = (myPos - targetPos).normalized;

        float angle = Mathf.Atan2(toOther.z, toOther.x) * Mathf.Rad2Deg + 180;
        float angleOpt = atan2Approximation(toOther.z, toOther.x) * Mathf.Rad2Deg + 180;

        Debug.DrawLine(myPos, targetPos, Color.yellow);
        return angle;
    }

    static float atan2Approximation(float y, float x)
    {
        float t0, t1, t2, t3, t4;
        t3 = Mathf.Abs(x);
        t1 = Mathf.Abs(y);
        t0 = Mathf.Max(t3, t1);
        t1 = Mathf.Min(t3, t1);
        t3 = 1f / t0;
        t3 = t1 * t3;
        t4 = t3 * t3;
        t0 = -0.013480470f;
        t0 = t0 * t4 + 0.057477314f;
        t0 = t0 * t4 - 0.121239071f;
        t0 = t0 * t4 + 0.195635925f;
        t0 = t0 * t4 - 0.332994597f;
        t0 = t0 * t4 + 0.999995630f;
        t3 = t0 * t3;
        t3 = (Mathf.Abs(y) > Mathf.Abs(x)) ? 1.570796327f - t3 : t3;
        t3 = (x < 0) ? 3.141592654f - t3 : t3;
        t3 = (y < 0) ? -t3 : t3;
        return t3;
    }





}
