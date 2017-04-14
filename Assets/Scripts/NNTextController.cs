using UnityEngine;
using System.Collections.Generic;
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

    static List<Image> activeImage;

    void OnEnable()
    {
        activeImage = new List<Image>();
        isActive = false;
        GetComponent<Image>().color = normalColor;
    }

    void OnDisable()
    {
        if (isActive)
            UIController.instance.DisableNetwork(i, true);
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (!isActive)
        {
            isActive = true;
            GetComponent<Image>().color = clickedMouseover;


            if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                if (activeImage != null)
                {
                    for (int i = 0; i < activeImage.Count; i++)
                    {
                        activeImage[i].color = normalColor;
                        activeImage[i].GetComponent<NNTextController>().isActive = false;
                        activeImage.RemoveAt(i);
                        i--;
                    }
                }
            }

            activeImage.Add(GetComponent<Image>());
            if (activeImage.Count == 1)
                UIController.instance.LoadNetwork(activeImage[0].GetComponent<NNTextController>().i);
            else
                UIController.instance.ActivateMultipleSelected(i);
        }
        else
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                for (int i = 0; i < activeImage.Count; i++)
                {
                    if (activeImage[i].Equals(GetComponent<Image>()))
                    {
                        activeImage.RemoveAt(i);
                        break;
                    }
                }

                isActive = false;
                GetComponent<Image>().color = mouseOverColor;

                if (activeImage.Count == 0)
                    UIController.instance.DisableNetwork(i, true);
                else if (activeImage.Count == 1)
                    UIController.instance.LoadNetwork(activeImage[0].GetComponent<NNTextController>().i);
                else
                    UIController.instance.DeactivateMultipleSelected(i);
            }
            else
            {
                for (int i = 0; i < activeImage.Count; i++)
                {
                    if (!activeImage[i].Equals(GetComponent<Image>()))
                    {
                        activeImage[i].GetComponent<NNTextController>().isActive = false;
                        activeImage[i].GetComponent<Image>().color = normalColor;
                        activeImage.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        UIController.instance.LoadNetwork(i);
                    }
                }
            }

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
        if (!isActive)
            GetComponent<Image>().color = normalColor;
        else
            GetComponent<Image>().color = clickedColor;

    }


}
