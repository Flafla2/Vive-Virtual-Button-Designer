using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveVirtualButtonManager : MonoBehaviour {

    private Dictionary<string, PadAABB> HashedButtons;

    void Awake ()
    {
        // Note, this call loads the profile from JSON if it hasn't been loaded already
        var profile = ViveVirtualButtonProfile.Instance;

        HashedButtons = new Dictionary<string, PadAABB>();

        foreach (var btn in profile.Buttons)
            HashedButtons.Add(btn.ButtonName, btn.Region);
    }

	public bool GetButtonHover(string buttonName, SteamVR_TrackedObject cnt)
    {
        var device = SteamVR_Controller.Input((int)cnt.index);

        Vector2 touchpad = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);

        if (!HashedButtons.ContainsKey(buttonName))
            return false;

        var aabb = HashedButtons[buttonName];
        float top = Mathf.Min(aabb.Point1.y, aabb.Point2.y);
        float left = Mathf.Min(aabb.Point1.x, aabb.Point2.x);
        float bot = Mathf.Max(aabb.Point1.y, aabb.Point2.y);
        float right = Mathf.Max(aabb.Point1.x, aabb.Point2.x);

        touchpad.y *= -1;

        if (aabb.IsPolar)
            touchpad = PadAABB.ToPolar(touchpad);

        return touchpad.x > left && touchpad.x < right && touchpad.y > top && touchpad.y < bot;
    }
}