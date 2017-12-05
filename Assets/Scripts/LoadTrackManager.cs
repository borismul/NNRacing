using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;

public class LoadTrackManager : MonoBehaviour {

    public static LoadTrackManager instance;

    FileInfo[] trackNames;

    public Button trackLoadButtonPrefab;

    public Image trackImage;

    public LevelBuilder builder;

    public List<string> selectedTrackNames = new List<string>();

    public Text loadPanelText;

    public List<Button> currentButtons = new List<Button>();

    public bool multiplePossible;

    Image enabledImage;

    public void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void OnEnable ()
    {
        for (int i = 0; i < currentButtons.Count; i++)
            Destroy(currentButtons[i].gameObject);

        selectedTrackNames.Clear();
        currentButtons.Clear();
        trackImage.enabled = false;


        loadPanelText.color = Color.black;
        loadPanelText.text = "Select a track to load";

        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/Tracks/");
        trackNames = info.GetFiles();

        float height = 0;

        for(int i = 0; i < trackNames.Length; i++)
        {
            Button button = Instantiate(trackLoadButtonPrefab, gameObject.transform, false);
            currentButtons.Add(button);
            button.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(trackNames[i].Name);

            button.GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, button.GetComponent<RectTransform>().rect.height);

            string name = trackNames[i].Name;
            int buttonNum = i;
            button.onClick.AddListener(delegate { SelectButtonAction(Path.GetFileNameWithoutExtension(name), buttonNum); });

            height += button.GetComponent<RectTransform>().rect.height;
        }

        if(height > GetComponent<RectTransform>().rect.height)
        {
            GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, height);
        }

        GetComponentInParent<ScrollRect>().verticalScrollbar.value = 0;

    }

    void SelectButtonAction(string name, int buttonNum)
    {
        Texture2D tex = SaveableObjects.LoadTrack(name).texture;
        trackImage.enabled = true;
        trackImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, -tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        selectedTrackNames.Add(name);

        if (multiplePossible)
        {
            currentButtons[buttonNum].onClick.RemoveAllListeners();
            currentButtons[buttonNum].onClick.AddListener(delegate { DeselectAction(name, buttonNum); });
        }

        if(transform.childCount > 1)
            currentButtons[buttonNum].transform.GetChild(1).GetComponent<Image>().enabled = true;

        if (enabledImage != null && !multiplePossible)
            enabledImage.enabled = false;

        enabledImage = currentButtons[buttonNum].transform.GetChild(1).GetComponent<Image>();
    }

    void DeselectAction(string name, int buttonNum)
    {
        Texture2D tex = SaveableObjects.LoadTrack(name).texture;
        trackImage.enabled = true;
        trackImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, -tex.width, -tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        currentButtons[buttonNum].onClick.RemoveAllListeners();
        currentButtons[buttonNum].onClick.AddListener(delegate { SelectButtonAction(name, buttonNum); });

        currentButtons[buttonNum].transform.GetChild(1).GetComponent<Image>().enabled = false;

        selectedTrackNames.Remove(name);

    }

    public void Error(string error)
    {
        loadPanelText.color = Color.red;
        loadPanelText.text = error;
    }

}
