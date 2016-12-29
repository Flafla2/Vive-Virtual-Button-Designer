using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ViveVirtualButtonProfile : ScriptableObject {

    // Instance / Singleton Management code //

    public static ViveVirtualButtonProfile Instance
    {
        get
        {
            if(_Instance != null)
                return _Instance;

            var resource = (ViveVirtualButtonProfile)Resources.Load("ViveVirtualButtonProfile");
            Debug.Log(resource.Buttons.Count);
            if (resource == null)
                resource = CreateInstance<ViveVirtualButtonProfile>();
            _Instance = resource;
            return resource;
        }
    }
    private static ViveVirtualButtonProfile _Instance;

    void Awake()
    {
        if (_Instance == null)
            _Instance = this;
        else
        {
            Debug.LogError("Only one instance of ViveVirtualButtonProfile may exist.  Destroying duplicate.");
            Destroy(this);
            return;
        }

        if(Buttons == null)
            Buttons = new List<VirtualButton>();
    }

   public void ForceReload()
    {
        var resource = (ViveVirtualButtonProfile)Resources.Load("ViveVirtualButtonProfile");
        Debug.Log(resource.Buttons.Count);
        if (resource == null)
            resource = CreateInstance<ViveVirtualButtonProfile>();
        _Instance = resource;
    }

    void OnDestroy()
    {
        if (_Instance == this)
            _Instance = null;
    }

    // IO Code //

#if UNITY_EDITOR
    public void Save()
    {
        if(Application.isPlaying)
        {
            Debug.LogError("Can't save Vive Button Profile while playing!  Save in edit mode.");
            return;
        }

        string asset = "/ViveVirtualButtonEditor/Resources/ViveVirtualButtonProfile.asset";
        if(!File.Exists(Application.dataPath + asset))
            AssetDatabase.CreateAsset(this, "Assets/"+asset);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif

    // Data / Button structure code //

    public List<VirtualButton> Buttons;

}