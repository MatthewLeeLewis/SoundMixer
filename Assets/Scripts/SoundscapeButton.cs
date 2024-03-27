using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class SoundscapeButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;
    private string dir;

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
