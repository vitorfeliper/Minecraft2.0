using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    public GameObject mainMenuObject;
    public GameObject settingsObject;

    [Header("Main Menu UI Elements")]

    public TextMeshProUGUI seedField;
    Player player;

    [Header("Settings Menu UI Elements")]

    public Slider viewDistanceSlider;
    public TextMeshProUGUI viewDistText;
    public Slider mouseSlider;
    public TextMeshProUGUI mouseTextSlider;
    public Toggle threadingToggle;
    public Toggle animatedChunksToggle;

    Settings settings;

    private void Awake()
    {
        if(!File.Exists(Application.dataPath + "/settings.cfg"))
        {
            settings = new Settings();
            string JsonExport = JsonUtility.ToJson(settings);
            File.WriteAllText(Application.dataPath + "/settings.cfg", JsonExport);
        }
        else
        {
            string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
            settings = JsonUtility.FromJson<Settings>(jsonImport);
        }
    }

    public void StartGame()
    {
        if(seedField.text.GetHashCode() >= 0)
        {
            VoxelData.seed = seedField.text.GetHashCode();
        }
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void EnterSettings()
    {
        viewDistanceSlider.value = settings.viewDistance;
        UpdateViewDstSlider();
        mouseSlider.value = settings.mouseSensitivity;

        UpdateMouseSlider();
        threadingToggle.isOn = settings.enableThreading;
        animatedChunksToggle.isOn = settings.enableAnimatedChunks;


        mainMenuObject.SetActive(false);
        settingsObject.SetActive(true);
    }

    public void LeaveSettings()
    {
        settings.viewDistance = (int)viewDistanceSlider.value;
        settings.mouseSensitivity = mouseSlider.value;
        settings.enableThreading = threadingToggle.isOn;
        settings.enableAnimatedChunks = animatedChunksToggle.isOn;

        string jsonExport = JsonUtility.ToJson(settings);
        //Debug.Log(jsonExport);

        File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);


        mainMenuObject.SetActive(true);
        settingsObject.SetActive(false);
    }

    public void LeaveSettings2()
    {
        settings.viewDistance = (int)viewDistanceSlider.value;
        settings.mouseSensitivity = mouseSlider.value;
        settings.enableThreading = threadingToggle.isOn;
        settings.enableAnimatedChunks = animatedChunksToggle.isOn;

        string jsonExport = JsonUtility.ToJson(settings);
        //Debug.Log(jsonExport);

        File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);

        settingsObject.SetActive(false);

    }

    public void BackMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void UpdateViewDstSlider()
    {
        viewDistText.text = "View Distance: " + viewDistanceSlider.value;
    }

    public void UpdateMouseSlider()
    {
        mouseTextSlider.text = "Sensitivity: " + mouseSlider.value.ToString("F1");
    }
}
