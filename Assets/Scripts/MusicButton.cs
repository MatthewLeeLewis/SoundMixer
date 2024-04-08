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
    [SerializeField] private Button deleteButton;

    private void Awake()
    {
        deleteButton.gameObject.SetActive(false);
        button.onClick.AddListener(() =>
        {
            MusicList.Instance.SetActiveMusic(dir);
            MusicPlayer.Instance.SetDirectory(dir);
            OnMusicChanged?.Invoke(this, EventArgs.Empty);
        });
        deleteButton.onClick.AddListener(() =>
        {
            if (MusicList.Instance.GetActiveMusic() == dir)
            {
                MusicList.Instance.SetActiveMusic("None");
            }
            Destroy(this.gameObject);
            Destroy(this);
        });
    }

    private void Start()
    {
        MusicPlayer.DisableMusicButtons += MusicPlayer_DisableMusicButtons;
        ActiveSoundscapes.Instance.ToggleDeleteMode += ActiveSoundscapes_ToggleDeleteMode;
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
        MusicPlayer.DisableMusicButtons -= MusicPlayer_DisableMusicButtons;
    }
}
