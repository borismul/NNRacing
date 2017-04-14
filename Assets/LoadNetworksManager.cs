using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;


public class LoadNetworksManager : MonoBehaviour {


    FileInfo[] annNames;

    public Button annLoadButtonPrefab;

    public GameObject NetworkPanel;
    public GameObject perceptron;
    public GameObject line;
    public GameObject loopLink;

    public Text multipleSelected;

    public string selectedNetworkName;

    public List<NeuralNetwork> currentNetworks = new List<NeuralNetwork>();

    public Text loadPanelText;

    public List<Button> currentButtons = new List<Button>();

    public List<Button> activeButtons = new List<Button>();


    // Use this for initialization
    void OnEnable()
    {
        for (int i = 0; i < currentButtons.Count; i++)
            Destroy(currentButtons[i].gameObject);

        currentButtons.Clear();
        activeButtons.Clear();

        loadPanelText.color = Color.black;
        loadPanelText.text = "Select a artificial neural network to challenge. \n (Press control to select multiple)";

        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/Artificial Neural Networks/");
        annNames = info.GetFiles();

        float height = 0;

        for (int i = 0; i < annNames.Length; i++)
        {
            Button button = Instantiate(annLoadButtonPrefab, gameObject.transform, false);
            currentButtons.Add(button);
            button.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(annNames[i].Name);

            button.GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, button.GetComponent<RectTransform>().rect.height);
            button.GetComponent<RectTransform>().localScale = Vector3.one;

            string name = annNames[i].Name;
            int index = i;
            button.onClick.AddListener(delegate { SetButtonAction(Path.GetFileNameWithoutExtension(name), index); });

            height += button.GetComponent<RectTransform>().rect.height;
        }

        if (height > GetComponent<RectTransform>().rect.height)
        {
            GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, height);
        }

        GetComponentInParent<ScrollRect>().verticalScrollbar.value = 0;

    }

    void SetButtonAction(string name, int index)
    {
        if (currentNetworks.Count == 1)
            currentNetworks[0].DestroyNetwork();
        
        Genome genome = SaveableObjects.SaveableNeuralNetwork.LoadNetwork(name);

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (activeButtons.Contains(currentButtons[index]))
            {
                int activeIndex = activeButtons.IndexOf(currentButtons[index]);
                activeButtons.RemoveAt(activeIndex);
                currentButtons[index].GetComponent<Image>().color = Color.white;
                currentNetworks.RemoveAt(activeIndex);
            }
            else
            {
                activeButtons.Add(currentButtons[index]);
                currentButtons[index].GetComponent<Image>().color = Color.grey;
                currentNetworks.Add(genome.CreateNetwork());
            }
        }
        else
        {
            for (int i = 0; i < activeButtons.Count; i++)
            {
                activeButtons[i].GetComponent<Image>().color = Color.white;
            }

            if (activeButtons.Contains(currentButtons[index]) && currentNetworks.Count == 1)
            {
                int activeIndex = activeButtons.IndexOf(currentButtons[index]);
                activeButtons.RemoveAt(activeIndex);
                currentNetworks.RemoveAt(activeIndex);

                currentButtons[index].GetComponent<Image>().color = Color.white;
            }
            else
            {
                currentButtons[index].GetComponent<Image>().color = Color.grey;
                activeButtons.Clear();
                currentNetworks.Clear();
                currentNetworks.Add(genome.CreateNetwork());
                activeButtons.Add(currentButtons[index]);
            }
        }

        if(currentNetworks.Count == 1)
            currentNetworks[0].VisualizeNetwork(NetworkPanel.GetComponent<RectTransform>(), perceptron, line, loopLink, false);

        selectedNetworkName = name;

        if (currentNetworks.Count > 1)
        {
            multipleSelected.enabled = true;
            multipleSelected.text = currentNetworks.Count.ToString() + " Networks Selected.";
        }
        else
            multipleSelected.enabled = false;


    }

    public void Error(string error)
    {
        loadPanelText.color = Color.red;
        loadPanelText.text = error;
    }

}
