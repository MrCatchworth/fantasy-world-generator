using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

public abstract class Updaters
{
    public static void TempImage(World world, Image img)
    {
        double[,] tempMap = world.GetMap<double>("temperature");
        Tuple<double,double> bounds = TerrainGenDemo.ArrayBounds2D(tempMap);
        
        double minTemp = bounds.Item1;
        double maxTemp = bounds.Item2;
        
        //int maxTemp = (int)world.GetProperty("maxtemp");
        //int minTemp = (int)world.GetProperty("mintemp");
        double tempDif = maxTemp - minTemp;
        
        Color hotColor = new Color(255,0,0,150);
        Color coldColor = new Color(255,0,0,0);
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                double t = (tempMap[x,y] - minTemp) / tempDif;
                Color pixCol = TerrainGenDemo.ColorLerp(coldColor, hotColor, t);
                img.SetPixel((uint)x, (uint)y, pixCol);
            }
        }
    }
    
    public static void MoistureImage(World world, Image img)
    {
        int[,] moistureMap = world.GetMap<int>("moisture");
        int width = world.Width;
        int height = world.Height;
        
        int moistureMin=0, moistureMax=0;
        for (int x=0; x<width; x++)
        {
            for (int y=0; y<height; y++)
            {
                if (moistureMap[x,y] > moistureMax)
                {
                    moistureMax = moistureMap[x,y];
                }
                if (moistureMap[x,y] < moistureMin)
                {
                    moistureMin = moistureMap[x,y];
                }
            }
        }
        int moistureDif = moistureMax-moistureMin;
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                int heightFromMin = moistureMap[x,y] - moistureMin;
                byte alpha = (byte)((double)heightFromMin/moistureDif * 255);
                img.SetPixel((uint)x, (uint)y, new Color(0,255,255,alpha));
            }
        }
    }
    
    public static void IslandImage(World world, Image img)
    {
        int[,] islandMap = world.GetMap<int>("islandid");
        
        byte tint = 160;
        Color[] islandColors = new Color[]
        {
            new Color(166,206,227,tint),
            new Color(31,120,180,tint),
            new Color(178,223,138,tint),
            new Color(51,160,44,tint),
            new Color(251,154,153,tint)
        };
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                int islandHere = islandMap[x,y];
                
                if (islandHere > 0)
                {
                    img.SetPixel((uint)x, (uint)y, islandColors[islandHere % islandColors.Length]);
                }
                else
                {
                    img.SetPixel((uint)x, (uint)y, new Color(255,255,255,0));
                }
            }
        }
    }
    
    public static void PressureImage(World world, Image img)
    {
        double[,] pressureMap = world.GetMap<double>("atmopressure");
        
        Color highColor = new Color(255,255,0,255);
        Color lowColor = new Color(0,255,0,255);
        int maxAlph = 240;
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                double p = pressureMap[x,y];
                Color col;
                
                if (p > 0)
                {
                    col = highColor;
                    col.A = (byte)(p * maxAlph);
                    if (x == 0 && y == 0) Console.WriteLine(""+col.A);
                }
                else
                {
                    col = lowColor;
                    col.A = (byte)(p * -1 * maxAlph);
                }
                
                img.SetPixel((uint)x, (uint)y, col);
            }
        }
    }
    
    public static void FitnessImage(World world, Image img)
    {
        VegetationFitness[,] fitnessMap = world.GetMap<VegetationFitness>("fitness");
        int[,] waterMap = world.GetMap<int>("water");
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                Color col;
                if (waterMap[x,y] != 0)
                {
                    col = new Color(50,50,200,255);
                }
                else
                {
                    Color vegColor = fitnessMap[x,y].GetPixelColor();
                    if (vegColor.A != 0)
                    {
                        col = vegColor;
                    }
                    else
                    {
                        col = new Color(200,200,50,255);
                    }
                }
                img.SetPixel((uint)x, (uint)y, col);
            }
        }
    }
    
    public static void HeightShadeImage(World world, Image img)
    {
        int[,] heightMap = world.GetMap<int>("height");
        Tuple<int,int> heightMinMax = TerrainGenDemo.ArrayBounds2D(heightMap);
        int heightMin = heightMinMax.Item1;
        int heightMax = heightMinMax.Item2;
        int heightRange = heightMax - heightMin;
        int heightMedian = (int)(heightMin + (heightRange * 0.5));
        int seaLevel = (int)world.GetProperty("sealevel");
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                Color col;
                int height = heightMap[x,y];
                int depth = heightMax - height;
                
                /*
                int slope = x < world.Width-1 ? heightMap[x+1,y] - height : 0;
                int slopeAlpha = Math.Min(Math.Abs(slope)*5, 255);
                byte slopeByte = (byte)slopeAlpha;
                if (slope > 0)
                {
                    col = new Color(255,255,255,slopeByte);
                }
                else
                {
                    col = new Color(0,0,0,slopeByte);
                }
                */
                
                double alphaFactor = 0.0;
                if (height > seaLevel)
                {
                    col = new Color(255,255,255,0);
                    alphaFactor = (double)(height - seaLevel) / (heightMax - seaLevel);
                }
                else
                {
                    col = new Color(0,0,0,0);
                    alphaFactor = (double)(seaLevel - height) / (seaLevel - heightMin);
                }
                double maxAlpha = 200.0;
                double rawAlpha = alphaFactor * maxAlpha;
                col.A = (byte)rawAlpha;
                
                img.SetPixel((uint)x, (uint)y, col);
            }
        }
    }
    
    public static void IsolineImage(World world, Image img)
    {
        int[,] heightMap = world.GetMap<int>("height");
        int lineSpacing = 100;
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                Color col = new Color(0,0,0,0);
                int multipleHere = heightMap[x,y] / lineSpacing;
                
                for (int xx=x-1; xx<x+2; xx++)
                {
                    for (int yy=y-1; yy<y+2; yy++)
                    {
                        if ((xx != 0 || yy != 0) && (xx >= 0 && xx < world.Width && yy >= 0 && yy < world.Height))
                        {
                            if (heightMap[xx,yy] / lineSpacing < multipleHere)
                            {
                                col = new Color(0,0,0,255);
                                break;
                            }
                        }
                    }
                }
                
                img.SetPixel((uint)x, (uint)y, col);
            }
        }
    }
}