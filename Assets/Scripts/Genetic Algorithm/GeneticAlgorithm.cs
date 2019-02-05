using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GeneticAlgorithm
{
    public List<Genome> genomes;
    public List<List<Genome>> oldGenomes;
    public List<Genome> bestGenomes;
    public List<Species> species;
    public Innovations innovations;
    public int currentGeneration;
    public int nextGenomeID;
    public int nextSpeciesID;
    public int populationSize;

    public float totalAdjustedFitness;
    public float averageAdjustedFitness;

    public int FittestGenome;
    public float bestFitnessSoFar;

    List<Genome> newPopulation = new List<Genome>();
    List<NeuralNetwork> newNetworks = new List<NeuralNetwork>();

    public GeneticAlgorithm(int size, int inputs, int outputs)
    {
        populationSize = size;
        currentGeneration = 0;
        innovations = null;
        nextGenomeID = 0;
        nextSpeciesID = 0;
        FittestGenome = 0;
        bestFitnessSoFar = 0;
        totalAdjustedFitness = 0;
        averageAdjustedFitness = 0;

        genomes = new List<Genome>();
        bestGenomes = new List<Genome>();
        species = new List<Species>();
        oldGenomes = new List<List<Genome>>();

        for (int i = 0; i < size; i++)
        {
            genomes.Add(new Genome(nextGenomeID, inputs, outputs, new Genome.MutParameters()));
            nextGenomeID++;
        }
        Genome genome = new Genome(1, inputs, outputs, new Genome.MutParameters());

        innovations = new Innovations(genome.GetConnectionGenes(), genome.GetPerceptronGenes());

    }

    public GeneticAlgorithm(int size, int inputs, int hidden, int[] hiddenNodes, int outputs)
    {
        populationSize = size;
        currentGeneration = 0;
        innovations = null;
        nextGenomeID = 0;
        nextSpeciesID = 0;
        FittestGenome = 0;
        bestFitnessSoFar = 0;
        totalAdjustedFitness = 0;
        averageAdjustedFitness = 0;

        genomes = new List<Genome>();
        bestGenomes = new List<Genome>();
        species = new List<Species>();
        oldGenomes = new List<List<Genome>>();

        for (int i = 0; i < size; i++)
        {
            genomes.Add(new Genome(nextGenomeID, inputs, hidden, hiddenNodes, outputs, new Genome.MutParameters()));
            nextGenomeID++;
        }

        Genome genome = new Genome(nextGenomeID, inputs, hidden, hiddenNodes, outputs, new Genome.MutParameters());

        innovations = new Innovations(genome.GetConnectionGenes(), genome.GetPerceptronGenes());
        GA_Parameters.maxSpecies = GA_Parameters.populationSize - 1;

    }

    public List<NeuralNetwork> CreateNetworks()
    {
        List<NeuralNetwork> networks = new List<NeuralNetwork>();

        for(int i =0; i < populationSize; i++)
        {
            NeuralNetwork network = genomes[i].CreateNetwork();
            networks.Add(network);
        }
        return networks;
    }

    public List<NeuralNetwork> DoGeneration(List<float> fitnesses, bool complexify)
    {
        ResetAndKill();

        for (int i = 0; i < genomes.Count; i++)
            genomes[i].SetFitness(fitnesses[i]);
        SortAndRecord();

        SpeciateAndCaluclateSpawnLevels();

        newPopulation.Clear();


        Genome offSpring = null;

        float bestFitness = -10000;
        int bestSpeciesIndex = -1;
        //for (int i = 0; i < species.Count; i++)
        //{
        //    if (species[i].leader.GetFitness() > bestFitness)
        //    {
        //        bestSpeciesIndex = i;
        //        bestFitness = species[i].leader.GetFitness();
        //    }
        //}

        //offSpring = new Genome(species[bestSpeciesIndex].leader);
        //newPopulation.Add(offSpring);

        if (complexify)
            Complexify(bestSpeciesIndex);
        else
            Simplify(bestSpeciesIndex);


        int storeNumber = Mathf.FloorToInt(genomes.Count * GA_Parameters.savePercentage / 100);
        if (storeNumber == 0)
            storeNumber = 1;

        oldGenomes.Add(new List<Genome>());
        for (int i = 0; i < storeNumber; i++)
        {
            oldGenomes[oldGenomes.Count-1].Add(genomes[i]);
        }
        genomes = newPopulation;
        newNetworks.Clear();

        for(int i = 0; i< genomes.Count; i++)
        {
            bool leader = false;
            bool bestOfAll = false;

            for (int j = 0; j < species.Count; j++)
            {
                if (i == 0)
                {
                    bestOfAll = true;
                    break;
                }
                else if (genomes[i].GetID() == species[j].leader.GetID())
                {
                    leader = true;
                    break;

                }
            }

            NeuralNetwork network = genomes[i].CreateNetwork(bestOfAll, leader);
            newNetworks.Add(network);
        }

        currentGeneration++;

        if(Random.Range(0f,1f) > 0.9f)
            System.GC.Collect();
        return newNetworks;
    }

    void Complexify(int bestSpeciesIndex)
    {
        int spawnedSoFar = 0;
        Genome offSpring;
        for (int i = 0; i < species.Count; i++)
        {
            if (spawnedSoFar < GA_Parameters.populationSize)
            {
                int NumToSpawn = Mathf.RoundToInt(species[i].spawnsRequired);
                bool chosenBestYet = false;

                while (NumToSpawn > 0)
                {
                    offSpring = null;
                    if (!chosenBestYet && i != bestSpeciesIndex)
                    {
                        offSpring = new Genome(species[i].leader);
                        chosenBestYet = true;
                    }
                    else
                    {
                        if (species[i].members.Count == 1)
                        {
                            offSpring = species[i].Spawn();
                        }
                        else
                        {
                            Genome g1 = species[i].Spawn();
                            if (Random.Range(0f, 1f) < GA_Parameters.crossOverRate)
                            {
                                Genome g2 = species[i].Spawn();

                                int attempts = 5;

                                while (g1.GetID() == g2.GetID() && attempts > 0)
                                {
                                    g2 = species[i].Spawn();

                                    attempts--;
                                }

                                if (g1.GetID() != g2.GetID())
                                {
                                    offSpring = Genome.Crossover(g1, g2, this);
                                }
                                else
                                    offSpring = g1;
                            }
                            else
                            {
                                offSpring = g1;
                            }
                        }

                        nextGenomeID++;
                        offSpring.SetID(nextGenomeID);
                    }



                    if (offSpring.NumPerceptrons() < GA_Parameters.maxPermittedPerceptrons)
                        offSpring.AddNeuron(offSpring.mutPar.addPercepProb, ref innovations, GA_Parameters.triesToFindLink);

                    offSpring.AddLink(offSpring.mutPar.addLinkProb, offSpring.mutPar.recurAddProb, ref innovations, GA_Parameters.triesToFindLoopedLink, GA_Parameters.addLinkAttempts);
                    offSpring.MutateWeights(offSpring.mutPar.weightMutRate, offSpring.mutPar.weightRepProb, offSpring.mutPar.maxWeightPerturbation);

                    offSpring.MutateActivationResponse(offSpring.mutPar.activationMutationChance, offSpring.mutPar.maxActivationPerturbation);
                    offSpring.SortGenes();

                    newPopulation.Add(offSpring);
                    spawnedSoFar++;

                    if (spawnedSoFar == GA_Parameters.populationSize)
                        NumToSpawn = 0;

                    NumToSpawn--;
                }
            }
        }

        if (spawnedSoFar < populationSize)
        {
            int required = GA_Parameters.populationSize - spawnedSoFar;

            while (required > 0)
            {
                newPopulation.Add(TournamentSelection(populationSize / 5));
                required--;
            }
        }
    }

    void Simplify(int bestSpeciesIndex)
    {
        int spawnedSoFar = 0;
        Genome offSpring;
        for (int i = 0; i < species.Count; i++)
        {
            if (spawnedSoFar < GA_Parameters.populationSize)
            {
                int NumToSpawn = Mathf.RoundToInt(species[i].spawnsRequired);
                bool chosenBestYet = false;

                while (NumToSpawn > 0)
                {
                    offSpring = null;
                    if (!chosenBestYet && i != bestSpeciesIndex)
                    {
                        offSpring = new Genome(species[i].leader);
                        chosenBestYet = true;
                    }
                    else
                    {
                        offSpring = species[i].Spawn();

                        nextGenomeID++;
                        offSpring.SetID(nextGenomeID);


                    }

                    offSpring.RemoveNeuron(offSpring.mutPar.addPercepProb, ref innovations);

                    offSpring.RemoveLink(offSpring.mutPar.addLinkProb);
                    offSpring.RemoveRecurrent(offSpring.mutPar.recurAddProb);
                    offSpring.MutateWeights(offSpring.mutPar.weightMutRate, offSpring.mutPar.weightRepProb, offSpring.mutPar.maxWeightPerturbation);

                    offSpring.MutateActivationResponse(offSpring.mutPar.activationMutationChance, offSpring.mutPar.maxActivationPerturbation);
                    offSpring.SortGenes();

                    newPopulation.Add(offSpring);
                    spawnedSoFar++;

                    if (spawnedSoFar == GA_Parameters.populationSize)
                        NumToSpawn = 0;

                    NumToSpawn--;
                }
            }
        }

        if (spawnedSoFar < populationSize)
        {
            int required = GA_Parameters.populationSize - spawnedSoFar;

            while (required > 0)
            {
                newPopulation.Add(TournamentSelection(populationSize / 5));
                required--;
            }
        }
    }

    Genome TournamentSelection(int numComparisons)
    {
        float bestFitness = 0;

        int index = 0;

        for(int i = 0; i < numComparisons; i++)
        {
            int thisTry = Random.Range(0, genomes.Count);

            if(genomes[thisTry].GetFitness() > bestFitness)
            {
                index = thisTry;
                bestFitness = genomes[thisTry].GetFitness();
            }
        }

        return genomes[index];
    }

    void ResetAndKill()
    {
        totalAdjustedFitness = 0;
        averageAdjustedFitness = 0;

        for (int i = 0; i < species.Count; i++)
        {
            species[i].Purge();

            if (species[i].generationsNoImprovement > GA_Parameters.numGensAllowedNoImprovement && species[i].bestFitness < bestFitnessSoFar)
            {
                species.RemoveAt(i);

                if (species.Count != 1)
                    i--;
            }
        }
    }

    void SortAndRecord()
    {
        genomes = genomes.OrderByDescending(o => o.GetFitness()).ToList();

        if (genomes[0].GetFitness() > bestFitnessSoFar)
            bestFitnessSoFar = genomes[0].GetFitness();

    }

    void AdjustCompatibiltyThreshHold()
    {
        if (GA_Parameters.maxSpecies < 1)
            return;

        float threshHoldIncrement = 0.01f;

        if (species.Count > GA_Parameters.maxSpecies)
            GA_Parameters.compatibilityThreshold += threshHoldIncrement;
        else if (species.Count < 2)
            GA_Parameters.compatibilityThreshold -= threshHoldIncrement;
        
    }

    void AdjustSpeciesFitness()
    {
        for (int i = 0; i < species.Count; i++)
            species[i].AdjustFitnesses();
    }

    void SpeciateAndCaluclateSpawnLevels()
    {
        bool added = false;
        AdjustCompatibiltyThreshHold();
        
        for(int i = 0; i < genomes.Count; i++)
        {
            for(int j = 0; j < species.Count; j++)
            {
                float compatibility = genomes[i].GetCompatibilityScore(ref species[j].leader);

                if(compatibility < GA_Parameters.compatibilityThreshold)
                {
                    species[j].AddMember(genomes[i]);

                    genomes[i].SetSpecies(species[j].ID);
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                species.Add(new Species(genomes[i], nextSpeciesID));
                nextSpeciesID++;
            }

            added = false;
        }

        AdjustSpeciesFitness();

        for (int i = 0; i < genomes.Count; i++)
            totalAdjustedFitness += genomes[i].GetAdjFitness();

        averageAdjustedFitness = totalAdjustedFitness / genomes.Count;

        for(int i = 0; i < genomes.Count; i++)
        {
            float toSpawn = genomes[i].GetAdjFitness() / averageAdjustedFitness;
            genomes[i].SetAmountToSpawn(toSpawn);
        }

        for (int i = 0; i < species.Count; i++)
            species[i].CaculateSpawnAmount();
    }
}
