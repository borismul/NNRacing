using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GA_Parameters {

    // Selection Parameters
    public static int youngBonusAgeThreshhold = 10;
    public static float youngFitnessBonus = 1.3f;
    public static int oldAgeThreshHold = 50;
    public static float oldAgePenalty = 0.7f;
    public static float survivalRate = 0.7f;

    // Mutation Parameters
    public static float chanceAddPerceptron = 0.1f;

    public static float chanceToAddLink = 0.03f;
    public static float chanceRecurrentLink = 0.07f;

    public static float activationMutationChance = 0.1f;
    public static float maxActivationPerturbation = 0.5f;

    public static float weightMutationRate = 0.1f;
    public static float maxWeightPerturbation = 0.5f;
    public static float weightReplaceProb = 0.3f;

    public static int maxPermittedPerceptrons = 500;

    // Species Parameters
    public static int maxSpecies = 30;
    public static int numGensAllowedNoImprovement = 20;
    public static float compatibilityThreshold = 0.33f;

    // Crossover Parameters
    public static float crossOverRate = 0.7f;

    // GA main properties
    public static int populationSize = 1;
    public static int fps;
    public static int laps;
    public static float simulationTime;
    public static bool stopAtCrash = false;
    public static float savePercentage;

    // NN properties
    public static int inputs = 8;
    public static int outputs = 4;

    // Car Properies
    public static float accSpeed;
    public static float breakSpeed;
    public static float turnSpeed;
    public static float maxSpeed;

    // Under The Hood Properties
    public static int carsPerFrame;
    public static int carUpdateRate = 0;

    public static int triesToFindLink = 100;
    public static int triesToFindLoopedLink = 100;
    public static int addLinkAttempts = 100;


}
