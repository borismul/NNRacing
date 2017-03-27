using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public InputField generations;
    public InputField individuals;
    public InputField carSimPerFrame;
    public InputField simTime;
    public InputField simFPS;
    public InputField WeightsProb;
    public InputField NNprob;

    public InputField maxCarSpeed;
    public InputField carAccSpeed;
    public InputField carTurnSpeed;

    public Button startButton;

    public NewCarController car;
    public CanvasGroup otherCanvas;

    public Text errorText;

    void Start()
    {
        otherCanvas.alpha = 0;
        otherCanvas.blocksRaycasts = false;

        generations.onValueChanged.AddListener(Generations);
        individuals.onValueChanged.AddListener(Individuals);
        carSimPerFrame.onValueChanged.AddListener(CarSimPerFrame);
        simTime.onValueChanged.AddListener(SimTime);
        simFPS.onValueChanged.AddListener(SimFPS);
        WeightsProb.onValueChanged.AddListener(WeightMut);
        NNprob.onValueChanged.AddListener(NNMut);
        maxCarSpeed.onValueChanged.AddListener(MaxSpeed);
        carAccSpeed.onValueChanged.AddListener(AccSpeed);
        carTurnSpeed.onValueChanged.AddListener(TurnSpeed);
        startButton.onClick.AddListener(StartSimulation);
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
        GeneticAlgorithm.instance.simulationTime = par;
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
        car.maxSpeed = par;
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
        car.accSpeed = par;
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
        car.turnSpeed = par;
    }

    void StartSimulation()
    {
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
            GeneticAlgorithm.instance.StartSim();
            otherCanvas.alpha = 1;
            otherCanvas.blocksRaycasts = true;
        }
        else
        {
            errorText.gameObject.SetActive(true);
        }
    }

}
