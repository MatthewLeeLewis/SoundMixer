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
    [SerializeField] private AudioSource Source, Source2, Source3;
    private AudioClip track;
    private string playerName;
    private string parentDir;
    private string dir;
    private float minTime;
    private float maxTime;
    private float volVar;
    private float pitchVar;
    private float panVar;
    private bool set = false;
    private bool initialized = false;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Toggle soundToggle;
    List<string> Files = new List<string>();
    List<AudioClip> Tracks = new List<AudioClip>();
    [SerializeField] private TMP_InputField minTimeInput;
    [SerializeField] private TMP_InputField maxTimeInput;
    [SerializeField] private Slider volSlider;
    [SerializeField] private Slider volVarSlider;
    [SerializeField] private Slider pitchVarSlider;
    [SerializeField] private Slider panVarSlider;
    private float baseVolume;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Transform deletePanel;
    [SerializeField] private Button confirmDelete;
    [SerializeField] private TMP_InputField deletionInputField;
    [SerializeField] private Toggle overlapToggle;

    private void Awake()
    {
        deletePanel.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        // Source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ActiveSoundscapes.Instance.ToggleDeleteMode += ActiveSoundscapes_ToggleDeleteMode;
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

    public void SetParentDir(string input)
    {
        parentDir = input;
    }

    private void PlaySound()
    {
        AudioSource activeSource = new AudioSource();
        if (Tracks.Count != 0)
        {
            int _listIndex = UnityEngine.Random.Range(0, Tracks.Count);

            track = Tracks[_listIndex];

            if (Source.isPlaying)
            {
                if (!Source2.isPlaying)
                {
                    activeSource = Source2;
                }
                else if (!Source3.isPlaying)
                {
                    activeSource = Source3;
                }
            }
            else
            {
                activeSource = Source;
            }

            if (activeSource != null)
            {
                activeSource.clip = track;

                // Calculate and set Volume using volVar
                float minimumVolume = (baseVolume - (baseVolume*volVar));
                float maximumVolume = (baseVolume + (baseVolume*volVar));

                if (minimumVolume <= 0f){minimumVolume = 0.001f;}
                if (maximumVolume > 1f){maximumVolume = 1f;}

                activeSource.volume = UnityEngine.Random.Range(minimumVolume, maximumVolume);

                // Calculate and set pitch using pitchVar
                float maximumPitch = 1f + pitchVar;
                float minimumPitch = 0.999f - pitchVar;

                activeSource.pitch = UnityEngine.Random.Range(minimumPitch, maximumPitch);

                // Calculate and set panning using panVar
                float minimumPan = 0f - panVar;
                float maximumPan = 0f + panVar;

                activeSource.panStereo = UnityEngine.Random.Range(minimumPan, maximumPan);

                // Play the sound and reset the player when the sound finishes
                activeSource.Play();
            }

            if (overlapToggle.isOn)
            {
                ResetPlayer();
            }
            else
            {
                if (activeSource != null)
                {
                    Invoke("ResetPlayer", activeSource.clip.length / Math.Abs(Source.pitch));
                }
                else
                {
                    ResetPlayer();
                }
            }
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
            if (fileString.EndsWith(".wav") || fileString.EndsWith(".mp3"))
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
            }
            else if (linesList[i].StartsWith("MaxTime = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                maxTime = float.Parse(subStrings[1]);
                if (maxTime < 0.0f){maxTime = 0.0f;}
                if (maxTime < minTime){maxTime = minTime;}
                maxTimeInput.text = maxTime.ToString();
            }
            else if (linesList[i].StartsWith("VolVar = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                volVar = float.Parse(subStrings[1]);
                if (volVar < 0.0f){volVar = 0.0f;}
                if (volVar > 1.0f){volVar = 1.0f;}
                volVarSlider.value = volVar;
            }
            else if (linesList[i].StartsWith("PitchVar = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                pitchVar = float.Parse(subStrings[1]);
                if (pitchVar < 0.0f){pitchVar = 0.0f;}
                if (pitchVar > 0.1f){pitchVar = 0.1f;}
                pitchVarSlider.value = pitchVar;
            }
            else if (linesList[i].StartsWith("PanVar = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                panVar = float.Parse(subStrings[1]);
                if (panVar < 0.0f){panVar = 0.0f;}
                if (panVar > 1.0f){panVar = 1.0f;}
                panVarSlider.value = panVar;
            }
            else if (linesList[i].StartsWith("BaseVolume = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                baseVolume = float.Parse(subStrings[1]);
                if (baseVolume < 0.01f){baseVolume = 0.01f;}
                else if (baseVolume > 1.0f){baseVolume = 1.0f;}
                volSlider.value = baseVolume;
            }
            else if (linesList[i].StartsWith("Active = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                soundToggle.isOn = Convert.ToBoolean(subStrings[1]);
            }
            else if (linesList[i].StartsWith("Overlap = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                overlapToggle.isOn = Convert.ToBoolean(subStrings[1]);
            }
        }
    }

    private void InitializeInputs()
    {
        minTimeInput.onValueChanged.AddListener(delegate {
            minTime = float.Parse(minTimeInput.text);
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
        soundToggle.onValueChanged.AddListener(delegate {
            if (Tracks.Count > 0)
            {
                WriteSettings();
                if (!soundToggle.isOn)
                {
                    StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
                }
            }
            else
            {
                soundToggle.isOn = false;
            }
        });
        overlapToggle.onValueChanged.AddListener(delegate {
            WriteSettings();
        });
        volSlider.onValueChanged.AddListener(delegate {
            baseVolume = volSlider.value;
            WriteSettings();
        });
        volVarSlider.onValueChanged.AddListener(delegate {
            volVar = volVarSlider.value;
            WriteSettings();
        });
        pitchVarSlider.onValueChanged.AddListener(delegate {
            pitchVar = pitchVarSlider.value;
            WriteSettings();
        });
        panVarSlider.onValueChanged.AddListener(delegate {
            panVar = panVarSlider.value;
            WriteSettings();
        });
        deleteButton.onClick.AddListener(() =>
        {
            deletePanel.gameObject.SetActive(true);
        });
        confirmDelete.onClick.AddListener(() =>
        {
            if (deletionInputField.text == playerName)
            {
                if (Source.isPlaying)
                {
                    Silence();
                }

                // Read settings to a list
                StreamReader reader = new StreamReader(parentDir + "/settings.ini");
                List<string> linesList = new List<string>();
                while (reader.Peek() >= 0)
                {
                    linesList.Add(reader.ReadLine());
                }
                reader.Close();

                // Rewrite the list
                StreamWriter writer = new StreamWriter(parentDir + "/settings.ini", false);
                for (int i = 0; i < linesList.Count; i++)
                {
                    if (linesList[i].StartsWith(playerName))
                    {
                        linesList.Remove(linesList[i]);
                    }
                }
                foreach (string line in linesList)
                {
                    writer.WriteLine(line);
                }
                writer.Close();

                Directory.Delete(dir, true);

                confirmDelete.interactable = false;
                Invoke("DeleteSoundPlayer", 3f);
                
            }
            else
            {
                deletePanel.gameObject.SetActive(false);
            }
        });
    }

    async Task<AudioClip> LoadClip(string path)
    {
        AudioClip clip = null;
        if (path.EndsWith(".wav"))
        {
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
        else if (path.EndsWith(".mp3"))
        {
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
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
        else
        {
            return null;
        }
    }

    public void WriteSettings()
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
            else if (linesList[i].StartsWith("PitchVar = "))
            {
                linesList[i] = "PitchVar = " + pitchVar.ToString();
            }
            else if (linesList[i].StartsWith("PanVar = "))
            {
                linesList[i] = "PanVar = " + panVar.ToString();
            }
            else if (linesList[i].StartsWith("BaseVolume = "))
            {
                linesList[i] = "BaseVolume = " + baseVolume.ToString();
            }
            else if (linesList[i].StartsWith("Active = "))
            {
                linesList[i] = "Active = " + soundToggle.isOn.ToString();
            }
            else if (linesList[i].StartsWith("Overlap = "))
            {
                linesList[i] = "Overlap = " + overlapToggle.isOn.ToString();
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

    private void DeleteSoundPlayer()
    {
        Destroy(this.gameObject);
        Destroy(this);
    }

    public string GetDir()
    {
        return dir;
    }

    public void SetBaseVolume(float inputVol)
    {
        if (inputVol < 0.01f){inputVol = 0.01f;}
        else if (inputVol > 1.0f){inputVol = 1.0f;}
        volSlider.value = inputVol;

        baseVolume = inputVol;
    }

    public void SetVolVar(float inputVolVar)
    {
        if (inputVolVar < 0.0f){inputVolVar = 0.0f;}
        else if (inputVolVar > 1.0f){inputVolVar = 1.0f;}
        volVarSlider.value = inputVolVar;
        volVar = inputVolVar;
    }

    public void SetPitchVar(float inputPitchVar)
    {
        if (inputPitchVar < 0.0f){inputPitchVar = 0.0f;}
        else if (inputPitchVar > 0.1f){inputPitchVar = 0.1f;}
        pitchVarSlider.value = inputPitchVar;
        pitchVar = inputPitchVar;
    }

    public void SetPanVar(float inputPanVar)
    {
        if (inputPanVar < 0.0f){inputPanVar = 0.0f;}
        else if (inputPanVar > 1.0f){inputPanVar = 1.0f;}
        panVarSlider.value = inputPanVar;
        panVar = inputPanVar;
    }

    public void SetMinTime(float inputMinTime)
    {
        if (inputMinTime < 0.0f){inputMinTime = 0.0f;}
        minTimeInput.text = inputMinTime.ToString();
        minTime = inputMinTime;
    }

    public void SetMaxTime(float inputMaxTime)
    {
        if (inputMaxTime < minTime){inputMaxTime = minTime;}
        maxTimeInput.text = inputMaxTime.ToString();
        maxTime = inputMaxTime;
    }

    public void SetOverlap(bool inputOverlap)
    {
        overlapToggle.isOn = inputOverlap;
    }

    public void SetActive(bool inputActive)
    {
        soundToggle.isOn = inputActive;
    }
}
