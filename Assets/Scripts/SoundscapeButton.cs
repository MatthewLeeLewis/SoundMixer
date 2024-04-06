using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class SoundscapeButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Toggle toggle;
    private string dir;
    private string soundscapeName;
    
    public static event EventHandler OnSoundscapesChanged;

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            UpdateActiveDirectories();
            OnSoundscapesChanged?.Invoke(this, EventArgs.Empty);
            DisableSoundscapeButtons();
        });
    }

    public void SetName(string text)
    {
        soundscapeName = text;
        textMeshPro.text = soundscapeName;
    }

    public void SetDir(string inputDir)
    {
        dir = inputDir;
    }

    public string GetDir()
    {
        return dir;
    }

    public string GetName()
    {
        return soundscapeName;
    }

    private void UpdateActiveDirectories()
    {
        string path = Application.persistentDataPath + "/soundscapes/activeSoundscapes.ini";

        StreamReader reader = new StreamReader(path);

        List<string> linesList = new List<string>();
        while (reader.Peek() >= 0)
        {
            linesList.Add(reader.ReadLine());
        }

        reader.Close();

        if (linesList.Contains(soundscapeName))
        {
            linesList.Remove(soundscapeName);

            StreamWriter writer = new StreamWriter(path, false);

            if (linesList.Count != 0)
            {
                foreach (string line in linesList)
                {
                    writer.WriteLine(line);
                }
                writer.Close();
            }
            else
            {
                writer.Write("");
                writer.Close();
            }

            ActiveSoundscapes.Instance.RemoveSoundscape(soundscapeName);
        }
        else
        {
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(soundscapeName);
            writer.Close();
        }  
    }
    
    public void SetToggle(bool input)
    {
        toggle.isOn = input;
    }

    private void DisableSoundscapeButtons()
    {
        button.interactable = false;
        Invoke("EnableSoundscapeButtons", 3);
    }

    private void EnableSoundscapeButtons()
    {
        button.interactable = true;
    }
}
