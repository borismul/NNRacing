using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;

public class LoadTrackManager : MonoBehaviour {

    FileInfo[] trackNames;

    public Button trackLoadButtonPrefab;

    public Image trackImage;

    public LevelBuilder builder;

    public string selectedTrackName;

    public Text loadPanelText;

    public List<Button> currentButtons = new List<Button>();

	// Use this for initialization
	void OnEnable ()
    {
        for (int i = 0; i < currentButtons.Count; i++)
            Destroy(currentButtons[i].gameObject);

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
            button.onClick.AddListener(delegate { SetButtonAction(Path.GetFileNameWithoutExtension(name)); });

            height += button.GetComponent<RectTransform>().rect.height;
        }

        if(height > GetComponent<RectTransform>().rect.height)
        {
            GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, height);
        }

        GetComponentInParent<ScrollRect>().verticalScrollbar.value = 0;

    }

    void SetButtonAction(string name)
    {
        Texture2D tex = SaveableObjects.LoadTrack(name).texture;
        trackImage.enabled = true;
        trackImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        selectedTrackName = name;
    }

    public void Error(string error)
    {
        loadPanelText.color = Color.red;
        loadPanelText.text = error;
    }

}
