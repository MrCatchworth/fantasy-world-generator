using System;
using System.Collections.Generic;

public class WorldGenerator
{
    private List<GeneratorStep> Steps;
    public readonly World MyWorld;
    
    public bool Finished
    {
        get;
        private set;
    }
    
    public WorldGenerator(World w)
    {
        Steps = new List<GeneratorStep>();
        MyWorld = w;
        Finished = false;
    }
    
    public void AddStep(GeneratorStep step)
    {
        if (!Steps.Contains(step))
        {
            Steps.Add(step);
        }
    }
    
    public GeneratorStep GetStep(string name)
    {
        foreach (GeneratorStep step in Steps)
        {
            if (step.Name == name)
            {
                return step;
            }
        }
        return null;
    }
    
    public void Run(int seed)
    {
        int stepsLeft = Steps.Count;
        
        while (stepsLeft > 0)
        {
            bool stepThisRun = false;
            
            foreach (GeneratorStep step in Steps)
            {
                if (step.CanRun())
                {
                    Console.WriteLine("Applying step "+step.Name);
                    
                    step.Apply(MyWorld, seed);
                    stepsLeft--;
                    stepThisRun = true;
                    
                    if (stepsLeft == 0)
                    {
                        return;
                    }
                }
            }
            
            if (!stepThisRun)
            {
                Finished = true;
                throw new InvalidOperationException("Ran out of steps while world generation was still running");
            }
        }
        
        Finished = true;
    }
    
    public void Reset()
    {
        foreach (GeneratorStep step in Steps)
        {
            step.Finished = false;
        }
        Finished = false;
    }
}