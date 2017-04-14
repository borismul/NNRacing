using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NetworkGridController : MonoBehaviour
{

    public GameObject childPrefab;

    int populationSize;

    // Use this for initialization
    void Start()
    {
        populationSize = GA_Parameters.populationSize;
        RectTransform thisRect = GetComponent<RectTransform>();
        thisRect.sizeDelta = new Vector2(thisRect.rect.width, (childPrefab.GetComponent<RectTransform>().rect.height + GetComponent<VerticalLayoutGroup>().spacing) * populationSize);
        thisRect.anchoredPosition = Vector2.zero;
        for (int i = 0; i < Mathf.FloorToInt(populationSize * GA_Parameters.savePercentage/100); i++)
        {
            GameObject child = (GameObject)Instantiate(childPrefab, this.transform, false);
            child.GetComponentInChildren<Text>().text = (i + 1).ToString();
            child.GetComponent<NNTextController>().i = i;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
