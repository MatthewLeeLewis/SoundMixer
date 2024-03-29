using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class SoundscapeButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    private string dir;

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            MusicPlayer.Instance.SetDirectory(dir);
        });
    }

    public void SetText(string text)
    {
        textMeshPro.text = text;
    }

    public void SetDir(string inputDir)
    {
        dir = inputDir;
    }

    private string GetDir()
    {
        return dir;
    }
}
