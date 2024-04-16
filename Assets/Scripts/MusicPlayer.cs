using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }
    public static event EventHandler DisableMusicButtons;

    public static event EventHandler OnMusicChanged;
    private bool active = false;
    private bool paused = false;

    public string activeDirectory; // References the current directory from which to play music files.

    private AudioSource Source; // Reference to the source for the audio.
    private AudioClip track; // Reference to the active track.

    List<string> Files = new List<string>(); // Reference to the list of files in the directory.
    List<AudioClip> Tracks = new List<AudioClip>(); // Reference to the list of tracks in the directory.
    private float baseVolume;



    private void Update()
    {
        if (paused == false)
        {
            if (active == true && !Source.isPlaying)
            {
                PlayTrack();
            }
            else if (active == true && Source.volume == 0f)
            {
                PlayTrack();
                StartCoroutine(FadeAudioSource.StartFade(Source, 3, baseVolume));
            }
            else if (active == false && Source.volume == 0f)
            {
                Source.Stop();
            }
        }
    }
    private void Awake()
    {
        if (Instance != null) // This if check ensures that multiple instances of this object do not exist and reports it if they do, and destroys the duplicate.
        {
            Debug.LogError("There's more than one MusicPlayer! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this; // This instantiates the instance.
        Source = GetComponent<AudioSource>(); // Sets the audio source.
    }

    private void Start()
    {
        Invoke("InitializePlayer", 0.1f);
    }

    public void InitializePlayer()
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
                string[] subStrings = linesList[i].Split(" = ");
                
                baseVolume = float.Parse(subStrings[1]);
                MusicList.Instance.SetVolSlider(baseVolume);
            }
            else if (linesList[i].StartsWith("Active = "))
            {
                string[] subStrings = linesList[i].Split(" = ");
                
                if (subStrings[1] != "None")
                {
                    MusicList.Instance.SetActiveMusic(subStrings[1]);
                    SetDirectory(subStrings[1]);
                    OnMusicChanged?.Invoke(this, EventArgs.Empty);
                }
                if (subStrings[1] == "None")
                {
                    MusicList.Instance.SetActiveMusic(subStrings[1]);
                }
            }
        }
    }

    public void PlayTrack()
    {
        if (Tracks.Count != 0)
        {
            int _listIndex = UnityEngine.Random.Range(0, Tracks.Count);

            track = Tracks[_listIndex];
            Source.clip = track;
            Source.Play();
            EngageActive();
        } 
        else 
        {
            Source.Stop();
            FalsifyActive();
        }
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

    public async void SetDirectory(string dir)
    {
        if (activeDirectory != dir || !Source.isPlaying)
        {
            if (active == true)
            {
                FalsifyActive();
                StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
            }
            activeDirectory = dir;

            FileInfo[] files; // Creates a temporary array of strings for the files.
            DirectoryInfo di = new DirectoryInfo(activeDirectory);
            files = di.GetFiles(); // Adds all files from the active directory to the array.

            if (Files.Count != 0)
            {
                Files.Clear();
            }
            if (Tracks.Count != 0)
            {
                Tracks.Clear();
            }
        
            foreach (FileInfo file in files) // Iterates through the array.
            {
                string fileString = file.ToString();
                if (fileString.EndsWith(".wav")) // If its a relevant audio file...
                {
                    Files.Add(fileString); // Add it to the actual array.

                    AudioClip newClip = await LoadClip(fileString);
                    Tracks.Add(newClip);
                }
            }
            EngageActive();
        }
    }

    public void SetVolume(float input)
    {
        baseVolume = input;
        Source.volume = input;
    }

    public float GetVolume()
    {
        return baseVolume;
    }

    public void Skip()
    {
        if (active == true)
        {
            paused = false;
            MusicList.Instance.SetInteractable(false);
            StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
            DisableMusicButtons?.Invoke(this, EventArgs.Empty);
        }
    }
    public void Pause()
    {
        if (active)
        {
            if (paused)
            {
                Source.Play();
            }
            else
            {
                Source.Pause();
            }
            paused = !paused;
        }
    }

    public void Stop()
    {
        if (active)
        {
            FalsifyActive();
            if (Source.isPlaying)
            {
                MusicList.Instance.SetInteractable(false);
                StartCoroutine(FadeAudioSource.StartFade(Source, 3, 0f));
                DisableMusicButtons?.Invoke(this, EventArgs.Empty);
            }
            else if (paused == true)
            {
                paused = false;
                Stop();
            }
        } 
    }

    private void FalsifyActive()
    {
        active = false;
        paused = false;
        MusicList.Instance.SetInteractable(false);
    }
    private void EngageActive()
    {
        active = true;
        MusicList.Instance.SetInteractable(true);
    }
}
