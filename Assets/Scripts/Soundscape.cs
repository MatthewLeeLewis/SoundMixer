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
    [SerializeField] private Transform presetConfirmPanel;
    [SerializeField] private TMP_InputField presetNameInputField;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button deletePresetButton;
    [SerializeField] private Button savePresetButton;
    [SerializeField] private TMP_Dropdown presetDropdown;
    private TypeDialogue typeDialogue;
    private List<string> instantiatedPlayers = new List<string>();
    private string dir;
    List<string> subDir = new List<string>();

    private void Awake()
    {
        typeDialogue = GetComponentInChildren<TypeDialogue>();
        presetConfirmPanel.gameObject.SetActive(false);
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
        string[] subDirArray = Directory.GetDirectories(dir);
        foreach (string directory in subDirArray)
        {
            subDir.Add(directory);
        }
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
                newLoopingSound.SetParentDir(dir);
                newLoopingSound.SetDir(directory);
            }
            else if (soundType == "Playlist")
            {
                Transform newSoundPlaylistTransform = Instantiate(soundPlaylistPrefab, contentPanel.transform);
                SoundPlaylist newSoundPlaylist = newSoundPlaylistTransform.GetComponent<SoundPlaylist>(); 

                newSoundPlaylist.SetName(dirName);
                newSoundPlaylist.SetParentDir(dir);
                newSoundPlaylist.SetDir(directory);
            }
        }
        SetUpButtons();
        ReadPresets();
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

        reader.Close();
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
        savePresetButton.onClick.AddListener(() =>
        {
            presetConfirmPanel.gameObject.SetActive(true);
        });
        deletePresetButton.onClick.AddListener(() =>
        {
            if (presetDropdown.value != 0)
            {
                string preset = presetDropdown.options[presetDropdown.value].text;
                FileInfo presetFile = new FileInfo(dir + "/" + preset);
                Debug.Log(presetFile.ToString());
                presetFile.Delete();
                presetDropdown.ClearOptions();

                DirectoryInfo di = new DirectoryInfo(dir);
                FileInfo[] files = di.GetFiles();

                List<string> filenames = new List<string>();
                filenames.Add("None");

                foreach (FileInfo file in files)
                {
                    string fileString = file.ToString();
                    if (fileString.EndsWith(".ini") && !fileString.EndsWith("settings.ini"))
                    {   
                        filenames.Add(file.Name);
                    }
                }
                presetDropdown.AddOptions(filenames);  
            }  
        });
        acceptButton.onClick.AddListener(() =>
        {
            if (presetNameInputField.text != null && !presetNameInputField.text.Contains("settings"))
            {
                if (!presetNameInputField.text.Contains("\\") && !presetNameInputField.text.Contains("/"))
                {
                    StreamWriter writer = new StreamWriter(dir + "/" + presetNameInputField.text + ".ini", false);
                    foreach (string directory in subDir)
                    {
                        writer.WriteLine(directory);
                        writer.WriteLine("--------------");

                        StreamReader reader = new StreamReader(directory + "/settings.ini");
                        List<string> linesList = new List<string>();

                        while (reader.Peek() >= 0)
                        {
                            linesList.Add(reader.ReadLine());
                        }

                        reader.Close();

                        foreach (string line in linesList)
                        {
                            writer.WriteLine(line);
                        }

                        writer.WriteLine("");
                    }
                    writer.Close();

                    List<string> newAdd = new List<string>();
                    newAdd.Add(presetNameInputField.text);

                    presetDropdown.AddOptions(newAdd);

                    presetConfirmPanel.gameObject.SetActive(false);
                }
            }
        });
        cancelButton.onClick.AddListener(() =>
        {
            presetConfirmPanel.gameObject.SetActive(false);
        });
        presetDropdown.onValueChanged.AddListener(delegate {
            SetPreset(presetDropdown.options[presetDropdown.value].text);
        });

    }

    public void AddSoundPlayer()
    {
        string[] subDirArray = Directory.GetDirectories(dir);
        foreach (string directory in subDirArray)
        {
            if (!subDir.Contains(directory))
            {
                subDir.Add(directory);
            }
        }

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
                    newLoopingSound.SetParentDir(dir);
                    newLoopingSound.SetDir(directory);
                }
                else if (soundType == "Playlist")
                {
                    Transform newSoundPlaylistTransform = Instantiate(soundPlaylistPrefab, contentPanel.transform);
                    SoundPlaylist newSoundPlaylist = newSoundPlaylistTransform.GetComponent<SoundPlaylist>(); 

                    newSoundPlaylist.SetName(dirName);
                    newSoundPlaylist.SetParentDir(dir);
                    newSoundPlaylist.SetDir(directory);
                }
                instantiatedPlayers.Add(dirName);
            }
        }
    }

    private void ReadPresets()
    {
        DirectoryInfo di = new DirectoryInfo(dir);
        FileInfo[] files = di.GetFiles();

        List<string> filenames = new List<string>();

        foreach (FileInfo file in files)
        {
            string fileString = file.ToString();
            if (fileString.EndsWith(".ini") && !fileString.EndsWith("settings.ini"))
            {
                filenames.Add(file.Name);
            }
        }
        presetDropdown.AddOptions(filenames);
    }

    private void SetPreset(string preset)
    {
        StreamReader reader = new StreamReader(dir + "/" + preset);

        List<string> linesList = new List<string>();
        while (reader.Peek() >= 0)
        {
            linesList.Add(reader.ReadLine());
        }

        reader.Close();

        for (int i = 0; i < linesList.Count; i++)
        {
            if (subDir.Contains(linesList[i]))
            {
                LoopingSound foundLoop = null;
                SoundPlaylist foundPlaylist = null;

                foreach (Transform child in contentPanel)
                {
                    if (child.gameObject.TryGetComponent<LoopingSound>(out LoopingSound testLoop))
                    {
                        if (testLoop.GetDir() == linesList[i])
                        {
                            foundLoop = testLoop;
                            break;
                        }
                    }
                    else if (child.gameObject.TryGetComponent<SoundPlaylist>(out SoundPlaylist testPlaylist))
                    {
                        if (testPlaylist.GetDir() == linesList[i])
                        {
                            foundPlaylist = testPlaylist;
                            break;
                        }
                    }
                }

                int j = i + 2;
                while (linesList[j] != "")
                {
                    if (linesList[j].StartsWith("BaseVolume = "))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        float baseVolume = float.Parse(subStrings[1]);

                        if (foundLoop != null)
                        {
                            foundLoop.SetBaseVolume(baseVolume);
                        }
                        else if (foundPlaylist != null)
                        {
                            foundPlaylist.SetBaseVolume(baseVolume);
                        }
                    }
                    else if (linesList[j].StartsWith("Pitch = "))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        float pitch = float.Parse(subStrings[1]);

                        if (foundLoop != null)
                        {
                            foundLoop.SetPitch(pitch);
                        }
                    }
                    else if (linesList[j].StartsWith("Panning"))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        float panning = float.Parse(subStrings[1]);

                        if (foundLoop != null)
                        {
                            foundLoop.SetPanning(panning);
                        }
                    }
                    else if (linesList[j].StartsWith("CurrentSound"))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        string currentSound = subStrings[1];

                        if (foundLoop != null)
                        {
                            foundLoop.SetCurrentSound(currentSound);
                        }
                    }
                    else if (linesList[j].StartsWith("AutoMode"))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        bool autoMode = Convert.ToBoolean(subStrings[1]);

                        if (foundLoop != null)
                        {
                            foundLoop.SetAutoMode(autoMode);
                        }
                    }
                    else if (linesList[j].StartsWith("VolVar"))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        float volVar = float.Parse(subStrings[1]);

                        if (foundLoop != null)
                        {
                            foundLoop.SetVolVar(volVar);
                        }
                        else if (foundPlaylist != null)
                        {
                            foundPlaylist.SetVolVar(volVar);
                        }
                    }
                    else if (linesList[j].StartsWith("PanVar"))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        float panVar = float.Parse(subStrings[1]);

                        if (foundLoop != null)
                        {
                            foundLoop.SetPanVar(panVar);
                        }
                        else if (foundPlaylist != null)
                        {
                            foundPlaylist.SetPanVar(panVar);
                        }
                    }
                    else if (linesList[j].StartsWith("PitchVar"))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        float pitchVar = float.Parse(subStrings[1]);

                        if (foundLoop != null)
                        {
                            foundLoop.SetPitchVar(pitchVar);
                        }
                        else if (foundPlaylist != null)
                        {
                            foundPlaylist.SetPitchVar(pitchVar);
                        }
                    }
                    else if (linesList[j].StartsWith("MinTime"))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        float minTime = float.Parse(subStrings[1]);

                        if (foundLoop != null)
                        {
                            foundLoop.SetMinTime(minTime);
                        }
                        else if (foundPlaylist != null)
                        {
                            foundPlaylist.SetMinTime(minTime);
                        }
                    }
                    else if (linesList[j].StartsWith("MaxTime"))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        float maxTime = float.Parse(subStrings[1]);

                        if (foundLoop != null)
                        {
                            foundLoop.SetMaxTime(maxTime);
                        }
                        else if (foundPlaylist != null)
                        {
                            foundPlaylist.SetMaxTime(maxTime);
                        }
                    }
                    else if (linesList[j].StartsWith("Overlap"))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        bool overlap = Convert.ToBoolean(subStrings[1]);

                        if (foundPlaylist != null)
                        {
                            foundPlaylist.SetOverlap(overlap);
                        }
                    }
                    else if (linesList[j].StartsWith("Active"))
                    {
                        string[] subStrings = linesList[j].Split(" = ");
                        bool active = Convert.ToBoolean(subStrings[1]);

                        if (foundLoop != null)
                        {
                            foundLoop.SetActive(active);
                        }
                        else if (foundPlaylist != null)
                        {
                            foundPlaylist.SetActive(active);
                        }
                    }

                    j++;
                }

                if (foundLoop != null)
                {
                    foundLoop.WriteSettings();
                }
                else if (foundPlaylist != null)
                {
                    foundPlaylist.WriteSettings();
                }
            }
        }
    }
}
