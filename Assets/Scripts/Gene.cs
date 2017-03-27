using UnityEngine;
using System.Collections.Generic;

public class Gene {
    public static int maximumAllowedPerceptronsInLayer = 8;
    public static int maximumAllowedLayers = 4;
    public string networkGene;
    public List<float> weightsGene;
    public Gene(string networkGene, List<float> weightsGene)
    {
        this.networkGene = networkGene;
        this.weightsGene = weightsGene;
    }   

    public static Gene Encode(NeuralNetwork network)
    {
        List<List<Perceptron>> layers = network.GetFullNetwork();
        string netGene = "";
        List<float> wGene = new List<float>();

        int number = -1;
        // Layers
        for (int i = 0; i < maximumAllowedLayers; i++)
        {
            number++;
            // Perceptrons
            for (int j = 0; j < maximumAllowedPerceptronsInLayer; j++)
            {
                if (layers[i][j] != null)
                {
                    netGene += "1";
                    wGene.Add(layers[i][j].theta);
                }
                else
                {
                    netGene += "0";
                    wGene.Add(0);
                }

                number++;
                // Connections
                for (int k = 0; k < maximumAllowedLayers; k++)
                {
                    for (int l = 0; l < maximumAllowedPerceptronsInLayer; l++)
                    {
                        if (k <= i)
                            continue;

                        bool hasConnection = false;
                        PerceptronConnection equalConnection = null;

                        if (layers[i][j] == null || layers[k][l] == null)
                        {
                            netGene += "00";
                            wGene.Add(0);
                            continue;
                        }
                        PerceptronConnection connection = new PerceptronConnection(layers[i][j], layers[k][l], 0);
                        foreach (PerceptronConnection compareConnection in layers[i][j].inputConnections)
                        {
                            if (connection.Equals(compareConnection))
                            {
                                hasConnection = true;
                                equalConnection = compareConnection;
                                break;
                            }
                        }
                        if (!hasConnection)
                        {
                            foreach (PerceptronConnection compareConnection in layers[i][j].outputConnections)
                            {
                                if (connection.Equals(compareConnection))
                                {
                                    hasConnection = true;
                                    equalConnection = compareConnection;
                                    break;
                                }
                            }
                        }
                        if (hasConnection)
                        {
                            netGene += "1";
                            wGene.Add(equalConnection.weight);
                            if (equalConnection.inputPerceptron.Equals(layers[i][j]))
                            {
                                netGene += "1";
                            }
                            else
                            {
                                netGene += "0";
                            }

                        }
                        else
                        {
                            netGene += "00";
                            wGene.Add(0);
                        }

                    }
                }
            }
        }
        return new Gene(netGene, wGene);
    }

    public static NeuralNetwork Decode(Gene gene)
    {
        string netGene = gene.networkGene;
        List<float> wGene = gene.weightsGene;
        NeuralNetwork network = new NeuralNetwork();

        int number = -1;
        int number2 = -1;
        int totalPerceptrons = 0;
        bool nowInputRow = false;
        bool inputDone = false;
        for (int i = 0; i < maximumAllowedLayers; i++)
        {
            network.GetFullNetwork().Add(new List<Perceptron>());

            if (nowInputRow && !inputDone)
            {
                inputDone = true;
                nowInputRow = false;
            }

            for (int j = 0; j < maximumAllowedPerceptronsInLayer; j++)
            {
                number++;
                number2++;
                int current = (int)char.GetNumericValue(netGene[number]);

                Perceptron newPerceptron;
                if (current == 1)
                {
                    if (!nowInputRow && !inputDone)
                        nowInputRow = true;
                    newPerceptron = new Perceptron(i, j, totalPerceptrons, wGene[number2], network);

                    if (nowInputRow && !inputDone)
                        newPerceptron.LinkPerceptrons(newPerceptron, 1f);

                    network.GetFullNetwork()[i].Add(newPerceptron);
                    totalPerceptrons++;   
                }
                else
                {
                    newPerceptron = null;
                    network.GetFullNetwork()[i].Add(newPerceptron);
                }

                number += 2 * (maximumAllowedLayers - i - 1) * maximumAllowedPerceptronsInLayer;
                number2 += (maximumAllowedLayers - i - 1) * maximumAllowedPerceptronsInLayer;
            }
        }

        number = -1;
        number2 = -1;
        for (int i = 0; i < maximumAllowedLayers; i++)
        {
            for (int j = 0; j < maximumAllowedPerceptronsInLayer; j++)
            {
                number++;
                number2++;
                int current = (int)char.GetNumericValue(netGene[number]);

                Perceptron newPerceptron = network.GetFullNetwork()[i][j];
                for (int k = i + 1; k < maximumAllowedLayers; k++)
                {
                    for (int l = 0; l < maximumAllowedPerceptronsInLayer; l++)
                    {
                        if (newPerceptron == null || network.GetFullNetwork()[k][l] == null)
                        {
                            number += 2;
                            number2++;
                            continue;
                        }
                        number++;
                        number2++;
                        current = (int)char.GetNumericValue(netGene[number]);
                        number++;

                        if (current == 1)
                        {
                            current = (int)char.GetNumericValue(netGene[number]);

                            if (current == 1)
                            {
                                newPerceptron.LinkPerceptrons(network.GetFullNetwork()[k][l], wGene[number2]);
                            }
                            else
                            {
                                network.GetFullNetwork()[k][l].LinkPerceptrons(newPerceptron, wGene[number2]);
                            }
                        }
                    }
                }
            }
        }

        for (int i = 0; i < network.GetFullNetwork().Count; i++)
        {
            network.GetLayers().Add(new List<Perceptron>());
            for (int j = 0; j < network.GetFullNetwork()[i].Count; j++)
            {
                network.GetLayers()[i].Add(network.GetFullNetwork()[i][j]);
            }
        }

        network.CleanUpNetwork();
        network.RemoveEmptyLayers();
        return network;
    }

    public static NeuralNetwork MakeChild(Gene dad, Gene mom, float networkMutationChance, float weighsMutationChance)
    {
        string networkGene = "";
        List<float> weightsGene = new List<float>();

        int number = -1;
        for (int i = 0; i < maximumAllowedLayers; i++)
        {
            for (int j = 0; j < maximumAllowedPerceptronsInLayer; j++)
            {
                number++;

                if ((float)NeuralNetwork.rand.NextDouble() < networkMutationChance)
                {
                    if ((float)NeuralNetwork.rand.NextDouble() > 0.5f)
                    {
                        networkGene += 1.ToString();
                    }
                    else
                    {
                        networkGene += 0.ToString();
                    }

                }
                else {
                    // Copy if has perceptron at this [i,j] position
                    if ((float)NeuralNetwork.rand.NextDouble() > 0.5f)
                        networkGene += dad.networkGene[number];
                    else
                        networkGene += mom.networkGene[number];
                }


                for (int k = i + 1; k < maximumAllowedLayers; k++)
                {
                    for (int l = 0; l < maximumAllowedPerceptronsInLayer; l++)
                    {
                        if ((float)NeuralNetwork.rand.NextDouble() < networkMutationChance)
                        {
                            for (int m = 0; m < 2; m++)
                            {
                                if ((float)NeuralNetwork.rand.NextDouble() > 0.5f)
                                {
                                    networkGene += 1.ToString();
                                }
                                else
                                {
                                    networkGene += 0.ToString();
                                }
                                number++;
                            }

                        }
                        else
                        {
                            number++;
                            bool isDad = (float)NeuralNetwork.rand.NextDouble() > 0.5f;

                            if (isDad)
                            {
                                networkGene += dad.networkGene[number].ToString() + dad.networkGene[number + 1].ToString();

                            }
                            else
                            {
                                networkGene += mom.networkGene[number].ToString() + mom.networkGene[number + 1].ToString();
                            }
                            number++;
                        }
                    }
                }
            }

        }

        for (int i = 0; i < dad.weightsGene.Count; i++)
        {
            float p = (float)NeuralNetwork.rand.NextDouble();
            float newWeight = mom.weightsGene[i] * p + (dad.weightsGene[i]) * (1 - p);

            if((float)NeuralNetwork.rand.NextDouble() < weighsMutationChance)
            {
                newWeight += ((float)NeuralNetwork.rand.NextDouble() - 0.5f)/2;
            }

            weightsGene.Add(newWeight);
        }

        NeuralNetwork child = Gene.Decode(new Gene(networkGene, weightsGene));
        child.NormalizeWeights();
        return child;

    }
}
