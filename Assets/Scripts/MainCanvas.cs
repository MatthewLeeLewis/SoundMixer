using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MainCanvas : MonoBehaviour
{
    private void Awake()
    {
        // If a directory does not exist for the soundscapes, create one, alongside a base sample soundscape.
        if (!Directory.Exists(Application.persistentDataPath + "/soundscapes"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/soundscapes/Sample");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/music"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/music/Sample");
        }
    }
}
