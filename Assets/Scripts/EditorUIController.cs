using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EditorUIController : MonoBehaviour
{
    public Button mainOptionsButton;

    public Button resetButton;
    public Button openSaveMenuButton;
    public Button loadPanelButton;
    public Button mainMenuButton;

    public Button saveTrackButton;
    public Button saveBackButton;

    public Button loadTrackButton;
    public Button loadBackButton;

    public InputField saveNameInput;

    public GameObject optionsPanel;
    public GameObject savePanel;
    public GameObject loadPanel;

    public Text saveTitleText;


    public LoadTrackManager loadTrackManager;
    TexturePainter painter;
	// Use this for initialization
	void Start ()
    {
        painter = TexturePainter.instance;
        painter.brushSize = 15;

        resetButton.onClick.AddListener(Reset);
        mainOptionsButton.onClick.AddListener(MainOptions);
        openSaveMenuButton.onClick.AddListener(SavePanel);
        loadPanelButton.onClick.AddListener(LoadPanel);
        mainMenuButton.onClick.AddListener(MainMenu);

        saveTrackButton.onClick.AddListener(SaveTrack);
        saveBackButton.onClick.AddListener(SaveBack);

        loadTrackButton.onClick.AddListener(LoadTrack);
        loadBackButton.onClick.AddListener(LoadBack);
    }

    void Reset()
    {
        painter.Reset();
    }

    void MainOptions()
    {
        if (optionsPanel.activeSelf)
            optionsPanel.SetActive(false);
        else
            optionsPanel.SetActive(true);
    }

    void SavePanel()
    {
        saveTitleText.color = Color.black;
        saveTitleText.text = "Enter a name for your track.";

        mainOptionsButton.gameObject.SetActive(false);
        optionsPanel.SetActive(false);

        savePanel.SetActive(true);

        painter.inMenu = true;
    }

    void LoadPanel()
    {
        mainMenuButton.gameObject.SetActive(false);
        optionsPanel.SetActive(false);

        loadPanel.SetActive(true);
        loadTrackManager.multiplePossible = false;

        painter.inMenu = true;

    }

    void MainMenu()
    {
        SceneManager.LoadScene("MainScene");
    }

    void SaveTrack()
    {
        string name = saveNameInput.text;
        try
        {
            if (TrackManager.trackManager.SaveTrack(name))
                SaveBack();
            else
            {
                saveTitleText.color = Color.red;
                saveTitleText.text = "Enter a correct name!";
            }

        }
        catch (System.Exception e)
        {
            print(e);
            saveTitleText.color = Color.red;
            saveTitleText.text = "Enter a correct name!";
        }
    }

    void SaveBack()
    {
        mainOptionsButton.gameObject.SetActive(true);
        optionsPanel.SetActive(true);

        savePanel.SetActive(false);

        painter.inMenu = false;
    }

    void LoadTrack()
    {
        Texture2D tex;
        if (loadTrackManager.selectedTrackNames[0] != "")
        {
            tex = SaveableObjects.LoadTrack(loadTrackManager.selectedTrackNames[0]).texture;
            tex.filterMode = FilterMode.Point;
            TrackManager.trackManager.GetComponent<Renderer>().material.SetTexture("_MainTex", (Texture)tex);
        }
        else
            loadTrackManager.Error("First select a track!");

        LoadBack();
        MainOptions();

    }

    void LoadBack()
    {
        mainMenuButton.gameObject.SetActive(true);
        optionsPanel.SetActive(true);

        loadPanel.SetActive(false);

        painter.inMenu = false;

    }



}
