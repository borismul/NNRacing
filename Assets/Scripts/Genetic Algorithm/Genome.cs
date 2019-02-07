using UnityEngine;
using System.Collections.Generic;
using System.Linq;
enum BestParent { mum, dad };

public class Genome
{
    int ID;
    List<NodeGene> perceptrons;
    List<ConnectionGene> connections;
    NeuralNetwork network;
    float fitness;
    float adjustedFitness;
    float amountToSpawn;
    int inputs;
    int outputs;
    int species;

    public MutParameters mutPar;

    static List<int> newPerceptrons = new List<int>();
    static MutParameters newMutPar;
    List<Perceptron> newPerceptronsObjects = new List<Perceptron>();

    int nodeGeneIndex = 0;
    int connectionGeneIndex = 0;

    public static List<Genome> genomePool;

    public Genome()
    {
        network = null;
        ID = 0;
        fitness = 0;
        adjustedFitness = 0;
        inputs = 0;
        outputs = 0;
        amountToSpawn = 0;

        perceptrons = new List<NodeGene>();
        connections = new List<ConnectionGene>();

        mutPar = new MutParameters();

        //Random.InitState((int)System.DateTime.Now.Ticks);

    }

    public Genome(int ID, int inputs, int outputs, MutParameters mutPar)
    {
        network = null;
        this.ID = ID;
        fitness = 0;
        adjustedFitness = 0;
        this.inputs = inputs;
        this.outputs = outputs;
        amountToSpawn = 0;
        species = 0;

        perceptrons = new List<NodeGene>();
        connections = new List<ConnectionGene>();

        for (int i = 0; i < inputs; i++)
        {
            perceptrons.Add(new NodeGene(NodeType.Sensor, i, new Vector2(0, (float)i / inputs)));
        }
        perceptrons.Add(new NodeGene(NodeType.Bias, inputs, new Vector2(0, 1), false));
        for (int i = 0; i < outputs; i++)
        {
            perceptrons.Add(new NodeGene(NodeType.Output, i + inputs + 1, new Vector2(1, (float)i / (outputs - 1))));
        }

        for (int i = 0; i < inputs + 1; i++)
        {
            for (int j = 0; j < outputs; j++)
            {
                connections.Add(new ConnectionGene(perceptrons[i].ID, perceptrons[inputs + j + 1].ID, true, inputs + outputs + 1 + NumGenes(), Random.Range(-1f, 1f)));
            }
        }

        this.mutPar = mutPar;
    }

    public Genome(int ID, int inputs, int hidden, int[] hiddenNodes, int outputs, MutParameters mutPar)
    {
        network = null;
        this.ID = ID;
        fitness = 0;
        adjustedFitness = 0;
        this.inputs = inputs;
        this.outputs = outputs;
        amountToSpawn = 0;
        species = 0;
        //Random.InitState((int)System.DateTime.Now.Ticks);

        perceptrons = new List<NodeGene>();
        connections = new List<ConnectionGene>();
        int count = 0;

        for(int i = 0; i < inputs; i++)
        {
            perceptrons.Add(new NodeGene(NodeType.Sensor, count, new Vector2(0, (float)i/inputs)));
            count++;
        }

        perceptrons.Add(new NodeGene(NodeType.Bias, count, new Vector2(0, 1), false));
        count++;
        inputs++;
        for(int i = 0; i < hidden; i++)
        {
            for(int j = 0; j < hiddenNodes[i]; j++)
            {
                perceptrons.Add(new NodeGene(NodeType.Hidden, count, new Vector2((float)(i + 1) / (hidden + 1), (float)j/hiddenNodes[i])));
                count++;
            }
        }

        for(int i = 0; i < outputs; i++)
        {
            perceptrons.Add(new NodeGene(NodeType.Output, count, new Vector2(1, (float)i/(outputs - 1))));
            count++;
        }

        int percepCount = 0;

        for (int layer = 0; layer < hidden + 1; layer++)
        {
            int numInCurLayer = 0;
            int numInNextLayer = 0;

            if (layer == 0)
            {
                numInCurLayer = inputs;
                numInNextLayer = hiddenNodes[0];
            }
            else if (layer - 1 == hidden)
            {
                numInCurLayer = hiddenNodes[hidden - 1];
                numInNextLayer = outputs;
            }
            else
            {
                numInCurLayer = hiddenNodes[layer - 1];

                if (hiddenNodes.Length - 1 >= layer)
                {
                    numInNextLayer = hiddenNodes[layer];
                }
                else
                {
                    numInNextLayer = outputs;
                }
            }
            int startPercep = percepCount;

            for (int j = 0; j < numInCurLayer; j++)
            {
                for (int z = 0; z < numInNextLayer; z++)
                {
                    int connec = startPercep + numInCurLayer + z;
                    connections.Add(new ConnectionGene(perceptrons[percepCount].ID, perceptrons[connec].ID, true, count + NumGenes(), Random.Range(-1f, 1f))); 
                }
                percepCount++;

            }
        }

        this.mutPar = mutPar;
    }

    public Genome(int ID, List<NodeGene> perceptrons, List<ConnectionGene> connections, int inputs, int outputs, MutParameters mutPar)
    {
        network = null;
        this.connections = connections;
        this.perceptrons = perceptrons;
        amountToSpawn = 0;
        fitness = 0;
        adjustedFitness = 0;
        this.inputs = inputs;
        this.outputs = outputs;

        this.mutPar = mutPar;

    }

    public Genome(Genome genome)
    {
        ID = genome.ID;
        perceptrons = new List<NodeGene>();
        for (int i = 0; i < genome.perceptrons.Count; i++)
        {
            NodeGene curGene = genome.perceptrons[i];
            perceptrons.Add(new NodeGene(curGene.type, curGene.ID, curGene.splitValues, curGene.actResponse, curGene.recurrent));
        }

        connections = new List<ConnectionGene>();
        for (int i = 0; i < genome.connections.Count; i++)
        {
            ConnectionGene curGene = genome.connections[i];
            connections.Add(new ConnectionGene(curGene.from, curGene.to, curGene.enabled, curGene.innovNum, curGene.weight, curGene.recurrent));
        }

        inputs = genome.inputs;
        outputs = genome.outputs;
        fitness = genome.fitness;
        adjustedFitness = genome.adjustedFitness;
        amountToSpawn = genome.amountToSpawn;
        species = genome.species;

        mutPar = genome.mutPar;

    }
   
    public void SetGenome(Genome genome)
    {
        nodeGeneIndex = 0;
        connectionGeneIndex = 0;
        ID = genome.ID;
        for (int i = 0; i < genome.perceptrons.Count; i++)
        {
            NodeGene curGene = genome.perceptrons[i];
            AddNodeGene(curGene);
        }

        for (int i = 0; i < genome.connections.Count; i++)
        {
            ConnectionGene curGene = genome.connections[i];
            AddConnectionGeneGene(curGene);
        }

        FinishGenome();

        inputs = genome.inputs;
        outputs = genome.outputs;
        fitness = 0;
        adjustedFitness = 0;
        amountToSpawn = 0;
        species = 0;

        mutPar = genome.mutPar;
    }

    public void SetGenome(int ID, List<NodeGene> perceptrons, List<ConnectionGene> connections, int inputs, int outputs, MutParameters mutPar)
    {
        nodeGeneIndex = 0;
        connectionGeneIndex = 0;
        this.ID = ID;
        for (int i = 0; i < perceptrons.Count; i++)
        {
            NodeGene curGene = perceptrons[i];
            AddNodeGene(curGene);
        }

        for (int i = 0; i < connections.Count; i++)
        {
            ConnectionGene curGene = connections[i];
            AddConnectionGeneGene(curGene);
        }

        FinishGenome();

        this.inputs = inputs;
        this.outputs = outputs;
        fitness = 0;
        adjustedFitness = 0;
        amountToSpawn = 0;
        species = 0;

        this.mutPar = mutPar;
    }

    public void SetGenome(int ID, int inputs, int outputs, MutParameters mutPar)
    {
        network = null;
        this.ID = ID;
        fitness = 0;
        adjustedFitness = 0;
        this.inputs = inputs;
        this.outputs = outputs;
        amountToSpawn = 0;
        species = 0;

        perceptrons = new List<NodeGene>();
        connections = new List<ConnectionGene>();

        for (int i = 0; i < inputs; i++)
        {
            perceptrons.Add(new NodeGene(NodeType.Sensor, i, new Vector2(0, (float)i / inputs)));
        }
        perceptrons.Add(new NodeGene(NodeType.Bias, inputs, new Vector2(0, 1), false));
        for (int i = 0; i < outputs; i++)
        {
            perceptrons.Add(new NodeGene(NodeType.Output, i + inputs + 1, new Vector2(1, (float)i / (outputs - 1))));
        }

        for (int i = 0; i < inputs + 1; i++)
        {
            for (int j = 0; j < outputs; j++)
            {
                connections.Add(new ConnectionGene(perceptrons[i].ID, perceptrons[inputs + j + 1].ID, true, inputs + outputs + 1 + NumGenes(), Random.Range(-1f, 1f)));
            }
        }

        this.mutPar = mutPar;
    }

    public void FinishGenome()
    {
        for (int i = this.perceptrons.Count - 1; i > nodeGeneIndex - 1; i--)
        {
            perceptrons.RemoveAt(i);
        }

        for (int i = this.connections.Count - 1; i > connectionGeneIndex - 1; i--)
        {
            connections.RemoveAt(i);
        }
    }

    public void AddNodeGene(NodeGene gene)
    {
        if (nodeGeneIndex < perceptrons.Count)
            perceptrons[nodeGeneIndex].SetGene(gene);
        else
            perceptrons.Add(new NodeGene(gene));

        nodeGeneIndex++;

    }

    public void AddNodeGene(int ID, NodeType type, bool recurrent, Vector2 splitValues, float actResponse)
    {
        if (nodeGeneIndex < perceptrons.Count)
            perceptrons[nodeGeneIndex].SetGene(ID, type, recurrent, splitValues, actResponse);
        else
            perceptrons.Add(new NodeGene(type, ID, splitValues, recurrent, actResponse));

        nodeGeneIndex++;

    }

    public void AddConnectionGeneGene(ConnectionGene gene)
    {
        if (connectionGeneIndex < connections.Count)
            connections[connectionGeneIndex].SetGene(gene);
        else
            connections.Add(new ConnectionGene(gene));

        connectionGeneIndex++;

    }

    public void InitializeWeights()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            connections[i].weight = Random.Range(-1f, 1f);
        }
    }

    // Unit Tested
    public NeuralNetwork CreateNetwork(bool bestOfAll = false, bool leader = false)
    {
        //newPerceptronsObjects.Clear();
        while (perceptrons.Count < newPerceptronsObjects.Count)
            newPerceptronsObjects.RemoveAt(newPerceptronsObjects.Count - 1);

        for (int i = 0; i < this.perceptrons.Count; i++)
        {
            if (i < newPerceptronsObjects.Count)
            {
                newPerceptronsObjects[i].SetPerceptron(perceptrons[i]);

            }
            else
            {
                Perceptron perceptron = new Perceptron(perceptrons[i]);

                newPerceptronsObjects.Add(perceptron);
            }

        }

        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].enabled)
            {
                Perceptron from = null;
                Perceptron to = null;
                int index;

                index = GetElementPos(connections[i].from);
                from = newPerceptronsObjects[index];

                index = GetElementPos(connections[i].to);
                to = newPerceptronsObjects[index];


                from.AddOutLink(connections[i].weight, from, to, connections[i].recurrent);
                to.AddInLink(connections[i].weight, from, to, connections[i].recurrent);
            }

        }

        for (int i = 0; i < newPerceptronsObjects.Count; i++)
            newPerceptronsObjects[i].Finish();

        if(network != null)
            network.SetNeuralNetwork(ref newPerceptronsObjects, inputs, bestOfAll, leader);
        else
            network = new NeuralNetwork(ref newPerceptronsObjects, inputs, bestOfAll, leader);

        return network;
    }

    int GetElementPos(int ID)
    {
        for (int i = 0; i < perceptrons.Count; i++)
        {
            if (perceptrons[i].ID == ID)
            {

                return i;
            }
        }

        Debug.LogWarning("Neuron ID " + ID.ToString() + " doesn't exist");
        return -1;
    }

    bool DuplicateLink(int from, int to)
    {
        for(int i = 0; i < connections.Count; i++)
        {
            if (from == connections[i].from && to == connections[i].to)
                return true;
        }

        return false;
    }

    public void AddLink(float mutationRate, float loopChance, ref Innovations innovation, int trysToFindLoop, int trysToAddLink)
    {
        if (Random.Range(0f, 1f) > mutationRate)
            return;

        int percep1 = -1;
        int percep2 = -1;

        bool recurrent = false;

        if(Random.Range(0f, 1f) < loopChance)
        {
            while(trysToFindLoop > 0)
            {
                int percepPos = Random.Range(inputs, perceptrons.Count - 1);

                if(!perceptrons[percepPos].recurrent && perceptrons[percepPos].type != NodeType.Bias && perceptrons[percepPos].type != NodeType.Sensor && perceptrons[percepPos].type != NodeType.Output)
                {
                    percep1 = perceptrons[percepPos].ID;
                    percep2 = percep1;

                    perceptrons[percepPos].recurrent = true;

                    recurrent = true;


                    trysToFindLoop = 0;
                }
                trysToFindLoop--;

            }
        }
        else
        {
            while (trysToAddLink > 0)
            {
                percep1 = perceptrons[Random.Range(0, perceptrons.Count - 1)].ID;
                percep2 = perceptrons[Random.Range(inputs, perceptrons.Count - 1)].ID;

                if( DuplicateLink(percep1, percep2) || percep1 == percep2 || 
                    (perceptrons[GetElementPos(percep1)].type == NodeType.Sensor && perceptrons[GetElementPos(percep2)].type == NodeType.Sensor) || 
                    (perceptrons[GetElementPos(percep1)].type == NodeType.Output && perceptrons[GetElementPos(percep2)].type == NodeType.Output) ||
                    (perceptrons[GetElementPos(percep1)].type == NodeType.Output && perceptrons[GetElementPos(percep2)].type == NodeType.Sensor) ||
                    perceptrons[GetElementPos(percep2)].type == NodeType.Sensor || perceptrons[GetElementPos(percep2)].type == NodeType.Bias)
                {
                    percep1 = -1;
                    percep2 = -1;
                }
                else
                {
                    trysToAddLink = 0;
                }
                trysToAddLink--;
            }
        }

        if (percep1 < 0 || percep2 < 0)
            return;

        int innovID = innovation.CheckInnovation(percep1, percep2, InnovationType.Connection);

        if (perceptrons[GetElementPos(percep1)].splitValues.x - perceptrons[GetElementPos(percep2)].splitValues.x > 0.01f)
        {
            recurrent = true;
        }
        if(innovID < 0)
        {
            innovation.CreateNewInnovation(percep1, percep2, InnovationType.Connection);

            int id = innovation.NextNumber() - 1;

            ConnectionGene gene = new ConnectionGene(percep1, percep2, true, id, Random.Range(-1f, 1f), recurrent);

            connections.Add(gene);
        }
        else
        {
            ConnectionGene gene = new ConnectionGene(percep1, percep2, true, innovID, Random.Range(-1f, 1f), recurrent);
            connections.Add(gene);
        }


    }

    public void RemoveLink(float mutationRate)
    {
        if (Random.Range(0f, 1f) > mutationRate)
            return;

        int con_num = Random.Range(0, connections.Count);
        ConnectionGene connection = connections[con_num];

        int from = connection.from;
        int to = connection.to;


        connections.RemoveAt(con_num);

        if (from == to)
            return;

        int fromIndex = 0;
        int toIndex = 0;
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].to == to || connections[i].from == to)
                toIndex++;

            if (connections[i].to == from || connections[i].from == from)
                fromIndex++;
        }

        if (fromIndex == 0 && perceptrons[GetElementPos(from)].type == NodeType.Hidden)
            perceptrons.RemoveAt(GetElementPos(from));
        if (toIndex == 0 && perceptrons[GetElementPos(to)].type == NodeType.Hidden)
            perceptrons.RemoveAt(GetElementPos(to));

    }

    public void AddNeuron(float mutationRate, ref Innovations innovation, int trysFindOldLink)
    {
        for (int i = 0; i < 20; i++)
        {
            if (Random.Range(0f, 1f) > mutationRate)
                return;

            bool done = false;

            int chosenLink = 0;

            int sizeThreshHold = inputs + outputs + 10;

            int from;
            int to;

            if (connections.Count < sizeThreshHold)
            {
                while (trysFindOldLink > 0)
                {
                    chosenLink = Random.Range(0, NumGenes() - 1 - (int)Mathf.Sqrt((float)NumGenes()));
                    Debug.Log(chosenLink);
                    from = connections[chosenLink].from;

                    if (connections[chosenLink].enabled && !connections[chosenLink].recurrent && perceptrons[GetElementPos(from)].type != NodeType.Bias)
                    {
                        done = true;
                        trysFindOldLink = 0;
                    }
                    trysFindOldLink--;
                }

                if (!done)
                    return;
            }
            else
            {
                while (!done)
                {
                    chosenLink = Random.Range(0, NumGenes() - 1);

                    from = connections[chosenLink].from;
                    if (connections[chosenLink].enabled && !connections[chosenLink].recurrent && perceptrons[GetElementPos(from)].type != NodeType.Bias)
                        done = true;
                }
            }

            // Think about this part
            //NodeType type = NodeType.Hidden;
            //if (CParams::bAdaptable && RandFloat() < CParams::dModulatoryChance)
            //{
            //    type = modulatory;
            //}

            connections[chosenLink].enabled = false;
            float orignalWeight = connections[chosenLink].weight;

            from = connections[chosenLink].from;
            to = connections[chosenLink].to;

            Vector2 splitValues = (perceptrons[GetElementPos(from)].splitValues + perceptrons[GetElementPos(to)].splitValues) / 2;
            int connectionID = innovation.CheckInnovation(from, to, InnovationType.Perceptron);

            if (connectionID >= 0)
            {
                int ID = innovation.GetNeuronID(connectionID);

                if (AlreadyHaveThisNeuronID(ID))
                {
                    connectionID = -1;
                }
            }

            if (connectionID < 0)
            {
                int ID = innovation.CreateNewInnovation(from, to, InnovationType.Perceptron, NodeType.Hidden, splitValues);

                perceptrons.Add(new NodeGene(NodeType.Hidden, ID, splitValues));
                int link1 = innovation.NextNumber();
                innovation.CreateNewInnovation(from, ID, InnovationType.Connection);
                ConnectionGene link1Gene = new ConnectionGene(from, ID, true, link1, 1.0f);
                connections.Add(link1Gene);

                int link2 = innovation.NextNumber();
                innovation.CreateNewInnovation(ID, to, InnovationType.Connection);
                ConnectionGene link2Gene = new ConnectionGene(ID, to, true, link2, orignalWeight);
                connections.Add(link2Gene);
            }
            else
            {
                int ID = innovation.GetNeuronID(connectionID);

                int link1 = innovation.CheckInnovation(from, ID, InnovationType.Connection);
                int link2 = innovation.CheckInnovation(ID, to, InnovationType.Connection);

                if (link1 < 0 || link2 < 0)
                {
                    Debug.LogWarning("Innovation should already exist");
                    return;
                }

                ConnectionGene link1Gene = new ConnectionGene(from, ID, true, link1, 1.0f);
                ConnectionGene link2Gene = new ConnectionGene(ID, to, true, link2, orignalWeight);

                connections.Add(link1Gene);
                connections.Add(link2Gene);

                NodeGene perceptron = new NodeGene(NodeType.Hidden, ID, splitValues);

                perceptrons.Add(perceptron);
            }
        }



    }
    
    public void RemoveNeuron(float mutationRate, ref Innovations innovation)
    {
        if (Random.Range(0f, 1f) > mutationRate)
            return;
        int[] numIn = new int[perceptrons.Count];
        int[] numOut = new int[perceptrons.Count];

        for (int i = 0; i < connections.Count; i++)
        {
            ConnectionGene connection = connections[i];
            //Debug.Log("To: " + GetElementPos(connection.to) + " From: " + GetElementPos(connection.from) + " Count: " + perceptrons.Count);
            numIn[GetElementPos(connection.to)]++;
            numOut[GetElementPos(connection.from)]++;
        }

        List<int> removalCandidates = new List<int>();
        for (int i = 0; i < perceptrons.Count; i++)
        {
            if ((numIn[i] <= 1 || numOut[i] <= 1) && perceptrons[i].type == NodeType.Hidden)
            {
                removalCandidates.Add(i);
            }
        }

        if (removalCandidates.Count == 0)
            return;

        int removePercep = removalCandidates[Random.Range(0, removalCandidates.Count)];

        List<int> inConn = new List<int>();
        List<int> outConn = new List<int>();
        int recurrentConn = -1;
        for (int i = 0; i < connections.Count; i++)
        {
            if (GetElementPos(connections[i].from) == removePercep && GetElementPos(connections[i].to) != removePercep)
            {
                outConn.Add(i);
            }
            else if (GetElementPos(connections[i].to) == removePercep && GetElementPos(connections[i].from) != removePercep)
            {
                inConn.Add(i);
            }
            else if (GetElementPos(connections[i].to) == removePercep && GetElementPos(connections[i].from) == removePercep)
                recurrentConn = i;
        }

        List<ConnectionGene> addConnections = new List<ConnectionGene>();
        for (int i = 0; i < inConn.Count; i++)
        {
            for (int j = 0; j < outConn.Count; j++)
            {
                int percep1 = connections[inConn[i]].from;
                int percep2 = connections[outConn[j]].to;

                if (DuplicateLink(percep1, percep2))
                    continue;

                int innovID = innovation.CheckInnovation(percep1, percep2, InnovationType.Connection);

                if (innovID < 0)
                {
                    innovation.CreateNewInnovation(percep1, percep2, InnovationType.Connection);

                    int id = innovation.NextNumber() - 1;

                    ConnectionGene gene = new ConnectionGene(percep1, percep2, true, id, Random.Range(-1f, 1f), false);

                    addConnections.Add(gene);
                }
                else
                {
                    ConnectionGene gene = new ConnectionGene(percep1, percep2, true, innovID, Random.Range(-1f, 1f), false);
                    addConnections.Add(gene);
                }

            }
        }

        List<int> removeRange = new List<int>();
        removeRange.AddRange(inConn);
        removeRange.AddRange(outConn);
        if(recurrentConn >= 0)
            removeRange.Add(recurrentConn);

        removeRange.Sort();

        for (int i = removeRange.Count - 1; i > -1; i--)
        {
            connections.RemoveAt(removeRange[i]);
        }

        for (int i = 0; i > addConnections.Count; i++)
        {
            connections.Add(addConnections[i]);
        }


        perceptrons.RemoveAt(removePercep);
    }

    public void RemoveRecurrent(float mutationRate)
    {
        if (Random.Range(0f, 1f) > mutationRate)
            return;

        List<int> possiblePerceptrons = new List<int>();
        for (int i = 0; i < perceptrons.Count; i++)
        {
            if (perceptrons[i].recurrent)
                possiblePerceptrons.Add(i);
        }

        if (possiblePerceptrons.Count == 0)
            return;

        int index = Random.Range(0, possiblePerceptrons.Count);

        perceptrons[possiblePerceptrons[index]].recurrent = false;

    }

    bool AlreadyHaveThisNeuronID(int ID)
    {
        for(int i = 0; i < perceptrons.Count; i++)
        {
            if (ID == perceptrons[i].ID)
                return true;
        }
        return false;
    }

    public void MutateWeights(float mutationRate , float newWeightProbability, float maxPerturbation)
    {
        for (int i = 0; i < connections.Count; i++) {
            if (Random.Range(0f, 1f) < mutationRate / connections.Count * 150)
            {
                if (Random.Range(0f, 1f) < newWeightProbability)
                    connections[i].weight = Random.Range(-1f, 1f);
                else
                    connections[i].weight += Random.Range(-1f, 1f) * maxPerturbation;
            }
        }
    }

    public void MutateActivationResponse(float mutationRate, float maxPerturbation)
    {
        for(int i = 0; i < perceptrons.Count; i++)
        {
            if (Random.Range(0f, 1f) < mutationRate / perceptrons.Count * 25)
                perceptrons[i].actResponse += Random.Range(-1f, 1f) * maxPerturbation;
        }
    }

    public float GetCompatibilityScore(ref Genome genome)
    {
        float numDisjoint = 0;
        float numExcess = 0;
        float numMatched = 0;

        float weightDifference = 0;

        int g1 = 0;
        int g2 = 0;

        while (g1 < connections.Count - 1 || g2 < genome.connections.Count - 1)
        {
            if(g1 >= connections.Count  - 1)
            {
                g2++;
                numExcess++;
                continue;
            }

            if (g2 >= genome.connections.Count - 1)
            {
                g1++;
                numExcess++;
                continue;
            }

            int ID1 = connections[g1].innovNum;
            int ID2 = genome.connections[g2].innovNum;

            if(ID1 == ID2)
            {
                g1++;
                g2++;
                numMatched++;

                weightDifference += Mathf.Abs(connections[g1].weight - genome.connections[g2].weight);
            }

            if(ID1 < ID2)
            {
                numDisjoint++;
                g1++;
            }

            if(ID1 > ID2)
            {
                numDisjoint++;
                g2++;
            }
        }

        int longest = genome.NumGenes();

        if (NumGenes() > longest)
            longest = NumGenes();

        float disJointMultiplier = 1;
        float excessMultiplier = 1;
        float matchedMultiplier = 0.4f;

        float score = (excessMultiplier * numExcess / longest) + (disJointMultiplier * numDisjoint / longest) + (matchedMultiplier * weightDifference / numMatched);

        return score;
    }

    public void SortGenes()
    {
        //connections = connections.OrderBy(o => o.innovNum).ToList();

        connections.Sort(new System.Comparison<ConnectionGene>((obj1, obj2) =>
        {
            int result = obj1.innovNum.CompareTo(obj2.innovNum);
            return result;
        }
        ));
    }

    public static Genome Crossover(ref Genome mum, ref Genome dad, GeneticAlgorithm ga)
    {
        BestParent bestParent = GetBestParent(ref mum, ref dad);

        List<int> newPerceptrons = new List<int>();

        int curMum = 0;
        int curDad = 0;

        ConnectionGene selectedGene = null;

        Genome newGenome = RecycledGenome();

        while (!(curMum == mum.connections.Count && curDad == dad.connections.Count))
        {
            // Dad has more genes
            if (curMum == mum.connections.Count && curDad != dad.connections.Count)
            {
                if (bestParent == BestParent.dad)
                    selectedGene = dad.GetConnectionGenes()[curDad];

                curDad++;


                if (selectedGene == null)
                    continue;
            }
            // Mum has more genes
            else if (curMum != mum.connections.Count && curDad == dad.connections.Count)
            {
                if (bestParent == BestParent.mum)
                    selectedGene = mum.GetConnectionGenes()[curMum];

                curMum++;


                if (selectedGene == null)
                    continue;
            }
            // Mum gene innovation number is lower
            else if (mum.GetConnectionGenes()[curMum].innovNum < dad.GetConnectionGenes()[curDad].innovNum)
            {
                if (bestParent == BestParent.mum)
                {
                    selectedGene = mum.GetConnectionGenes()[curMum];
                }

                curMum++;


                if (selectedGene == null)
                    continue;
            }
            // Dad gene innovation number is lower
            else if (mum.GetConnectionGenes()[curMum].innovNum > dad.GetConnectionGenes()[curDad].innovNum)
            {
                if (bestParent == BestParent.dad)
                {
                    selectedGene = dad.GetConnectionGenes()[curDad];
                }

                curDad++;


                if (selectedGene == null)
                    continue;
            }
            // Both innovation numbers are equal
            else if (mum.GetConnectionGenes()[curMum].innovNum == dad.GetConnectionGenes()[curDad].innovNum)
            {
                if (Random.Range(0f, 1f) > 0.5f)
                    selectedGene = mum.GetConnectionGenes()[curMum];
                else
                    selectedGene = dad.GetConnectionGenes()[curDad];

                curDad++;
                curMum++;
            }
            else
            {
                Debug.Log("wtf");
            }

            if (selectedGene == null)
            {
                Debug.Log("Weird");
                continue;
            }
            if (newGenome.connectionGeneIndex == 0)
                newGenome.AddConnectionGeneGene(new ConnectionGene(selectedGene));
            else
            {
                if(newGenome.connections[newGenome.connectionGeneIndex - 1].innovNum != selectedGene.innovNum)
                {
                    newGenome.AddConnectionGeneGene(new ConnectionGene(selectedGene));
                }
            }

            newPerceptrons = AddPercepID(selectedGene.from, newPerceptrons);
            newPerceptrons = AddPercepID(selectedGene.to, newPerceptrons);
        }

        newPerceptrons.Sort();

        for (int i = 0; i < GA_Parameters.inputs + 1; i++)
            newGenome = ga.innovations.CreateNeuronFromID(i, newGenome);

        for (int i = 0; i < GA_Parameters.outputs; i++)
            newGenome = ga.innovations.CreateNeuronFromID(GA_Parameters.inputs + 1 + i, newGenome);

        for (int i = 0; i < newPerceptrons.Count; i++)
            newGenome = ga.innovations.CreateNeuronFromID(newPerceptrons[i], newGenome);

        MutParameters mutPar = new MutParameters(mum, dad);

        newGenome.ID = ga.nextGenomeID;
        newGenome.inputs = mum.inputs;
        newGenome.outputs = mum.outputs;
        newGenome.mutPar = mutPar;

        ga.nextGenomeID++;

        newGenome.FinishGenome();

        return newGenome;

    }

    static List<int> AddPercepID(int nodeID, List<int> perceptrons)
    {
        for(int i = 0; i < perceptrons.Count; i++)
        {
            if(perceptrons[i] == nodeID)
            {
                return perceptrons;
            }
        }

        perceptrons.Add(nodeID);

        return perceptrons;
    }

    static BestParent GetBestParent(ref Genome mum, ref Genome dad)
    {
        BestParent best;

        if (mum.GetFitness() == dad.GetFitness())
        {
            if (mum.NumGenes() == dad.NumGenes())
            {
                if (Random.Range(0f, 1f) > 0.5f)
                    best = BestParent.mum;
                else
                    best = BestParent.dad;
            }
            else
            {
                if (mum.NumGenes() < dad.NumGenes())
                    best = BestParent.mum;
                else
                    best = BestParent.dad;
            }
        }

        else
        {
            if (mum.GetFitness() > dad.GetFitness())
                best = BestParent.mum;
            else
                best = BestParent.dad;

        }

        return best;
    }

    public static int RouletteSelection(int n)
    {
        n += 1;
        int index = Random.Range(1,(n * (n + 1) / 2));
        int temp = 0;

        for (int i = n; i > 0; i--)
        {
            temp += (n - (i - 1));

            if (index < temp)
                return (i - 1);
        }

        return -1;
    }

    public static void CreateGenomePool(int generationSize)
    {
        genomePool = new List<Genome>();
        for (int i = 0; i < generationSize*2; i++)
            genomePool.Add(new Genome());
    }

    public static Genome RecycledGenome(Genome genome)
    {
        Genome recycledGenome = genomePool[0];
        genomePool.RemoveAt(0);

        recycledGenome.SetGenome(genome);
        return recycledGenome;
        //return new Genome(genome);
    }

    public static Genome RecycledGenome(int ID, int inputs, int outputs, MutParameters mutPar)
    {

        Genome recycledGenome = genomePool[0];
        genomePool.RemoveAt(0);

        recycledGenome.SetGenome(ID, inputs, outputs, mutPar);
        return recycledGenome;
        //return new Genome(ID, inputs, outputs, mutPar);

    }


    public static Genome RecycledGenome(int ID, List<NodeGene> perceptrons, List<ConnectionGene> connections, int inputs, int outputs, MutParameters mutPar)
    {
        Genome recycledGenome = genomePool[0];
        genomePool.RemoveAt(0);

        recycledGenome.SetGenome(ID, perceptrons, connections, inputs, outputs, mutPar);
        return recycledGenome;
        //return new Genome(ID, perceptrons, connections, inputs, outputs, mutPar);

    }

    public static Genome RecycledGenome()
    {
        Genome recycledGenome = genomePool[0];
        genomePool.RemoveAt(0);

        recycledGenome.connectionGeneIndex = 0;
        recycledGenome.nodeGeneIndex = 0;

        return recycledGenome;
        //return new Genome(ID, perceptrons, connections, inputs, outputs, mutPar);

    }

    public static void AddToRecycledGenomes(List<Genome> genomes)
    {
        genomePool.AddRange(genomes);
    }

    public int GetID()
    {
        return ID;
    }
    public void SetID(int ID)
    {
        this.ID = ID;
    }

    public int NumGenes()
    {
        return connections.Count;
    }
    public int NumPerceptrons()
    {
        return perceptrons.Count;
    }
    public int NumInputs()
    {
        return inputs;
    }
    public int NumOutPuts()
    {
        return outputs;
    }

    public float GetAmountToSpawn()
    {
        return amountToSpawn;
    }
    public void SetAmountToSpawn(float amountToSpawn)
    {
        this.amountToSpawn = amountToSpawn;
    }

    public void SetFitness(float fitness)
    {
        this.fitness = fitness;
    }
    public void SetAdjFitness(float adjFitness)
    {
        adjustedFitness = adjFitness;
    }
    public float GetFitness()
    {
        return fitness;
    }
    public float GetAdjFitness()
    {
        return adjustedFitness;
    }

    public int GetSpecies()
    {
        return species;
    }
    public void SetSpecies(int species)
    {
        this.species = species;
    }

    public NeuralNetwork GetNetwork()
    {
        return network;
    }

    public float GetSplitY(int val)
    {
        return perceptrons[val].splitValues.y;
    }

    public List<ConnectionGene> GetConnectionGenes()
    {
        return connections;
    }
    public List<NodeGene> GetPerceptronGenes()
    {
        return perceptrons;
    }

    [System.Serializable]
    public class MutParameters
    {
        public float addPercepProb;
        public float addLinkProb;
        public float recurAddProb;
        public float weightMutRate;
        public float maxWeightPerturbation;
        public float weightRepProb;
        public float activationMutationChance;
        public float maxActivationPerturbation;

        public MutParameters()
        {
            addPercepProb = GA_Parameters.chanceAddPerceptron;
            addLinkProb = GA_Parameters.chanceToAddLink;
            recurAddProb = GA_Parameters.chanceRecurrentLink;
            weightMutRate = GA_Parameters.weightMutationRate;
            maxWeightPerturbation = GA_Parameters.maxWeightPerturbation;
            weightRepProb = GA_Parameters.weightReplaceProb;
            activationMutationChance = GA_Parameters.activationMutationChance;
            maxActivationPerturbation = GA_Parameters.maxActivationPerturbation;
    }

        public MutParameters(Genome mum, Genome dad)
        {
            
            float crossoverRate;

            if (Random.Range(0f, 1f) < GA_Parameters.mutParMutRate)
            {
                addPercepProb = Random.Range(0f, 1f);

                if (addPercepProb > 1f)
                    addPercepProb = 1f;
                else if (addPercepProb < 0f)
                    addPercepProb = 0f;
            }
            else
            {
                crossoverRate = Random.Range(0f, 1f);

                if (crossoverRate < 0.5f)
                    addPercepProb = mum.mutPar.addPercepProb;
                else
                    addPercepProb = dad.mutPar.addPercepProb;
            }
            if (Random.Range(0f, 1f) < GA_Parameters.mutParMutRate)
            {
                addLinkProb = Random.Range(0f, 1f);
                if (addLinkProb > 1f)
                    addLinkProb = 1f;
                else if (addLinkProb < 0f)
                    addLinkProb = 0f;
            }
            else
            {
                crossoverRate = Random.Range(0f, 1f);
                if (crossoverRate < 0.5f)
                    addLinkProb = mum.mutPar.addLinkProb;
                else
                    addLinkProb = dad.mutPar.addLinkProb;
            }
            if (Random.Range(0f, 1f) < GA_Parameters.mutParMutRate)
            {
                recurAddProb = Random.Range(0f, 1f);
                if (recurAddProb > 1f)
                    recurAddProb = 1f;
                else if (recurAddProb < 0f)
                    recurAddProb = 0f;
            }
            else
            {
                crossoverRate = Random.Range(0f, 1f);
                if (crossoverRate < 0.5f)
                    recurAddProb = mum.mutPar.recurAddProb;
                else
                    recurAddProb = dad.mutPar.recurAddProb;
            }
            if (Random.Range(0f, 1f) < GA_Parameters.mutParMutRate)
            {
                weightMutRate = Random.Range(0f, 1f);
                if (weightMutRate > 1f)
                    weightMutRate = 1f;
                else if (weightMutRate < 0f)
                    weightMutRate = 0f;
            }
            else
            {
                crossoverRate = Random.Range(0f, 1f);
                if (crossoverRate < 0.5f)
                    weightMutRate = mum.mutPar.weightMutRate;
                else
                    weightMutRate = dad.mutPar.weightMutRate;
            }
            if (Random.Range(0f, 1f) < GA_Parameters.mutParMutRate)
            {
                maxWeightPerturbation = Random.Range(0f, 1f);
                if (maxWeightPerturbation > 1f)
                    maxWeightPerturbation = 1f;
                else if (maxWeightPerturbation < 0f)
                    maxWeightPerturbation = 0f;
            }
            else
            {
                crossoverRate = Random.Range(0f, 1f);
                if (crossoverRate < 0.5f)
                    maxWeightPerturbation = mum.mutPar.maxWeightPerturbation;
                else
                    maxWeightPerturbation = dad.mutPar.maxWeightPerturbation;
            }
            if (Random.Range(0f, 1f) < GA_Parameters.mutParMutRate)
            {
                weightRepProb = Random.Range(0f, 1f);
                if (weightRepProb > 1f)
                    weightRepProb = 1f;
                else if (weightRepProb < 0f)
                    weightRepProb = 0f;
            }
            else
            {
                crossoverRate = Random.Range(0f, 1f);
                if (crossoverRate < 0.5f)
                    weightRepProb = mum.mutPar.weightRepProb;
                else
                    weightRepProb = dad.mutPar.weightRepProb;
            }
            if (Random.Range(0f, 1f) < GA_Parameters.mutParMutRate)
            {
                maxActivationPerturbation = Random.Range(0f, 1f);
                if (maxActivationPerturbation > 1f)
                    maxActivationPerturbation = 1f;
                else if (maxActivationPerturbation < 0f)
                    maxActivationPerturbation = 0f;
            }
            else
            {
                crossoverRate = Random.Range(0f, 1f);
                if (crossoverRate < 0.5f)
                    maxActivationPerturbation = mum.mutPar.maxActivationPerturbation;
                else
                    maxActivationPerturbation = dad.mutPar.maxActivationPerturbation;
            }
            if (Random.Range(0f, 1f) < GA_Parameters.mutParMutRate)
            {
                activationMutationChance = Random.Range(0f, 1f);
                if (activationMutationChance > 1f)
                    activationMutationChance = 1f;
                else if (activationMutationChance < 0f)
                    activationMutationChance = 0f;
            }
            else
            {
                crossoverRate = Random.Range(0f, 1f);
                if (crossoverRate < 0.5f)
                    activationMutationChance = mum.mutPar.activationMutationChance;
                else
                    activationMutationChance = dad.mutPar.activationMutationChance;
            }

        }

    }

    // Save and Load can be implemented
}
