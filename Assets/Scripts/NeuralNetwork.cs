using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class NeuralNetwork
{
    List<List<Perceptron>> layers;
    List<List<Perceptron>> fullNetwork;
    public int inputPerceptrons;
    public int outputPerceptrons;
    public int hiddenLayers;
    public int totalPerceptrons;
    public int maxPerceptronsInLayer;
    public float backStepChance;
    public List<List<Text>> output;
    public float Fitness;

    public static System.Random rand = new System.Random();

    public NeuralNetwork()
    {
        layers = new List<List<Perceptron>>();
        fullNetwork = new List<List<Perceptron>>();
    }

    public NeuralNetwork(params int[] network)
    {
        if (network.Length < 2)
        {
            Debug.LogError("Cannot create a neural network with 1 layer of perceptrons");
            return;
        }
        layers = new List<List<Perceptron>>();
        fullNetwork = new List<List<Perceptron>>();

        inputPerceptrons = network[0];
        outputPerceptrons = network[network.Length - 1];
        hiddenLayers = network.Length - 2;

        CreatePerceptrons(network);
        LinkPerceptrons(network);
        SetWeights();
    }

    public NeuralNetwork(int networkPathsPerInput, float backStepChance, params int[] network)
    {
        if (network.Length < 2)
        {
            Debug.LogError("Cannot create a neural network with 1 layer of perceptrons");
            return;
        }
        layers = new List<List<Perceptron>>();
        fullNetwork = new List<List<Perceptron>>();

        inputPerceptrons = network[0];
        outputPerceptrons = network[network.Length - 1];
        hiddenLayers = network.Length - 2;
        this.backStepChance = backStepChance;

        CreatePerceptrons(network);
        LinkPerceptrons(network, networkPathsPerInput);
        SetWeights();
    }

    void CreatePerceptrons(int[] network)
    {
        // Loop through the network layers
        for (int i = 0; i < Gene.maximumAllowedLayers; i++)
        {
            fullNetwork.Add(new List<Perceptron>());
            // Loop through each of the perceptron inside the layer
            for (int j = 0; j < Gene.maximumAllowedPerceptronsInLayer; j++)
            {
                if (i >= network.Length || j >= network[i])
                {
                    fullNetwork[i].Add(null);
                    continue;
                }
                // If we add the first node, first create a new layer list
                if (j == 0)
                    layers.Add(new List<Perceptron>());

                // Add perceptron
                layers[i].Add(new Perceptron(i, j, totalPerceptrons, this));
                fullNetwork[i].Add(layers[i][j]);
                totalPerceptrons++;

                if (maxPerceptronsInLayer < j)
                    maxPerceptronsInLayer = j;
            }
        }
    }

    // Link the perceptrons in a orderly fashion (i.e. first layer is connected to second etc)
    void LinkPerceptrons(int[] network)
    {
        // Link the input layer to itself to get a weight for the inputs
        for (int i = 0; i < network[0]; i++)
        {
            layers[0][i].LinkPerceptrons(layers[0][i]);
        }
        // Loop through each of the layers
        for (int i = 0; i < network.Length; i++)
        {
            // Loop through each of the perceptron in the current layer
            for (int j = 0; j < network[i]; j++)
            {
                // If this is not yet the first to last layer connect all perceptrons in the current layer as input layer
                // to the next layer as output layer
                if (layers.Count - 1 > i)
                {
                    for (int k = 0; k < network[i + 1]; k++)
                    {
                        // Link perceptron j in layer i to perceptron k in layer i + 1
                        layers[i][j].LinkPerceptrons(layers[i + 1][k]);
                    }
                }
            }
        }
    }

    // Link the perceptrons in a unorderly fashion (all layers can be connected with all layers)
    void LinkPerceptrons(int[] network, int networkPathsPerInput)
    {

        // Link the input layer to itself to get a weight for the inputs
        for (int i = 0; i < network[0]; i++)
        {
            layers[0][i].LinkPerceptrons(layers[0][i]);
        }

        for (int i = 0; i < inputPerceptrons; i++)
        {
            for (int j = 0; j < networkPathsPerInput; j++)
            {
                CreatePath(layers[0][i]);
            }
        }

        foreach (Perceptron perceptron in GetOutputLayer())
        {
            while (perceptron.inputConnections.Count == 0)
            {
                CreatePath(layers[0][rand.Next(0, network[0])]);
            }
        }
        // Check if a perceptron does not have any input or output perceptrons, if so remove it
        CleanUpNetwork();
        RemoveEmptyLayers();
    }

    void CreatePath(Perceptron startPerceptron)
    {
        int iter2 = 0;
        Perceptron curPerceptron = startPerceptron;

        while (curPerceptron.layer != hiddenLayers + 1 && iter2 < 200)
        {
            bool succes = false;
            int iter = 0;
            while (!succes && iter < 100)
            {
                iter++;
                int num = rand.Next(0, totalPerceptrons);
                Perceptron perceptron = GetPerceptron(num);

                if (perceptron.layer == curPerceptron.layer || perceptron.layer == 0)
                    continue;
                else if (perceptron.layer < curPerceptron.layer)
                {
                    if ((float)rand.NextDouble() < backStepChance)
                    {
                        curPerceptron.LinkPerceptrons(perceptron, out succes);
                        if (succes)
                            curPerceptron = perceptron;
                    }
                }
                else
                {
                    curPerceptron.LinkPerceptrons(perceptron, out succes);
                    if (succes)
                        curPerceptron = perceptron;
                }
            }
            iter2++;
        }
    }

    public void CleanUpNetwork()
    {

        // Loop through the layers
        for (int i = 0; i < layers.Count; i++)
        {
            // loop through the perceptrons inside the layer
            for (int j = 0; j < layers[i].Count; j++)
            {
                // if a perceptron does not have any input or output connections
                if (layers[i][j] == null)
                {
                    // Remove the perceptron
                    RemovePerceptron(i, j);
                    j--;
                }
            }
        }
        RemoveEmptyLayers();

        // Loop through the layers
        for (int i = 0; i < layers.Count - 1; i++)
        {
            // loop through the perceptrons inside the layer
            for (int j = 0; j < layers[i].Count; j++)
            {

                // if a perceptron does not have any input or output connections
                if (layers[i][j] == null || layers[i][j].inputConnections.Count == 0 || layers[i][j].outputConnections.Count == 0)
                {
                    // Remove the perceptron
                    RemovePerceptron(i, j);
                    CleanUpNetwork();
                }
            }
        }
    }

    public void RemoveEmptyLayers()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].Count == 0)
            {
                layers.RemoveAt(i);
                i--;

                for (int j = i + 1; j < layers.Count; j++)
                {
                    for (int k = 0; k < layers[j].Count; k++)
                        layers[j][k].layer = j;

                }
            }
        }
    }

    public void RemovePerceptron(int layer, int percep)
    {
        //// renumber the perceptrons with a higher number than the one to be removed
        //for (int j = percep + 1; j < layers[layer].Count; j++)
        //{
        //    if (layers[layer][j] != null)
        //        layers[layer][j].number--;
        //}

        if (layers[layer][percep] == null)
        {
            layers[layer].RemoveAt(percep);
            return;
        }

        for (int i = 0; i < layers[layer][percep].inputConnections.Count; i++)
        {
            layers[layer][percep].inputConnections[0].RemoveConnection();
            i--;
        }

        for (int i = 0; i < layers[layer][percep].outputConnections.Count; i++)
        {
            layers[layer][percep].outputConnections[0].RemoveConnection();
            i--;
        }

        // Remove the perceptron from the network
        layers[layer][percep] = null;
        layers[layer].RemoveAt(percep);
    }

    void SetWeights()
    {
        foreach (List<Perceptron> perceptrons in layers)
        {
            foreach (Perceptron perceptron in perceptrons)
            {
                perceptron.InitializeWeights();
            }
        }
    }

    public void NormalizeWeights()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].Count; j++)
            {
                layers[i][j].NormalizeWeights();
            }
        }
    }

    public List<List<Perceptron>> GetLayers()
    {
        return layers;
    }

    public List<List<Perceptron>> GetFullNetwork()
    {
        return fullNetwork;
    }

    public List<float> GetOutput(List<float> input)
    {
        if (input.Count != layers[0].Count)
        {
            Debug.LogWarning("Number of inputs most be equal to inputs");
            return null;
        }
        for (int i = 0; i < input.Count; i++)
        {
            layers[0][i].DetermineOutput(input[i]);
        }
        for (int i = 1; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].Count; j++)
            {
                layers[i][j].DetermineOutput();
            }
        }
        List<float> outputs = new List<float>();

        for (int i = 0; i < layers[layers.Count - 1].Count; i++)
        {
            outputs.Add(layers[layers.Count - 1][i].output);
        }

        if (output != null)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                for (int j = 0; j < layers[i].Count; j++)
                { 
                    if(output[i][j] != null)
                        output[i][j].text = layers[i][j].output.ToString("F2");
                }
            }
        }

        return outputs;

    }

    public List<Perceptron> GetInputLayer()
    {
        return layers[0];
    }

    public List<Perceptron> GetOutputLayer()
    {
        return layers[layers.Count - 1];
    }

    public List<List<Perceptron>> GetHiddenLayers()
    {
        if (hiddenLayers != 0)
            return layers.GetRange(1, layers.Count - 2);

        return new List<List<Perceptron>>();
    }

    public Perceptron GetPerceptron(int absoluteNumber)
    {
        foreach (List<Perceptron> layer in layers)
        {
            foreach (Perceptron perceptron in layer)
            {
                if (perceptron.absoluteNumber == absoluteNumber)
                    return perceptron;
            }
        }

        return null;
    }

    public void VisualizeNetwork(GameObject perceptron, GameObject hSpace, GameObject vSpace, GameObject line, Transform parent, string name)
    {
        if (NNVisualization.instance != null)
            GameObject.Destroy(NNVisualization.instance.gameObject);

        output = new List<List<Text>>();

        GameObject temp = new GameObject();
        GameObject connections = (GameObject)GameObject.Instantiate(temp, parent.transform, false);
        connections.name = "Connections";
        connections.AddComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        connections.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        connections.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        connections.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        GameObject perceptrons = (GameObject)GameObject.Instantiate(temp, parent.transform, false);
        perceptrons.name = "Perceptrons";
        perceptrons.AddComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        perceptrons.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        perceptrons.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        perceptrons.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        GameObject.Destroy(temp);
        RectTransform rtPerceptron = perceptron.GetComponent<RectTransform>();
        RectTransform rtHSpace = hSpace.GetComponent<RectTransform>();
        RectTransform rtVSpace = vSpace.GetComponent<RectTransform>();

        Vector2 perceptronSize = new Vector2(rtPerceptron.rect.width * rtPerceptron.localScale.x, rtPerceptron.rect.height * rtPerceptron.localScale.y);
        Vector2 hSpaceSize = new Vector2(rtHSpace.rect.width * rtHSpace.localScale.x, rtHSpace.rect.height * rtHSpace.localScale.y);
        Vector2 vSpaceSize = new Vector2(rtVSpace.rect.width * rtVSpace.localScale.x, rtVSpace.rect.height * rtVSpace.localScale.y);
        Vector2 startPos = new Vector3(perceptronSize.x / 2, -perceptronSize.y / 2);

        int maxJ = 0;
        for (int i = 0; i < layers.Count; i++)
        {
            output.Add(new List<Text>());
            for (int j = 0; j < layers[i].Count; j++)
            {
                if (layers[i][j] == null)
                    continue;

                Vector2 curPos = startPos + new Vector2((layers[i][j].layer * (hSpaceSize.x + perceptronSize.x)), -layers[i][j].number * (perceptronSize.y + vSpaceSize.y));
                GameObject CurPerceptron = (GameObject)GameObject.Instantiate(perceptron, perceptrons.transform, false);
                CurPerceptron.GetComponent<RectTransform>().anchoredPosition = curPos;

                foreach (PerceptronConnection connection in layers[i][j].inputConnections)
                {
                    Perceptron inPerceptron = connection.inputPerceptron;

                    if (inPerceptron == null)
                        continue;

                    Vector2 inPos = startPos + new Vector2((inPerceptron.layer * (hSpaceSize.x + perceptronSize.x)), -inPerceptron.number * (perceptronSize.y + vSpaceSize.y)) + Vector2.right * perceptronSize.x / 2;
                    Vector2 outPos = curPos + Vector2.left * perceptronSize.x / 2;
                    float rotation = Mathf.Atan2(inPos.y - outPos.y, inPos.x - outPos.x);
                    GameObject curConnection = (GameObject)GameObject.Instantiate(line, connections.transform, false);
                    curConnection.GetComponent<RectTransform>().sizeDelta = new Vector2((inPos - outPos).magnitude, 3);
                    curConnection.GetComponent<RectTransform>().anchoredPosition = outPos;
                    curConnection.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, rotation * Mathf.Rad2Deg);
                }

                if (maxJ < j)
                    maxJ = j;
                output[i].Add(CurPerceptron.GetComponentInChildren<Text>());
                output[i][j].text = "";
            }
        }

        float width = parent.GetComponent<RectTransform>().rect.width;
        float height = parent.GetComponent<RectTransform>().rect.height;
        float netWidth = ((layers.Count * (hSpaceSize.x + perceptronSize.x))) - hSpaceSize.x;
        float netHeight = (((maxJ + 1) * (perceptronSize.y + vSpaceSize.y))) - vSpaceSize.y;

        float scaleX = width / netWidth;
        float scaleY = height / netHeight;


        if(scaleX < scaleY)
            parent.localScale = Vector3.one * scaleX;
        else
        {
            parent.GetComponent<RectTransform>().anchoredPosition += new Vector2(width - netWidth * scaleY, 0) * scaleY + new Vector2(width - netWidth, 0) * scaleY/2;
            parent.localScale = Vector3.one * scaleY;
        }
    }

    public bool IsFunctional()
    {
        if (layers[0].Count != inputPerceptrons || layers[layers[0].Count - 1].Count != outputPerceptrons)
            return false;

        return true;
    }
}
