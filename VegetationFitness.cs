using System;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

public struct VegetationFitness
{
    public double Grass;
    public double TreeConifer;
    public double TreeDeciduous;
    public double TreeTropical;
    
    public VegetationFitness(double temperature, double moisture)
    {
        Grass = 0.0;
        TreeConifer = 0.0;
        TreeDeciduous = 0.0;
        TreeTropical = 0.0;
        Generate(temperature, moisture);
    }
    
    public void Generate(double temperature, double moisture)
    {
        Grass = ComputeFitness(temperature, moisture, 283.0, 30.0, 80.0, 3000.0);
        TreeConifer = ComputeFitness(temperature, moisture, 258.0, 20.0, 250.0, 750.0);
        TreeDeciduous = ComputeFitness(temperature, moisture, 288.0, 20.0, 700.0, 1200.0);
        TreeTropical = ComputeFitness(temperature, moisture, 298.0, 20.0, 1500.0, 2000.0);
    }
    
    private double ComputeFitness(double temperature, double moisture, double perfectTemp, double tempTolerance, double neededMoisture, double perfectMoisture)
    {
        double curFitness = 100.0;
        
        double fitnessPerKelvin = 100.0 / tempTolerance;
        curFitness -= Math.Abs(perfectTemp - temperature) * fitnessPerKelvin;
        curFitness = Math.Max(curFitness, 0.0);
        
        double moistureEffectRange = perfectMoisture - neededMoisture;
        
        double moistureValue = (moisture - neededMoisture) / moistureEffectRange;
        moistureValue = Math.Min(moistureValue, 1.0);
        moistureValue = Math.Max(moistureValue, 0.0);
        curFitness *= moistureValue;
        
        curFitness = Math.Min(curFitness, 100.0);
        curFitness = Math.Max(curFitness, 0.0);
        
        return curFitness;
    }
    
    public Color GetPixelColor()
    {
        if (TreeTropical > 0 && TreeTropical >= TreeConifer && TreeTropical >= TreeDeciduous && TreeTropical >= Grass)
        {
            return new Color(0,230,0,255);
        }
        if (TreeDeciduous > 0 && TreeDeciduous >= TreeConifer && TreeDeciduous >= Grass)
        {
            return new Color(50,180,50,255);
        }
        if (TreeConifer > 0 && TreeConifer >= Grass)
        {
            return new Color(20,100,20,255);
        }
        if (Grass > 0)
        {
            //return new Color(100,200,100,255);
            return TerrainGenDemo.ColorLerp(new Color(200,200,50,255), new Color(100,200,100,255), Math.Min(Grass/20.0, 1.0));
        }
        return new Color(0,0,0,0);
    }
    
    public override string ToString()
    {
        return String.Format("Grass: {0:F1} -- Conif: {1:F1} -- Decid: {2:F1} -- Trop: {3:F1}", Grass, TreeConifer, TreeDeciduous, TreeTropical);
    }
}