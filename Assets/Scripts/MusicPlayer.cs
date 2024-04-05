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

    public static event EventHandler OnMusicChanged;
    private bool active = false;

    public string activeDirectory; // References the current directory from which to play music files.

    private AudioSource Source; // Reference to the source for the audio.
    private AudioClip track; // Reference to the active track.

    List<string> Files = new List<string>(); // Reference to the list of files in the directory.
    List<AudioClip> Tracks = new List<AudioClip>(); // Reference to the list of tracks in the directory.



    private void Update()
    {
        if (active == true && !Source.isPlaying)
        {
            PlayTrack();
        }
        else if (active == true && Source.volume == 0)
        {
            PlayTrack();
            StartCoroutine(FadeAudioSource.StartFade(Source, 3, 1f));
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
        if (ReadSettings() != "None")
        {
            MusicList.Instance.SetActiveMusic(ReadSettings());
            SetDirectory(ReadSettings());
            OnMusicChanged?.Invoke(this, EventArgs.Empty);
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
            active = true;
        } 
        else 
        {
            Source.Stop();
            active = false;
        }
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

    public async void SetDirectory(string dir)
    {
        if (activeDirectory != dir)
        {
            if (active == true)
            {
                active = false;
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
            active = true;
        }
    }

    private string ReadSettings()
    {
        string path = Application.persistentDataPath + "/music/activeMusic.ini";

        StreamReader reader = new StreamReader(path);
        string readLine = reader.ReadLine();
        
        while (!readLine.StartsWith("Active"))
        {
            readLine = reader.ReadLine();
        }
        string[] subStrings = readLine.Split(" = ");
        string active = subStrings[1];

        reader.Close();

        return active;
    } 

}
