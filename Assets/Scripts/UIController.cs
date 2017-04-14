using UnityEngine;
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
    public Text generationText;
    public Text individualText;
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


    List<GameObject> lines = new List<GameObject>();

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

    public GameObject savePanel;
    public InputField saveInput;
    public Button saveButton;
    public Button cancelSave;

    public Button pauseButton;
    public Button resumeButton;
    public Button stopButton;
    public Button mainMenu;

    public Text pausingText;

    public GameObject networkPanel;
    public GameObject networkVisualize;
    Vector2 nnVisualizeStart;
    Vector2 networkStart;
    public List<NeuralNetwork> activeNetworks = new List<NeuralNetwork>();
    Genome currentGenome;
    public int curGeneration;

    public GameObject manualCarPrefab;
    public WinLoseCanvasManager winLoseMenu;
    public GameObject mainMenuCanvas;

    // Singleton
    public static UIController instance;

    CarTrainer carTrainer;

    public CarController car;

    public List<Coroutine> activeRoutines = new List<Coroutine>();

    public bool runningPlay;

    public Camera UICamera;
    public GameObject particles;

    void Awake()
    {
        instance = this;
        carTrainer = CarTrainer.instance;

        AnalysePlotArea();
    }

    void Start()
    {
        particles.SetActive(true);
        SetActiveParticles(true);
        pauseButton.onClick.AddListener(_Pause);
        resumeButton.onClick.AddListener(Resume);
        nextGenerationInput.onClick.AddListener(GenerationUp);
        previousGenerationInput.onClick.AddListener(GenerationDown);
        generationInput.onValueChanged.AddListener(SetGeneration);
        playButton.onClick.AddListener(Play);
        quitButton.onClick.AddListener(Quit);
        quitPlayButton.onClick.AddListener(QuitPlay);
        challengeButton.onClick.AddListener(Challenge);
        saveButton.onClick.AddListener(SaveNN);
        cancelSave.onClick.AddListener(SaveQuit);
        saveNetworkButton.onClick.AddListener(SavePanel);
        mainMenu.onClick.AddListener(MainMenu);

        nnVisualizeStart = networkVisualize.GetComponent<RectTransform>().anchoredPosition;
        networkStart = networkPanel.GetComponent<RectTransform>().anchoredPosition;
    }

    public void UpdateUI(float maxFitness, int generation, int individual, float fitness, float time)
    {
        generationText.text = "Generation: " + (generation).ToString();
        individualText.text = "Individual: " + (individual).ToString();
        maximumFitnessText.text = "Maximum fitness: " + maxFitness.ToString("F2");
        //timeText.text = "Time: " + time.ToString("F2");
        //fitnessText.text = "Current Fitness: " + fitness.ToString("F2");

    }

    public void UpdateUI(float maxFitness, float avgFitness, int generation, int individual, float fitness, float time, bool genFinish)
    {
        UpdateUI(maxFitness, generation, individual, fitness, time);
        UpdateGraph(maxFitness, avgFitness, genFinish);
    }

    void AnalysePlotArea()
    {
        margin = plotArea.rect.width * 0.1f;
        plotWidth = plotArea.rect.width - 2 * margin;
        plotHeight = plotArea.rect.height - 2 * margin;
    }

    void UpdateGraph(float maxFitness, float avgFitness, bool genFinish)
    {
        maxFitness = Mathf.Clamp(maxFitness, 0, Mathf.Infinity);
        avgFitness = Mathf.Clamp(avgFitness, 0, Mathf.Infinity);
        if (genFinish)
        {
            plotDataAvg.Add(avgFitness);
            plotDataMax.Add(maxFitness);
        }

        if (plotDataAvg.Count < 1)
            return;

        float plotMaxX = Mathf.Ceil((float)plotDataAvg.Count / 10) * 10;
        float plotMaxY = Mathf.Ceil((float)maxFitness / 100) * 100;

        DestroyPlotLines();
        PlotAxis(plotMaxX, plotMaxY);
        PlotMaxAvg(plotMaxX, plotMaxY);

    }

    void PlotAxis(float plotMaxX, float plotMaxY)
    {

        float xDiff = plotWidth / numGridLines;
        float yDiff = plotHeight / numGridLines;

        for (int i = 0; i < numGridLines; i++)
        {
            Text temp = (Text)Instantiate(gridLabel, plotArea.transform, false);
            RectTransform line = temp.GetComponent<RectTransform>();
            line.anchoredPosition = new Vector3(1.5f * margin, (i + 1) * yDiff + 1.5f * margin) - new Vector3(line.rect.width, line.rect.height / 2);
            temp.text = ((i + 1) * plotMaxY / 10).ToString();

            lines.Add(line.gameObject);

            temp = (Text)Instantiate(gridLabel, plotArea.transform, false);
            line = temp.GetComponent<RectTransform>();
            line.anchoredPosition = new Vector3((i + 1) * xDiff + 1.5f * margin, 1.5f * margin) - new Vector3(line.rect.width / 2, line.rect.height);
            temp.text = (((i + 1) * plotMaxX / 10) + 1).ToString();

            lines.Add(line.gameObject);
        }
    }

    void PlotMaxAvg(float plotMaxX, float plotMaxY)
    {

        Vector2 point1;
        Vector2 point2;

        float xDiff = plotWidth / plotMaxX;
        float yDiff = plotHeight / plotMaxY;

        for (int i = 0; i < plotDataMax.Count - 1; i++)
        {
            point1 = new Vector2(i * xDiff + 1.5f * margin, plotDataMax[i] * yDiff + 1.5f * margin);
            point2 = new Vector2((i + 1) * xDiff + 1.5f * margin, plotDataMax[i + 1] * yDiff + 1.5f * margin);

            PlotLinePiece(point1, point2, maxLine);

            point1 = new Vector2(i * xDiff + 1.5f * margin, plotDataAvg[i] * yDiff + 1.5f * margin);
            point2 = new Vector2((i + 1) * xDiff + 1.5f * margin, plotDataAvg[i + 1] * yDiff + 1.5f * margin);

            PlotLinePiece(point1, point2, avgLine);
        }

        //point1 = new Vector2((plotDataMax.Count - 1) * xDiff + 1.5f * margin, plotDataMax[plotDataMax.Count - 1] * yDiff + 1.5f * margin);
        //point2 = new Vector2((plotDataMax.Count) * xDiff + 1.5f * margin, currentMax * yDiff + 1.5f * margin);

        //PlotLinePiece(point1, point2, maxLine);

        //point1 = new Vector2((plotDataMax.Count - 1) * xDiff + 1.5f * margin, plotDataAvg[plotDataMax.Count - 1] * yDiff + 1.5f * margin);
        //point2 = new Vector2((plotDataMax.Count) * xDiff + 1.5f * margin, currentAvg * yDiff + 1.5f * margin);

        //PlotLinePiece(point1, point2, avgLine);
    }

    void PlotLinePiece(Vector3 point1, Vector3 point2, GameObject linePrefab, float lineThickness = 3)
    {
        float lineWidth = (point1 - point2).magnitude;
        float lineRot = Mathf.Atan2(point2.y - point1.y, point2.x - point1.x);

        GameObject temp = (GameObject)Instantiate(linePrefab, plotArea.transform);
        RectTransform line = temp.GetComponent<RectTransform>();
        line.sizeDelta = new Vector2(lineWidth, lineThickness);
        line.anchoredPosition = point1;
        line.rotation = Quaternion.Euler(0, 0, lineRot * Mathf.Rad2Deg);
        line.localScale = Vector3.one;

        lines.Add(line.gameObject);
    }

    void DestroyPlotLines()
    {
        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();
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
        activeNetworks.Add(carTrainer.ga.oldGenomes[curGeneration-1][i].CreateNetwork());
        networkVisualize.GetComponent<RectTransform>().anchoredPosition = nnVisualizeStart;
        activeNetworks[0].VisualizeNetwork(networkVisualize.GetComponent<RectTransform>(), perceptron, line, loopLink, false);
        AIcurrentFitness.text = "Fitness: " + carTrainer.ga.oldGenomes[curGeneration-1][i].GetFitness().ToString("F1");

        currentI = i;
    }

    public void ActivateMultipleSelected(int i)
    {
        for (int k = 0; k < networkVisualize.transform.childCount; k++)
        {
            Destroy(networkVisualize.transform.GetChild(k).gameObject);
        }
        multipleSelected.gameObject.SetActive(true);
        activeNetworks.Add(carTrainer.generationNetworks[i]);
        //float maxFitness = 0;
        //foreach (NeuralNetwork network in activeNetworks)
        //{
        //    if (network.Fitness > maxFitness)
        //        maxFitness = network.Fitness;
        //}

        //AIcurrentFitness.text = "Maximum Fitness of selection: " + activeNetworks[0].Fitness.ToString("F1");

    }

    public void DeactivateMultipleSelected(int i)
    {
        multipleSelected.gameObject.SetActive(true);
        activeNetworks.Remove(carTrainer.generationNetworks[i]);
        //float maxFitness = 0;

        //foreach (NeuralNetwork network in activeNetworks)
        //{
        //    if (network.Fitness > maxFitness)
        //        maxFitness = network.Fitness;
        //}

        //AIcurrentFitness.text = "Maximum Fitness of selection: " + activeNetworks[0].Fitness.ToString("F1");

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
        networks.SetActive(true);
        generationInput.text = (carTrainer.curGeneration).ToString();
        curGeneration = carTrainer.curGeneration;

        if (curGeneration - 1 < 1)
            previousGenerationInput.interactable = false;
        else
            previousGenerationInput.interactable = true;
        if (curGeneration + 1 > carTrainer.curGeneration)
            nextGenerationInput.interactable = false;
        else
            nextGenerationInput.interactable = true;

        SetActiveParticles(false);
    }

    void SetActiveParticles(bool isActive)
    {
        ParticleSystem[] particlessys = particles.GetComponentsInChildren<ParticleSystem>();
        for(int i = 0; i < particlessys.Length; i++)
        {
            var em = particlessys[i].emission;
            if(!isActive)
                em.rateOverTime = 0;
            else
                em.rateOverTime = 3;
        }
    }

    void Resume()
    {
        SetActiveParticles(true);
        networks.SetActive(false);
        pauseButton.interactable = true;
        resumeButton.interactable = false;
        networkPanel.SetActive(false);
        multipleSelected.gameObject.SetActive(false);

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

    void Play()
    {
        ResetPlay();

        mainPanel.GetComponent<CanvasGroup>().alpha = 0;
        networkPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        quitPlayButton.gameObject.SetActive(true);

        activeRoutines.Add(StartCoroutine(_Play(false)));
    }

    public void Challenge()
    {
        ResetPlay();

        mainPanel.GetComponent<CanvasGroup>().alpha = 0;
        networkPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        quitPlayButton.gameObject.SetActive(true);

        activeRoutines.Add(StartCoroutine(_Play(true)));
    }

    IEnumerator _Play(bool challenge)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yourCurrentFitness.gameObject.SetActive(true);
        timeText.gameObject.SetActive(true);
        multipleSelected.gameObject.SetActive(false);
        timeText.text = GA_Parameters.simulationTime.ToString("F2");
        List<NeuralNetwork> networks = new List<NeuralNetwork>();
        SetActiveParticles(false);

        networks.AddRange(activeNetworks);

        for(int i = 0; i < activeNetworks.Count; i++)
        {
            activeNetworks[i].Reset();
        }

        if (challenge)
            networks.Add(null);

        if (challenge)
        {
            winLoseMenu.gameObject.SetActive(true);
            winLoseMenu.ReadySetGo();

            Coroutine routine = carTrainer.StartCoroutine(carTrainer.CreateCars(networks, true, false, 4));
            activeRoutines.Add(routine);
        }

        else
        {
            Coroutine routine = carTrainer.StartCoroutine(carTrainer.CreateCars(networks, false, true, 0));
            activeRoutines.Add(routine);



        }
        runningPlay = true;
        float measureTime = Time.realtimeSinceStartup;

        while (runningPlay)
        {
            if (CameraController.currentActiveMainCamera == null)
            {
                for (int i = 0; i < CarController.cameras.Count; i++)
                {
                    if (CarController.cameras[i] == null)
                    {
                        CarController.cameras.RemoveAt(i);
                        i--;
                    }
                }

                CarController.cameras[0].gameObject.SetActive(true);
                CameraController.currentActiveMainCamera = CarController.cameras[0];
            }
            activeNetworks[0].SetNetworkValues();


            float maxAIfitness = 0;

            for (int i = 0; i < carTrainer.carControllers.Count; i++)
            {
                if (carTrainer.carControllers[i].GetNetwork() == null)
                    yourCurrentFitness.text = "Your fitness: " + carTrainer.carControllers[i].GetFitnessTracker().GetFitness().ToString("F1");
                else
                {
                    if (carTrainer.carControllers[i].GetFitnessTracker().GetFitness() > maxAIfitness)
                        maxAIfitness = carTrainer.carControllers[i].GetFitnessTracker().GetFitness();
                }
            }

            AIcurrentFitness.text = "Max AI fitness: " + maxAIfitness.ToString("F1");
            timeText.text = "Time Left: " + (GA_Parameters.simulationTime - carTrainer.carControllers[0].GetFitnessTracker().time).ToString("F2");

            if (Input.GetKeyDown(KeyCode.V) && !challenge)
            {
                for (int i = 0; i < CarController.cameras.Count; i++)
                {
                    if (CarController.cameras[i].gameObject.activeSelf)
                    {
                        CarController.cameras[i].gameObject.SetActive(false);

                        if (i != CarController.cameras.Count - 1)
                            CarController.cameras[i + 1].gameObject.SetActive(true);
                        else
                            CarController.cameras[0].gameObject.SetActive(true);
                        break;
                    }
                }
            }
            measureTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(0.025f);

        }

        if (challenge)
        {
            float maxFitness = 0;
            NeuralNetwork maxNetwork = null;
            for (int i = 0; i < carTrainer.carControllers.Count; i++)
            {
                if (carTrainer.carControllers[i].GetFitnessTracker().GetFitness() > maxFitness)
                {
                    maxFitness = carTrainer.carControllers[i].GetFitnessTracker().GetFitness();
                    maxNetwork = carTrainer.carControllers[i].GetNetwork();
                }
            }


            if (maxNetwork == null)
                winLoseMenu.WinLose(true);
            else
                winLoseMenu.WinLose(false);
        }
    }

    void ResetPlay()
    {
        foreach (Coroutine routine in activeRoutines)
        {
            StopCoroutine(routine);
        }

        winLoseMenu.CancelAll();

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

        carTrainer.SetUpdateRate(0);
    }

    void Quit()
    {
        Application.Quit();
    }
}
