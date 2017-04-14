//using UnityEngine;
//using System.Collections.Generic;

//public class Generation
//{

//    public List<NeuralNetwork> networks;
//    public float maxFitness;
//    public float avgFitness;

//    public Generation(List<NeuralNetwork> networks)
//    {
//        this.networks = networks;
//    }

//    public void SetFitness(float maxFitness, float avgFitness)
//    {
//        this.maxFitness = maxFitness;
//        this.avgFitness = avgFitness;
//    }

//    public void Order()
//    {
//        List<NeuralNetwork> orderedList = new List<NeuralNetwork>();

//        foreach (NeuralNetwork network in networks)
//        {
//            //if (network.Fitness == 0)
//            //    continue;

//            bool didAdd = false;
//            for (int i = 0; i < orderedList.Count; i++)
//            {
//                if (network.Fitness >= orderedList[i].Fitness)
//                {
//                    didAdd = true;
//                    orderedList.Insert(i, network);
//                    break;
//                }
//            }

//            if (!didAdd)
//                orderedList.Add(network);
//        }

//        networks = orderedList;
//    }
//}
