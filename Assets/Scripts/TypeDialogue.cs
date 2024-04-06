using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.IO;

public class TypeDialogue : MonoBehaviour
{
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button cancelButton; 
    private TMP_Dropdown dropdown;
    private TMP_InputField inputField;
    private Soundscape soundscape;

    private string dir;

    private void Awake()
    {
        dropdown = GetComponentInChildren<TMP_Dropdown>();
        inputField = GetComponentInChildren<TMP_InputField>();
        soundscape = GetComponentInParent<Soundscape>();
    }
    private void Start() 
    {
        Hide();
        cancelButton.onClick.AddListener(() =>
        {
            Hide();
        });
        acceptButton.onClick.AddListener(() =>
        {
            if (inputField.text != null && !Directory.Exists(dir + "/" + inputField.text))
            {
                if (!inputField.text.Contains("\\") && !inputField.text.Contains("/"))
                {
                    StreamWriter mainSettingsWriter = new StreamWriter(dir + "/settings.ini", true);

                    Directory.CreateDirectory(dir + "/" + inputField.text);

                    StreamWriter writer = new StreamWriter(dir + "/" + inputField.text + "/settings.ini", false);
                    
                    if (dropdown.value == 0) // Looping Sound
                    {
                        mainSettingsWriter.WriteLine(inputField.text + " = " + "LoopToggle");
                        writer.WriteLine("BaseVolume = 1");
                        writer.WriteLine("CurrentSound = ");
                        writer.WriteLine("Active = False");
                    }
                    else if (dropdown.value == 1) // Sound Playlist
                    {
                        mainSettingsWriter.WriteLine(inputField.text + " = " + "Playlist");
                        writer.WriteLine("MinTime = 0.5");
                        writer.WriteLine("MaxTime = 5");
                        writer.WriteLine("VolVar = 0.2");
                        writer.WriteLine("BaseVolume = 1");
                        writer.WriteLine("Active = False");
                    }
                    mainSettingsWriter.Close();
                    writer.Close();
                    soundscape.AddSoundPlayer();
                    Hide();
                }
            }
        });
    }
    public void Show() 
    {
        gameObject.SetActive(true);
    }

    private void Hide() 
    {
        gameObject.SetActive(false);
    }

    public void SetDir(string input)
    {
        dir = input;
    }
}