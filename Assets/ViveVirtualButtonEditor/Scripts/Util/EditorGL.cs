using UnityEngine;

// Adapted From: http://wiki.unity3d.com/index.php/GLDraw
public class EditorGL
{
    /*
     * Clipping code: http://forum.unity3d.com/threads/17066-How-to-draw-a-GUI-2D-quot-line-quot?p=230386#post230386
     * Thick line drawing code: http://unifycommunity.com/wiki/index.php?title=VectorLine
     */
    protected static bool clippingEnabled;
    protected static Rect clippingBounds;
    public static Material lineMaterial = null;

    /* @ Credit: "http://cs-people.bu.edu/jalon/cs480/Oct11Lab/clip.c" */
    protected static bool clip_test(float p, float q, ref float u1, ref float u2)
    {
        float r;
        bool retval = true;

        if (p < 0.0)
        {
            r = q / p;
            if (r > u2)
                retval = false;
            else if (r > u1)
                u1 = r;
        }
        else if (p > 0.0)
        {
            r = q / p;
            if (r < u1)
                retval = false;
            else if (r < u2)
                u2 = r;
        }
        else if (q < 0.0)
            retval = false;

        return retval;
    }

    protected static bool segment_rect_intersection(Rect bounds, ref Vector2 p1, ref Vector2 p2)
    {
        float u1 = 0.0f, u2 = 1.0f, dx = p2.x - p1.x, dy;

        if (clip_test(-dx, p1.x - bounds.xMin, ref u1, ref u2))
        {
            if (clip_test(dx, bounds.xMax - p1.x, ref u1, ref u2))
            {
                dy = p2.y - p1.y;
                if (clip_test(-dy, p1.y - bounds.yMin, ref u1, ref u2))
                {
                    if (clip_test(dy, bounds.yMax - p1.y, ref u1, ref u2))
                    {
                        if (u2 < 1.0)
                        {
                            p2.x = p1.x + u2 * dx;
                            p2.y = p1.y + u2 * dy;
                        }

                        if (u1 > 0.0)
                        {
                            p1.x += u1 * dx;
                            p1.y += u1 * dy;
                        }
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public static void BeginGroup(Rect position)
    {
        clippingEnabled = true;
        clippingBounds = new Rect(0, 0, position.width, position.height);
        GUI.BeginGroup(position);
    }

    public static void EndGroup()
    {
        GUI.EndGroup();
        clippingBounds = new Rect(0, 0, Screen.width, Screen.height);
        clippingEnabled = false;
    }

    public static void CreateMaterial()
    {
        if (lineMaterial != null)
            return;

        var shader = Resources.Load<Shader>("EditorLineShader");
        if(shader == null)
            return;

        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    public static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
    {
        if (Event.current == null)
            return;
        if (Event.current.type != EventType.repaint)
            return;

        if (clippingEnabled)
            if (!segment_rect_intersection(clippingBounds, ref start, ref end))
                return;

        CreateMaterial();
        if (lineMaterial == null) return;

        lineMaterial.SetPass(0);

        Vector3 startPt;
        Vector3 endPt;

        if (width == 1)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);
            startPt = new Vector3(start.x, start.y, 0);
            endPt = new Vector3(end.x, end.y, 0);
            GL.Vertex(startPt);
            GL.Vertex(endPt);
        }
        else
        {
            GL.Begin(GL.QUADS);
            GL.Color(color);
            startPt = new Vector3(end.y, start.x, 0);
            endPt = new Vector3(start.y, end.x, 0);
            Vector3 perpendicular = (startPt - endPt).normalized * width;
            Vector3 v1 = new Vector3(start.x, start.y, 0);
            Vector3 v2 = new Vector3(end.x, end.y, 0);
            GL.Vertex(v1 - perpendicular);
            GL.Vertex(v1 + perpendicular);
            GL.Vertex(v2 + perpendicular);
            GL.Vertex(v2 - perpendicular);
        }
        GL.End();
    }

    public static void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color color)
    {
        if (Event.current == null)
            return;
        if (Event.current.type != EventType.repaint)
            return;

        CreateMaterial();
        if (lineMaterial == null) return;

        lineMaterial.SetPass(0);

        GL.Begin(GL.TRIANGLES);
        GL.Color(color);
        GL.Vertex(p1);
        GL.Vertex(p2);
        GL.Vertex(p3);
        GL.End();
    }

    public static void DrawTriangleStrip(Vector2[] verts, Color color, int len = -1)
    {
        if (Event.current == null)
            return;
        if (Event.current.type != EventType.repaint)
            return;

        if (len < 0)
            len = verts.Length;

            CreateMaterial();
        if (lineMaterial == null) return;

        lineMaterial.SetPass(0);

        GL.Begin(GL.TRIANGLE_STRIP);
        GL.Color(color);
        for (int x = 0; x < len; x++)
            GL.Vertex(verts[x]);
        GL.End();
    }

    public static void DrawPolyLine(Vector2[] verts, Color color)
    {
        if (Event.current == null)
            return;
        if (Event.current.type != EventType.repaint)
            return;

        CreateMaterial();
        if (lineMaterial == null) return;

        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);
        GL.Color(color);
        for (int x = 0; x < verts.Length - 1; x++)
        {
            GL.Vertex(verts[x]);
            GL.Vertex(verts[x + 1]);
        }
            
        GL.End();
    }
}