using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using B83.ExpressionParser;


public class MainMenuController : MonoBehaviour
{
    public InputField generations;
    public InputField individuals;
    public InputField carSimPerFrame;
    public InputField simTime;
    public InputField simFPS;
    public InputField WeightsProb;
    public InputField NNprob;
    public InputField laps;
    public Toggle stopAtCrash;

    public InputField maxCarSpeed;
    public InputField carAccSpeed;
    public InputField breakSpeed;
    public InputField carTurnSpeed;

    public InputField fitnessInput;

    public Button nextButton;
    public Button backButton;
    public Button checkButton;

    public CarController car;
    public CanvasGroup otherCanvas;

    public Text errorText;

    public GameObject thisCanvas;
    public GameObject parametersPanel;
    public GameObject fitnessPanel;

    Expression exp;

    void Start()
    {
        otherCanvas.alpha = 0;
        otherCanvas.blocksRaycasts = false;
        backButton.interactable = false;
        backButton.onClick.AddListener(BackButton);

        generations.onValueChanged.AddListener(Generations);
        generations.text = PlayerPrefs.GetString("generations");
        if(generations.text == "")
            generations.text = "100";

        individuals.onValueChanged.AddListener(Individuals);
        individuals.text = PlayerPrefs.GetString("individuals");
        if (individuals.text == "")
            individuals.text = "100";

        carSimPerFrame.onValueChanged.AddListener(CarSimPerFrame);
        carSimPerFrame.text = PlayerPrefs.GetString("carSimPerFrame");
        if (carSimPerFrame.text == "")
            carSimPerFrame.text = "1";

        simTime.onValueChanged.AddListener(SimTime);
        simTime.text = PlayerPrefs.GetString("simTime");
        if (simTime.text == "")
            simTime.text = "30";

        simFPS.onValueChanged.AddListener(SimFPS);
        simFPS.text = PlayerPrefs.GetString("simFPS");
        if (simFPS.text == "")
            simFPS.text = "60";

        WeightsProb.onValueChanged.AddListener(WeightMut);
        WeightsProb.text = PlayerPrefs.GetString("WeightsProb");
        if (WeightsProb.text == "")
            WeightsProb.text = "0.05";

        NNprob.onValueChanged.AddListener(NNMut);
        NNprob.text = PlayerPrefs.GetString("NNprob");
        if (NNprob.text == "")
            NNprob.text = "0.0005";

        maxCarSpeed.onValueChanged.AddListener(MaxSpeed);
        maxCarSpeed.text = PlayerPrefs.GetString("maxCarSpeed");
        if (maxCarSpeed.text == "")
            maxCarSpeed.text = "50";

        carAccSpeed.onValueChanged.AddListener(AccSpeed);
        carAccSpeed.text = PlayerPrefs.GetString("carAccSpeed");
        if (carAccSpeed.text == "")
            carAccSpeed.text = "10";

        breakSpeed.onValueChanged.AddListener(BreakSpeed);
        breakSpeed.text = PlayerPrefs.GetString("breakSpeed");
        if (breakSpeed.text == "")
            breakSpeed.text = "20";

        carTurnSpeed.onValueChanged.AddListener(TurnSpeed);
        carTurnSpeed.text = PlayerPrefs.GetString("carTurnSpeed");
        if (carTurnSpeed.text == "")
            carTurnSpeed.text = "80";

        fitnessInput.onValueChanged.AddListener(FitnessInput);
        fitnessInput.text = PlayerPrefs.GetString("FitnessEQ");
        if (fitnessInput.text == "")
            fitnessInput.text = "(x + x / (t + 0.001) * l) + 0.9 ^ n";

        laps.onValueChanged.AddListener(Laps);
        laps.text = PlayerPrefs.GetString("Laps");
        if (laps.text == "")
            laps.text = "1";

        stopAtCrash.onValueChanged.AddListener(StopAtCrash);
        stopAtCrash.isOn = PlayerPrefs.GetInt("StopAtCrash") > 0;

        nextButton.onClick.AddListener(NextButton);
        checkButton.onClick.AddListener(CheckButton);
    }

    void StopAtCrash(bool value)
    {
        if (value)
            PlayerPrefs.SetInt("StopAtCrash", 1);
        else
            PlayerPrefs.SetInt("StopAtCrash", 0);

        GeneticAlgorithm.instance.stopAtCrash = value;
    }

    void Laps(string input)
    {
        if (laps.text == "")
            return;

        int par = int.Parse(input);
        if (par < 1)
        {
            par = 1;
            laps.text = 1.ToString();
        }

        GeneticAlgorithm.instance.laps = par;
        PlayerPrefs.SetString("Laps", laps.text);
    }

    void FitnessInput(string input)
    {
        PlayerPrefs.SetString("FitnessEQ", input);

        foreach (Text text in GetComponentsInChildren<Text>())
        {
            text.color = Color.black;
        }
    }

    void Generations(string input)
    {
        if (generations.text == "")
            return;

        int gen = int.Parse(input);
        if (gen < 1)
        {
            gen = 1;
            generations.text = 1.ToString();
        }

        GeneticAlgorithm.instance.numGenerations = gen;
        PlayerPrefs.SetString("generations", generations.text);
    }

    void Individuals(string input)
    {
        if (individuals.text == "")
            return;

        int ind = int.Parse(input);
        if (ind < 1)
        {
            ind = 1;
            individuals.text = 1.ToString();
        }

        GeneticAlgorithm.instance.populationSize = ind;
        PlayerPrefs.SetString("individuals", individuals.text);
    }

    void CarSimPerFrame(string input)
    {
        if (carSimPerFrame.text == "")
            return;

        int par = int.Parse(input);
        if (par < 1)
        {
            carSimPerFrame.text = 1.ToString();
            par = 1;
        }
        else if (par > 50)
        {
            carSimPerFrame.text = 50.ToString();
            par = 50;
        }
        GeneticAlgorithm.instance.carsPerFrame = par;
        PlayerPrefs.SetString("carSimPerFrame", carSimPerFrame.text);

    }

    void SimTime(string input)
    {
        if (simTime.text == "")
            return;

        int par = int.Parse(input);
        if (par < 1)
        {
            simTime.text = 1.ToString();
            par = 1;
        }
        else if (par > 500)
        {
            par = 500;
            simTime.text = 500.ToString();
        }
        GeneticAlgorithm.instance.simulationTime = par
            ;
        PlayerPrefs.SetString("simTime", simTime.text);

    }

    void SimFPS(string input)
    {
        if (simFPS.text == "")
            return;

        int par = int.Parse(input);
        if (par < 1)
        {
            simFPS.text = 10.ToString();
            par = 1;
        }
        else if (par > 500)
        {
            par = 500;
            simFPS.text = 500.ToString();
        }
        GeneticAlgorithm.instance.fps = par;

        PlayerPrefs.SetString("simFPS", simFPS.text);

    }

    void WeightMut(string input)
    {
        if (WeightsProb.text == "")
            return;

        float par = float.Parse(input);
        if (par < 0)
        {
            WeightsProb.text = 0.ToString();
            par = 0;
        }
        else if (par > 1)
        {
            par = 1;
            WeightsProb.text = 1.ToString();
        }
        GeneticAlgorithm.instance.NNWeightsMutationChance = par;
        PlayerPrefs.SetString("WeightsProb", WeightsProb.text);

    }

    void NNMut(string input)
    {
        if (NNprob.text == "")
            return;

        float par = float.Parse(input);
        if (par < 0)
        {
            NNprob.text = 0.ToString();
            par = 0;
        }
        else if (par > 1)
        {
            par = 1;
            NNprob.text = 1.ToString();
        }
        GeneticAlgorithm.instance.NNMutationChance = par;
        PlayerPrefs.SetString("NNprob", NNprob.text);

    }

    void MaxSpeed(string input)
    {
        if (maxCarSpeed.text == "")
            return;

        float par = float.Parse(input);
        if (par < 0.1f)
        {
            maxCarSpeed.text = 0.1f.ToString();
            par = 0.1f;
        }
        else if (par > 500)
        {
            par = 500;
            maxCarSpeed.text = 500.ToString();
        }
        GeneticAlgorithm.instance.maxVelocity = par;
        PlayerPrefs.SetString("maxCarSpeed", maxCarSpeed.text);

    }

    void AccSpeed(string input)
    {
        if (carAccSpeed.text == "")
            return;

        float par = float.Parse(input);
        if (par < 0.1f)
        {
            carAccSpeed.text = 0.1f.ToString();
            par = 0.1f;
        }
        else if (par > 100)
        {
            par = 100;
            carAccSpeed.text = 100.ToString();
        }
        GeneticAlgorithm.instance.accSpeed = par;
        PlayerPrefs.SetString("carAccSpeed", carAccSpeed.text);

    }

    void BreakSpeed(string input)
    {
        if (breakSpeed.text == "")
            return;

        float par = float.Parse(input);
        if (par < 0.1f)
        {
            breakSpeed.text = 0.1f.ToString();
            par = 0.1f;
        }
        else if (par > 500)
        {
            par = 500;
            breakSpeed.text = 500.ToString();
        }
        GeneticAlgorithm.instance.breakSpeed = par;
        PlayerPrefs.SetString("breakSpeed", breakSpeed.text);
    }

    void TurnSpeed(string input)
    {
        if (carTurnSpeed.text == "")
            return;

        float par = float.Parse(input);
        if (par < 0.1f)
        {
            carTurnSpeed.text = 0.1f.ToString();
            par = 0.1f;
        }
        else if (par > 500)
        {
            par = 500;
            carTurnSpeed.text = 500.ToString();
        }
        GeneticAlgorithm.instance.turnSpeed = par;
        PlayerPrefs.SetString("carTurnSpeed", carTurnSpeed.text);

    }

    void StartSimulation()
    {
        if (!CheckFitnessText())
            return;

        if(generations.text != "" &&
           individuals.text != "" && 
           carSimPerFrame.text != "" &&
           simFPS.text != "" &&
           WeightsProb.text != "" &&
           NNprob.text != "" &&
           maxCarSpeed.text != "" &&
           carAccSpeed.text != "" &&
           carTurnSpeed.text != "")
        {
            otherCanvas.gameObject.SetActive(true);
            GeneticAlgorithm.instance.StartSim();
            otherCanvas.alpha = 1;
            otherCanvas.blocksRaycasts = true;
            thisCanvas.SetActive(false);
        }
        else
        {
            errorText.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        Tabbing();
    }

    void Tabbing()
    {
        Selectable current = null;
        Selectable next = null;
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
        {
            if (EventSystem.current.currentSelectedGameObject == null)
                return;

            current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
            next = current.FindSelectableOnUp();

            if (next == null)
            {
                next = carTurnSpeed.GetComponent<Selectable>();
            }

            if (current == stopAtCrash.GetComponent<Selectable>())
            {
                next = NNprob.GetComponent<Selectable>();
            }
            next.Select();
        }
        else if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (EventSystem.current.currentSelectedGameObject == null)
                return;

            current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
            next = current.FindSelectableOnDown();

            if (next == null)
            {
                next = stopAtCrash.GetComponent<Selectable>();
            }

            if (current == carTurnSpeed.GetComponent<Selectable>())
            {
                next = generations.GetComponent<Selectable>();
            }
            next.Select();
        }

    }

    void NextButton()
    {
        parametersPanel.SetActive(false);
        fitnessPanel.SetActive(true);
        backButton.interactable = true;
        nextButton.GetComponentInChildren<Text>().text = "Start";
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(StartSimulation);
        CheckFitnessText();
    }

    void BackButton()
    {
        parametersPanel.SetActive(true);
        backButton.interactable = false;
        nextButton.GetComponentInChildren<Text>().text = "Next";
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextButton);
        fitnessPanel.SetActive(false);
    }

    bool CheckFitnessText()
    {
        if (fitnessInput.text == "")
            return false;

        ExpressionParser parser = new ExpressionParser();

        try
        {
            exp = parser.EvaluateExpression(fitnessInput.text);
        }
        catch
        {
            foreach (Text text in fitnessInput.GetComponentsInChildren<Text>())
            {
                text.color = Color.red;
            }
            return false;
        }
        List<string> expectedKeys = new List<string>() { "x", "t", "c", "l" };
        FitnessTracker.keys = new List<string>();
        foreach (string key in exp.Parameters.Keys)
        {
            bool isEqual = false;
            for (int i = 0; i < expectedKeys.Count; i++)
            {
                if (key == expectedKeys[i])
                {
                    isEqual = true;
                    FitnessTracker.keys.Add(key);
                    break;
                }
            }

            if (!isEqual)
            {
                foreach (Text text in fitnessInput.GetComponentsInChildren<Text>())
                {
                    text.color = Color.red;
                }
                return false;
            }
        }

        bool[] isInput = new bool[expectedKeys.Count];

        for (int i = 0; i < expectedKeys.Count; i++)
        {
            foreach (string key in FitnessTracker.keys)
            {
                if (key == expectedKeys[i])
                    isInput[i] = true;
            }
        }

        FitnessTracker.isInput = isInput;


        if (exp.Parameters.Count < 1 || exp.Parameters.Count > expectedKeys.Count)
        {
            foreach (Text text in fitnessInput.GetComponentsInChildren<Text>())
            {
                text.color = Color.red;
            }
            return false;
        }

        foreach (Text text in fitnessInput.GetComponentsInChildren<Text>())
        {
            text.color = Color.green;
        }

        
        FitnessTracker.fitnessDelegate = exp.ToDelegate(FitnessTracker.keys.ToArray());

        double[] input = new double[FitnessTracker.keys.Count];
        for (int i = 0; i < FitnessTracker.keys.Count; i++)
        {
            input[i] = 4;
        }
        try
        {
            FitnessTracker.fitnessDelegate.Invoke(input);
        }
        catch (System.Exception)
        {
            foreach (Text text in fitnessInput.GetComponentsInChildren<Text>())
            {
                text.color = Color.red;
            }
            return false;
        }

        return true;
    }

    void CheckButton()
    {
        CheckFitnessText();
    }
}
