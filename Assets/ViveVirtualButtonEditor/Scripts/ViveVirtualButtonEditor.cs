#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ViveVirtualButtonEditor : EditorWindow {

    private static float TouchpadCenX = 372f / 742f;
    private static float TouchpadCenY = 401f / 894f;
    private static float Aspect = 742f / 894f;
    private static float PadDimX = 438f / 742f;
    private static float PadDimY = 438f / 894f;
    private static float PolarAngleVertLimit = 15;

    [MenuItem("Window/Vive Virtual Button Editor")]
    static void CreateWindow()
    {
        var window = EditorWindow.GetWindow<ViveVirtualButtonEditor>();
        window.Init();
        window.Show();
    }

    private SerializedProperty p_buttons;

    private List<bool> ShouldDrawPreview;

    public void Init()
    {
        Undo.undoRedoPerformed += () => Repaint();

        minSize = new Vector2(375, 0);
        titleContent = new GUIContent("Vive Touchpad");

        var o_profile = new SerializedObject(ViveVirtualButtonProfile.Instance);
        p_buttons = o_profile.FindProperty("Buttons");

        ShouldDrawPreview = new List<bool>();
        for (int x = 0; x < ViveVirtualButtonProfile.Instance.Buttons.Count; x++)
            ShouldDrawPreview.Add(true);
    }

    private Vector2 scrollPosition = Vector2.zero;

    void OnGUI()
    {
        var skin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/ViveVirtualButtonEditor/Art/Window.guiskin");
        var s_texture = skin.customStyles[0];

        GUIStyle bold_wrap = new GUIStyle(EditorStyles.boldLabel);
        bold_wrap.wordWrap = true;

        var profile = ViveVirtualButtonProfile.Instance;

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.MaxWidth(position.width));

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("HTC Vive Touchpad Button Designer", bold_wrap);
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical();
        GUIStyle mini = new GUIStyle(EditorStyles.miniLabel);
        mini.padding = new RectOffset(0,0,0,0);
        GUILayout.Label("Adrian Biagioli 2017", mini);

        mini.normal.textColor = new Color(51f/255f,102f/255f,187f/255f);
        GUILayout.Label("flafla2.github.io", mini);
        mini.normal.textColor = Color.black;

        Rect linkRect = GUILayoutUtility.GetLastRect();
        EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);
        if(linkRect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.MouseDown)
                Application.OpenURL("http://flafla2.github.io");
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        if(GUILayout.Button("Save Configuration"))
        {
            profile.Save();
            Debug.Log("Saved Vive Virtual Button Configuration");
        }

        //if(GUILayout.Button("Force Reload Configuration"))
        //{
        //    profile.ForceReload();
        //}

        GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width));

        var img = AssetDatabase.LoadAssetAtPath<Texture>("Assets/ViveVirtualButtonEditor/Art/circlepad-graphic.png");

        GUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.MaxHeight(position.height));
        GUILayout.FlexibleSpace();
        Rect GraphicRect = GUILayoutUtility.GetAspectRect(Aspect);
        GUI.DrawTexture(GraphicRect, img);
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        scrollPosition = EditorGUILayout.BeginScrollView(
            scrollPosition,
            GUILayout.MinWidth(225), GUILayout.MaxWidth(Mathf.Max(225, position.width - 300)),
            GUILayout.MaxHeight(position.height),
            GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        GUIStyle checkbox = new GUIStyle(EditorStyles.toggle);
        checkbox.stretchHeight = true;
        checkbox.alignment = TextAnchor.MiddleCenter;
        
        for (int x = 0; x < profile.Buttons.Count; x++)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            var cur = profile.Buttons[x];
            if (cur.ButtonName.Trim().Equals(""))
                GUILayout.Label("Button " + (x + 1), bold_wrap);
            else
                GUILayout.Label(cur.ButtonName, bold_wrap);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("-", GUILayout.MaxWidth(25)))
            {
                Undo.RecordObject(profile, "Remove Button");
                profile.Buttons.RemoveAt(x);
                ShouldDrawPreview.RemoveAt(x);
                x--;
                continue;
            }
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.TextField("Button Name", cur.ButtonName);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(profile, "Change Button Name");
                cur.ButtonName = newName;
            }

            ShouldDrawPreview[x] = EditorGUILayout.Toggle("Draw Preview Graphic", ShouldDrawPreview[x]);

            EditorGUI.BeginChangeCheck();
            bool newIsPolar = EditorGUILayout.Toggle("Is Polar Coordinate", cur.Region.IsPolar);
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(profile, "Change Button Is Polar");
                cur.Region.IsPolar = newIsPolar;
            }

            EditorGUI.BeginChangeCheck();
            Vector2 p1 = EditorGUILayout.Vector2Field("Point 1", cur.Region.Point1);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(profile, "Change Button Region P1");
                cur.Region.Point1 = p1;
                
                if(cur.Region.IsPolar)
                {
                    cur.Region.Point1.x = Mathf.Clamp(cur.Region.Point1.x, 0, 1);
                    cur.Region.Point1.y = Mathf.Clamp(cur.Region.Point1.y, -360, 360);
                }
                else
                {
                    cur.Region.Point1.x = Mathf.Clamp(cur.Region.Point1.x, -1, 1);
                    cur.Region.Point1.y = Mathf.Clamp(cur.Region.Point1.y, -1, 1);
                }
            }

            EditorGUI.BeginChangeCheck();
            Vector2 p2 = EditorGUILayout.Vector2Field("Point 2", cur.Region.Point2);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(profile, "Change Button Region P2");
                cur.Region.Point2 = p2;

                if (cur.Region.IsPolar)
                {
                    cur.Region.Point2.x = Mathf.Clamp(cur.Region.Point2.x, 0, 1);
                    cur.Region.Point2.y = Mathf.Clamp(cur.Region.Point2.y, -360, 360);
                }
                else
                {
                    cur.Region.Point2.x = Mathf.Clamp(cur.Region.Point2.x, -1, 1);
                    cur.Region.Point2.y = Mathf.Clamp(cur.Region.Point2.y, -1, 1);
                }
            }
        }

        if(GUILayout.Button("Add Button"))
        {
            Undo.RecordObject(profile, "Add Button");
            var nxt = new VirtualButton();
            nxt.ButtonName = "";
            nxt.Region = new PadAABB();
            profile.Buttons.Add(nxt);

            ShouldDrawPreview.Add(true);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        Vector2 center = new Vector2(TouchpadCenX * GraphicRect.width, TouchpadCenY * GraphicRect.height) + GraphicRect.position;
        Vector2 dim = new Vector2(PadDimX * GraphicRect.width, PadDimY * GraphicRect.height);

        for(int i=0; i<profile.Buttons.Count;i++)
        {
            if (!ShouldDrawPreview[i])
                continue;

            var btn = profile.Buttons[i];

            float hue = ((0.1f * i) % 1.0f) * 0.85f;
            Color clr = Color.HSVToRGB(hue, 0.75f, 1);
            clr.a = 0.75f;
            float hue_bdr = ((0.1f * (i+1)) % 1.0f) * 0.85f;
            Color bdr = Color.HSVToRGB(hue, 0.9f, 1);

            if (btn.Region.IsPolar)
            {
                float min_r = Mathf.Min(btn.Region.Point1.x, btn.Region.Point2.x);
                float max_r = Mathf.Max(btn.Region.Point1.x, btn.Region.Point2.x);
                float min_t = Mathf.Min(btn.Region.Point1.y, btn.Region.Point2.y);
                float max_t = Mathf.Max(btn.Region.Point1.y, btn.Region.Point2.y);

                int PolarVertCount = (int)Mathf.Ceil((max_t - min_t) / PolarAngleVertLimit) + 1;
                Vector2[] upperset = new Vector2[PolarVertCount];
                Vector2[] lowerset = new Vector2[PolarVertCount];

                int x = 0;
                for (float l=min_t;l<max_t;l+=PolarAngleVertLimit)
                {
                    lowerset[x] = PadAABB.ToEuclidean(new Vector2(min_r, l));
                    upperset[x] = PadAABB.ToEuclidean(new Vector2(max_r, l));

                    x++;
                }

                Debug.Assert(x == PolarVertCount || x == PolarVertCount - 1);
                lowerset[PolarVertCount - 1] = PadAABB.ToEuclidean(new Vector2(min_r, max_t));
                upperset[PolarVertCount - 1] = PadAABB.ToEuclidean(new Vector2(max_r, max_t));

                for(x = 0; x < PolarVertCount; x++)
                {
                    lowerset[x].x = lowerset[x].x * dim.x / 2 + center.x;
                    lowerset[x].y = lowerset[x].y * dim.y / 2 + center.y;
                    upperset[x].x = upperset[x].x * dim.x / 2 + center.x;
                    upperset[x].y = upperset[x].y * dim.y / 2 + center.y;
                }

                Vector2[] strip = new Vector2[PolarVertCount * 2];
                for(x=0;x<PolarVertCount;x++)
                {
                    strip[2 * x] = lowerset[x];
                    strip[2 * x + 1] = upperset[x];
                }

                EditorGL.DrawTriangleStrip(strip, clr);
                EditorGL.DrawPolyLine(upperset, bdr);
                EditorGL.DrawPolyLine(lowerset, bdr);
                EditorGL.DrawLine(upperset[0], lowerset[0], bdr, 1);
                EditorGL.DrawLine(upperset[PolarVertCount-1], lowerset[PolarVertCount - 1], bdr, 1);
            } else
            {
                float top = Mathf.Min(btn.Region.Point1.y, btn.Region.Point2.y) * dim.y / 2 + center.y;
                float left = Mathf.Min(btn.Region.Point1.x, btn.Region.Point2.x) * dim.x / 2 + center.x;
                float bot = Mathf.Max(btn.Region.Point1.y, btn.Region.Point2.y) * dim.y / 2 + center.y;
                float right = Mathf.Max(btn.Region.Point1.x, btn.Region.Point2.x) * dim.x / 2 + center.x;

                Vector2 TL = new Vector2(left, top);
                Vector2 TR = new Vector2(right, top);
                Vector2 BL = new Vector2(left, bot);
                Vector2 BR = new Vector2(right, bot);

                EditorGL.DrawTriangle(TL, TR, BL, clr);
                EditorGL.DrawTriangle(TR, BR, BL, clr);
                EditorGL.DrawPolyLine(new Vector2[] { TL, TR, BR, BL, TL }, bdr);
            }
        }
    }
}
#endif