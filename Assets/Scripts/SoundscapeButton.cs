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
    [SerializeField] private Button deleteButton;
    [SerializeField] private Transform deletePanel;
    [SerializeField] private Button confirmDelete;
    [SerializeField] private TMP_InputField deletionInputField;

    public static event EventHandler OnSoundscapesChanged;

    private void Awake()
    {
        deleteButton.gameObject.SetActive(false);
        deletePanel.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        button.onClick.AddListener(() =>
        {
            UpdateActiveDirectories();
            OnSoundscapesChanged?.Invoke(this, EventArgs.Empty);
            DisableSoundscapeButtons();
        });
        deleteButton.onClick.AddListener(() =>
        {
            deletePanel.gameObject.SetActive(true);
        });
        confirmDelete.onClick.AddListener(() =>
        {
            if (deletionInputField.text == soundscapeName)
            {
                RemoveSoundscapeFromActive();
                OnSoundscapesChanged?.Invoke(this, EventArgs.Empty);
                Directory.Delete(dir, true);
                SoundscapeList.Instance.SetUpDir();
                SoundscapeList.Instance.SetUpButtons();
            }
            deletePanel.gameObject.SetActive(false);
        });
    }

    private void Start()
    {
        ActiveSoundscapes.Instance.ToggleDeleteMode += ActiveSoundscapes_ToggleDeleteMode;
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

    private void RemoveSoundscapeFromActive()
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
        }

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

    private void ActiveSoundscapes_ToggleDeleteMode(object sender, bool deleteMode)
    {
        if (gameObject != null)
        {
            deleteButton.gameObject.SetActive(deleteMode);
        }
    }

    private void OnDestroy()
    {
        ActiveSoundscapes.Instance.ToggleDeleteMode -= ActiveSoundscapes_ToggleDeleteMode;
    }
}
