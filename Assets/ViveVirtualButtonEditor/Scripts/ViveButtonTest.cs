using System;
using Valve.VR;
using UnityEngine;
using UnityEngine.UI;

public class ViveButtonTest : MonoBehaviour {

    public SteamVR_TrackedObject[] Controllers;

    public Text DebugText;
	
	void Update () {
        string output = "";

        int x = 1;
        foreach (var c in Controllers)
        {
            output += "Controller " + x + ": ";
            foreach (var a in ViveVirtualButtonProfile.Instance.Buttons)
            {
                if (!c.isValid)
                    continue;
                if (ViveVirtualButtonManager.Instance.GetButtonHover(a.ButtonName, c))
                    output += "Hovering " + a.ButtonName + " ";
            }
            output += "\n\n";
            x++;
        }

        DebugText.text = output;

        
	}
}
