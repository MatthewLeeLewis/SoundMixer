using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ActiveSoundscapes : MonoBehaviour
{
    public static ActiveSoundscapes Instance { get; private set; }



    [SerializeField] private Transform contentPanel;
    [SerializeField] private Transform soundscapePrefab;

    private List<string> activeSoundscapes; 

    private void Awake()
    {
        if (Instance != null) // This if check ensures that multiple instances of this object do not exist and reports it if they do, and destroys the duplicate.
        {
            Debug.LogError("There's more than one ActiveSoundscapes menu! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this; // This instantiates the instance.

        activeSoundscapes = new List<string>();
        ReadSoundscapes();
        SetUpSoundscapes();
        SoundscapeList.Instance.SetToggles(activeSoundscapes);
    }

    private void Start()
    {
        SoundscapeButton.OnSoundscapesChanged += SoundscapeButton_OnSoundscapesChanged;
    }

    private void SetUpSoundscapes()
    {
        foreach (Transform soundscape in contentPanel)
        {
            Destroy(soundscape.gameObject);
        }
        foreach (string name in activeSoundscapes)
        {
            Transform newSoundscapeTransform = Instantiate(soundscapePrefab, contentPanel.transform);
            Soundscape newSoundscape = newSoundscapeTransform.GetComponent<Soundscape>();

            newSoundscape.SetName(name);
            newSoundscape.SetDir(Application.persistentDataPath + "/soundscapes/" + name);
        }
    }

    public void RemoveSoundscape(string input)
    {
        activeSoundscapes.Remove(input);
        ReadSoundscapes();
    }

    private void ReadSoundscapes()
    {
        string path = Application.persistentDataPath + "/soundscapes/activeSoundscapes.ini";
        StreamReader reader = new StreamReader(path);
        
        while (reader.Peek() >= 0)
        {
            string line = reader.ReadLine();
            if (!activeSoundscapes.Contains(line))
            {
                activeSoundscapes.Add(line);
            }
        }
        reader.Close();
    }

    private void SoundscapeButton_OnSoundscapesChanged(object sender, EventArgs e)
    {
        ReadSoundscapes();
        SetUpSoundscapes();
        SoundscapeList.Instance.SetToggles(activeSoundscapes);
    }
}
