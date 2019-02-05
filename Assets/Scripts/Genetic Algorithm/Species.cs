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
        //for(int i = 0; i < members.Count; i++)
        //{
        //    if (members[i].GetID() != leader.GetID())
        //        Genome.freeGenomes.Add(members[i]);
        //}
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

            int maxIndex = (int)(GA_Parameters.survivalRate * members.Count);
            //int index = Genome.RouletteSelection(maxIndex);

            int index = Random.Range(0, maxIndex);

            //if (Genome.freeGenomes.Count > 0)
            //{
            //    Genome genome = Genome.freeGenomes[0];
            //    Genome.freeGenomes.RemoveAt(0);
            //    genome.SetGenome(members[index]);
            //    return genome;
            //}
            //else
                return new Genome(members[index]);
            //int n = (int)(GA_Parameters.survivalRate * members.Count);

            //int index = Mathf.RoundToInt(Random.Range(0f, 1f) * (n * (n + 1) / 2));
            //int temp = 0;

            //for (int i = n; i > 0; i--)
            //{
            //    temp += n - (i - 1);

            //    if (index < temp)
            //    {
            //        return members[i - 1];
            //    }
            //}
        }

        return new Genome(members[0]);
    }

}
