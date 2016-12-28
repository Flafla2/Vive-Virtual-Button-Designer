using UnityEngine;
using System;

[Serializable]
public class VirtualButton
{
    public string ButtonName;
    public PadAABB Region;
}

[Serializable]
public class PadAABB
{
    public Vector2 Point1;
    public Vector2 Point2;

    public bool IsPolar;

    public static Vector2 ToPolar(Vector2 e)
    {
        Vector2 ret = new Vector2();
        ret.x = Mathf.Sqrt(e.x * e.x + e.y * e.y);
        ret.y = Mathf.Atan2(e.y, e.x);

        return ret;
    }

    public static Vector2 ToEuclidean(Vector2 p)
    {
        Vector2 ret = new Vector2();
        ret.x = Mathf.Cos(p.y) * p.x;
        ret.y = Mathf.Sin(p.y) * p.x;

        return ret;
    }
}