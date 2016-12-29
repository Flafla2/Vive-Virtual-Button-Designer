using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveButtonTest : MonoBehaviour {

    public SteamVR_TrackedObject[] Controllers;
    public ViveVirtualButtonManager Manager;
	
	// Update is called once per frame
	void Update () {
		foreach(var a in ViveVirtualButtonProfile.Instance.Buttons)
        {
            foreach(var c in Controllers)
            {
                if (!c.isValid)
                    continue;
                if (Manager.GetButtonHover(a.ButtonName, c))
                    Debug.Log("Hovering " + a.ButtonName);
            }
        }
	}
}
