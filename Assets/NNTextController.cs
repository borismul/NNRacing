using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NNTextController : MonoBehaviour, IPointerClickHandler
{
    public Color clickedColor;
    public Color clickedMouseover;
    public Color mouseOverColor;
    public Color normalColor;
    public int i;

    public bool isActive = false;

    static Image activeImage;

    public int network;

    void OnEnable()
    {
        isActive = false;
        activeImage = null;
        GetComponent<Image>().color = normalColor;
    }

    void OnDisable()
    {
        if(isActive)
            UIController.instance.DisableNetwork(i, true);
    }

    public void OnPointerClick(PointerEventData data)
    {
        
        if (!isActive)
        {
            isActive = true;
            GetComponent<Image>().color = clickedMouseover;

            if (activeImage != null)
            {
                activeImage.color = normalColor;
                activeImage.GetComponent<NNTextController>().isActive = false;
            }
            activeImage = GetComponent<Image>();

            UIController.instance.LoadNetwork(i);
        }
        else
        {
            isActive = false;
            activeImage = GetComponent<Image>();
            activeImage.color = mouseOverColor;
            activeImage = null;

            UIController.instance.DisableNetwork(i, true);
        }
    }

    void Start()
    {
        GetComponent<Image>().color = normalColor;
    }

    public void OnPointerEnter()
    {
        if (!isActive)
            GetComponent<Image>().color = mouseOverColor;
        else
            GetComponent<Image>().color = clickedMouseover;
    }

    public void OnPointerExit()
    {
        if(!isActive)
            GetComponent<Image>().color = normalColor;
        else
            GetComponent<Image>().color = clickedColor;

    }


}
