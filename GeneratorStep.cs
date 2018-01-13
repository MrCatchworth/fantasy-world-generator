using System;

public class GeneratorStep
{
    public delegate void StepApplier(World w, int seed);
    
    public readonly WorldGenerator Owner;
    public readonly string Name;
    private readonly string[] Dependencies;
    private readonly StepApplier Applier;
    public bool Finished;
    
    public GeneratorStep(WorldGenerator owner, string name, string[] deps, StepApplier app)
    {
        Owner = owner;
        Name = name;
        Applier = app;
        Dependencies = (string[])deps.Clone();
        Finished = false;
    }
    
    private bool DependenciesSatisfied()
    {
        foreach (string dep in Dependencies)
        {
            if (!Owner.GetStep(dep).Finished)
            {
                return false;
            }
        }
        return true;
    }
    
    public bool CanRun()
    {
        if (Finished)
        {
            return false;
        }
        return DependenciesSatisfied();
    }
    
    public void Apply(World w, int seed)
    {
        if (!DependenciesSatisfied())
        {
            throw new InvalidOperationException(Name+": A dependent generation step hasn't been run yet");
        }
        if (Finished)
        {
            throw new InvalidOperationException(Name+": This step was alread run; clear finished flag to run again");
        }
        Applier(w, seed);
        Finished = true;
    }
}