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
    [SerializeField] private AudioSource Source, Source2;
    bool source2Active = false;
    private AudioClip track;
    private string playerName;
    private string parentDir;
    private string dir;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Toggle soundToggle;
    private TMP_Dropdown dropdown;
    [SerializeField] private Slider volSlider;
    [SerializeField] private Slider pitchSlider;
    [SerializeField] private Slider panningSlider;
    List<string> Files = new List<string>();
    List<AudioClip> Tracks = new List<AudioClip>();
    private float baseVolume;
    private float pitch;
    private float panning;
    private float volVar;
    private float panVar;
    private float pitchVar;
    private float minTime;
    private float maxTime;
    bool set = false;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button resetPitchButton;
    [SerializeField] private Button resetPanningButton;
    [SerializeField] private Transform deletePanel;
    [SerializeField] private Button confirmDelete;
    [SerializeField] private TMP_InputField deletionInputField;
    [SerializeField] private Toggle autoToggle;
    [SerializeField] private TMP_InputField minTimeInput;
    [SerializeField] private TMP_InputField maxTimeInput;
    [SerializeField] private Slider volAutoSlider;
    [SerializeField] private Slider volVarSlider;
    [SerializeField] private Slider panVarSlider;
    [SerializeField] private Slider pitchVarSlider;
    [SerializeField] private Transform autoContainer;
    [SerializeField] private Transform manualContainer;

    private void Update()
    {
        if (!autoToggle.isOn)
        {
            if (!source2Active)
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
            else
            {
                if (soundToggle.isOn && !Source2.isPlaying)
                {
                    Source2.volume = 0;
                    PlayTrack();
                }
                else if (soundToggle.isOn && Source2.volume == 0)
                {
                    PlayTrack();
                    StartCoroutine(FadeAudioSource.StartFade(Source2, 3, baseVolume));
                }
            }
        }
        else
        {
            if (!source2Active)
            {
                if (soundToggle.isOn && set == false)
                {
                    AutoPlayTrack();
                }
            }
            else
            {
                if (soundToggle.isOn && set == false)
                {
                    AutoPlayTrack();
                }
            }
        }
    }
    private void Awake()
    {
        deleteButton.gameObject.SetActive(false);
        deletePanel.gameObject.SetActive(false);
        dropdown = GetComponentInChildren<TMP_Dropdown>();
    }

    private void Start()
    {
        ActiveSoundscapes.Instance.ToggleDeleteMode += ActiveSoundscapes_ToggleDeleteMode;
    }
    private void SetUp()
    {
        InitializeTracks();
        InitializeSettings();
        InitializeInputs();
        ToggleAutoDisplay();
    }

    private void ToggleAutoDisplay()
    {
        autoContainer.gameObject.SetActive(autoToggle.isOn);
        manualContainer.gameObject.SetActive(!autoToggle.isOn);
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

    public void PlayTrack()
    {
        if (!source2Active)
        {
            if (Tracks.Count != 0)
            {
                track = Tracks[dropdown.value];
                Source.pitch = pitch;
                Source.panStereo = panning;
                Source.clip = track;
                Source.Play();
            } 
            else 
            {
                Source.Stop();
            }
        }
        else
        {
            if (Tracks.Count != 0)
            {
                track = Tracks[dropdown.value];
                Source2.pitch = pitch;
                Source2.panStereo = panning;
                Source2.clip = track;
                Source2.Play();
            } 
            else 
            {
                Source2.Stop();
            }
        }
    }
    public void AutoPlayTrack()
    {
        if (autoToggle.isOn)
        {
            if (!source2Active)
            {
                if (Tracks.Count != 0)
                {
                    track = Tracks[dropdown.value];
                    Source.clip = track;
                    Source.volume = 0;

                    // Calculate and set Volume using volVar
                    float minimumVolume = (baseVolume - (baseVolume*volVar));
                    float maximumVolume = (baseVolume + (baseVolume*volVar));

                    if (minimumVolume <= 0f){minimumVolume = 0.001f;}
                    if (maximumVolume > 1f){maximumVolume = 1f;}

                    float newVol = UnityEngine.Random.Range(minimumVolume, maximumVolume);

                    // Calculate and set pitch using pitchVar
                    float maximumPitch = 1f + pitchVar;
                    float minimumPitch = 0.999f - pitchVar;

                    Source.pitch = UnityEngine.Random.Range(minimumPitch, maximumPitch);

                    // Calculate and set panning using panVar
                    float minimumPan = 0f - panVar;
                    float maximumPan = 0f + panVar;

                    Source.panStereo = UnityEngine.Random.Range(minimumPan, maximumPan);

                    Source.Play();
                    StartCoroutine(FadeAudioSource.StartFade(Source, 3, newVol));
                    DisableInputs();

                    float interval = UnityEngine.Random.Range(minTime, maxTime);
                    set = true;
                    Invoke("AutoFadeSource", interval);
                } 
                else 
                {
                    Source.Stop();
                }
            }
            else
            {
                if (Tracks.Count != 0)
                {
                    track = Tracks[dropdown.value];
                    Source2.clip = track;
                    Source2.volume = 0;

                    // Calculate and set Volume using volVar
                    float minimumVolume = (baseVolume - (baseVolume*volVar));
                    float maximumVolume = (baseVolume + (baseVolume*volVar));

                    if (minimumVolume <= 0f){minimumVolume = 0.001f;}
                    if (maximumVolume > 1f){maximumVolume = 1f;}

                    float newVol = UnityEngine.Random.Range(minimumVolume, maximumVolume);

                    // Calculate and set pitch using pitchVar
                    float maximumPitch = 1f + pitchVar;
                    float minimumPitch = 0.999f - pitchVar;

                    Source2.pitch = UnityEngine.Random.Range(minimumPitch, maximumPitch);

                    // Calculate and set panning using panVar
                    float minimumPan = 0f - panVar;
                    float maximumPan = 0f + panVar;

                    Source2.panStereo = UnityEngine.Random.Range(minimumPan, maximumPan);

                    Source2.Play();
                    StartCoroutine(FadeAudioSource.StartFade(Source2, 3, newVol));

                    float interval = UnityEngine.Random.Range(minTime, maxTime);
                    set = true;
                    Invoke("AutoFadeSource", interval);
                } 
                else 
                {
                    Source2.Stop();
                }
            }
        }
    }

    private void AutoFadeSource()
    {
        if (autoToggle.isOn)
        {
            if (!source2Active)
            {
                StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
                DisableInputs();
            }
            else
            {
                StartCoroutine(FadeAudioSource.StartFade(Source2, 3, 0f));
                DisableInputs();
            }
            float interval = UnityEngine.Random.Range(minTime, maxTime);
            Invoke("AutoPlayTrack", interval);
        }
    }

    private async void InitializeTracks()
    {
        dropdown.ClearOptions();
        Tracks.Clear();
        Files.Clear();

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
                volAutoSlider.value = baseVolume;
            }
            else if (linesList[i].StartsWith("Pitch = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                pitch = float.Parse(subStrings[1]);
                if (pitch < 0.9f){pitch = 0.9f;}
                else if (pitch > 1.1f){pitch = 1.1f;}
                pitchSlider.value = pitch;
            }
            else if (linesList[i].StartsWith("Panning = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                panning = float.Parse(subStrings[1]);
                if (panning < -1f){panning = -1f;}
                else if (panning > 1.0f){panning = 1.0f;}
                panningSlider.value = panning;
            }
            else if (linesList[i].StartsWith("CurrentSound = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                dropdown.value = dropdown.options.FindIndex(option => option.text == subStrings[1]);
            }
            else if (linesList[i].StartsWith("AutoMode = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                autoToggle.isOn = Convert.ToBoolean(subStrings[1]);
            }
            else if (linesList[i].StartsWith("VolVar = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                volVar = float.Parse(subStrings[1]);
                if (volVar < 0.0f){volVar = 0.0f;}
                else if (volVar > 1.0f){volVar = 1.0f;}
                volVarSlider.value = volVar;
            }
            else if (linesList[i].StartsWith("PanVar = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                panVar = float.Parse(subStrings[1]);
                if (panVar < 0.0f){panVar = 0.0f;}
                else if (panVar > 1.0f){panVar = 1.0f;}
                panVarSlider.value = panVar;
            }
            else if (linesList[i].StartsWith("PitchVar = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                pitchVar = float.Parse(subStrings[1]);
                if (pitchVar < 0.0f){pitchVar = 0.0f;}
                else if (pitchVar > 0.1f){pitchVar = 0.1f;}
                pitchVarSlider.value = pitchVar;
            }
            else if (linesList[i].StartsWith("MinTime = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                minTime = float.Parse(subStrings[1]);
                if (minTime < 3.0f){minTime = 3.0f;}
                minTimeInput.text = minTime.ToString();
            }
            else if (linesList[i].StartsWith("MaxTime = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                maxTime = float.Parse(subStrings[1]);
                if (maxTime < minTime){maxTime = minTime;}
                maxTimeInput.text = maxTime.ToString();
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
            if (Tracks.Count > 0 && dropdown.options.Count > 0)
            {
                WriteSettings();
                if (!soundToggle.isOn && !source2Active)
                {
                    StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
                }
                else if (!soundToggle.isOn && source2Active)
                {
                    StartCoroutine(FadeAudioSource.StartFade(Source2, 3, 0f));
                }
            }
            else
            {
                soundToggle.isOn = false;
            }
            DisableInputs();
        });
        autoToggle.onValueChanged.AddListener(delegate {
            ToggleAutoDisplay();
            DisableInputs();
            WriteSettings();
            if (!autoToggle.isOn)
            {
                set = false;
            }
        });
        dropdown.onValueChanged.AddListener(delegate {
            WriteSettings();
            if (!source2Active && Source.volume != 0)
            {
                StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
                ChangeSource();
            }
            else if (source2Active && Source2.volume != 0)
            {
                StartCoroutine(FadeAudioSource.StartFade(Source2, 3, 0f));
                ChangeSource();
            }
            DisableInputs();
        });
        volSlider.onValueChanged.AddListener(delegate {
            baseVolume = volSlider.value;
            if (Source.volume != 0 && soundToggle.isOn)
            {
                Source.volume = baseVolume;
            }
            else if (Source2.volume != 0 && soundToggle.isOn)
            {
                Source2.volume = baseVolume;
            }
            volAutoSlider.value = baseVolume;
            WriteSettings();
        });
        volAutoSlider.onValueChanged.AddListener(delegate {
            baseVolume = volAutoSlider.value;
            volSlider.value = baseVolume;
            WriteSettings();
        });
        pitchSlider.onValueChanged.AddListener(delegate {
            pitch = pitchSlider.value;
            Source.pitch = pitch;
            Source2.pitch = pitch;
            WriteSettings();
        });
        resetPitchButton.onClick.AddListener(() =>
        {
            pitch = 1f;
            Source.pitch = pitch;
            Source2.pitch = pitch;
            pitchSlider.value = pitch;
            WriteSettings();
        });
        panningSlider.onValueChanged.AddListener(delegate {
            panning = panningSlider.value;
            Source.panStereo = panning;
            Source2.panStereo = panning;
            WriteSettings();
        });
        resetPanningButton.onClick.AddListener(() =>
        {
            panning = 0f;
            Source.panStereo = panning;
            Source2.panStereo = panning;
            panningSlider.value = panning;
            WriteSettings();
        });
        deleteButton.onClick.AddListener(() =>
        {
            deletePanel.gameObject.SetActive(true);
        });
        minTimeInput.onValueChanged.AddListener(delegate {
            minTime = float.Parse(minTimeInput.text);
            if (minTime < 3.0f)
            {
                minTime = 3.0f;
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
            if (maxTime < 3.0f)
            {
                maxTime = 3.0f;
                maxTimeInput.text = maxTime.ToString();
            }
            if (maxTime < minTime)
            {
                minTime = maxTime;
                minTimeInput.text = minTime.ToString(); 
            }
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
        confirmDelete.onClick.AddListener(() =>
        {
            if (deletionInputField.text == playerName)
            {
                Silence();

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
            if (linesList[i].StartsWith("BaseVolume = "))
            {
                linesList[i] = "BaseVolume = " + baseVolume.ToString();
            }
            else if (linesList[i].StartsWith("Pitch = "))
            {
                linesList[i] = "Pitch = " + pitch.ToString();
            }
            else if (linesList[i].StartsWith("Panning = "))
            {
                linesList[i] = "Panning = " + panning.ToString();
            }
            else if (linesList[i].StartsWith("CurrentSound = "))
            {
                linesList[i] = "CurrentSound = " + dropdown.options[dropdown.value].text;
            }
            else if (linesList[i].StartsWith("AutoMode = "))
            {
                linesList[i] = "AutoMode = " + autoToggle.isOn.ToString();
            } 
            else if (linesList[i].StartsWith("VolVar = "))
            {
                linesList[i] = "VolVar = " + volVar.ToString();
            }
            else if (linesList[i].StartsWith("PanVar = "))
            {
                linesList[i] = "PanVar = " + panVar.ToString();
            }
            else if (linesList[i].StartsWith("PitchVar = "))
            {
                linesList[i] = "PitchVar = " + pitchVar.ToString();
            }
            else if (linesList[i].StartsWith("MinTime = "))
            {
                linesList[i] = "MinTime = " + minTime.ToString();
            }
            else if (linesList[i].StartsWith("MaxTime = "))
            {
                linesList[i] = "MaxTime = " + maxTime.ToString();
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

    private void DisableInputs()
    {
        dropdown.interactable = false;
        volSlider.interactable = false;
        pitchSlider.interactable = false;
        panningSlider.interactable = false;
        resetPanningButton.interactable = false;
        resetPitchButton.interactable = false;
        soundToggle.interactable = false;
        autoToggle.interactable = false;
        volAutoSlider.interactable = false;
        volVarSlider.interactable = false;
        panVarSlider.interactable = false;
        pitchVarSlider.interactable = false;
        minTimeInput.interactable = false;
        maxTimeInput.interactable = false;
        Invoke("EnableInputs", 3);
    }

    private void EnableInputs()
    {
        dropdown.interactable = true;
        volSlider.interactable = true;
        pitchSlider.interactable = true;
        panningSlider.interactable = true;
        resetPanningButton.interactable = true;
        resetPitchButton.interactable = true;
        soundToggle.interactable = true;
        autoToggle.interactable = true;
        volAutoSlider.interactable = true;
        volVarSlider.interactable = true;
        panVarSlider.interactable = true;
        pitchVarSlider.interactable = true;
        minTimeInput.interactable = true;
        maxTimeInput.interactable = true;
    }

    public void Silence()
    {
        if (!source2Active)
        {
            StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
        }
        else if (source2Active)
        {
            StartCoroutine(FadeAudioSource.StartFade(Source2, 3, 0f));
        }
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

    private void ChangeSource()
    {
        source2Active = !source2Active;
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
        volAutoSlider.value = inputVol;

        baseVolume = inputVol;
        
        if (Source.volume != 0 && soundToggle.isOn)
        {
            Source.volume = baseVolume;
        }
        else if (Source2.volume != 0 && soundToggle.isOn)
        {
            Source2.volume = baseVolume;
        }
    }

    public void SetPitch(float inputPitch)
    {
        if (inputPitch < 0.9f){inputPitch = 0.9f;}
        else if (inputPitch > 1.1f){inputPitch = 1.1f;}
        pitchSlider.value = inputPitch;

        pitch = inputPitch;

        Source.pitch = pitch;
        Source2.pitch = pitch;
    }

    public void SetPanning(float inputPanning)
    {
        if (inputPanning < -1.0f){inputPanning = -1.0f;}
        else if (inputPanning > 1.0f){inputPanning = 1.0f;}
        panningSlider.value = inputPanning;

        panning = inputPanning;

        Source.panStereo = panning;
        Source2.panStereo = panning;
    }

    public void SetCurrentSound(string inputCurrentSound)
    {
        dropdown.value = dropdown.options.FindIndex(option => option.text == inputCurrentSound);
    }

    public void SetAutoMode(bool inputBool)
    {
        autoToggle.isOn = inputBool;
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
        if (inputMinTime < 3.0f){inputMinTime = 3.0f;}
        minTimeInput.text = inputMinTime.ToString();
        minTime = inputMinTime;
    }

    public void SetMaxTime(float inputMaxTime)
    {
        if (inputMaxTime < minTime){inputMaxTime = minTime;}
        maxTimeInput.text = inputMaxTime.ToString();
        maxTime = inputMaxTime;
    }

    public void SetActive(bool inputActive)
    {
        soundToggle.isOn = inputActive;
    }
}
