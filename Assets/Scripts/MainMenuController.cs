using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using B83.ExpressionParser;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu Elements")]
    public GameObject mainPanel;
    public Text errorText;
    public Button startTraining;
    public Button challengeNetwork;
    public Button trainingOptions;
    public Button trackEditor;
    public Button QuitButton;

    [Header("Network Selector Elements")]
    public GameObject networkSelectorMenu;
    public Button selectorNetwork;
    public Button networkSelectorBack;

    [Header("Track Selector Elements")]
    public GameObject trackSelectorMenu;
    public GameObject trackPrefab;
    public Button selectorTrack;
    public Button trackSelectorBack;
    public Button RemoveButton;

    [Header("Training options panel elements")]
    public GameObject trainingOptionsPanel;
    public Button trainingParameters;
    public Button fitnessFunction;
    public Button trainingOptionsBackButton;
    public Button racingOptionsButton;

    [Header("Racing options panel elements")]
    public GameObject racingOptionsPanel;
    public Button applyButton;

    [Header("Training parameter panel elements")]
    public GameObject trainingParametersPanel;
    public Toggle advanceToggle;
    public Button trainingParamtersBackButton;
    public GameObject basicPanel;
    public GameObject advancePanel;

    [Header("Basic Section")]
    public InputField populationSizeSimple;
    public InputField simulationTimeSimple;
    public InputField numberOfLapsSimple;
    public Toggle stopAtCrashSimple;
    public InputField accelerationSimple;
    public InputField breakForceSimple;
    public InputField turnRateSimple;
    public InputField maximumVelocitySimple;

    [Header("Advanced Section")]
    public InputField populationSizeAdvanced;
    public InputField simulationTimeAdvanced;
    public InputField numberOfLapsAdvanced;
    public InputField simulationFps;
    public InputField savePercentage;
    public Toggle stopAtCrashAdvanced;
    public InputField accelerationAdvanced;
    public InputField breakForceAdvanced;
    public InputField turnRateAdvanced;
    public InputField maximumSpeedAdvanced;
    public InputField crossoverRate;
    public InputField networkInputs;
    public InputField networkOutputs;
    public InputField maximumSpecies;
    public InputField ageAllowedNoImprovement;
    public InputField compatibilityThreshold;
    public InputField survivalRate;
    public InputField youngAgeThreshold;
    public InputField oldAgeThreshold;
    public InputField youngFitnessBonus;
    public InputField oldFitnessBonus;
    public InputField perceptronAddProbability;
    public InputField synapseAddProbability;
    public InputField recurrentLinkProbability;
    public InputField activationMutationProbability;
    public InputField maxActivationPerturbation;
    public InputField weightMutationProbability;
    public InputField maxWeightPerturbation;
    public InputField weightReplacementProbability;
    public InputField maxPermittedPerceptrons;

    [Header("Fitness Function Elements")]
    public GameObject fitnessPanel;
    public InputField fitness;
    public Button fitnessAccept;

    [Header("Link Elements")]
    public GameObject trainingCanvas;
    public LoadTrackManager loadTrackManager;
    public LoadNetworksManager loadNetworksManager;
    public UIController uiController;
    public GameObject challengeMenu;

    public Toggle breakWithSpaceToggle;

    public LoadTrackManagerDuringTraining loadManagerDuringTraining;

    private void Awake()
    {
        Screen.SetResolution(1920, 1080, true);

    }
    void Start()
    {
        Screen.SetResolution(1920, 1080, true);

        InitializeMainMenu();
        InitializeNetworkSelectorPanel();
        InitializeTrainingOptions();
        InitializeTrackSelectorPanel();
        InitializeTrainingParameters();
        InitializeFitnessPanel();
    }

    // Initializes menu elements
    void InitializeMainMenu()
    {
        startTraining.onClick.AddListener(StartTraining);
        challengeNetwork.onClick.AddListener(ChallengeNetwork);
        trainingOptions.onClick.AddListener(TrainingOptions);
        trackEditor.onClick.AddListener(TrackEditor);
        QuitButton.onClick.AddListener(Quit);
    }

    void StartTraining()
    {
        trackSelectorMenu.SetActive(true);
        loadTrackManager.multiplePossible = true;
        trackSelectorBack.onClick.RemoveAllListeners();
        trackSelectorBack.onClick.AddListener(SelectorBack);
        selectorTrack.onClick.RemoveAllListeners();
        selectorTrack.onClick.AddListener(SelectorTrack);
        mainPanel.SetActive(false);
    }

    void ChallengeNetwork()
    {
        networkSelectorMenu.SetActive(true);
        mainPanel.SetActive(false);
    }

    void TrainingOptions()
    {
        mainPanel.SetActive(false);
        trainingOptionsPanel.SetActive(true);
    }

    void TrackEditor()
    {
        SceneManager.LoadScene(1);
    }

    void Quit()
    {
        Application.Quit();
    }

    // Initializes track selector panel
    void InitializeTrackSelectorPanel()
    {
        selectorTrack.onClick.AddListener(SelectorTrack);
        trackSelectorBack.onClick.AddListener(SelectorBack);
    }

    void SelectorTrack()
    {
        trainingCanvas.SetActive(true);
        CarTrainer.instance.StartSim();
        RacingCanvasController.toTrain = true;
        transform.parent.gameObject.SetActive(false);
    }

    void SelectorBack()
    {
        trackSelectorMenu.SetActive(false);
        mainPanel.SetActive(true);
    }

    void SelectorBackToNetwork()
    {
        trackSelectorMenu.SetActive(false);
        networkSelectorMenu.SetActive(true);
    }

    // Initializes track selector panel
    void InitializeNetworkSelectorPanel()
    {
        selectorNetwork.onClick.AddListener(SelectorNetwork);
        networkSelectorBack.onClick.AddListener(SelectorNetworkBack);
    }

    void SelectorNetwork()
    {
        //if (loadNetworksManager.currentNetworks.Count == 0)
        //    return;

        trainingCanvas.SetActive(true);
        trainingCanvas.SetActive(false);

        trackSelectorMenu.SetActive(true);
        loadTrackManager.multiplePossible = false;

        uiController.activeNetworks.Clear();
        uiController.activeNetworks.AddRange(loadNetworksManager.currentNetworks);
        trackSelectorBack.onClick.RemoveAllListeners();
        trackSelectorBack.onClick.AddListener(SelectorBackToNetwork);
        selectorTrack.onClick.RemoveAllListeners();
        selectorTrack.onClick.AddListener(StartChallengeNetwork);
        networkSelectorMenu.SetActive(false);

        if (loadNetworksManager.currentNetworks.Count != 0)
        {
            loadNetworksManager.currentNetworks.Clear();
        }
    }

    void SelectorNetworkBack()
    {

        networkSelectorMenu.SetActive(false);
        mainPanel.SetActive(true);
        loadNetworksManager.currentNetworks.Clear();
    }

    void StartChallengeNetwork()
    {
        if (loadTrackManager.selectedTrackNames.Count == 0)
            return;

        trainingCanvas.SetActive(true);
        trainingCanvas.GetComponent<CanvasGroup>().alpha = 0;
        //TrackManager manager = Instantiate(trackPrefab).GetComponent<TrackManager>();
        RacingCanvasController.toTrain = false;

        StartCoroutine(_StartChallengeNetwork());

    }

    IEnumerator _StartChallengeNetwork()
    {
        yield return null;
        uiController.Challenge(loadTrackManager.selectedTrackNames);
        loadManagerDuringTraining.selectedTrackNames = loadTrackManager.selectedTrackNames;
        //challengeMenu.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }

    // Initializes training options panel
    void InitializeTrainingOptions()
    {
        trainingParameters.onClick.AddListener(TrainingParameters);
        fitnessFunction.onClick.AddListener(FitnessFunction);
        trainingOptionsBackButton.onClick.AddListener(TrainingOptionsBack);
        racingOptionsButton.onClick.AddListener(RacingOptions);
        applyButton.onClick.AddListener(ApplyRacingOptions);
    }

    void TrainingParameters()
    {
        trainingOptionsPanel.SetActive(false);
        trainingParametersPanel.SetActive(true);
    }

    void FitnessFunction()
    {
        fitnessPanel.SetActive(true);
        trainingOptionsPanel.SetActive(false);
    }

    void TrainingOptionsBack()
    {
        mainPanel.SetActive(true);
        trainingOptionsPanel.SetActive(false);
    }

    void InitializeTrainingParameters()
    {
        trainingParamtersBackButton.onClick.AddListener(TrainingParametersBack);
        advanceToggle.onValueChanged.AddListener(AdvancedToggle);

        InitializeTrainingParametersBasic();
        InitializeTrainingParametersAdvanced();
    }

    void TrainingParametersBack()
    {
        advanceToggle.isOn = false;
        trainingOptionsPanel.SetActive(true);
        trainingParametersPanel.SetActive(false);
        basicPanel.SetActive(true);
        advancePanel.SetActive(false);
    }

    void AdvancedToggle(bool isOn)
    {
        if(isOn)
        {
            advancePanel.SetActive(true);
            basicPanel.SetActive(false);
        }
        else
        {
            advancePanel.SetActive(false);
            basicPanel.SetActive(true);
        }
    }

    void RacingOptions()
    {
        racingOptionsPanel.SetActive(true);
        trainingOptionsPanel.SetActive(false);

    }

    void ApplyRacingOptions()
    {
        racingOptionsPanel.SetActive(false);
        trainingOptionsPanel.SetActive(true);
    }

    // Initialize the training parameters options basic panel
    void InitializeTrainingParametersBasic()
    {
        populationSizeSimple.onValueChanged.AddListener(PopulationSizeSimple);

        // population size
        int parInt = PlayerPrefs.GetInt("populationSize", -1);
        if (parInt == -1)
            populationSizeSimple.text = 50.ToString();
        else
            populationSizeSimple.text = parInt.ToString();

        breakWithSpaceToggle.onValueChanged.AddListener(SetSpaceBarBreaking);

        // Space bar braking
        parInt = PlayerPrefs.GetInt("brake_w_space", -1);
        if (parInt == -1)
            breakWithSpaceToggle.isOn = false;
        else
        {
            if (parInt == 1)
            {
                breakWithSpaceToggle.isOn = true;
                GA_Parameters.breakWithSpace = true;
            }
            else
                breakWithSpaceToggle.isOn = false;
        }


        simulationTimeSimple.onValueChanged.AddListener(SimulationTimeSimple);

        // simulation time
        float parFloat = PlayerPrefs.GetFloat("simulationTime", -1f);
        if (parFloat == -1)
            simulationTimeSimple.text = 30.ToString();
        else
            simulationTimeSimple.text = parFloat.ToString();

        numberOfLapsSimple.onValueChanged.AddListener(NumberOfLapsSimple);

        // number of laps
        parInt = PlayerPrefs.GetInt("numberOfLaps", -1);
        if (parInt == -1)
            numberOfLapsSimple.text = 1.ToString();
        else
            numberOfLapsSimple.text = parInt.ToString();

        stopAtCrashSimple.onValueChanged.AddListener(StopAtCrashSimple);

        // stop at crash
        parInt = PlayerPrefs.GetInt("stopAtCrash", -1);
        if (parInt == -1)
            stopAtCrashSimple.isOn = true;
        else
        {
            if (parInt == 1)
                stopAtCrashSimple.isOn = true;
            else
                stopAtCrashSimple.isOn = false;
        }
        GA_Parameters.stopAtCrash = stopAtCrashSimple.isOn;

        accelerationSimple.onValueChanged.AddListener(AccelerationSimple);

        // acceleration
        parFloat = PlayerPrefs.GetFloat("acceleration", -1);
        if (parFloat == -1)
            accelerationSimple.text = 10.ToString();
        else
            accelerationSimple.text = parFloat.ToString();

        breakForceSimple.onValueChanged.AddListener(BreakForceSimple);

        // break force
        parFloat = PlayerPrefs.GetFloat("breakForce", -1);
        if (parFloat == -1)
            breakForceSimple.text = 20.ToString();
        else
            breakForceSimple.text = parFloat.ToString();

        turnRateSimple.onValueChanged.AddListener(TurnRateSimple);

        // turn Rate
        parFloat = PlayerPrefs.GetFloat("turnRate", -1);
        if (parFloat == -1)
            turnRateSimple.text = 80.ToString();
        else
            turnRateSimple.text = parFloat.ToString();

        maximumVelocitySimple.onValueChanged.AddListener(MaximumVelocitySimple);

        // maximim velocity
        parFloat = PlayerPrefs.GetFloat("maximumVelocity", -1);
        if (parFloat == -1)
            maximumVelocitySimple.text = 50.ToString();
        else
            maximumVelocitySimple.text = parFloat.ToString();

    }

    void InitializeTrainingParametersAdvanced()
    {
        populationSizeAdvanced.onValueChanged.AddListener(PopulationSizeAdvanced);

        // population size
        int parInt = PlayerPrefs.GetInt("populationSize", -1);
        if (parInt == -1)
            populationSizeAdvanced.text = 50.ToString();
        else
            populationSizeAdvanced.text = parInt.ToString();

        simulationTimeAdvanced.onValueChanged.AddListener(SimulationTimeAdvanced);

        // simulation time
        float parFloat = PlayerPrefs.GetFloat("simulationTime", -1f);
        if (parFloat == -1)
            simulationTimeAdvanced.text = 100.ToString();
        else
            simulationTimeAdvanced.text = parFloat.ToString();

        numberOfLapsAdvanced.onValueChanged.AddListener(NumberOfLapsAdvanced);

        // number of laps
        parInt = PlayerPrefs.GetInt("numberOfLaps", -1);
        if (parInt == -1)
            numberOfLapsAdvanced.text = 3.ToString();
        else
            numberOfLapsAdvanced.text = parInt.ToString();

        simulationFps.onValueChanged.AddListener(SimulationFPS);

        // simulation FPS
        parInt = PlayerPrefs.GetInt("simulationFPS", -1);
        if (parInt == -1)
            simulationFps.text = 60.ToString();
        else
            simulationFps.text = parInt.ToString();

        savePercentage.onValueChanged.AddListener(SavePercentage);

        // save percentage
        parFloat = PlayerPrefs.GetFloat("savePercentage", -1);
        if (parFloat == -1)
            savePercentage.text = 100f.ToString();
        else
            savePercentage.text = parFloat.ToString();
        stopAtCrashAdvanced.onValueChanged.AddListener(StopAtCrashAdvanced);

        // stop at crash
        parInt = PlayerPrefs.GetInt("stopAtCrash", -1);
        if (parInt == -1)
            stopAtCrashSimple.isOn = true;
        else
        {
            if (parInt == 1)
                stopAtCrashAdvanced.isOn = true;
            else
                stopAtCrashAdvanced.isOn = false;
        }

        accelerationAdvanced.onValueChanged.AddListener(AccelerationAdvanced);

        // acceleration
        parFloat = PlayerPrefs.GetFloat("acceleration", -1);
        if (parFloat == -1)
            accelerationAdvanced.text = 10.ToString();
        else
            accelerationAdvanced.text = parFloat.ToString();

        breakForceAdvanced.onValueChanged.AddListener(BreakForceAdvanced);

        // break force
        parFloat = PlayerPrefs.GetFloat("breakForce", -1);
        if (parFloat == -1)
            breakForceAdvanced.text = 20.ToString();
        else
            breakForceAdvanced.text = parFloat.ToString();

        turnRateAdvanced.onValueChanged.AddListener(TurnRateAdvanced);

        // turn Rate
        parFloat = PlayerPrefs.GetFloat("turnRate", -1);
        if (parFloat == -1)
            turnRateAdvanced.text = 80.ToString();
        else
            turnRateAdvanced.text = parFloat.ToString();

        maximumSpeedAdvanced.onValueChanged.AddListener(MaximumVelocityAdvanced);

        // maximim velocity
        parFloat = PlayerPrefs.GetFloat("maximumVelocity", -1);
        if (parFloat == -1)
            maximumSpeedAdvanced.text = 50.ToString();
        else
            maximumSpeedAdvanced.text = parFloat.ToString();

        crossoverRate.onValueChanged.AddListener(CrossoverRate);

        // crossover Rate
        parFloat = PlayerPrefs.GetFloat("crossoverRate", -1);
        if (parFloat == -1)
            crossoverRate.text = 0.7f.ToString();
        else
            crossoverRate.text = parFloat.ToString();

        networkInputs.onValueChanged.AddListener(NetworkInputs);

        // network Inputs
        parInt = PlayerPrefs.GetInt("networkInputs", -1);
        if (parInt == -1)
            networkInputs.text = 30.ToString();
        else
            networkInputs.text = parInt.ToString();

        networkOutputs.onValueChanged.AddListener(NetworkOutputs);

        // network OutPuts
        parInt = PlayerPrefs.GetInt("networkOutputs", -1);
        if (parInt == -1)
            networkOutputs.text = 6.ToString();
        else
            networkOutputs.text = parInt.ToString();

        maximumSpecies.onValueChanged.AddListener(MaximumSpecies);

        // maximum species
        parInt = PlayerPrefs.GetInt("maximumSpecies", -1);
        if (parInt == -1)
            maximumSpecies.text = 20.ToString();
        else
            maximumSpecies.text = parInt.ToString();

        ageAllowedNoImprovement.onValueChanged.AddListener(AgeAllowedNoImprovements);

        // age allowed no improvement
        parInt = PlayerPrefs.GetInt("ageAllowedNoImprovements", -1);
        if (parInt == -1)
            ageAllowedNoImprovement.text = 30.ToString();
        else
            ageAllowedNoImprovement.text = parInt.ToString();

        compatibilityThreshold.onValueChanged.AddListener(CompatibilityThreshold);

        // compatibility threshold
        parFloat = PlayerPrefs.GetFloat("compatibilityThreshold", -1);
        if (parFloat == -1)
            compatibilityThreshold.text = 0.33f.ToString();
        else
            compatibilityThreshold.text = parFloat.ToString();

        survivalRate.onValueChanged.AddListener(SurvivalRate);

        // survival rate
        parFloat = PlayerPrefs.GetFloat("survivalRate", -1);
        if (parFloat == -1)
            survivalRate.text = 0.7f.ToString();
        else
            survivalRate.text = parFloat.ToString();

        youngAgeThreshold.onValueChanged.AddListener(YoungAgeThreshold);

        // young age threshold
        parInt = PlayerPrefs.GetInt("youngAgeThreshold", -1);
        if (parInt == -1)
            youngAgeThreshold.text = 10.ToString();
        else
            youngAgeThreshold.text = parInt.ToString();

        oldAgeThreshold.onValueChanged.AddListener(OldAgeThreshold);

        // old age threshold
        parInt = PlayerPrefs.GetInt("oldAgeThreshold", -1);
        if (parInt == -1)
            oldAgeThreshold.text = 40.ToString();
        else
            oldAgeThreshold.text = parInt.ToString();

        youngFitnessBonus.onValueChanged.AddListener(YoungFitnessBonus);

        // young fitness Bonus
        parFloat = PlayerPrefs.GetFloat("youngFitnessBonus", -1);
        if (parFloat == -1)
            youngFitnessBonus.text = 1.3f.ToString();
        else
            youngFitnessBonus.text = parFloat.ToString();

        oldFitnessBonus.onValueChanged.AddListener(OldFitnessPenalty);

        // old fitness Bonus
        parFloat = PlayerPrefs.GetFloat("oldFitnessPenalty", -1);
        if (parFloat == -1)
            oldFitnessBonus.text = 0.7f.ToString();
        else
            oldFitnessBonus.text = parFloat.ToString();

        perceptronAddProbability.onValueChanged.AddListener(PerceptronAddProbability);

        // perceptron add probability
        parFloat = PlayerPrefs.GetFloat("perceptronAddProbability", -1);
        if (parFloat == -1)
            perceptronAddProbability.text = 0.1f.ToString();
        else
            perceptronAddProbability.text = parFloat.ToString();

        synapseAddProbability.onValueChanged.AddListener(SynapseAddProbability);

        // synapse add probability
        parFloat = PlayerPrefs.GetFloat("synapseAddProbability", -1);
        if (parFloat == -1)
            synapseAddProbability.text = 0.15f.ToString();
        else
            synapseAddProbability.text = parFloat.ToString();

        recurrentLinkProbability.onValueChanged.AddListener(RecurrentLinkProbability);

        // recurrent link probability
        parFloat = PlayerPrefs.GetFloat("recurrentLinkProbability", -1);
        if (parFloat == -1)
            recurrentLinkProbability.text = 0.1f.ToString();
        else
            recurrentLinkProbability.text = parFloat.ToString();

        activationMutationProbability.onValueChanged.AddListener(ActivationMutationProbability);

        // activation mutation probability
        parFloat = PlayerPrefs.GetFloat("activationMutationProbability", -1);
        if (parFloat == -1)
            activationMutationProbability.text = 0.1f.ToString();
        else
            activationMutationProbability.text = parFloat.ToString();

        maxActivationPerturbation.onValueChanged.AddListener(MaxActivationPerturbation);

        // max activation perturbation
        parFloat = PlayerPrefs.GetFloat("maxActivationPerturbation", -1);
        if (parFloat == -1)
            maxActivationPerturbation.text = 0.5f.ToString();
        else
            maxActivationPerturbation.text = parFloat.ToString();

        weightMutationProbability.onValueChanged.AddListener(WeightMutationProbability);

        // weight mutation probability
        parFloat = PlayerPrefs.GetFloat("weightMutationProbability", -1);
        if (parFloat == -1)
            weightMutationProbability.text = 0.1f.ToString();
        else
            weightMutationProbability.text = parFloat.ToString();

        maxWeightPerturbation.onValueChanged.AddListener(MaxWeightPerturbation);

        // max activation perturbation
        parFloat = PlayerPrefs.GetFloat("maxWeightPerturbation", -1);
        if (parFloat == -1)
            maxWeightPerturbation.text = 0.5f.ToString();
        else
            maxWeightPerturbation.text = parFloat.ToString();

        weightReplacementProbability.onValueChanged.AddListener(WeightReplacementProbability);

        // weight replacement probability
        parFloat = PlayerPrefs.GetFloat("weightReplacementProbability", -1);
        if (parFloat == -1)
            weightReplacementProbability.text = 0.1f.ToString();
        else
            weightReplacementProbability.text = parFloat.ToString();

        maxPermittedPerceptrons.onValueChanged.AddListener(MaxPermittedPerceptrons);

        // max permitted perceptrons
        parInt = PlayerPrefs.GetInt("maxPermittedPerceptrons", -1);
        if (parInt == -1)
            maxPermittedPerceptrons.text = 400.ToString();
        else
            maxPermittedPerceptrons.text = parInt.ToString();

    }

    void PopulationSizeSimple(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        if (par < 1)
            par = 1;
        GA_Parameters.populationSize = par;
        populationSizeAdvanced.text = populationSizeSimple.text;

        PlayerPrefs.SetInt("populationSize", par);
    }

    public void SetSpaceBarBreaking(bool isOn)
    {
        GA_Parameters.breakWithSpace = isOn;


        if (isOn)
            PlayerPrefs.SetInt("brake_w_space", 1);
        else
            PlayerPrefs.SetInt("brake_w_space", 0);


    }

    void SimulationTimeSimple(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.simulationTime = par;
        simulationTimeAdvanced.text = simulationTimeSimple.text;

        PlayerPrefs.SetFloat("simulationTime", par);

    }

    void NumberOfLapsSimple(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        if (par < 1)
            par = 1;
        GA_Parameters.laps = par;
        numberOfLapsAdvanced.text = numberOfLapsSimple.text;

        PlayerPrefs.SetInt("numberOfLaps", par);

    }

    void StopAtCrashSimple(bool isOn)
    {
        stopAtCrashAdvanced.isOn = isOn;
        if(isOn)
            PlayerPrefs.SetInt("stopAtCrash", 1);
        else
            PlayerPrefs.SetInt("stopAtCrash", 0);

        GA_Parameters.stopAtCrash = isOn;
    }

    void AccelerationSimple(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.accSpeed = par;
        accelerationAdvanced.text = accelerationSimple.text;

        PlayerPrefs.SetFloat("acceleration", par);

    }

    void BreakForceSimple(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.breakSpeed = par;
        breakForceAdvanced.text = breakForceSimple.text;

        PlayerPrefs.SetFloat("breakForce", par);

    }

    void TurnRateSimple(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.turnSpeed = par;
        turnRateAdvanced.text = turnRateSimple.text;

        PlayerPrefs.SetFloat("turnRate", par);

    }

    void MaximumVelocitySimple(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.maxSpeed = par;
        maximumSpeedAdvanced.text = maximumVelocitySimple.text;

        PlayerPrefs.SetFloat("maximumVelocity", par);
    }

    // Advanced Panel
    void PopulationSizeAdvanced(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        if (par < 1)
            par = 1;
        GA_Parameters.populationSize = par;
        populationSizeSimple.text = populationSizeAdvanced.text;

        PlayerPrefs.SetInt("populationSize", par);
    }

    void SimulationTimeAdvanced(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.simulationTime = par;
        simulationTimeSimple.text = simulationTimeAdvanced.text;

        PlayerPrefs.SetFloat("simulationTime", par);

    }

    void NumberOfLapsAdvanced(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        if (par < 1)
            par = 1;
        GA_Parameters.laps = par;
        numberOfLapsSimple.text = numberOfLapsAdvanced.text;

        PlayerPrefs.SetInt("numberOfLaps", par);

    }

    void SimulationFPS(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        if (par < 1)
            par = 1;
        GA_Parameters.fps = par;

        PlayerPrefs.SetInt("simulationFPS", par);

    }

    void SavePercentage(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par > 100)
            par = 100;
        else if (par < 0)
            par = 0;

        GA_Parameters.savePercentage = par;

        PlayerPrefs.SetFloat("savePercentage", par);
    }

    void StopAtCrashAdvanced(bool isOn)
    {
        stopAtCrashSimple.isOn = isOn;
        if (isOn)
            PlayerPrefs.SetInt("stopAtCrash", 1);
        else
            PlayerPrefs.SetInt("stopAtCrash", 0);

        GA_Parameters.stopAtCrash = isOn;

    }

    void AccelerationAdvanced(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.accSpeed = par;
        accelerationSimple.text = accelerationAdvanced.text;

        PlayerPrefs.SetFloat("acceleration", par);

    }

    void BreakForceAdvanced(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.breakSpeed = par;
        breakForceSimple.text = breakForceAdvanced.text;

        PlayerPrefs.SetFloat("breakForce", par);

    }

    void TurnRateAdvanced(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.turnSpeed = par;
        turnRateSimple.text = turnRateAdvanced.text;

        PlayerPrefs.SetFloat("turnRate", par);

    }

    void MaximumVelocityAdvanced(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.maxSpeed = par;
        maximumVelocitySimple.text = maximumSpeedAdvanced.text;

        PlayerPrefs.SetFloat("maximumVelocity", par);
    }

    void CrossoverRate(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par > 1)
            par = 1;
        else if (par < 0)
            par = 0;

        GA_Parameters.crossOverRate = par;

        PlayerPrefs.SetFloat("crossoverRate", par);
    }

    void NetworkInputs(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        if (par < 2)
            par = 2;

        GA_Parameters.inputs = par;

        PlayerPrefs.SetInt("networkInputs", par);

    }

    void NetworkOutputs(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        par = 6;
        GA_Parameters.outputs = par;

        PlayerPrefs.SetInt("networkOutputs", par);

    }

    void MaximumSpecies(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        if (par < 1)
            par = 1;
        GA_Parameters.maxSpecies = par;

        PlayerPrefs.SetInt("maximumSpecies", par);

    }

    void AgeAllowedNoImprovements(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        if (par < 1)
            par = 1;
        GA_Parameters.numGensAllowedNoImprovement = par;

        PlayerPrefs.SetInt("ageAllowedNoImprovements", par);

    }

    void CompatibilityThreshold(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par > 1)
            par = 1;
        else if (par < 0)
            par = 0;

        GA_Parameters.compatibilityThreshold = par;

        PlayerPrefs.SetFloat("compatibilityThreshold", par);
    }

    void SurvivalRate(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par > 1)
            par = 1;
        else if (par < 0)
            par = 0;

        GA_Parameters.survivalRate = par;

        PlayerPrefs.SetFloat("survivalRate", par);
    }

    void YoungAgeThreshold(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.youngBonusAgeThreshhold = par;

        PlayerPrefs.SetInt("youngAgeThreshold", par);

    }

    void OldAgeThreshold(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.oldAgeThreshHold = par;

        PlayerPrefs.SetInt("oldAgeThreshold", par);

    }

    void YoungFitnessBonus(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par < 1)
            par = 1;

        GA_Parameters.youngFitnessBonus = par;

        PlayerPrefs.SetFloat("youngFitnessBonus", par);
    }

    void OldFitnessPenalty(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par < 0)
            par = 0;
        else if (par > 1)
            par = 1;

        GA_Parameters.oldAgePenalty = par;

        PlayerPrefs.SetFloat("oldFitnessPenalty", par);
    }

    void PerceptronAddProbability(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par > 1)
            par = 1;
        else if (par < 0)
            par = 0;

        GA_Parameters.chanceAddPerceptron = par;

        PlayerPrefs.SetFloat("perceptronAddProbability", par);
    }

    void SynapseAddProbability(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par > 1)
            par = 1;
        else if (par < 0)
            par = 0;

        GA_Parameters.chanceToAddLink = par;

        PlayerPrefs.SetFloat("synapseAddProbability", par);
    }

    void RecurrentLinkProbability(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par > 1)
            par = 1;
        else if (par < 0)
            par = 0;

        GA_Parameters.chanceRecurrentLink = par;

        PlayerPrefs.SetFloat("recurrentLinkProbability", par);
    }

    void ActivationMutationProbability(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par > 1)
            par = 1;
        else if (par < 0)
            par = 0;

        GA_Parameters.activationMutationChance = par;

        PlayerPrefs.SetFloat("activationMutationProbability", par);
    }

    void MaxActivationPerturbation(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par < 0)
            par = 0;

        GA_Parameters.maxActivationPerturbation = par;

        PlayerPrefs.SetFloat("maxActivationPerturbation", par);
    }

    void WeightMutationProbability(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par > 1)
            par = 1;
        else if (par < 0)
            par = 0;

        GA_Parameters.weightMutationRate = par;

        PlayerPrefs.SetFloat("weightMutationProbability", par);
    }

    void MaxWeightPerturbation(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par < 0)
            par = 0;

        GA_Parameters.maxWeightPerturbation = par;

        PlayerPrefs.SetFloat("maxWeightPerturbation", par);
    }

    void WeightReplacementProbability(string text)
    {
        if (text == "")
            return;

        float par = float.Parse(text);

        if (par > 1)
            par = 1;
        else if (par < 0)
            par = 0;

        GA_Parameters.weightReplaceProb = par;

        PlayerPrefs.SetFloat("weightReplacementProbability", par);
    }

    void MaxPermittedPerceptrons(string text)
    {
        if (text == "")
            return;

        int par = int.Parse(text);
        if (par < 0)
            par = 0;
        GA_Parameters.maxPermittedPerceptrons = par;

        PlayerPrefs.SetInt("maxPermittedPerceptrons", par);

    }

    void InitializeFitnessPanel()
    {
        fitnessAccept.onClick.AddListener(FitnessAccept);
        fitness.onValueChanged.AddListener(Fitness);

        if (PlayerPrefs.GetString("fitnessEQ") != "")
            fitness.text = PlayerPrefs.GetString("fitnessEQ");
        else
            fitness.text = "x + x/t * l";

        CheckFitnessText();
    }

    void FitnessAccept()
    {
        if (CheckFitnessText())
        {
            fitnessPanel.SetActive(false);
            trainingOptionsPanel.SetActive(true);
        }
    }

    void Fitness(string text)
    {
        foreach(Text inputText in fitness.GetComponentsInChildren<Text>())
            inputText.color = Color.black;
    }


    Expression exp;

    void Update()
    {
        //Tabbing();
    }

    //void Tabbing()
    //{
    //    Selectable current = null;
    //    Selectable next = null;
    //    if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
    //    {
    //        if (EventSystem.current.currentSelectedGameObject == null)
    //            return;

    //        current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
    //        next = current.FindSelectableOnUp();

    //        if (next == null)
    //        {
    //            next = carTurnSpeed.GetComponent<Selectable>();
    //        }

    //        if (current == stopAtCrash.GetComponent<Selectable>())
    //        {
    //            next = NNprob.GetComponent<Selectable>();
    //        }
    //        next.Select();
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Tab))
    //    {
    //        if (EventSystem.current.currentSelectedGameObject == null)
    //            return;

    //        current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
    //        next = current.FindSelectableOnDown();

    //        if (next == null)
    //        {
    //            next = stopAtCrash.GetComponent<Selectable>();
    //        }

    //        if (current == carTurnSpeed.GetComponent<Selectable>())
    //        {
    //            next = generations.GetComponent<Selectable>();
    //        }
    //        next.Select();
    //    }

    //}

    bool CheckFitnessText()
    {
        if (fitness.text == "")
            return false;

        ExpressionParser parser = new ExpressionParser();

        try
        {
            exp = parser.EvaluateExpression(fitness.text);
        }
        catch
        {
            foreach (Text text in fitness.GetComponentsInChildren<Text>())
            {
                text.color = Color.red;
            }
            return false;
        }
        List<string> expectedKeys = new List<string>() { "x", "t", "c", "l", "L", "f", "Vmax", "n" };
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
                foreach (Text text in fitness.GetComponentsInChildren<Text>())
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
            foreach (Text text in fitness.GetComponentsInChildren<Text>())
            {
                text.color = Color.red;
            }
            return false;
        }

        foreach (Text text in fitness.GetComponentsInChildren<Text>())
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
            foreach (Text text in fitness.GetComponentsInChildren<Text>())
            {
                text.color = Color.red;
            }
            return false;
        }

        PlayerPrefs.SetString("fitnessEQ", fitness.text);
        return true;
    }



}
