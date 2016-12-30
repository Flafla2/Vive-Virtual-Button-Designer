using Valve.VR;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class ControllerButtonPreviewer : MonoBehaviour {

    [SerializeField]
    private ButtonVisualData[] Buttons;
    [SerializeField]
    private GameObject ButtonPrefab;
    [SerializeField]
    private float ButtonOffset = 0.005f;

    private SteamVR_TrackedObject _TrackedController;
    private Dictionary<string, ButtonObjectData> ButtonDataSet;

    private SteamVR_RenderModel model;

    private int h_hover;
    private int h_press;

    private bool Initialized = false;

    void Awake() {
        ButtonDataSet = new Dictionary<string, ButtonObjectData>();
        _TrackedController = GetComponent<SteamVR_TrackedObject>();

        h_hover = Animator.StringToHash("Hover");
        h_press = Animator.StringToHash("Press");

        model = GetComponentInChildren<SteamVR_RenderModel>();
    }

    private bool Initialize()
    {
        var trackpad = model.FindComponent("trackpad");
        if (trackpad == null)
            return false;

        GameObject parent = new GameObject();
        parent.transform.SetParent(trackpad.GetChild(0), false);
        parent.transform.localRotation = Quaternion.Euler(0, 0, 90);
        parent.transform.localPosition = Vector3.forward * ButtonOffset;

        foreach (var b in Buttons)
        {
            GameObject spawned = Instantiate<GameObject>(ButtonPrefab, Vector3.zero, Quaternion.identity);
            spawned.transform.SetParent(parent.transform, false);
            Animator anim = spawned.GetComponent<Animator>();
            if (anim == null)
            {
                Debug.LogError("Button Prefab MUST contain an animator!  Destroying ControllerButtonPreviewer.");
                Destroy(this);
                return false;
            }

            MeshFilter filter = spawned.GetComponent<MeshFilter>();
            if (filter == null)
                filter = spawned.AddComponent<MeshFilter>();
            MeshRenderer renderer = spawned.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = spawned.AddComponent<MeshRenderer>();
            renderer.material = b.ButtonMaterial;

            ButtonObjectData data;
            data.anim = anim;
            data.obj = spawned;
            data.filter = filter;
            data.renderer = renderer;
            ButtonDataSet.Add(b.ButtonName, data);

            ButtonPreviewRenderer prev = spawned.GetComponent<ButtonPreviewRenderer>();
            if (prev == null)
                prev = spawned.AddComponent<ButtonPreviewRenderer>();
            prev.ButtonName = b.ButtonName;
        }

        Initialized = true;
        return true;
    }

    private void Deinitialize()
    {
        foreach (var b in ButtonDataSet.Values)
            if(b.obj != null)
                Destroy(b.obj);
        ButtonDataSet.Clear();

        Initialized = false;
    }

    void Update()
    {
        bool shouldBeInitialized = _TrackedController.isValid;
        if(Initialized != shouldBeInitialized)
        {
            if (Initialized)
                Deinitialize();
            else
                Initialize();
        }

        if (!Initialized)
            return;
        var ipt = SteamVR_Controller.Input((int)_TrackedController.index);
        bool touch = ipt.GetTouch(EVRButtonId.k_EButton_SteamVR_Touchpad);
        bool press = ipt.GetPress(EVRButtonId.k_EButton_SteamVR_Touchpad);
        foreach (var b in Buttons)
        {
            var data = ButtonDataSet[b.ButtonName];

            if (touch && ViveVirtualButtonManager.Instance.GetButtonHover(b.ButtonName, _TrackedController))
            {
                data.anim.SetBool(h_hover, true);
                data.anim.SetBool(h_press, press);
            } else
            {
                data.anim.SetBool(h_hover, false);
                data.anim.SetBool(h_press, false);
            }
        }
    }

    public GameObject GetObjectForButton(string button)
    {
        if (!ButtonDataSet.ContainsKey(button))
            return null;
        return ButtonDataSet[button].obj;
    }

    public MeshRenderer GetRendererForButton(string button)
    {
        if (!ButtonDataSet.ContainsKey(button))
            return null;
        return ButtonDataSet[button].renderer;
    }

    private struct ButtonObjectData
    {
        public GameObject obj;
        public Animator anim;
        public MeshFilter filter;
        public MeshRenderer renderer;
    }
}

[Serializable]
public class ButtonVisualData
{
    public string ButtonName;
    public Material ButtonMaterial;
}