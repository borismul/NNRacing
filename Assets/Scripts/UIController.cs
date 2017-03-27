using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    // Neural Network characteristics
    public GameObject perceptron;
    public GameObject hSpace;
    public GameObject vSpace;
    public GameObject line;

    // Textboxes
    public Text maximumFitnessText;
    public Text generationText;
    public Text individualText;
    public Text timeText;

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

    float currentAvg;
    float currentMax;

    List<GameObject> lines = new List<GameObject>();

    int currentI = -1;

    // Menu objects

    public GameObject mainPanel;
    public GameObject networks;
    public InputField generationInput;
    public Button nextGenerationInput;
    public Button previousGenerationInput;
    public Button playButton;
    public Button quitButton;

    public Button pauseButton;
    public Button resumeButton;
    public Button stopButton;

    public Text pausingText;

    public GameObject networkPanel;
    public Text networkFitness;
    public GameObject networkVisualize;
    Vector2 nnVisualizeStart;
    Vector2 networkStart;
    NeuralNetwork activeNetwork;

    public int curGeneration;

    // Singleton
    public static UIController instance;

    GeneticAlgorithm geneticAlgorithm;

    public NewCarController car;

    Coroutine co;

    void Awake()
    {
        instance = this;
        AnalysePlotArea();
    }

    void Start()
    {
        geneticAlgorithm = GeneticAlgorithm.instance;
        pauseButton.onClick.AddListener(_Pause);
        resumeButton.onClick.AddListener(Resume);
        nextGenerationInput.onClick.AddListener(GenerationUp);
        previousGenerationInput.onClick.AddListener(GenerationDown);
        generationInput.onValueChanged.AddListener(SetGeneration);
        playButton.onClick.AddListener(Play);
        quitButton.onClick.AddListener(Quit);
        nnVisualizeStart = networkVisualize.GetComponent<RectTransform>().anchoredPosition;
        networkStart = networkPanel.GetComponent<RectTransform>().anchoredPosition;
    }

    public void UpdateUI(float maxFitness, int generation, int individual, float fitness, float time)
    {
        generationText.text = "Generation: " + (generation + 1).ToString();
        individualText.text = "Individual: " + (individual + 1).ToString();
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
        if (genFinish)
        {
            plotDataAvg.Add(avgFitness);
            plotDataMax.Add(maxFitness);
        }
        else
        {
            currentAvg = avgFitness;
            currentMax = maxFitness;
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
            temp.text = ((i + 1) * plotMaxX / 10).ToString();

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

    public void LoadNetwork(int i)
    {
        if (networkPanel.activeSelf == false)
            networkPanel.SetActive(true);

        for (int k = 0; k < networkVisualize.transform.childCount; k++)
        {
            Destroy(networkVisualize.transform.GetChild(k).gameObject);
        }
        activeNetwork = geneticAlgorithm.generations[curGeneration - 1].networks[i];
        networkVisualize.GetComponent<RectTransform>().anchoredPosition = nnVisualizeStart;
        activeNetwork.VisualizeNetwork(perceptron, hSpace, vSpace, line, networkVisualize.transform, "Network");
        networkFitness.text = "Fitness: " + activeNetwork.Fitness.ToString("F1");

        currentI = i;
    }
    
    public void DisableNetwork(int i, bool nonActive)
    {
        geneticAlgorithm.generations[curGeneration-1].networks[i].output = null;
        networkPanel.SetActive(false);

        if(nonActive)
            currentI = -1;
    }

    void _Pause()
    {
        StartCoroutine("Pause");
    }

    IEnumerator Pause()
    { 
        geneticAlgorithm.pause = true;
        pauseButton.interactable = false;
        pausingText.gameObject.SetActive(true);

        while (!geneticAlgorithm.isPaused)
            yield return null;

        pausingText.gameObject.SetActive(false);
        resumeButton.interactable = true;   
        networks.SetActive(true);
        generationInput.text = (geneticAlgorithm.curGeneration).ToString();
        curGeneration = geneticAlgorithm.curGeneration;

        if (curGeneration - 1 < 1)
            previousGenerationInput.interactable = false;
        else
            previousGenerationInput.interactable = true;
        if (curGeneration + 1 > geneticAlgorithm.curGeneration)
            nextGenerationInput.interactable = false;
        else
            nextGenerationInput.interactable = true;
    }

    void Resume()
    {
        networks.SetActive(false);
        pauseButton.interactable = true;
        resumeButton.interactable = false;
        networkPanel.SetActive(false);

        geneticAlgorithm.isPaused = false;
    }

    void GenerationUp()
    {
        DisableNetwork(currentI, false);
        curGeneration++;
        generationInput.text = (curGeneration).ToString();

        if (curGeneration + 1 > geneticAlgorithm.curGeneration)
            nextGenerationInput.interactable = false;
        if (previousGenerationInput.interactable == false)
            previousGenerationInput.interactable = true;

        if(currentI >= 0)
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

        if(currentI >= 0)
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
        catch(System.Exception e)
        {
            generationInput.text = curGeneration.ToString();
            return;
        }
        if (curGeneration > geneticAlgorithm.curGeneration)
            curGeneration = geneticAlgorithm.curGeneration;
        else if (curGeneration < 1)
            curGeneration = 1;

        generationInput.text = curGeneration.ToString();

        if (currentI >= 0)
            LoadNetwork(currentI);
    }

    void Play()
    {
        mainPanel.GetComponent<CanvasGroup>().alpha = 0;
        networkPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        networkFitness.text = 0.ToString();
        playButton.GetComponentInChildren<Text>().text = "Quit Play";
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(QuitPlay);
        geneticAlgorithm.currentNetwork = activeNetwork;

        DistanceTracker.instance.CompleteReset();
        car.Reset();
        co = StartCoroutine("_Play");
    }

    void ContinuePlay()
    {
        StopCoroutine(co);
        DistanceTracker.instance.CompleteReset();
        car.Reset();

        co = StartCoroutine("_Play");
    }

    IEnumerator _Play()
    {
        float currentTime = 0;
        int fps = geneticAlgorithm.fps;
        int carUpdateRate = 1;

        currentTime = 0;
        car.Reset();
        bool stop = false;
        for (int tracks = 0; tracks < 2; tracks++)
        {
            currentTime = 0;
            stop = false;
            DistanceTracker.instance.NextTrack();
            car.Reset();
            for (int k = 0; k < geneticAlgorithm.simulationTime; k++)
            {
                for (int l = 0; l < fps; l++)
                {
                    DistanceTracker.instance.UpdateDistance(currentTime);
                    currentTime += 1f / (float)fps;
                    geneticAlgorithm.SetOutput();
                    if (!car.UpdateCar(1f / (float)fps))
                    {
                        DistanceTracker.instance.Penalty(0.9f);
                        car.Reset();
                    }
                    else if (!DistanceTracker.instance.UpdateDistance(currentTime) || stop)
                    {
                        stop = true;
                        break;
                    }

                    if (l != 0 && carUpdateRate != 0 && l % carUpdateRate == 0)
                    {
                        for (int m = 0; m < (float)60 / fps / carUpdateRate; m++)
                        {
                            //UIController.instance.UpdateUI(maxFitness, i, j, curFitness + Mathf.Clamp(DistanceTracker.instance.distance, 0, Mathf.Infinity), currentTime);
                            yield return null;
                            if (carUpdateRate == 0)
                                break;
                        }
                    }
                }

                if (stop)
                    break;
            }

        }
        ContinuePlay();

    }

    void QuitPlay()
    {
        StopCoroutine(co);
        DistanceTracker.instance.CompleteReset();
        car.Reset();
        mainPanel.GetComponent<CanvasGroup>().alpha = 1;
        networkPanel.GetComponent<RectTransform>().anchoredPosition = networkStart;
        networkFitness.text = "Fitness: " + activeNetwork.Fitness.ToString("F2");
        playButton.GetComponentInChildren<Text>().text = "Play";
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(Play);
    }

    void Quit()
    {
        Application.Quit();
    }
}
