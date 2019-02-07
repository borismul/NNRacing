using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class NeuralNetwork
{
    List<Perceptron> perceptrons;
    List<GameObject> visualizeObjects = new List<GameObject>();
    List<Text> texts = new List<Text>();

    List<float> output = new List<float>();
    int perceptron;
    Perceptron curPerceptron;
    float sum;
    Link currentLink;
    float weight;
    float neuronOutput;
    public int inputs;
    List<Perceptron> outPerceptrons = new List<Perceptron>();
    public bool bestOfAll;
    public bool leader;

    public NeuralNetwork(ref List<Perceptron> perceptrons, int inputs, bool bestOfAll = false, bool leader = false)
    {
        this.bestOfAll = bestOfAll;
        this.leader = leader;
        this.perceptrons = perceptrons;
        this.inputs = inputs;
    }

    public void SetNeuralNetwork(ref List<Perceptron> perceptrons, int inputs, bool bestOfAll = false, bool leader = false)
    {
        this.bestOfAll = bestOfAll;
        this.leader = leader;
        this.perceptrons = perceptrons;
        this.inputs = inputs;
    }

    public List<float> Update(List<float> inputs)
    {
        output.Clear();
        outPerceptrons.Clear();
        perceptron = 0;
        while(perceptrons[perceptron].type == NodeType.Sensor)
        {
            perceptrons[perceptron].output = inputs[perceptron];
            perceptron++;
        }

        while (perceptron < perceptrons.Count)
        {
            curPerceptron = perceptrons[perceptron];

            sum = 0;
            curPerceptron.SumActivation = 0;

            for (int j = 0; j < curPerceptron.inLinks.Count; j++)
            {
                currentLink = curPerceptron.inLinks[j];

                weight = currentLink.weight;

                neuronOutput = currentLink.from.output;

                sum += weight * neuronOutput;
                curPerceptron.SumActivation += weight * neuronOutput;
            }

            if (curPerceptron.type != NodeType.Output)
            {
                curPerceptron.lastOutput = curPerceptron.output;
                curPerceptron.output = Sigmoid(curPerceptron.SumActivation, curPerceptron.activationResponse);
            }
            else
            {
                outPerceptrons.Add(curPerceptron);
            }

            perceptron++;
        }
        SoftMax(outPerceptrons);
        return output;
    }

    float Sigmoid(float input, float response)
    {
        return (1f / (1f + Mathf.Exp(-(input +  response))));
    }

    float Step(float input, float response)
    {
        if (input + response > 0)
            return 1;
        else
            return 0;
    }

    float ReLu(float input, float response)
    {
        if (input + response < 0)
            return 0;
        else
            return (input + response);
    }

    void SoftMax(List<Perceptron> outputsPerceptrons)
    {
        float outSum = 0;
        for (int i = 0; i < 3; i++)
        {
            outSum += Mathf.Exp(outputsPerceptrons[i].SumActivation - outputsPerceptrons[i].activationResponse);
        }

        for (int i = 0; i < 3; i++)
        {
            outputsPerceptrons[i].output = Mathf.Exp(outputsPerceptrons[i].SumActivation - outputsPerceptrons[i].activationResponse) / outSum;
            output.Add(outputsPerceptrons[i].output);
        }
        outSum = 0;

        for (int i = 3; i < 6; i++)
        {
            outSum += Mathf.Exp(outputsPerceptrons[i].SumActivation - outputsPerceptrons[i].activationResponse);

        }

        for (int i = 3; i < 6; i++)
        {
            outputsPerceptrons[i].output = Mathf.Exp(outputsPerceptrons[i].SumActivation - outputsPerceptrons[i].activationResponse) / outSum;
            output.Add(outputsPerceptrons[i].output);
        }

    }

    public void Reset()
    {
        for(int i = 0; i < perceptrons.Count; i++)
        {
            perceptrons[i].output = 0;
        }
    }

    // Unit tested
    public void VisualizeNetwork(RectTransform panel, GameObject perceptronPrefab, GameObject linkPrefab, GameObject loopPrefab, bool play)
    {
        float width = panel.rect.width;
        float height = panel.rect.height;

        Vector2 margin = new Vector2(width * 0.2f, height * 0.2f);

        for (int i = 0; i < perceptrons.Count; i++)
        {
            Perceptron perceptronFrom = perceptrons[i];

            GameObject perceptron = Object.Instantiate(perceptronPrefab, panel.transform);
            visualizeObjects.Add(perceptron);
            Vector2 positionfrom = new Vector2();

            if(width > height)
            {
                positionfrom.x = perceptrons[i].splitValues.x;
                positionfrom.y = perceptrons[i].splitValues.y;
            }
            else
            {
                positionfrom.x = perceptrons[i].splitValues.y;
                positionfrom.y = perceptrons[i].splitValues.x;
            }
            positionfrom.y *= (height - margin.y);
            positionfrom.x *= (width - margin.x);

            positionfrom = positionfrom + margin / 2;

            RectTransform perceptronRect = perceptron.GetComponent<RectTransform>();
            perceptronRect.anchoredPosition = positionfrom;
            perceptronRect.localScale = Vector3.one;

            texts.Add(perceptron.GetComponentInChildren<Text>());

            for (int j = 0; j < perceptrons[i].outLinks.Count; j++)
            {
                if (perceptrons[i].outLinks[j].to == perceptrons[i].outLinks[j].from)
                {
                    GameObject loopLink = Object.Instantiate(loopPrefab, panel.transform);
                    visualizeObjects.Add(loopLink);
                    loopLink.transform.SetAsFirstSibling();
                    loopLink.GetComponent<RectTransform>().anchoredPosition = positionfrom + Vector2.up * 5;
                    loopLink.GetComponent<RectTransform>().localScale = Vector2.one;
                    loopLink.GetComponent<Image>().color = Color.blue;
                }

                Perceptron perceptronTo = perceptrons[i].outLinks[j].to;
                Vector2 positionTo = perceptronTo.splitValues;

                if (width > height)
                {
                    positionTo.x = perceptronTo.splitValues.x;
                    positionTo.y = perceptronTo.splitValues.y;
                }
                else
                {
                    positionTo.x = perceptronTo.splitValues.y;
                    positionTo.y = perceptronTo.splitValues.x;
                }

                positionTo.y *= (height - margin.y);
                positionTo.x *= (width - margin.x);
                positionTo = positionTo + margin / 2;

                float lineLength = Vector2.Distance(positionfrom, positionTo);
                float lineRotation = Mathf.Atan2(positionTo.y - positionfrom.y, positionTo.x - positionfrom.x);

                GameObject link = Object.Instantiate(linkPrefab, panel.transform);
                visualizeObjects.Add(link);

                link.transform.SetAsFirstSibling();
                if(!perceptrons[i].outLinks[j].recurrent)
                    link.GetComponent<Image>().color = Color.black;
                else
                    link.GetComponent<Image>().color = Color.red;

                RectTransform lineRect = link.GetComponent<RectTransform>();

                lineRect.sizeDelta = new Vector2(lineLength, 2);
                lineRect.anchoredPosition = positionfrom;
                lineRect.localRotation = Quaternion.Euler(0, 0, lineRotation * Mathf.Rad2Deg);
                lineRect.localScale = Vector3.one;
            }
        }
    }

    public void SetNetworkValues()
    {
        if (visualizeObjects.Count == 0)
            return;

        for(int i = 0; i < perceptrons.Count; i++)
        {
            if(texts[i] != null)
                texts[i].text = perceptrons[i].output.ToString("F2");
        }
    }

    public void DestroyNetwork()
    {
        for(int i = 0; i < visualizeObjects.Count; i++)
        {
            Object.Destroy(visualizeObjects[i]);
        }
        visualizeObjects.Clear();
        texts.Clear();
    }
}

public class Perceptron
{
    public List<Link> inLinks;
    public List<Link> outLinks;

    public float SumActivation;
    public float lastOutput;
    public float output;

    public NodeType type;

    public int ID;

    public float activationResponse;

    public Vector2 pos;
    public Vector2 splitValues;

    int outLinkAddIndex = 0;
    int inLinkAddIndex = 0;


    public Perceptron(NodeType type, int ID, Vector2 splitValues, float activationResponse)
    {
        this.type = type;
        this.ID = ID;
        this.splitValues = splitValues;
        this.activationResponse = activationResponse;

        SumActivation = 0;
        output = 0;
        lastOutput = 0;
        pos = Vector2.zero;

        inLinks = new List<Link>();
        outLinks = new List<Link>();

        outLinkAddIndex = 0;
        inLinkAddIndex = 0;


    }

    public Perceptron(NodeGene perceptronGene)
    {
        type = perceptronGene.type;
        ID = perceptronGene.ID;
        splitValues = perceptronGene.splitValues;
        activationResponse = perceptronGene.actResponse;

        SumActivation = 0;
        output = 0;
        lastOutput = 0;
        pos = Vector2.zero;

        inLinks = new List<Link>();
        outLinks = new List<Link>();

        outLinkAddIndex = 0;
        inLinkAddIndex = 0;
    }

    public void SetPerceptron(NodeGene perceptronGene)
    {
        type = perceptronGene.type;
        ID = perceptronGene.ID;
        splitValues = perceptronGene.splitValues;
        activationResponse = perceptronGene.actResponse;

        SumActivation = 0;
        output = 0;
        lastOutput = 0;
        pos = Vector2.zero;

        outLinkAddIndex = 0;
        inLinkAddIndex = 0;
    }

    public void AddOutLink(float weight, Perceptron from, Perceptron to, bool recurrent)
    {
        if (outLinkAddIndex < outLinks.Count)
            outLinks[outLinkAddIndex].SetLink(weight, from, to, recurrent);
        else
            outLinks.Add(new Link(weight, from, to, recurrent));

        outLinkAddIndex++;
    }

    public void AddInLink(float weight, Perceptron from, Perceptron to, bool recurrent)
    {
        if (inLinkAddIndex < inLinks.Count)
            inLinks[inLinkAddIndex].SetLink(weight, from, to, recurrent);
        else
            inLinks.Add(new Link(weight, from, to, recurrent));

        inLinkAddIndex++;
    }

    public void Finish()
    {
        for (int i = outLinks.Count - 1; i >= outLinkAddIndex; i--)
            outLinks.RemoveAt(i);

        for (int i = inLinks.Count - 1; i >= inLinkAddIndex; i--)
            inLinks.RemoveAt(i);

    }
}

public class Link
{
    public Perceptron from;
    public Perceptron to;

    public float weight;

    public bool recurrent;

    public Link(float weight, Perceptron from, Perceptron to, bool recurrent)
    {
        this.weight = weight;
        this.from = from;
        this.to = to;
        this.recurrent = recurrent;
    }

    public void SetLink(float weight, Perceptron from, Perceptron to, bool recurrent)
    {
        this.weight = weight;
        this.from = from;
        this.to = to;
        this.recurrent = recurrent;
    }
}
