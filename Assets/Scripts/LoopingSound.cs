using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class LoopingSound : MonoBehaviour
{
    private AudioSource Source;
    private AudioClip track;
    private string playerName;
    private string dir;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Toggle soundToggle;
    private TMP_Dropdown dropdown;
    private Slider volSlider;
    List<string> Files = new List<string>();
    List<AudioClip> Tracks = new List<AudioClip>();
    private float baseVolume;

    private void Update()
    {
        if (soundToggle.isOn && !Source.isPlaying)
        {
            Source.volume = 0;
            PlayTrack();
        }
        else if (soundToggle.isOn && Source.volume == 0)
        {
            PlayTrack();
            StartCoroutine(FadeAudioSource.StartFade(Source, 3, baseVolume));
        }
    }
    private void Awake()
    {
        dropdown = GetComponentInChildren<TMP_Dropdown>();
        dropdown.ClearOptions();
        Source = GetComponent<AudioSource>();
        volSlider = GetComponentInChildren<Slider>();
    }
    private void SetUp()
    {
        //InitializeDropdown();
        //InitializeToggle();
        InitializeTracks();
        InitializeSettings();
        InitializeInputs();
    }

    public void SetName(string inputName)
    {
        playerName = inputName;
        textMeshPro.text = playerName;
    }

    public void SetDir(string input)
    {
        dir = input;
        SetUp();
    }

    public void PlayTrack()
    {
        if (Tracks.Count != 0)
        {
            track = Tracks[dropdown.value];
            Source.clip = track;
            Source.Play();
            //active = true;
        } 
        else 
        {
            Source.Stop();
            //active = false;
        }
    }
    private async void InitializeTracks()
    {
        DirectoryInfo di = new DirectoryInfo(dir);
        FileInfo[] files = di.GetFiles();

        List<string> filenames = new List<string>();

        foreach (FileInfo file in files)
        {
            string fileString = file.ToString();
            if (fileString.EndsWith(".wav"))
            {
                Files.Add(fileString);

                filenames.Add(file.Name);

                AudioClip newClip = await LoadClip(fileString);
                Tracks.Add(newClip);
            }
        }
        dropdown.AddOptions(filenames);
    }
    private void InitializeSettings()
    {
        string path = dir + "/settings.ini";

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
                string[] subStrings = linesList[i].Split(" = ");
                baseVolume = float.Parse(subStrings[1]);
                if (baseVolume < 0.01f){baseVolume = 0.01f;}
                else if (baseVolume > 1.0f){baseVolume = 1.0f;}
                volSlider.value = baseVolume;
                Debug.Log("Base Volume = " + baseVolume);
            }
            else if (linesList[i].StartsWith("CurrentSound = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                dropdown.value = dropdown.options.FindIndex(option => option.text == subStrings[1]);
            }
            else if (linesList[i].StartsWith("Active = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                soundToggle.isOn = Convert.ToBoolean(subStrings[1]);
            }
        }
    }

    private void InitializeInputs()
    {
        soundToggle.onValueChanged.AddListener(delegate {
            WriteSettings();
            if (!soundToggle.isOn)
            {
                StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
            }
        });
        dropdown.onValueChanged.AddListener(delegate {
            WriteSettings();
            StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
            DisableDropdown();
        });
        volSlider.onValueChanged.AddListener(delegate {
            baseVolume = volSlider.value;
            Source.volume = baseVolume;
            WriteSettings();
        });
    }
    private void InitializeToggle()
    {
        string path = dir + "/settings.ini";

        StreamReader reader = new StreamReader(path);
        string readLine = reader.ReadLine();

        DirectoryInfo di = new DirectoryInfo(dir);
        string dirName = di.Name;

        while (!readLine.StartsWith("Active"))
        {
            readLine = reader.ReadLine();
        }

        string[] subStrings = readLine.Split(" = ");
        bool active = Convert.ToBoolean(subStrings[1]);

        soundToggle.isOn = active;
        reader.Close();

        soundToggle.onValueChanged.AddListener(delegate {
            WriteSettings();
            if (!soundToggle.isOn)
            {
                StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
            }
        });
    }

    private async void InitializeDropdown()
    {
        FileInfo[] files;
        DirectoryInfo di = new DirectoryInfo(dir);
        files = di.GetFiles();

        List<string> filenames = new List<string>();

        foreach (FileInfo file in files)
        {
            string fileString = file.ToString();
            if (fileString.EndsWith(".wav"))
            {
                Files.Add(fileString);

                filenames.Add(file.Name);

                AudioClip newClip = await LoadClip(fileString);
                Tracks.Add(newClip);
            }
            
        }
        dropdown.AddOptions(filenames);

        string path = dir + "/settings.ini";

        StreamReader reader = new StreamReader(path);
        string readLine = reader.ReadLine();

        string dirName = di.Name;

        while (!readLine.StartsWith("CurrentSound"))
        {
            readLine = reader.ReadLine();
        }

        string[] subStrings = readLine.Split(" = ");
        string active = subStrings[1];

        dropdown.value = dropdown.options.FindIndex(option => option.text == active);
        reader.Close();

        dropdown.onValueChanged.AddListener(delegate {
            WriteSettings();
            StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
        });
        volSlider.onValueChanged.AddListener(delegate {
            baseVolume = volSlider.value;
            WriteSettings();
        });
    }

    async Task<AudioClip> LoadClip(string path)
    {
        AudioClip clip = null;
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
        {
            uwr.SendWebRequest();

            try
            {
                while (!uwr.isDone) await Task.Delay(5);

                if (uwr.error != null)  
                {
                    Debug.Log($"{uwr.error}");
                }
                else
                {
                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                }
            }
            catch (Exception err)
            {
                Debug.Log($"{err.Message}, {err.StackTrace}");
            }
        }
        return clip;
    }

    private void WriteSettings()
    {
        string path = dir + "/settings.ini";

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
                linesList[i] = "BaseVolume = " + baseVolume.ToString();
            }
            else if (linesList[i].StartsWith("CurrentSound = "))
            {
                linesList[i] = "CurrentSound = " + dropdown.options[dropdown.value].text;
            }
            else if (linesList[i].StartsWith("Active = "))
            {
                linesList[i] = "Active = " + soundToggle.isOn.ToString();
            } 
        }

        StreamWriter writer = new StreamWriter(path, false);
        foreach (string line in linesList)
        {
            writer.WriteLine(line);
        }
        writer.Close();
    }

    private void DisableDropdown()
    {
        dropdown.interactable = false;
        Invoke("EnableDropdown", 3);
    }

    private void EnableDropdown()
    {
        dropdown.interactable = true;
    }

    public void Silence()
    {
        StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
    }
}
