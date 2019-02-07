using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RacingCanvasController : MonoBehaviour {

    public static RacingCanvasController racingCanvas;

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

    public GameObject pauseMenu;
    public Button continueButton;
    public Button restartButton;
    public Button optionsButton;
    public Button exitButton;

    public GameObject optionsMenu;
    public Button optionsApplyButton;
    public Dropdown resolutionDropdown;
    public Slider antiAliasingSlider;
    public Text antiAliasingText;
    public Slider shadowDistSlider;
    public Text shadowDistText;
    public Slider shadowQualitySlider;
    public Text ShadowQualityText;
    public Slider detailDistSlider;
    public Text detailDistText;

    public UIController uiController;
    public LoadTrackManagerDuringTraining loadTrackManager;

    public Text splitText;

    public bool wasChallenging;

    float startFontSize = 80;

    public static bool toTrain = true;

    bool paused = false;

    public Text fitnessText;

    void Awake()
    {
        racingCanvas = this;
        InitializeMenus();
        
    }

    void InitializeMenus()
    {
        retryButton.onClick.AddListener(RetryButton);
        restartButton.onClick.AddListener(RetryButton);
        continueButton.onClick.AddListener(UnPause);
        optionsButton.onClick.AddListener(Options);

        OptionsMenu();

    }

    void OptionsMenu()
    {
        optionsApplyButton.onClick.AddListener(OptionsApply);

        antiAliasingSlider.onValueChanged.AddListener(AntiAliasChange);
        int val = PlayerPrefs.GetInt("anti-aliasing");

        if (val == 0)
            val = 3;

        antiAliasingSlider.value = val - 1;

        AntiAliasing(val - 1);
        AntiAliasChange(val - 1);

        shadowDistSlider.onValueChanged.AddListener(ShadowDistChange);

        val = PlayerPrefs.GetInt("shadowDist");

        if (val == 0)
            val = 3;

        shadowDistSlider.value = val - 1;

        ShadowDistance(val - 1);
        ShadowDistChange(val - 1);

        shadowQualitySlider.onValueChanged.AddListener(ShadowQualityChange);

        val = PlayerPrefs.GetInt("shadowQuality");

        if (val == 0)
            val = 3;

        shadowQualitySlider.value = val - 1;

        ShadowDetail(val - 1);
        ShadowQualityChange(val - 1);

        detailDistSlider.onValueChanged.AddListener(DetailDistanceChange);

        val = PlayerPrefs.GetInt("detailDist");

        if (val == 0)
            val = 3;

        detailDistSlider.value = val - 1;

        DetailDistance(val - 1);
        DetailDistance(val - 1);
    }

    public void OnEnable()
    {
        countDownText.gameObject.SetActive(false);
        countDownText.fontSize = (int)startFontSize;

        if (toTrain)
            SetToTrain();
        else
            SetToMain();

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !paused)
            Pause();

        else if(Input.GetKeyDown(KeyCode.Escape) && paused)
            UnPause();
        
    }

    public void UpdateCanvas(CarController car, float time, float fitness)
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

        if (car.aIPlayer == null)
            fitnessText.text = fitness.ToString("0.00");

    }

    public void FinishedCanvas(CarController car)
    {
        finishCanvas.SetActive(true);
        raceCanvas.SetActive(false);
        if (car == null)
        {
            positionPanelFinish.SetActive(false);
            return;
        }

        if (car.GetCompetitors() > 1)
        {
            positionPanelFinish.SetActive(true);
            positionTextFinish.text = car.GetPosition().ToString();
            totalCompetitorsFinish.text = "/" + car.GetCompetitors().ToString();
        }
        else
            positionPanelFinish.SetActive(false);


    }

    void BackToMain()
    {
        UnPause();
        SceneManager.LoadScene("MainScene");
    }

    void BackToTrain()
    {
        UnPause();
        CameraController.instance.menuCamera.SetActive(true);
        CameraController.instance.gameObject.SetActive(false);
        gameObject.SetActive(false);
        UIController.instance.ResetPlay();
        TrackManager.DeleteTrack();
        RaceManager.raceManager.SetViewSettings(RaceManager.ViewType.MenuView, true);
    }

    void RetryButton()
    {
        UnPause();
        RaceManager.raceManager.runRace = false;
        if (wasChallenging)
            uiController.Challenge(loadTrackManager.selectedTrackNames);
        else
            uiController.Play(loadTrackManager.selectedTrackNames);

        pauseMenu.SetActive(false);

    }

    public void SetToMain()
    {
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(BackToMain);

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(BackToMain);
        exitButton.GetComponentInChildren<Text>().text = "Main Menu";
    }

    public void SetToTrain()
    {
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(BackToTrain);

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(BackToTrain);
        exitButton.GetComponentInChildren<Text>().text = "Back";
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

    void Pause()
    {
        paused = true;

        pauseMenu.SetActive(true);
        RaceManager.raceManager.Pause();
    }

    void UnPause()
    {
        optionsMenu.SetActive(false);
        paused = false;

        pauseMenu.SetActive(false);
        RaceManager.raceManager.UnPause();
    }

    void Options()
    {
        optionsMenu.SetActive(true);
        pauseMenu.SetActive(false);
    }

    void OptionsApply()
    {
        optionsMenu.SetActive(false);
        pauseMenu.SetActive(true);

        AntiAliasing((int)antiAliasingSlider.value);
        ShadowDistance((int)shadowDistSlider.value);
        ShadowDetail((int)shadowQualitySlider.value);
        DetailDistance((int)detailDistSlider.value);
        Resolution(resolutionDropdown.value);
    }

    void AntiAliasing(int i)
    {
        QualitySettings.antiAliasing = i;
        PlayerPrefs.SetInt("anti-aliasing", i + 1);

    }

    void ShadowDistance(int i)
    {
        if (i == 0)
            QualitySettings.shadowDistance = 20;
        else if (i == 1)
            QualitySettings.shadowDistance = 50;
        else if (i == 2)
            QualitySettings.shadowDistance = 100;
        else if (i == 3)
            QualitySettings.shadowDistance = 300;

        PlayerPrefs.SetInt("shadowDist", i + 1);

    }

    void ShadowDetail(int i)
    {
        if (i == 0)
        {
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.shadowCascades = 0;
            QualitySettings.shadows = ShadowQuality.Disable;
        }
        else if (i == 1)
        {
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowCascades = 0;
            QualitySettings.shadows = ShadowQuality.HardOnly;
        }
        else if (i == 2)
        {
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowCascades = 2;
            QualitySettings.shadows = ShadowQuality.All;
        }
        else if (i == 3)
        {
            {
                QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                QualitySettings.shadowCascades = 4;
                QualitySettings.shadows = ShadowQuality.All;
            }
        }

        PlayerPrefs.SetInt("shadowQuality", i + 1);

    }

    void DetailDistance(int i)
    {
        QualitySettings.lodBias = (i*1.3f + 1);

        PlayerPrefs.SetInt("detailDist", i + 1);
    }

    void AntiAliasChange(float i)
    {
        List<string> options = new List<string>() { "None", "2X Multi Sampling", "4X Multi Sampling", "8X Multi Sampling" };

        int index = (int)i;

        antiAliasingText.text = options[index];
    }

    void ShadowDistChange(float i)
    {
        List<string> options = new List<string>() { "Low", "Medium", "High", "Very High" };

        int index = (int)i;

        shadowDistText.text = options[index];
    }

    void ShadowQualityChange(float i)
    {
        List<string> options = new List<string>() { "No Shadows", "Low", "Medium", "High" };

        int index = (int)i;

        ShadowQualityText.text = options[index];
    }

    void DetailDistanceChange(float i)
    {
        List<string> options = new List<string>() { "Low", "Medium", "High", "Very High" };

        int index = (int)i;

        detailDistText.text = options[index];
    }
    
    void Resolution(int i)
    {
        if (i == 0)
            Screen.SetResolution(1920, 1080, true);
        else if (i == 1)
            Screen.SetResolution(1600, 900, true);
        else if (i == 2)
            Screen.SetResolution(1280, 720, true);
    }

    public IEnumerator SetSplit(float split)
    {
        float sign = Mathf.Sign(split);

        split = Mathf.Abs(split);
        int timeM = (int)(split / 60);
        int timeS = (int)(split - timeM * 60);
        int timeMS = (int)((split - timeM * 60 - timeS) * 100);


        splitText.gameObject.SetActive(true);

        string plusText = "";

        if (sign > 0)
            plusText = "+";
        else
            plusText = "-";

        if (timeM > 0)
            splitText.text = plusText + " " +  timeM.ToString("D1") + ":" + Mathf.Abs(timeS).ToString("D2") + ":" + Mathf.Abs(timeMS).ToString("D2");
        else if(timeS >= 10)
            splitText.text = plusText + " " + timeS.ToString("D2") + ":" + Mathf.Abs(timeMS).ToString("D2");
        else
            splitText.text = plusText + " " + timeS.ToString("D1") + ":" + Mathf.Abs(timeMS).ToString("D2");

        if (sign <= 0)
            splitText.color = Color.green;
        else
            splitText.color = Color.red;

        yield return new WaitForSeconds(4f);
        splitText.gameObject.SetActive(false);


    }
}
