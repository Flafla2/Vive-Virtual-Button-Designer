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
    private static int PolarVertCount = 8;

    [MenuItem("Window/Vive Virtual Button Editor")]
    static void CreateWindow()
    {
        var window = EditorWindow.GetWindow<ViveVirtualButtonEditor>();
        window.Init();
        window.Show();
    }

    private SerializedProperty p_buttons;

    public void Init()
    {
        Undo.undoRedoPerformed += () => Repaint();

        minSize = new Vector2(375, 0);

        var o_profile = new SerializedObject(ViveVirtualButtonProfile.Instance);
        p_buttons = o_profile.FindProperty("Buttons");
    }

    private Vector2 scrollPosition = Vector2.zero;

    void OnGUI()
    {
        var skin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/ViveVirtualButtonEditor/Art/Window.guiskin");
        var s_texture = skin.customStyles[0];

        var profile = ViveVirtualButtonProfile.Instance;

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.MaxWidth(position.width));

        if(GUILayout.Button("Save Configuration"))
        {
            profile.Save();
            Debug.Log("Saved Vive Virtual Button Configuration");
        }

        if(GUILayout.Button("Force Reload Configuration"))
        {
            profile.ForceReload();
        }

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

        
        for (int x = 0; x < profile.Buttons.Count; x++)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            var cur = profile.Buttons[x];
            if (cur.ButtonName.Trim().Equals(""))
                GUILayout.Label("Button " + (x + 1));
            else
                GUILayout.Label(cur.ButtonName);

            if(GUILayout.Button("-", GUILayout.MaxWidth(25)))
            {
                Undo.RecordObject(profile, "Remove Button");
                profile.Buttons.RemoveAt(x);
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
            }

            EditorGUI.BeginChangeCheck();
            Vector2 p2 = EditorGUILayout.Vector2Field("Point 2", cur.Region.Point2);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(profile, "Change Button Region P2");
                cur.Region.Point2 = p2;
            }
        }

        if(GUILayout.Button("Add Button"))
        {
            Undo.RecordObject(profile, "Add Button");
            var nxt = new VirtualButton();
            nxt.ButtonName = "";
            nxt.Region = new PadAABB();
            profile.Buttons.Add(nxt);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        Vector2 center = new Vector2(TouchpadCenX * GraphicRect.width, TouchpadCenY * GraphicRect.height) + GraphicRect.position;
        Vector2 dim = new Vector2(PadDimX * GraphicRect.width, PadDimY * GraphicRect.height);

        foreach(var btn in profile.Buttons)
        {
            if(btn.Region.IsPolar)
            {
                Vector2[] upperset = new Vector2[PolarVertCount];
                Vector2[] lowerset = new Vector2[PolarVertCount];

                float min_r = Mathf.Min(btn.Region.Point1.x, btn.Region.Point2.x);
                float max_r = Mathf.Max(btn.Region.Point1.x, btn.Region.Point2.x);
                float min_t = Mathf.Min(btn.Region.Point1.y, btn.Region.Point2.y);
                float max_t = Mathf.Max(btn.Region.Point1.y, btn.Region.Point2.y);

                for (int x=0;x< PolarVertCount;x++)
                {
                    float a = (float)x / (float)(PolarVertCount-1);

                    float angle = Mathf.Lerp(min_t, max_t, a);

                    lowerset[x] = PadAABB.ToEuclidean(new Vector2(min_r, angle));
                    upperset[x] = PadAABB.ToEuclidean(new Vector2(max_r, angle));

                    lowerset[x].x = lowerset[x].x * dim.x / 2 + center.x;
                    lowerset[x].y = lowerset[x].y * dim.y / 2 + center.y;
                    upperset[x].x = upperset[x].x * dim.x / 2 + center.x;
                    upperset[x].y = upperset[x].y * dim.y / 2 + center.y;
                }

                Vector2[] strip = new Vector2[PolarVertCount * 2];
                for(int x=0;x<PolarVertCount;x++)
                {
                    strip[2 * x] = lowerset[x];
                    strip[2 * x + 1] = upperset[x];
                }

                EditorGL.DrawTriangleStrip(strip, Color.blue);
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

                EditorGL.DrawTriangle(TL, TR, BL, Color.red);
                EditorGL.DrawTriangle(TR, BR, BL, Color.red);
            }
        }
    }
}
#endif