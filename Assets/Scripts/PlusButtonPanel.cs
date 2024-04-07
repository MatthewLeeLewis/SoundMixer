using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.IO;

public class PlusButtonPanel : MonoBehaviour
{
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button cancelButton; 
    private TMP_InputField inputField;
    private string dir;

    private void Awake()
    {
        inputField = GetComponentInChildren<TMP_InputField>();
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
            if (inputField.text != null && !Directory.Exists(Application.persistentDataPath + dir + inputField.text))
            {
                if (!inputField.text.Contains("\\") && !inputField.text.Contains("/"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + dir + inputField.text);
                    
                    if (dir == "/soundscapes/")
                    {
                        SoundscapeList.Instance.SetUpDir();
                        SoundscapeList.Instance.SetUpButtons();
                    }
                    else if (dir == "/music/")
                    {
                        MusicList.Instance.SetUpDir();
                        MusicList.Instance.SetUpButtons();
                    }
                    
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