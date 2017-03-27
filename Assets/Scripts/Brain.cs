using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Brain : MonoBehaviour {

    float distForw;
    float distTopLeft;
    float distTopRight;
    float distLeft;
    float distRight;
    float velocity;

    float acc;
    float turn;

    NeuralNetwork currentNetwork;

    public GameObject perceptron;
    public GameObject hSpace;
    public GameObject vSpace;
    public GameObject line;
    public GameObject maxLine;
    public GameObject avgLine;

    public Text maximumFitnessText;
    public Text generationText;
    public Text individualText;

    public int networksPerGeneration;
    public float networkMutationChance;
    public float weightsMutationChance;

    public float testTime;
    public float measureTime;

    Vector3 pos;

    Vector3 prevPos;

    List<List<NeuralNetwork>> networks = new List<List<NeuralNetwork>>();
    List<List<float>> fittness = new List<List<float>>();

    int generation = 0;
    int networkNum = 0;

    float maxFitness;

    public static float time;

    public static Brain instance;

    List<float> maxFitnesses = new List<float>();
    List<float> avgFitnesses = new List<float>();

    List<GameObject> lines = new List<GameObject>();

    public RectTransform plotArea;
    float plotWidth;
    float plotHeight;
    float margin;

    float prevTimeSetting;

    public LayerMask mask;

    List<float> posInputs = new List<float>();
    int passesInGen = 0;
    public static bool fiftyPass = false;

    // Use this for initialization
    void Start () {
        Application.runInBackground = true;
        instance = this;
        networks.Add(new List<NeuralNetwork>());
        fittness.Add(new List<float>());

        margin = plotArea.rect.width * 0.05f;
        plotWidth = plotArea.rect.width - margin;
        plotHeight = plotArea.rect.height - margin;

        for (int i =0; i< networksPerGeneration; i++)
        {
            networks[generation].Add(new NeuralNetwork(4, 8, 2));
        }

        currentNetwork = networks[0][0];

        generationText.text = "Generation: " + generation.ToString();
        individualText.text = "Individual: " + networkNum.ToString();
        maximumFitnessText.text = "Maximum fitness: " + maxFitness.ToString("F2");
        pos = transform.position;
        prevPos = transform.position;

        GetComponent<CarController>().Reset();
    }

    // Update is called once per frame
    void Update ()
    {


        time += Time.deltaTime;
        if(time - measureTime > testTime)
        {
            StopTest(false);
        }

        GetComponent<CarController>().SetInputs(acc, turn);
    }

    void FixedUpdate()
    {
        GetInput();
        GetOutput();

    }

    public void StopTest(bool pass)
    {
        acc = 0;
        turn = 0;


        networkNum++;

        if (pass)
        {
            passesInGen++;
        }
        float curFitness;
        if (!pass)
            curFitness = Mathf.Pow(DistanceTracker.instance.distance, 1f); /*/ ((DistanceTracker.instance.time - measureTime) + 0.001f)*/
        else
            curFitness = Mathf.Pow(DistanceTracker.instance.distance, 1f) + Mathf.Pow(DistanceTracker.instance.distance, 1f) / (DistanceTracker.instance.time - measureTime + 0.001f);

        if (curFitness < 0)
            curFitness = 0;
        fittness[generation].Add(curFitness);
        if (curFitness > maxFitness)
            maxFitness = curFitness;
        DistanceTracker.instance.NextTrack();

        generationText.text = "Generation: " + generation.ToString();
        individualText.text = "Individual: " + networkNum.ToString();
        maximumFitnessText.text = "Maximum fitness: " + maxFitness.ToString("F2");
        if (networkNum >= networksPerGeneration)
        {
            if ((float)passesInGen / (float)networksPerGeneration > 0.5f && !fiftyPass)
            {
                fiftyPass = true;
                DistanceTracker.instance.curTrackPoints = DistanceTracker.instance.trackpoints2;
            }

            CreateNexGen();
            UpdateGraph();

            passesInGen = 0;
        }

        currentNetwork = networks[generation][networkNum];
        //currentNetwork.VisualizeNetwork(perceptron, hSpace, vSpace, line, "child");
        measureTime = time;
        prevPos = transform.position;
        pos = transform.position;
        GetComponent<CarController>().Reset();


    }

    void CreateNexGen()
    {
        prevTimeSetting = Time.timeScale;
        Time.timeScale = 0;

        networks.Add(new List<NeuralNetwork>());
        fittness.Add(new List<float>());
        float sum = 0;
        for(int i = 0; i<networksPerGeneration; i++)
        {
            sum += fittness[generation][i];
        }

        avgFitnesses.Add(sum / networksPerGeneration);
        maxFitnesses.Add(maxFitness);

        if (sum > 0)
        {
            for (int i = 0; i < networksPerGeneration; i++)
            {
                fittness[generation][i] /= sum;
            }
        }

        sum = 0;
        for (int i = 0; i < networksPerGeneration; i++)
        {


            NeuralNetwork dad = null;
            NeuralNetwork mom = null;
            while (dad == null || mom == null)
            {
                float dadNum = (float)NeuralNetwork.rand.NextDouble();
                float momNum = (float)NeuralNetwork.rand.NextDouble();

                sum = 0;

                for (int j = 0; j < networksPerGeneration; j++)
                {
                    sum += fittness[generation][j];

                    if (dadNum < sum)
                    {
                        dadNum = 100;
                        dad = networks[generation][j];
                        if (dad != null && dad.Equals(mom))
                        {
                            dad = null;
                            mom = null;
                            break;
                        }
                    }
                    if (momNum < sum)
                    {
                        momNum = 100;
                        mom = networks[generation][j];
                        if (dad != null && dad.Equals(mom))
                        {
                            dad = null;
                            mom = null;
                            break;
                        }
                    }
                }
            }
            try
            {
                networks[generation + 1].Add(Gene.MakeChild(Gene.Encode(dad), Gene.Encode(mom), networkMutationChance, weightsMutationChance));
            }
            catch(System.Exception e)
            {
                i--;
            }
        }
        networkNum = 0;
        generation++;

        Time.timeScale = prevTimeSetting;

    }

    void UpdateGraph()
    {
        DestroyPlotLines();

        for (int i = 0; i < maxFitnesses.Count-1; i++)
        {
            Vector2 point1 = new Vector2(i * plotWidth / (maxFitnesses.Count -1) + margin/2, maxFitnesses[i] / maxFitness * plotHeight + margin / 2);
            Vector2 point2 = new Vector2((i + 1) * plotWidth / (maxFitnesses.Count-1) + margin / 2, maxFitnesses[i + 1] / maxFitness * plotHeight + margin / 2);

            float lineWidth = (point1 - point2).magnitude;
            float lineRot = Mathf.Atan2(point2.y - point1.y, point2.x - point1.x);

            GameObject line = (GameObject)Instantiate(maxLine, plotArea.transform);
            line.GetComponent<RectTransform>().sizeDelta = new Vector2(lineWidth, 3);
            line.GetComponent<RectTransform>().anchoredPosition = point1;
            line.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, lineRot * Mathf.Rad2Deg);

            lines.Add(line);

            point1 = new Vector2(i * plotWidth / (maxFitnesses.Count - 1) + margin / 2, avgFitnesses[i] / maxFitness * plotHeight + margin / 2);
            point2 = new Vector2((i + 1) * plotWidth / (maxFitnesses.Count - 1) + margin / 2, avgFitnesses[i + 1] / maxFitness * plotHeight + margin / 2);

            lineWidth = (point1 - point2).magnitude;
            lineRot = Mathf.Atan2(point2.y - point1.y, point2.x - point1.x);

            line = (GameObject)Instantiate(avgLine, plotArea.transform);
            line.GetComponent<RectTransform>().sizeDelta = new Vector2(lineWidth, 3);
            line.GetComponent<RectTransform>().anchoredPosition = point1;
            line.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, lineRot * Mathf.Rad2Deg);

            lines.Add(line);

        }
    }

    void DestroyPlotLines()
    {
        foreach(GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();
    }

    void GetInput()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity);
        distForw = Mathf.Clamp(hit.distance, 0, 100)/100;

        Physics.Raycast(transform.position, transform.forward + (2f / 3f) * transform.right, out hit, Mathf.Infinity);
        distTopRight = Mathf.Clamp(hit.distance, 0, 100) / 100;
        Debug.DrawRay(transform.position, transform.forward + (2f / 3f) * transform.right);

        Physics.Raycast(transform.position, transform.forward - (2f / 3f) * transform.right, out hit, Mathf.Infinity);
        distTopLeft = Mathf.Clamp(hit.distance, 0, 100) / 100;
        Debug.DrawRay(transform.position, transform.forward - (2f/3f) * transform.right);

        Physics.Raycast(transform.position, transform.right, out hit, Mathf.Infinity);
        distRight = Mathf.Clamp(hit.distance, 0, 100) / 100;
        Debug.DrawRay(transform.position, transform.right);

        Physics.Raycast(transform.position, -transform.right, out hit, Mathf.Infinity);
        distLeft = Mathf.Clamp(hit.distance, 0, 100) / 100;
        Debug.DrawRay(transform.position, -transform.right);

        velocity = GetComponent<CarController>().velocity.magnitude/5;

        posInputs.Clear();
        posInputs.AddRange(new List<float>() { distForw, distTopLeft, distTopRight, velocity, distLeft, distRight, 1, 1 });
    }

    void GetOutput()
    {

        List<float> inputs = posInputs.GetRange(0, currentNetwork.GetLayers()[0].Count);
        try
        {
            List<float> output = currentNetwork.GetOutput(inputs);
            for(int i= 0; i < output.Count; i++)
            {
                if (output[i] > 0f)
                    output[i] = 1;
                else
                    output[i] = -1;
            }

            //if ((Mathf.Abs(output[0] - 1) < 0.01f && Mathf.Abs(output[1] - 1) < 0.01f) || (Mathf.Abs(output[2] - 1) < 0.01f && Mathf.Abs(output[3] - 1) < 0.01f))
            //    StopTest(false);

            acc = output[0];
            turn = output[1];

            //if (acc < 0.01f && turn < 0.01f && GetComponent<CarController>().velocity.magnitude < 0.01f)
            //    StopTest(false);
        }
        catch(System.Exception e)
        {
            StopTest(false);
        }
    }
}
