using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WinLoseCanvasManager : MonoBehaviour {

    public Text bigText;

    public static WinLoseCanvasManager instance;

    float timeStart;
    float otherTimer;

	// Use this for initialization
	void Start ()
    {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ReadySetGo()
    {
        bigText.gameObject.SetActive(true);
        bigText.fontSize = 100;
        bigText.color = Color.blue;
        bigText.text = "Ready";

        timeStart = Time.realtimeSinceStartup;
        otherTimer = timeStart + 1;

        InvokeRepeating("CountDown", 1, 1f/60);

    }

    void CountDown()
    {
        bigText.color = Color.yellow;
        bigText.fontSize -= 1;
        bigText.text = (Mathf.CeilToInt(4 - (Time.realtimeSinceStartup - timeStart))).ToString();

        if(Time.realtimeSinceStartup - otherTimer > 1)
        {
            ResetSize();
            otherTimer = Time.realtimeSinceStartup;
        }

        if ((4 - (Time.realtimeSinceStartup - timeStart) < 0))
        {
            bigText.text = "GO!";
            Invoke("ResetCanvas", 1);
        }
    }

    void ResetSize()
    {
        bigText.fontSize = 100;
    }

    void ResetCanvas()
    {
        CancelInvoke("CountDown");
        CancelInvoke("ResetSize");
        bigText.gameObject.SetActive(false);
    }

    public void WinLose(bool win)
    {
        if (win)
        {
            bigText.color = Color.green;
            bigText.text = "You Win";
        }
        else
        {
            bigText.color = Color.red;
            bigText.text = "You Lose";

        }

        bigText.gameObject.SetActive(true);
    }

    public void CancelAll()
    {
        CancelInvoke("ResetCanvas");
        CancelInvoke("CountDown");

        bigText.gameObject.SetActive(false);
    }
}
