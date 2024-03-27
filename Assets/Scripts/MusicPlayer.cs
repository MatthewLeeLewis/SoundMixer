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

    public string activeDirectory; // References the current directory from which to play music files.

    private AudioSource Source; // Reference to the source for the audio.
    public AudioClip track; // Reference to the active track.

    List<string> Files = new List<string>(); // Reference to the list of files in the directory.
    List<AudioClip> Tracks = new List<AudioClip>(); // Reference to the list of tracks in the directory.

    private void Awake()
    {
        if (Instance != null) // This if check ensures that multiple instances of this object do not exist and reports it if they do, and destroys the duplicate.
        {
            Debug.LogError("There's more than one MusicPlayer! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this; // This instantiates the instance.

        //Debug Code to set the active directory directly.
        activeDirectory = @"C:\Users\poodl\AppData\LocalLow\MattLewisCode\SoundMixer\soundscapes\Sample"; 
    }
    
    async void Start()
    {
        Source = GetComponent<AudioSource>(); // Sets the audio source.

        FileInfo[] files; // Creates a temporary array of strings for the files.
        DirectoryInfo di = new DirectoryInfo(activeDirectory);
        files = di.GetFiles(); // Adds all files from the active directory to the array.

        
        foreach (FileInfo file in files) // Iterates through the array.
        {
            string fileString = file.ToString();
            if (fileString.EndsWith(".wav") || fileString.EndsWith(".mp3")) // If its a relevant audio file...
            {
                Files.Add(fileString); // Add it to the actual array.

                AudioClip newClip = await LoadClip(fileString);
                Tracks.Add(newClip);
            }
        }
        PlayTrack(0);
    }

    public void PlayTrack(int _listIndex)
    {
        track = Tracks[_listIndex];
        Source.clip = track;
        Source.Play();
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

}
