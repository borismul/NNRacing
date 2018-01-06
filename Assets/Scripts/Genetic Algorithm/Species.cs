using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Species
{
    public Genome leader;
    public List<Genome> members;
    public int ID;
    public float bestFitness;
    public int generationsNoImprovement;
    public int age;
    public float spawnsRequired;

    public Species(Genome startingGenome, int ID)
    {
        this.ID = ID;
        bestFitness = startingGenome.GetFitness();
        generationsNoImprovement = 0;
        age = 0;
        leader = startingGenome;
        spawnsRequired = 0;

        members = new List<Genome>();
        members.Add(startingGenome);

        //Random.InitState((int)System.DateTime.Now.Ticks);
    }

    public void AddMember(Genome genome)
    {
        if(leader == null || genome.GetFitness() > leader.GetFitness())
        {
            bestFitness = genome.GetFitness();
            generationsNoImprovement = 0;
            leader = genome;
        }

        members.Add(genome);
    }

    public void Purge()
    {
        members.Clear();

        age++;
        generationsNoImprovement++;
        spawnsRequired = 0;
    }

    public void AdjustFitnesses()
    {
        for (int i = 0; i < members.Count; i++)
        {
            float fitness = members[i].GetFitness();

            if (age < GA_Parameters.youngBonusAgeThreshhold)
                fitness *= GA_Parameters.youngFitnessBonus;
            else if (age > GA_Parameters.oldAgeThreshHold)
                fitness *= GA_Parameters.oldAgePenalty;


            float adjustedFitness = fitness / members.Count;
            members[i].SetAdjFitness(adjustedFitness);
        }
    }

    public void CaculateSpawnAmount()
    {
        for (int i = 0; i < members.Count; i++)
            spawnsRequired += members[i].GetAmountToSpawn();
    }

    public Genome Spawn()
    {
        Genome offSpring;

        if (members.Count == 1)
            offSpring = members[0];
        else
        {
            int maxIndexSize = (int)(GA_Parameters.survivalRate * members.Count) + 1;

            int index = Random.Range(0, maxIndexSize);
            offSpring = members[index];

            //int maxN = members.Count * (members.Count + 1) / 2;
            //int n = Random.Range(0, maxN);
            //int sum = 0;
            //int index = 0;
            //for (int i = 0; i < members.Count; i++)
            //{
            //    sum += i;

            //    if (sum > n)
            //    {
            //        index = members.Count - 1 - i;
            //        break;
            //    }

            //}

            //offSpring = members[index];
        }

        return offSpring;
    }

}
