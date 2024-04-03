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
    [SerializeField] private Toggle checkBox;
    private string dir;
    private string soundscapeName;

    public static event EventHandler OnSoundscapesChanged;

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            UpdateActiveDirectories();
            OnSoundscapesChanged?.Invoke(this, EventArgs.Empty);
        });
        checkBox.onValueChanged.AddListener( delegate
        {
            UpdateActiveDirectories();
            OnSoundscapesChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    public void SetText(string text)
    {
        soundscapeName = text;
        textMeshPro.text = soundscapeName;
    }

    public void SetDir(string inputDir)
    {
        dir = inputDir;
    }

    private string GetDir()
    {
        return dir;
    }

    private void UpdateActiveDirectories()
    {
        string path = Application.persistentDataPath + "/soundscapes/activeSoundscapes.ini";

        StreamReader reader = new StreamReader(path);
        StreamWriter writer = new StreamWriter(path);

        List<string> linesList = new List<string>();
        while (reader.ReadLine() != null)
        {
            linesList.Add(reader.ReadLine());
        }

        reader.Close();

        if (linesList.Contains(soundscapeName))
        {
            linesList.Remove(soundscapeName);
            writer.Close();

            StreamWriter writer2 = new StreamWriter(path, false);

            foreach (string line in linesList)
            {
                writer2.WriteLine(line);
            }
            writer2.Close();
            checkBox.isOn = true;
        }
        else
        {
            writer.WriteLine(soundscapeName);
            writer.Close();
            checkBox.isOn = false;
        }
    }

    private void InitialSetUp()
    {
        string path = Application.persistentDataPath + "/soundscapes/activeSoundscapes.ini";
        checkBox.isOn = false;

        StreamReader reader = new StreamReader(path);

        while(reader.ReadLine() != null)
        {
            if (reader.ReadLine() == soundscapeName)
            {
                checkBox.isOn = true;
            }
        }
        reader.Close();
    }
}
