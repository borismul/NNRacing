using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RacingCanvasController : MonoBehaviour {

    public GameObject raceCanvas;
    public Text currentLap;
    public Text totalLaps;

    public Text totalTime;

    public GameObject positionPanel;
    public Text positionText;
    public Text totalCompetitors;

    public Text speed;

    public Text countDownText;

    public GameObject finishCanvas;

    public GameObject positionPanelFinish;
    public Text positionTextFinish;
    public Text totalCompetitorsFinish;
    public Button backButton;
    public Button retryButton;
    public Button backButtonImediately;

    float startFontSize = 80;

    void Start()
    {
        retryButton.onClick.AddListener(RetryButton);
        backButton.onClick.AddListener(BackButton);
        backButtonImediately.onClick.AddListener(BackButton);
    }

    public void OnEnable()
    {
        countDownText.gameObject.SetActive(false);
        countDownText.fontSize = (int)startFontSize;

    }

    public void UpdateCanvas(CarController car, float time)
    {
        raceCanvas.SetActive(true);
        finishCanvas.SetActive(false);

        currentLap.text = car.GetCurrentLap().ToString();
        totalLaps.text = "/" + GA_Parameters.laps.ToString();

        int timeM = (int)(time / 60);
        int timeS = (int)(time - timeM * 60);
        int timeMS = (int)((time - timeM * 60 - timeS) * 100);

        totalTime.text = timeM.ToString("D2") + ":" + timeS.ToString("D2") + ":" + timeMS.ToString("D2");

        if (car.GetCompetitors() > 1)
        {
            positionPanel.SetActive(true);
            positionText.text = car.GetPosition().ToString();
            totalCompetitors.text = "/" + car.GetCompetitors().ToString();
        }
        else
            positionPanel.SetActive(false);

        speed.text = Mathf.RoundToInt(car.GetCurrentVelocity()).ToString();

    }

    public void FinishedCanvas(CarController car)
    {
        if (!raceCanvas.activeSelf)
            return;

        raceCanvas.SetActive(false);
        finishCanvas.SetActive(true);

        if (car.GetCompetitors() > 1)
        {
            positionPanelFinish.SetActive(true);
            positionTextFinish.text = car.GetPosition().ToString();
            totalCompetitorsFinish.text = "/" + car.GetCompetitors().ToString();
        }
        else
            positionPanelFinish.SetActive(false);

    }

    void RetryButton()
    {
        countDownText.gameObject.SetActive(true);
        UIController.instance.Challenge();
    }

    void BackButton()
    {
        CameraController.instance.menuCamera.SetActive(true);
        CameraController.instance.gameObject.SetActive(false);
        gameObject.SetActive(false);
        UIController.instance.ResetPlay();
        RaceManager.raceManager.SetViewSettings(RaceManager.ViewType.MenuView, true);
    }

    public IEnumerator CountDown()
    {
        countDownText.gameObject.SetActive(true);
        float fontsize = countDownText.fontSize;
        
        float intCountDown = 0;

        while (3 - intCountDown > 0)
        {
            countDownText.text = (3 - intCountDown).ToString();
            fontsize -= startFontSize * Time.deltaTime;

            if (fontsize <= 0)
            {
                fontsize = startFontSize;
                intCountDown++;
                countDownText.text = (3 - intCountDown).ToString();
            }

            countDownText.fontSize = (int)fontsize;
            totalTime.text = 0.ToString("D2") + ":" + 0.ToString("D2") + ":" + 0.ToString("D2");

            yield return null;
        }


        if (3 - intCountDown < 1)
            countDownText.text = "Go!";

        countDownText.fontSize = (int)startFontSize;

        UIController.instance.activeRoutines.Add(StartCoroutine(DeactivateCountDown()));
    }

    IEnumerator DeactivateCountDown()
    {
        float startTime = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup - startTime < 1f)
            yield return null;


        countDownText.gameObject.SetActive(false);

    }

}
