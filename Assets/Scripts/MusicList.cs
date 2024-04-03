using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MusicList : MonoBehaviour

{
    public static MusicList Instance { get; private set; }
    [SerializeField] private Transform contentPanel;
    [SerializeField] private Transform buttonPrefab;

    string[] dir; // Instantiates a string array for directories.

    private void Awake()
    {
        if (Instance != null) // This if check ensures that multiple instances of this object do not exist and reports it if they do, and destroys the duplicate.
        {
            Debug.LogError("There's more than one MusicList! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this; // This instantiates the instance.

        dir = Directory.GetDirectories(Application.persistentDataPath + "/music"); // Sets dir to the directories in the AppData path.
        SetUpButtons();
    }

    public string[] GetDir()
    {
        return dir;
    }

    private void SetUpButtons()
    {
        foreach (Transform button in contentPanel)
        {
            Destroy(button.gameObject);
        }
        foreach (string directory in dir)
        {
            Transform newButtonTransform = Instantiate(buttonPrefab, contentPanel.transform);
            MusicButton newButton = newButtonTransform.GetComponent<MusicButton>();

            DirectoryInfo di = new DirectoryInfo(directory);
            string dirName = di.Name;

            newButton.SetText(dirName);
            newButton.SetDir(directory);
        }
    }
}
