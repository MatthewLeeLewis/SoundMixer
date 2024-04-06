using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class MusicButton : MonoBehaviour
{
    public static event EventHandler OnMusicChanged;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Toggle toggle;
    private string dir;
    private string musicButton_Name = "";

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            MusicList.Instance.SetActiveMusic(dir);
            MusicPlayer.Instance.SetDirectory(dir);
            OnMusicChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    private void Start()
    {
        MusicPlayer.DisableMusicButtons += MusicPlayer_DisableMusicButtons;
    }

    public void SetName(string text)
    {
        musicButton_Name = text;
        textMeshPro.text = musicButton_Name;
    }

    public string GetName()
    {
        return musicButton_Name;
    }

    public void SetDir(string inputDir)
    {
        dir = inputDir;
    }

    public string GetDir()
    {
        return dir;
    }

    public void SetToggle(bool input)
    {
        toggle.isOn = input;
    }

    private void MusicPlayer_DisableMusicButtons(object sender, EventArgs e)
    {
        button.interactable = false;
        Invoke("EnableMusicButtons", 3);
    }

    private void EnableMusicButtons()
    {
        button.interactable = true;
    }
}
