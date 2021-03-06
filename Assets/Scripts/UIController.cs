﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{

    // Neural Network characteristics
    public GameObject perceptron;
    public GameObject loopLink;
    public GameObject line;

    // Textboxes
    public Text maximumFitnessText;
    public Text totalRaceTimeText;
    public Text generationText;
    public Text trackText;
    public Text timeText;
    public Text AIcurrentFitness;
    public Text yourCurrentFitness;
    public Text timeLeft;
    public Text multipleSelected;

    // Plot characteristics
    public RectTransform plotArea;

    public GameObject maxLine;
    public GameObject avgLine;
    public GameObject axisLine;

    List<float> plotDataMax = new List<float>();
    List<float> plotDataAvg = new List<float>();

    float margin;
    float plotWidth;
    float plotHeight;

    int numGridLines = 10;
    public Text gridLabel;

    List<GameObject> maxLines = new List<GameObject>();
    List<GameObject> avgLines = new List<GameObject>();
    List<Text> axisLabels = new List<Text>();

    int currentI = -1;
    // Menu objects

    public GameObject mainPanel;
    public GameObject networks;
    public InputField generationInput;
    public Button nextGenerationInput;
    public Button previousGenerationInput;
    public Button playButton;
    public Button saveNetworkButton;
    public Button quitPlayButton;
    public Button challengeButton;
    public Button quitButton;
    public Image backImage;
    public GameObject savePanel;
    public InputField saveInput;
    public Button saveButton;
    public Button cancelSave;

    public Button pauseButton;
    public Button resumeButton;
    public Button stopButton;
    public Button mainMenu;
    public Button liveViewButton;

    public Button tenStepsButton;
    public Button fiftyStepsButton;
    public Button hundredStepsButton;
    public Button fiveHundredStepsButton;
    public Button thousendStepsButton;
    public Button allStepsButton;
    public Button bestStepsButton;



    public GameObject liveViewCanvas;
    public InputField maxSpeedInput;

    public GameObject TrackSelectMenu;

    public Text pausingText;

    public GameObject networkPanel;
    public GameObject networkVisualize;
    Vector2 nnVisualizeStart;
    Vector2 networkStart;
    public List<Genome> activeNetworks = new List<Genome>();
    Genome currentGenome;
    public int curGeneration;

    public GameObject manualCarPrefab;
    public WinLoseCanvasManager winLoseMenu;
    public GameObject mainMenuCanvas;

    public static UIController instance;

    CarTrainer carTrainer;
    RaceManager raceManager;
    public CarController car;

    public List<Coroutine> activeRoutines = new List<Coroutine>();

    public bool runningPlay;

    public Camera UICamera;
    public Camera liveViewCamera;
    public GameObject particles;
    ParticleSystem[] particlessys;

    public RacingCanvasController racingCanvasController;

    int plotLength;

    int setPlotMinX = 10;
    bool bestSteps = false;
    float lastMaxFitness = 0;
    float lastAvgFitness = 0;
    Button activeButton;

    void Awake()
    {
        instance = this;
        carTrainer = CarTrainer.instance;
        raceManager = RaceManager.raceManager;
        AnalysePlotArea();
    }

    void Start()
    {
        particlessys = particles.GetComponentsInChildren<ParticleSystem>();
        particles.SetActive(true);
        SetActiveParticles(true);
        pauseButton.onClick.AddListener(_Pause);
        resumeButton.onClick.AddListener(Resume);
        nextGenerationInput.onClick.AddListener(GenerationUp);
        previousGenerationInput.onClick.AddListener(GenerationDown);
        generationInput.onValueChanged.AddListener(SetGeneration);
        playButton.onClick.AddListener(PlayButton);
        quitButton.onClick.AddListener(Quit);
        quitPlayButton.onClick.AddListener(QuitPlay);
        challengeButton.onClick.AddListener(ChallengeButton);
        saveButton.onClick.AddListener(SaveNN);
        cancelSave.onClick.AddListener(SaveQuit);
        saveNetworkButton.onClick.AddListener(SavePanel);
        mainMenu.onClick.AddListener(MainMenu);
        liveViewButton.onClick.AddListener(LiveViewButton);
        maxSpeedInput.onEndEdit.AddListener(UpdateMaxSpeed);
        nnVisualizeStart = networkVisualize.GetComponent<RectTransform>().anchoredPosition;
        networkStart = networkPanel.GetComponent<RectTransform>().anchoredPosition;

        tenStepsButton.onClick.AddListener(TenSteps);
        fiftyStepsButton.onClick.AddListener(FiftySteps);
        hundredStepsButton.onClick.AddListener(HundredSteps);
        fiveHundredStepsButton.onClick.AddListener(FiveHundredSteps);
        allStepsButton.onClick.AddListener(AllSteps);
        bestStepsButton.onClick.AddListener(BestSteps);
        AllSteps();
    }

    private void Update()
    {
        //print("Gen: " + curGeneration + " i: " + currentI);
    }


    public void UpdateUI(int track, int totalTracks)
    {
        if (totalTracks == 1)
            trackText.text = "";
        else
            trackText.text = "Track: " + (track + 1).ToString() + " / " + totalTracks.ToString();
    }


    public void UpdateUI(float maxFitness, int generation, float totalRaceTime, bool updateTime)
    {
        generationText.text = "Generation: " + (generation).ToString();
        maximumFitnessText.text = "Maximum fitness: " + maxFitness.ToString("F2");

        if (updateTime)
        {
            if (totalRaceTime > 0)
            {
                int timeM = (int)(totalRaceTime / 60);
                int timeS = (int)(totalRaceTime - timeM * 60);
                int timeMS = (int)((totalRaceTime - timeM * 60 - timeS) * 100);
                totalRaceTimeText.text = "Best total race time: " + timeM.ToString("D2") + ":" + timeS.ToString("D2") + ":" + timeMS.ToString("D2");
            }
            else
            {
                totalRaceTimeText.text = "Best total race time: NA until car finished track(s).";
            }
        }

    }

    public void UpdateUI(float maxFitness, float currentBestFitness, float avgFitness, int generation, float totalRaceTime, bool updateTime)
    {
        UpdateUI(maxFitness, generation, totalRaceTime, updateTime);
        UpdateGraph(currentBestFitness, avgFitness, true);
    }

    void AnalysePlotArea()
    {
        margin = plotArea.rect.width * 0.00f;
        plotWidth = plotArea.rect.width - 2 * margin;
        plotHeight = plotArea.rect.height - 2 * margin;
    }

    void UpdateGraph(float maxFitness, float avgFitness, bool add = false)
    {
        maxFitness = Mathf.Clamp(maxFitness, 0, Mathf.Infinity);
        avgFitness = Mathf.Clamp(avgFitness, 0, Mathf.Infinity);

        lastAvgFitness = avgFitness;
        lastMaxFitness = maxFitness;

        if (add)
        {
            plotDataAvg.Add(avgFitness);
            plotDataMax.Add(maxFitness);
        }
        if (bestSteps && plotDataMax.Count - carTrainer.bestGeneration != setPlotMinX)
            DestroyAllLinePieces();

        if (bestSteps)
            setPlotMinX = plotDataMax.Count - carTrainer.bestGeneration;

        int plotMinX = 0;
        if (plotDataMax.Count > setPlotMinX)
            plotMinX = plotDataMax.Count - setPlotMinX;


        if (plotDataAvg.Count < 1)
            return;

        float plotMaxX = plotDataMax.Count;

        if (plotMaxX - plotMinX < 10)
            plotMaxX = plotMinX + 10;

        float plotMaxY = Mathf.Ceil((float)maxFitness / 100) * 100;

        PlotAxis(plotMaxX, plotMaxY, plotMinX);
        PlotMaxAvg(plotMaxX, plotMaxY, plotMinX);

    }

    void PlotAxis(float plotMaxX, float plotMaxY, int plotMinX)
    {

        float xDiff = plotWidth / numGridLines;
        float yDiff = plotHeight / numGridLines;

        for (int i = 0; i < numGridLines + 1; i++)
        {
            Text temp;
            if (axisLabels.Count <= 2 * i)
            {
                temp = (Text)Instantiate(gridLabel, plotArea.transform, false);
                axisLabels.Add(temp);
            }
            else
            {
                temp = axisLabels[2 * i];
            }

            RectTransform line = temp.GetComponent<RectTransform>();
            line.anchoredPosition = new Vector3(-1.5f * margin, (i) * yDiff - 1.5f * margin) - new Vector3(line.rect.width, line.rect.height / 2) ;
            temp.text = ((i) * plotMaxY / 10).ToString();

            if (axisLabels.Count <= 2 * i + 1)
            {
                temp = (Text)Instantiate(gridLabel, plotArea.transform, false);
                axisLabels.Add(temp);
            }
            else
                temp = axisLabels[2 * i + 1];

            line = temp.GetComponent<RectTransform>();
            line.anchoredPosition = new Vector3((i) * xDiff - 1.5f * margin, -1.5f * margin) - new Vector3(line.rect.width / 2, line.rect.height);
            temp.text = ((int)(((i) * (plotMaxX - plotMinX)/ 10) + 1 + plotMinX)).ToString();

        }
    }

    void PlotMaxAvg(float plotMaxX, float plotMaxY, int plotMinX)
    {
        Vector2 point1;
        Vector2 point2;

        float xDiff = plotWidth / (plotMaxX - plotMinX);
        float yDiff = plotHeight / plotMaxY;

        float skipSize = (plotMaxX - plotMinX)/2500;
        if (skipSize < 1)
            skipSize = 1;
        int counter = 0;
        for (float i = plotMinX; i < plotDataMax.Count - 1; i += skipSize)
        {
            int index = Mathf.RoundToInt(i);
            int nextIndex = Mathf.RoundToInt(i + skipSize);
            nextIndex = Mathf.Min(nextIndex, plotDataMax.Count - 1);
            point1 = new Vector2((index - plotMinX) * xDiff + 1.5f * margin, plotDataMax[index] * yDiff + 1.5f * margin);
            point2 = new Vector2((nextIndex - plotMinX) * xDiff + 1.5f * margin, plotDataMax[nextIndex] * yDiff + 1.5f * margin);

            PlotLinePiece(point1, point2, maxLine, counter, true);
            counter++;


        }
        counter = 0;

        for (float i = plotMinX; i < plotDataAvg.Count - 1; i += skipSize)
        {
            int index = Mathf.RoundToInt(i);
            int nextIndex = Mathf.RoundToInt(i + skipSize);
            nextIndex = Mathf.Min(nextIndex, plotDataAvg.Count - 1);

            point1 = new Vector2((index - plotMinX) * xDiff + 1.5f * margin, plotDataAvg[index] * yDiff + 1.5f * margin);
            point2 = new Vector2((nextIndex - plotMinX) * xDiff + 1.5f * margin , plotDataAvg[nextIndex] * yDiff + 1.5f * margin);

            PlotLinePiece(point1, point2, avgLine, counter, false);
            counter++;

        }

        //point1 = new Vector2((plotDataMax.Count - 1) * xDiff + 1.5f * margin, plotDataMax[plotDataMax.Count - 1] * yDiff + 1.5f * margin);
        //point2 = new Vector2((plotDataMax.Count) * xDiff + 1.5f * margin, currentMax * yDiff + 1.5f * margin);

        //PlotLinePiece(point1, point2, maxLine);

        //point1 = new Vector2((plotDataMax.Count - 1) * xDiff + 1.5f * margin, plotDataAvg[plotDataMax.Count - 1] * yDiff + 1.5f * margin);
        //point2 = new Vector2((plotDataMax.Count) * xDiff + 1.5f * margin, currentAvg * yDiff + 1.5f * margin);

        //PlotLinePiece(point1, point2, avgLine);
    }

    void PlotLinePiece(Vector3 point1, Vector3 point2, GameObject linePrefab, int i, bool max, float lineThickness = 3)
    {
        float lineWidth = (point1 - point2).magnitude;
        float lineRot = Mathf.Atan2(point2.y - point1.y, point2.x - point1.x);

        GameObject temp = null;

        if (max && maxLines.Count <= i)
        {
            temp = (GameObject)Instantiate(linePrefab, plotArea.transform);
            maxLines.Add(temp);
        }
        else if (max && maxLines.Count > i)
            temp = maxLines[i];

        if (!max && avgLines.Count <= i)
        {
            temp = (GameObject)Instantiate(linePrefab, plotArea.transform);
            avgLines.Add(temp);
        }
        else if (!max && avgLines.Count > i)
        {
            temp = avgLines[i];
        }

        RectTransform line = temp.GetComponent<RectTransform>();
        line.sizeDelta = new Vector2(lineWidth, lineThickness);
        line.anchoredPosition = point1;
        if (lineRot == 0)
            lineRot = 0.00001f;
        line.rotation = Quaternion.Euler(0, 0, lineRot * Mathf.Rad2Deg);
        line.localScale = Vector3.one;

    }

    void DestroyAllLinePieces()
    {
        for(int i = 0; i < avgLines.Count; i++)
        {
            GameObject line = avgLines[i];
            Destroy(line.gameObject);
            line = maxLines[i];
            Destroy(line.gameObject);
        }
        maxLines.Clear();
        avgLines.Clear();
    }
    void LiveViewButton()
    {
        backImage.enabled = false;
        liveViewCamera.gameObject.SetActive(true);
        liveViewCamera.enabled = true;
        liveViewCanvas.SetActive(true);
        liveViewButton.onClick.RemoveAllListeners();
        liveViewButton.GetComponentInChildren<Text>().text = "Quit Live View";
        liveViewButton.onClick.AddListener(QuitLiveView);
        SetActiveParticles(false);
        maxSpeedInput.text = 0.ToString();
        UpdateMaxSpeed(maxSpeedInput.text);
        raceManager.SetViewSettings(RaceManager.ViewType.TopView, true);
        raceManager.canChangeFrames = false;

    }

    void QuitLiveView()
    {
        backImage.enabled = true;
        liveViewCamera.enabled = false;
        liveViewCanvas.SetActive(false);
        liveViewButton.onClick.RemoveAllListeners();
        liveViewButton.GetComponentInChildren<Text>().text = "View Live Training";

        if (!carTrainer.isPaused)
            SetActiveParticles(true);

        liveViewButton.onClick.AddListener(LiveViewButton);
        raceManager.SetViewSettings(RaceManager.ViewType.MenuView, true);
        raceManager.canChangeFrames = true;
    }

    void UpdateMaxSpeed(string maxSpeedstr)
    {
        maxSpeedInput.onEndEdit.RemoveAllListeners();
        float maxSpeed = float.Parse(maxSpeedstr);

        if (maxSpeed <= 0)
        {
            maxSpeedInput.text = 0f.ToString();
            maxSpeed = 9999999999f;
        }
        else if (maxSpeed > 9999)
        {
            maxSpeedInput.text = 9999f.ToString();
            maxSpeed = 9999f;
        }

        RaceManager.raceManager.SetUpdateRate(maxSpeed, true);
        maxSpeedInput.onEndEdit.AddListener(UpdateMaxSpeed);

    }

    void SavePanel()
    {
        savePanel.SetActive(true);
    }

    void SaveNN()
    {
        if (saveInput.text == "")
            return;

        savePanel.SetActive(false);
        if (activeNetworks.Count == 1)
        {
            SaveableObjects.SaveableNeuralNetwork saveableNetwork = new SaveableObjects.SaveableNeuralNetwork(saveInput.text, currentGenome);
            saveableNetwork.Save();
        }
    }

    void SaveQuit()
    {
        savePanel.SetActive(false);
    }

    void MainMenu()
    {
        MyThreadPool.DestroyThreadPool();
        SceneManager.LoadScene("MainScene");
    }

    public void LoadNetwork(int i)
    {
        if (networkPanel.activeSelf == false)
            networkPanel.SetActive(true);

        multipleSelected.gameObject.SetActive(false);

        for (int k = 0; k < networkVisualize.transform.childCount; k++)
        {
            Destroy(networkVisualize.transform.GetChild(k).gameObject);
        }
        activeNetworks.Clear();
        currentGenome = carTrainer.ga.oldGenomes[curGeneration - 1][i];
        activeNetworks.Add(carTrainer.ga.oldGenomes[curGeneration - 1][i]);
        networkVisualize.GetComponent<RectTransform>().anchoredPosition = nnVisualizeStart;
        activeNetworks[0].CreateNetwork().VisualizeNetwork(networkVisualize.GetComponent<RectTransform>(), perceptron, line, loopLink, false);
        AIcurrentFitness.text = "Fitness: " + carTrainer.ga.oldGenomes[curGeneration - 1][i].GetFitness().ToString("F1");

        currentI = i;
    }

    public void ActivateMultipleSelected(int i)
    {
        for (int k = 0; k < networkVisualize.transform.childCount; k++)
        {
            Destroy(networkVisualize.transform.GetChild(k).gameObject);
        }
        multipleSelected.gameObject.SetActive(true);
        activeNetworks.Add(carTrainer.ga.oldGenomes[curGeneration - 1][i]);

    }

    public void DeactivateMultipleSelected(int i)
    {
        multipleSelected.gameObject.SetActive(true);
        activeNetworks.Remove(carTrainer.ga.oldGenomes[curGeneration - 1][i]);

    }

    public void DisableNetwork(int i, bool nonActive)
    {
        //carTrainer.generations[curGeneration - 1].networks[i].output = null;
        networkPanel.SetActive(false);

        if (nonActive)
            currentI = -1;
    }

    void _Pause()
    {
        StartCoroutine("Pause");
    }

    IEnumerator Pause()
    {
        carTrainer.pause = true;
        pauseButton.interactable = false;
        pausingText.gameObject.SetActive(true);

        while (!carTrainer.isPaused)
            yield return null;

        pausingText.gameObject.SetActive(false);
        resumeButton.interactable = true;
        liveViewButton.interactable = false;
        networks.SetActive(true);
        generationInput.text = (carTrainer.bestGeneration + 1).ToString();
        curGeneration = carTrainer.bestGeneration + 1;

        if (curGeneration - 1 < 1)
            previousGenerationInput.interactable = false;
        else
            previousGenerationInput.interactable = true;
        if (curGeneration + 1 > carTrainer.curGeneration)
            nextGenerationInput.interactable = false;
        else
            nextGenerationInput.interactable = true;

        SetActiveParticles(false);

        QuitLiveView();
    }

    void PlayButton()
    {
        TrackSelectMenu.SetActive(true);
        LoadTrackManagerDuringTraining.instance.challenge = false;
    }

    public void Play(List<string> trackNames)
    {
        ResetPlay();
        AddPlayers(false);
        raceManager.SetViewSettings(RaceManager.ViewType.AICarView, false);
        liveViewCamera.gameObject.SetActive(false);
        activeRoutines.Add(StartCoroutine(raceManager.StartRace(false, trackNames, true, false)));
        racingCanvasController.wasChallenging = false;
    }

    void AddPlayers(bool challenge)
    {
        raceManager.ResetPlayers();

        if (challenge)
        {
            KeyCode[] keycodes = new KeyCode[4];
            keycodes[0] = KeyCode.UpArrow;

            if(!GA_Parameters.breakWithSpace)
                keycodes[1] = KeyCode.DownArrow;
            else
                keycodes[1] = KeyCode.Space;

            keycodes[2] = KeyCode.LeftArrow;
            keycodes[3] = KeyCode.RightArrow;
            raceManager.AddHumanPlayer("Me", keycodes);
        }
        
        for (int i = 0; i < activeNetworks.Count; i++)
        {
            raceManager.AddAIPlayer(i.ToString(), activeNetworks[i].CreateNetwork(), i);
        }

        raceManager.FinishPlayers(activeNetworks.Count);
    }

    public void Challenge(List<string> trackNames)
    {
        ResetPlay();

        AddPlayers(true);
        raceManager.SetViewSettings(RaceManager.ViewType.HumanCarView, false);
        liveViewCamera.gameObject.SetActive(false);
        activeRoutines.Add(StartCoroutine(raceManager.StartRace(false, trackNames, true, false)));
        racingCanvasController.wasChallenging = true;
    }

    void ChallengeButton()
    {
        TrackSelectMenu.SetActive(true);
        LoadTrackManagerDuringTraining.instance.challenge = true;
    }

    void SetActiveParticles(bool isActive)
    {

        for (int i = 0; i < particlessys.Length; i++)
        {

            if (isActive == false)
                particlessys[i].gameObject.SetActive(false);
            else
            {
                particlessys[i].gameObject.SetActive(true);
            }

        }
    }

    void Resume()
    {
        ResetPlay();
        raceManager.ResetPlayers();
        raceManager.SetViewSettings(RaceManager.ViewType.MenuView, true);
        SetActiveParticles(true);
        networks.SetActive(false);
        pauseButton.interactable = true;
        resumeButton.interactable = false;
        networkPanel.SetActive(false);
        multipleSelected.gameObject.SetActive(false);
        liveViewButton.interactable = true;

        carTrainer.isPaused = false;
    }

    void GenerationUp()
    {
        DisableNetwork(currentI, false);
        curGeneration++;
        generationInput.text = (curGeneration).ToString();

        if (curGeneration + 1 > carTrainer.curGeneration)
            nextGenerationInput.interactable = false;
        if (previousGenerationInput.interactable == false)
            previousGenerationInput.interactable = true;

        if (currentI >= 0)
            LoadNetwork(currentI);
    }

    void GenerationDown()
    {
        DisableNetwork(currentI, false);

        curGeneration--;
        generationInput.text = (curGeneration).ToString();

        if (curGeneration - 1 < 1)
            previousGenerationInput.interactable = false;
        if (nextGenerationInput.interactable == false)
            nextGenerationInput.interactable = true;

        if (currentI >= 0)
            LoadNetwork(currentI);

    }

    void SetGeneration(string gen)
    {
        if (gen == "")
            return;

        try
        {
            curGeneration = int.Parse(gen);
        }
        catch (System.Exception)
        {
            generationInput.text = curGeneration.ToString();
            return;
        }
        if (curGeneration > carTrainer.curGeneration)
            curGeneration = carTrainer.curGeneration;
        else if (curGeneration < 1)
            curGeneration = 1;

        generationInput.text = curGeneration.ToString();

        if (currentI >= 0)
            LoadNetwork(currentI);
    }

    public void ResetPlay()
    {
        foreach (Coroutine routine in activeRoutines)
        {
            StopCoroutine(routine);
        }

    }

    void QuitPlay()
    {
        SetActiveParticles(false);
        UICamera.enabled = true;
        mainPanel.GetComponent<CanvasGroup>().alpha = 1;
        networkPanel.GetComponent<RectTransform>().anchoredPosition = networkStart;

        yourCurrentFitness.gameObject.SetActive(false);
        quitPlayButton.gameObject.SetActive(false);
        timeText.gameObject.SetActive(false);

        foreach (Coroutine routine in activeRoutines)
        {
            StopCoroutine(routine);
        }

        winLoseMenu.CancelAll();
        TrackManager.DeleteTrack();

    }

    void Quit()
    {
        Application.Quit();
    }

    void TenSteps()
    {
        if (setPlotMinX > 10)
            DestroyAllLinePieces();
        setPlotMinX = 10;
        bestSteps = false;
        UpdateGraph(lastMaxFitness, lastAvgFitness);
        ActivateButton(tenStepsButton);

    }

    void FiftySteps()
    {
        if (setPlotMinX > 50)
            DestroyAllLinePieces();
        setPlotMinX = 50;
        bestSteps = false;
        UpdateGraph(lastMaxFitness, lastAvgFitness);
        ActivateButton(fiftyStepsButton);


    }

    void HundredSteps()
    {
        if (setPlotMinX > 100)
            DestroyAllLinePieces();
        setPlotMinX = 100;
        bestSteps = false;
        UpdateGraph(lastMaxFitness, lastAvgFitness);
        ActivateButton(hundredStepsButton);


    }

    void FiveHundredSteps()
    {
        if (setPlotMinX > 500)
            DestroyAllLinePieces();
        setPlotMinX = 500;
        bestSteps = false;
        UpdateGraph(lastMaxFitness, lastAvgFitness);
        ActivateButton(fiveHundredStepsButton);


    }

    void AllSteps()
    {
        if (setPlotMinX > 99999999)
            DestroyAllLinePieces();
        setPlotMinX = 99999999;

        bestSteps = false;
        UpdateGraph(lastMaxFitness, lastAvgFitness);
        ActivateButton(allStepsButton);


    }

    void BestSteps()
    {
        if (setPlotMinX > maxLines.Count - carTrainer.bestGeneration)
            DestroyAllLinePieces();

        bestSteps = true;
        setPlotMinX = maxLines.Count - carTrainer.bestGeneration;
        UpdateGraph(lastMaxFitness, lastAvgFitness);
        ActivateButton(bestStepsButton);
    }

    void ActivateButton(Button button)
    {
        if (activeButton != null)
            activeButton.GetComponent<Image>().color = Color.white;
        button.GetComponent<Image>().color = new Color(179f/255, 208f/255, 255f/255);
        activeButton = button;
    }
}
