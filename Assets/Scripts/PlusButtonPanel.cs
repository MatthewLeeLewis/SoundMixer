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
            if (inputField.text != null && !Directory.Exists(Application.persistentDataPath + "/soundscapes/" + inputField.text))
            {
                if (!inputField.text.Contains("\\") && !inputField.text.Contains("/"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + "/soundscapes/" + inputField.text);
                    
                    SoundscapeList.Instance.SetUpDir();
                    SoundscapeList.Instance.SetUpButtons();
                    
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
}