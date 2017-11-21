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

    static List<NodeGene> offspringPerceptrons = new List<NodeGene>();
    static List<ConnectionGene> offspringConnections = new List<ConnectionGene>();

    static List<int> newPerceptrons = new List<int>();
    static MutParameters newMutPar;
    List<Perceptron> newPerceptronsObjects = new List<Perceptron>();

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
        //Random.InitState((int)System.DateTime.Now.Ticks);

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
            perceptrons.Add(new NodeGene(curGene.type, curGene.ID, curGene.splitValues, curGene.recurrent, curGene.actResponse));
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

    public void InitializeWeights()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            connections[i].weight = Random.Range(-1f, 1f);
        }
    }

    // Unit Tested
    public NeuralNetwork CreateNetwork()
    {

        newPerceptronsObjects.Clear();
        for(int i = 0; i < this.perceptrons.Count; i++)
        {
            if (newPerceptrons.Count <= i)
            {
                Perceptron perceptron = new Perceptron(this.perceptrons[i].type,
                                                        this.perceptrons[i].ID,
                                                        this.perceptrons[i].splitValues,
                                                        this.perceptrons[i].actResponse);
                newPerceptronsObjects.Add(perceptron);
            }
            else
            {
                Perceptron perceptron = new Perceptron(this.perceptrons[i].type,
                                                        this.perceptrons[i].ID,
                                                        this.perceptrons[i].splitValues,
                                                        this.perceptrons[i].actResponse);
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


                Link connection = new Link( connections[i].weight,
                                            from,
                                            to,
                                            connections[i].recurrent);

                from.outLinks.Add(connection);
                to.inLinks.Add(connection);
            }
        }

        network = new NeuralNetwork(ref newPerceptronsObjects, inputs);
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
                int percepPos = Random.Range(inputs + 1, perceptrons.Count - 1);

                if(!perceptrons[percepPos].recurrent && perceptrons[percepPos].type != NodeType.Bias && perceptrons[percepPos].type != NodeType.Sensor)
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
                percep2 = perceptrons[Random.Range(inputs + 1, perceptrons.Count - 1)].ID;

                if(DuplicateLink(percep1, percep2) || percep1 == percep2)
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

        if (perceptrons[GetElementPos(percep1)].splitValues.y - perceptrons[GetElementPos(percep2)].splitValues.y > 0.01f)
            recurrent = true;

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
            if(g1 == connections.Count  - 1)
            {
                g2++;
                numExcess++;
                continue;
            }

            if (g2 == genome.connections.Count - 1)
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
        connections = connections.OrderBy(o => o.innovNum).ToList();
    }

    public static Genome Crossover(Genome mum, Genome dad, GeneticAlgorithm ga)
    {
        BestParent bestParent = GetBestParent(ref mum, ref dad);

        List<NodeGene> offspringPerceptrons = new List<NodeGene>();
        List<ConnectionGene> offspringConnections = new List<ConnectionGene>();

        List<int> newPerceptrons = new List<int>();

        int curMum = 0;
        int curDad = 0;

        ConnectionGene selectedGene = null;

        while(!(curMum == mum.connections.Count && curDad == dad.connections.Count))
        {
            if (curMum == mum.connections.Count && curDad != dad.connections.Count)
            {
                if(bestParent == BestParent.dad)
                    selectedGene = dad.GetConnectionGenes()[curDad];

                curDad++;
            }
            else if(curMum != mum.connections.Count && curDad == dad.connections.Count)
            {
                if (bestParent == BestParent.mum)
                    selectedGene = mum.GetConnectionGenes()[curMum];

                curMum++;
            }
            else if(mum.GetConnectionGenes()[curMum].innovNum < dad.GetConnectionGenes()[curDad].innovNum)
            {
                if(bestParent == BestParent.mum)
                {
                    selectedGene = mum.GetConnectionGenes()[curMum];
                }

                curMum++;
            }
            else if (mum.GetConnectionGenes()[curMum].innovNum > dad.GetConnectionGenes()[curDad].innovNum)
            {
                if (bestParent == BestParent.dad)
                {
                    selectedGene = dad.GetConnectionGenes()[curDad];
                }

                curDad++;
            }
            else if (mum.GetConnectionGenes()[curMum].innovNum == dad.GetConnectionGenes()[curDad].innovNum)
            {
                if (Random.Range(0f, 1f) > 0.5f)
                    selectedGene = mum.GetConnectionGenes()[curMum];
                else
                    selectedGene = dad.GetConnectionGenes()[curDad];

                curDad++;
                curMum++;
            }

            if (offspringConnections.Count == 0)
                offspringConnections.Add(selectedGene);
            else
            {
                if(offspringConnections[offspringConnections.Count - 1].innovNum != selectedGene.innovNum)
                {
                    offspringConnections.Add(selectedGene);
                }
            }
            AddPercepID(selectedGene.from, ref newPerceptrons);
            AddPercepID(selectedGene.to, ref newPerceptrons);

        }

        newPerceptrons.Sort();

        for (int i = 0; i < newPerceptrons.Count; i++)
            offspringPerceptrons.Add(ga.innovations.CreateNeuronFromID(newPerceptrons[i]));

        MutParameters mutPar = new MutParameters(mum, dad);


        Genome offspring = new Genome(ga.nextGenomeID, offspringPerceptrons, offspringConnections, mum.inputs, mum.outputs, mutPar);
        ga.nextGenomeID++;

        return offspring;

    }

    static void AddPercepID(int nodeID, ref List<int> perceptrons)
    {
        for(int i = 0; i < perceptrons.Count; i++)
        {
            if(perceptrons[i] == nodeID)
            {
                return;
            }
        }

        perceptrons.Add(nodeID);
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
