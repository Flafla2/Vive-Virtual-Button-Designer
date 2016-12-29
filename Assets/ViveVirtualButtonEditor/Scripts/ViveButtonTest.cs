using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveButtonTest : MonoBehaviour {

    public SteamVR_TrackedObject[] Controllers;
	
	// Update is called once per frame
	void Update () {
		foreach(var a in ViveVirtualButtonProfile.Instance.Buttons)
        {
            foreach(var c in Controllers)
            {
                if (!c.isValid)
                    continue;
                if (ViveVirtualButtonManager.Instance.GetButtonHover(a.ButtonName, c))
                    Debug.Log("Hovering " + a.ButtonName);
            }
        }
	}
}
