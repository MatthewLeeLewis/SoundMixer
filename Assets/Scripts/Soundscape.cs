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
    private string dir;
    string[] subDir;

    public void SetName(string name)
    {
        soundscapeName.text = name;
    }

    public void SetDir(string inputDir)
    {
        dir = inputDir;
        subDir = Directory.GetDirectories(dir);
        SetUp();
    }

    public void SetUp()
    {
        foreach (Transform soundPlayer in contentPanel)
        {
            Destroy(soundPlayer.gameObject);
        }
        foreach (string directory in subDir)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            string dirName = di.Name;

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

}
