using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class Soundscape : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI soundscapeName;
    [SerializeField] private Transform contentPanel;
    [SerializeField] private Transform soundPlaylistPrefab;
    [SerializeField] private Transform loopingSoundPrefab;
    [SerializeField] private Button folderButton;
    [SerializeField] private Button plusButton;
    private TypeDialogue typeDialogue;
    private List<string> instantiatedPlayers = new List<string>();
    private string dir;
    string[] subDir;

    private void Awake()
    {
        typeDialogue = GetComponentInChildren<TypeDialogue>();
    }

    public void SetName(string name)
    {
        soundscapeName.text = name;
    }

    public string GetName()
    {
        return soundscapeName.text;
    }

    public void SetDir(string inputDir)
    {
        dir = inputDir;
        subDir = Directory.GetDirectories(dir);
        typeDialogue.SetDir(dir);
        SetUp();
    }

    public void SetUp()
    {
        foreach (Transform soundPlayer in contentPanel)
        {
            Destroy(soundPlayer.gameObject);
            Destroy(soundPlayer);
        }
        foreach (string directory in subDir)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            string dirName = di.Name;

            instantiatedPlayers.Add(dirName);

            string soundType = ReadSettings(dirName);

            if (soundType == "LoopToggle")
            {
                Transform newLoopingSoundTransform = Instantiate(loopingSoundPrefab, contentPanel.transform);
                LoopingSound newLoopingSound = newLoopingSoundTransform.GetComponent<LoopingSound>();

                newLoopingSound.SetName(dirName);
                newLoopingSound.SetDir(directory);
            }
            else if (soundType == "Playlist")
            {
                Transform newSoundPlaylistTransform = Instantiate(soundPlaylistPrefab, contentPanel.transform);
                SoundPlaylist newSoundPlaylist = newSoundPlaylistTransform.GetComponent<SoundPlaylist>(); 

                newSoundPlaylist.SetName(dirName);
                newSoundPlaylist.SetDir(directory);
            }
        }
        SetUpButtons();
    }

    private string ReadSettings(string inputName)
    {
        string path = dir + "/settings.ini";

        StreamReader reader = new StreamReader(path);
        string readLine = reader.ReadLine();

        while (!readLine.StartsWith(inputName))
        {
            readLine = reader.ReadLine();
        }

        string[] subStrings = readLine.Split(" = ");

        return subStrings[1];
    }

    public void Silence()
    {
        foreach (Transform soundPlayer in contentPanel)
        {
            if (soundPlayer.GetComponent<LoopingSound>())
            {
                LoopingSound loopingSound = soundPlayer.GetComponent<LoopingSound>();
                loopingSound.Silence();
            }
            else if (soundPlayer.GetComponent<SoundPlaylist>())
            {
                SoundPlaylist soundPlaylist = soundPlayer.GetComponent<SoundPlaylist>();
                soundPlaylist.Silence();
            }
        }
    }

    private void SetUpButtons()
    {
        folderButton.onClick.AddListener(() =>
        {
            string itemPath = dir + "/settings.ini";

            // Get rid of forward slashes to appease explorer.exe.
            itemPath = itemPath.Replace(@"/", @"\");   

            System.Diagnostics.Process.Start("explorer.exe", "/select,"+itemPath);
        });
        plusButton.onClick.AddListener(() =>
        {
            typeDialogue.Show();
        });
    }

    public void AddSoundPlayer()
    {
        subDir = Directory.GetDirectories(dir);

        foreach (string directory in subDir)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            string dirName = di.Name;

            if (!instantiatedPlayers.Contains(dirName))
            {
                string soundType = ReadSettings(dirName);

                if (soundType == "LoopToggle")
                {
                    Transform newLoopingSoundTransform = Instantiate(loopingSoundPrefab, contentPanel.transform);
                    LoopingSound newLoopingSound = newLoopingSoundTransform.GetComponent<LoopingSound>();

                    newLoopingSound.SetName(dirName);
                    newLoopingSound.SetDir(directory);
                }
                else if (soundType == "Playlist")
                {
                    Transform newSoundPlaylistTransform = Instantiate(soundPlaylistPrefab, contentPanel.transform);
                    SoundPlaylist newSoundPlaylist = newSoundPlaylistTransform.GetComponent<SoundPlaylist>(); 

                    newSoundPlaylist.SetName(dirName);
                    newSoundPlaylist.SetDir(directory);
                }
            }
        }
    }
}
