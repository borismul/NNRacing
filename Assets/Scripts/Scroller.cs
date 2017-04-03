using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scroller : MonoBehaviour, IScrollHandler
{

    public void OnScroll(PointerEventData data)
    {
        ScrollRect scrollRect;
        scrollRect = GetComponent<ScrollRect>();

        if (scrollRect == null)
            GetComponentInParent<ScrollRect>().verticalScrollbar.value += (float)data.scrollDelta.y / 50;
    }
}
