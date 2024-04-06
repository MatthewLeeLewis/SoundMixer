using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SoundscapeList : MonoBehaviour

{
    public static SoundscapeList Instance { get; private set; }
    [SerializeField] private Transform contentPanel;
    [SerializeField] private Transform buttonPrefab;
    private List<string> instantiatedButtons = new List<string>();
    [SerializeField] private Button plusButton;
    [SerializeField] private Button refreshButton;
    private PlusButtonPanel plusButtonPanel;

    private List<string> dir = new List<string>(); // Instantiates a string array for directories.

    private void Awake()
    {
        if (Instance != null) // This if check ensures that multiple instances of this object do not exist and reports it if they do, and destroys the duplicate.
        {
            Debug.LogError("There's more than one SoundscapeList! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this; // This instantiates the instance.

        plusButtonPanel = GetComponentInChildren<PlusButtonPanel>();

        SetUpDir(); // Sets dir to the directories in the AppData path.
        SetUpButtons();
        SetUpMiscButtons();
    }

    public void SetUpDir()
    {
        dir.Clear();
        string[] dirArray = Directory.GetDirectories(Application.persistentDataPath + "/soundscapes");
        foreach (string directory in dirArray)
        {
            dir.Add(directory);
        }
    }

    public void SetUpButtons()
    {
        foreach (Transform button in contentPanel)
        {
            SoundscapeButton soundscapeButton = button.GetComponent<SoundscapeButton>();
            
            if (!dir.Contains(soundscapeButton.GetDir()))
            {
                instantiatedButtons.Remove(soundscapeButton.GetName());
                Destroy(button.gameObject);
                Destroy(button);
            }
        }
        foreach (string directory in dir)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            string dirName = di.Name;

            if (!instantiatedButtons.Contains(di.Name))
            {
                Transform newButtonTransform = Instantiate(buttonPrefab, contentPanel.transform);
                SoundscapeButton newButton = newButtonTransform.GetComponent<SoundscapeButton>();

                newButton.SetName(dirName);
                newButton.SetDir(directory);
                instantiatedButtons.Add(di.Name);
            } 
        }
    }  

    public void SetToggles(List<string> activeSoundscapes)
    {
        foreach (Transform button in contentPanel)
        {
            SoundscapeButton currentButton = button.GetComponent<SoundscapeButton>();
            if (activeSoundscapes.Contains(currentButton.GetName()))
            {
                currentButton.SetToggle(true);
            }
            else
            {
                currentButton.SetToggle(false);
            }
        }
    } 

    private void SetUpMiscButtons()
    {
        plusButton.onClick.AddListener(() =>
        {
            plusButtonPanel.Show();
        });
        refreshButton.onClick.AddListener(() =>
        {
            
        });
    } 
}
