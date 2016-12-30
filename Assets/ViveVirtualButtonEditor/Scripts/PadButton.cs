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
        ret.y = Mathf.Atan2(e.y, e.x) * Mathf.Rad2Deg;

        return ret;
    }

    public static Vector2 ToEuclidean(Vector2 p)
    {
        Vector2 ret = new Vector2();
        ret.x = Mathf.Cos(p.y * Mathf.Deg2Rad) * p.x;
        ret.y = Mathf.Sin(p.y * Mathf.Deg2Rad) * p.x;

        return ret;
    }

    /// \brief Creates a 2D mesh based on this region.  This is useful for visualizing the buttons in-game.
    /// 
    /// \param verts A buffer for Vertices to reduce duplicate allocation.  If this is too small (or if null) the
    ///              array will be reallocated.  The actual output here can be interpreted as GL_TRIANGLE_STRIP.
    /// \param AnglePerVertex If IsPolar is true, this defines the angular step taken by each vertex.  If IsPolar is not
    ///                       true, this is ignored.
    public void MakeButtonMesh(ref Vector2[] verts, out int verts_len, float AnglePerVertex)
    {
        if(IsPolar)
        {
            float min_r = Mathf.Min(Point1.x, Point2.x);
            float max_r = Mathf.Max(Point1.x, Point2.x);
            float min_t = Mathf.Min(Point1.y, Point2.y);
            float max_t = Mathf.Max(Point1.y, Point2.y);

            int PolarVertCount = (int)Mathf.Ceil((max_t - min_t) / AnglePerVertex) + 1;
            if (verts == null || verts.Length < PolarVertCount * 2)
                verts = new Vector2[PolarVertCount * 2];
            verts_len = 2 * PolarVertCount;

            int x = 0;
            for (float l = min_t; l < max_t; l += AnglePerVertex)
            {
                verts[2*x] = PadAABB.ToEuclidean(new Vector2(min_r, l));
                verts[2*x+1] = PadAABB.ToEuclidean(new Vector2(max_r, l));

                x++;
            }

            Debug.Assert(x == PolarVertCount || x == PolarVertCount - 1);
            verts[2*(PolarVertCount - 1)]     = PadAABB.ToEuclidean(new Vector2(min_r, max_t));
            verts[2*(PolarVertCount - 1) + 1] = PadAABB.ToEuclidean(new Vector2(max_r, max_t));
        } else
        {
            float top   = Mathf.Min(Point1.y, Point2.y);
            float left  = Mathf.Min(Point1.x, Point2.x);
            float bot   = Mathf.Max(Point1.y, Point2.y);
            float right = Mathf.Max(Point1.x, Point2.x);

            verts_len = 4;
            if (verts == null || verts.Length < 4)
                verts = new Vector2[4];

            verts[0] = new Vector2(left, top);  // TL
            verts[1] = new Vector2(right, top); // TR
            verts[2] = new Vector2(left, bot);  // BL
            verts[3] = new Vector2(right, bot); // BR
        }
    }
}