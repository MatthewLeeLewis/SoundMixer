using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SoundscapeList : MonoBehaviour

{
    [SerializeField] private Transform content;
    [SerializeField] private Transform buttonPrefab;

    private void Awake()
    {
        string[] dir = Directory.GetDirectories(Application.persistentDataPath + "/soundscapes");

        foreach (Transform button in content)
        {
            Destroy(button.gameObject);
        }
        foreach (string directory in dir)
        {
            Instantiate(buttonPrefab, content.transform);
        }
    }
}
