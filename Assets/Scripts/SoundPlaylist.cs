using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class SoundPlaylist : MonoBehaviour
{
    private AudioSource Source;
    private AudioClip track;
    private string playerName;
    private string dir;
    private float minTime;
    private float maxTime;
    private float volVar;
    private bool set = false;
    private bool initialized = false;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Toggle soundToggle;
    List<string> Files = new List<string>();
    List<AudioClip> Tracks = new List<AudioClip>();
    [SerializeField] private TMP_InputField minTimeInput;
    [SerializeField] private TMP_InputField maxTimeInput;
    [SerializeField] private TMP_InputField volVarInput;
    private Slider volSlider;
    private float baseVolume;

    private void Awake()
    {
        Source = GetComponent<AudioSource>();
        volSlider = GetComponentInChildren<Slider>();
    }

    private void Update()
    {
        if (initialized)
        {
            if (soundToggle.isOn && !set)
            {
                set = true;
                float interval = UnityEngine.Random.Range(minTime, maxTime);

                Invoke("PlaySound", interval);
            }
            else if (soundToggle.isOn && Source.volume == 0)
            {
                StartCoroutine(FadeAudioSource.StartFade(Source, 3, baseVolume));
            }
        }
    }

    private void SetUp()
    {
        InitializeTracks();
        InitializeSettings();
        InitializeInputs();
        initialized = true;
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

    private void PlaySound()
    {
        if (Tracks.Count != 0)
        {
            int _listIndex = UnityEngine.Random.Range(0, Tracks.Count);

            track = Tracks[_listIndex];
            Source.clip = track;

            float minimumVolume = baseVolume - volVar;
            float maximumVolume = baseVolume + volVar;

            if (maximumVolume > 1f)
            {
                maximumVolume = 1f;
            }
            if (minimumVolume <= 0f)
            {
                minimumVolume = 0.01f;
            }

            Source.volume = UnityEngine.Random.Range(minimumVolume, maximumVolume);
            Debug.Log("Volume Sound Played at: " + Source.volume.ToString());
            Source.Play();

            Invoke("ResetPlayer", Source.clip.length);
        } 
        else 
        {
            Source.Stop();
        }
    }

    private void ResetPlayer()
    {
        set = false;
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
            if (linesList[i].StartsWith("MinTime = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                minTime = float.Parse(subStrings[1]);
                if (minTime < 0.0f){minTime = 0.0f;}
                minTimeInput.text = minTime.ToString();
                Debug.Log("MinTime = " + minTime);
            }
            else if (linesList[i].StartsWith("MaxTime = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                maxTime = float.Parse(subStrings[1]);
                if (maxTime < 0.0f){maxTime = 0.0f;}
                if (maxTime < minTime){maxTime = minTime;}
                maxTimeInput.text = maxTime.ToString();
                Debug.Log("MaxTime = " + maxTime);
            }
            else if (linesList[i].StartsWith("VolVar = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                volVar = float.Parse(subStrings[1]);
                if (volVar < 0.0f){volVar = 0.0f;}
                volVarInput.text = volVar.ToString();
                Debug.Log("VolVar = " + volVar);
            }
            else if (linesList[i].StartsWith("BaseVolume = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                baseVolume = float.Parse(subStrings[1]);
                if (baseVolume < 0.01f){baseVolume = 0.01f;}
                else if (baseVolume > 1.0f){baseVolume = 1.0f;}
                volSlider.value = baseVolume;
                Debug.Log("Base Volume = " + baseVolume);
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
        minTimeInput.onValueChanged.AddListener(delegate {
            minTime = float.Parse(minTimeInput.text);
            Debug.Log("MinTime = " + minTime);
            if (minTime < 0.0f)
            {
                minTime = 0.0f;
                minTimeInput.text = minTime.ToString();
            }
            if (minTime > maxTime)
            {
                maxTime = minTime;
                maxTimeInput.text = maxTime.ToString(); 
            }
            WriteSettings();
        });
        maxTimeInput.onValueChanged.AddListener(delegate {
            maxTime = float.Parse(maxTimeInput.text);
            Debug.Log("MaxTime = " + maxTime);
            if (maxTime < 0.0f)
            {
                maxTime = 0.0f;
                maxTimeInput.text = maxTime.ToString();
            }
            if (maxTime < minTime)
            {
                minTime = maxTime;
                minTimeInput.text = minTime.ToString(); 
            }
            WriteSettings();
        });
        volVarInput.onValueChanged.AddListener(delegate {
            volVar = float.Parse(volVarInput.text);
            Debug.Log("VolVar = " + volVar);
            if (volVar > 1.0f)
            {
                volVar = 1.0f;
                volVarInput.text = volVar.ToString();
            }
            else if (volVar < 0.0f)
            {
                volVar = 0.0f;
                volVarInput.text = volVar.ToString();
            }
            WriteSettings();
        });
        soundToggle.onValueChanged.AddListener(delegate {
            WriteSettings();
            if (!soundToggle.isOn)
            {
                StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
            }
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
            if (linesList[i].StartsWith("MinTime = "))
            {
                linesList[i] = "MinTime = " + minTime.ToString();
            }
            else if (linesList[i].StartsWith("MaxTime = "))
            {
                linesList[i] = "MaxTime = " + maxTime.ToString();
            }
            else if (linesList[i].StartsWith("VolVar = "))
            {
                linesList[i] = "VolVar = " + volVar.ToString();
            }
            else if (linesList[i].StartsWith("BaseVolume = "))
            {
                linesList[i] = "BaseVolume = " + baseVolume.ToString();
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

    public void Silence()
    {
        StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
    }
}
