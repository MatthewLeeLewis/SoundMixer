using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MusicList : MonoBehaviour

{
    public static MusicList Instance { get; private set; }
    [SerializeField] private Transform contentPanel;
    [SerializeField] private Transform buttonPrefab;
    public string activeMusic = "";

    string[] dir; // Instantiates a string array for directories.

    private void Awake()
    {
        if (Instance != null) // This if check ensures that multiple instances of this object do not exist and reports it if they do, and destroys the duplicate.
        {
            Debug.LogError("There's more than one MusicList! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this; // This instantiates the instance.

        dir = Directory.GetDirectories(Application.persistentDataPath + "/music"); // Sets dir to the directories in the AppData path.
        SetUpButtons();
    }

    private void Start()
    {
        MusicPlayer.OnMusicChanged += MusicPlayer_OnMusicChanged;
        MusicButton.OnMusicChanged += MusicButton_OnMusicChanged;
    }

    public string[] GetDir()
    {
        return dir;
    }

    private void SetUpButtons()
    {
        foreach (Transform button in contentPanel)
        {
            Destroy(button.gameObject);
        }
        foreach (string directory in dir)
        {
            Transform newButtonTransform = Instantiate(buttonPrefab, contentPanel.transform);
            MusicButton newButton = newButtonTransform.GetComponent<MusicButton>();

            DirectoryInfo di = new DirectoryInfo(directory);
            string dirName = di.Name;

            newButton.SetName(dirName);
            newButton.SetDir(directory);
        }
    }

    public void SetActiveMusic(string input)
    {
        activeMusic = input;
        WriteSettings();
    }

    private void MusicButton_OnMusicChanged(object sender, EventArgs e)
    {
        foreach (Transform button in contentPanel)
        {
            MusicButton currentButton = button.GetComponent<MusicButton>();
            if (currentButton.GetDir() == activeMusic)
            {
                currentButton.SetToggle(true);
            }
            else
            {
                currentButton.SetToggle(false);
            }
        }
    }

    private void MusicPlayer_OnMusicChanged(object sender, EventArgs e)
    {
        foreach (Transform button in contentPanel)
        {
            MusicButton currentButton = button.GetComponent<MusicButton>();
            if (currentButton.GetDir() == activeMusic)
            {
                currentButton.SetToggle(true);
            }
            else
            {
                currentButton.SetToggle(false);
            }
        }
    }

    private void WriteSettings()
    {
        string path = Application.persistentDataPath + "/music/activeMusic.ini";

        StreamReader reader = new StreamReader(path);

        List<string> linesList = new List<string>();
        while (reader.Peek() >= 0)
        {
            linesList.Add(reader.ReadLine());
        }

        reader.Close();

        for (int i = 0; i < linesList.Count; i++)
        {
            if (linesList[i].StartsWith("Active = "))
            {
                linesList[i] = "Active = " + activeMusic;
            }
        }

        StreamWriter writer = new StreamWriter(path, false);
        foreach (string line in linesList)
        {
            writer.WriteLine(line);
        }
        writer.Close();
    }
}
