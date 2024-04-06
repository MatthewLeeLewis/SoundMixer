using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MusicList : MonoBehaviour

{
    public static MusicList Instance { get; private set; }

    [SerializeField] private Transform contentPanel;
    [SerializeField] private Transform buttonPrefab;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Slider volSlider;
    [SerializeField] private Button folderButton;
    [SerializeField] private Button refreshButton;
    public string activeMusic = "None";
    private List<string> instantiatedButtons = new List<string>();

    private TextMeshProUGUI pauseText;

    private List<string> dir = new List<string>(); // Instantiates a string array for directories.

    private void Awake()
    {
        if (Instance != null) // This if check ensures that multiple instances of this object do not exist and reports it if they do, and destroys the duplicate.
        {
            Debug.LogError("There's more than one MusicList! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this; // This instantiates the instance.

        SetUpDir(); // Sets dir to the directories in the AppData path.
        SetUpButtons();
    }

    private void Start()
    {
        MusicPlayer.OnMusicChanged += OnMusicChanged;
        MusicButton.OnMusicChanged += OnMusicChanged;
        SetUpMusicSettings();
    }

    private void SetUpDir()
    {
        dir.Clear();
        string[] dirArray = Directory.GetDirectories(Application.persistentDataPath + "/music");
        foreach (string directory in dirArray)
        {
            dir.Add(directory);
        }
    }

    private void SetUpButtons()
    {
        foreach (Transform button in contentPanel)
        {
            MusicButton musicButton = button.GetComponent<MusicButton>();
            
            if (!dir.Contains(musicButton.GetDir()))
            {
                instantiatedButtons.Remove(musicButton.GetName());
                Destroy(button.gameObject);
                Destroy(button);
            }
        }
        foreach (string directory in dir)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            string dirName = di.Name;

            if (!instantiatedButtons.Contains(di.Name))
            {
                Transform newButtonTransform = Instantiate(buttonPrefab, contentPanel.transform);
                MusicButton newButton = newButtonTransform.GetComponent<MusicButton>();

                newButton.SetName(dirName);
                newButton.SetDir(directory);
                instantiatedButtons.Add(di.Name);
            } 
        }
    }

    public void SetActiveMusic(string input)
    {
        activeMusic = input;
        WriteSettings();
    }

    private void OnMusicChanged(object sender, EventArgs e)
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
            if (linesList[i].StartsWith("BaseVolume = "))
            {
                linesList[i] = "BaseVolume = " + MusicPlayer.Instance.GetVolume().ToString();
            }
            else if (linesList[i].StartsWith("Active = "))
            {
                linesList[i] = "Active = " + activeMusic;
                Debug.Log("activeMusic is set to " + activeMusic);
            }
        }

        StreamWriter writer = new StreamWriter(path, false);
        foreach (string line in linesList)
        {
            writer.WriteLine(line);
        }
        writer.Close();
    }

    private void SetUpMusicSettings()
    {
        pauseText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();

        stopButton.onClick.AddListener(() =>
        {
            MusicPlayer.Instance.Stop();
            activeMusic = "None";
            OnMusicChanged(this, EventArgs.Empty);
            WriteSettings();
        });
        skipButton.onClick.AddListener(() =>
        {
            MusicPlayer.Instance.Skip();
        });
        pauseButton.onClick.AddListener(() =>
        {
            MusicPlayer.Instance.Pause();
        });
        volSlider.onValueChanged.AddListener(delegate {
            MusicPlayer.Instance.SetVolume(volSlider.value);
            WriteSettings();
        });
        folderButton.onClick.AddListener(() =>
        {
            string itemPath = (Application.persistentDataPath + "/music/activeMusic.ini");

            // Get rid of forward slashes to appease explorer.exe.
            itemPath = itemPath.Replace(@"/", @"\");   

            System.Diagnostics.Process.Start("explorer.exe", "/select,"+itemPath);
        });
        refreshButton.onClick.AddListener(() =>
        {
            SetUpDir();
            SetUpButtons();
            OnMusicChanged(this, EventArgs.Empty);
            Debug.Log("Refreshed!");
        });
    }

    public void SetVolSlider(float input)
    {
        volSlider.value = input;
    }

    public void SetPauseText(string input)
    {
        pauseText.text = input;
    }

    public void SetInteractable(bool input)
    {
        stopButton.interactable = input;
        pauseButton.interactable = input;
        skipButton.interactable = input;
    }
}
