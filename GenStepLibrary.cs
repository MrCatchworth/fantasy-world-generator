using System;
using System.Collections.Generic;
using SFML.System;

public class GenStepLibrary
{
    public static void ComputeSeaWater(World world, int seed)
    {
        
        //return a sea level such that about a certain percentage will be sea
        List<int> heights = new List<int>();
        int[,] heightMap = world.GetMap<int>("height");
        int[,] waterMap = world.GetOrAddMap<int>("water");
        
        //71% is same as the earth
        double seaFraction = 0.71;
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                heights.Add(heightMap[x,y]);
            }
        }
        
        heights.Sort();
        int seaIndex = (int)(heights.Count*seaFraction);
        
        int seaLevel = heights[seaIndex];
        world.SetProperty("sealevel", seaLevel);
        
        //for now only the sea is water
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                if (heightMap[x,y] <= seaLevel)
                {
                    waterMap[x,y] = 1;
                }
                else
                {
                    waterMap[x,y] = 0;
                }
            }
        }
    }
    
    public static void ComputeRainfall(World world, int seed)
    {
        int[,] heightMap = world.GetMap<int>("height");
        int[,] waterMap = world.GetMap<int>("water");
        double[,] pressureMap = world.GetMap<double>("atmopressure");
        int[,] moistureMap = world.GetOrAddMap<int>("moisture");
        int maxHeight = (int)world.GetProperty("maxheight");
        
        //what direction the wind blows in
        int windX = 1;
        int windY = 0;
        
        int windXDir = 1;
        int windYDir = 0;
        
        int width = world.Width;
        int height = world.Height;
        
        int seaLevel = (int)world.GetProperty("sealevel");
        
        //get a bonus to moisture when we're near a body of water
        for (int x=0; x<width; x++)
        {
            for (int y=0; y<height; y++)
            {
                moistureMap[x,y] = 0;
                
                //scan adjacent cells for water
                for (int xScan=-1; xScan<=1; xScan++)
                {
                    for (int yScan=-1; yScan<=1; yScan++)
                    {
                        if (x+xScan < 0 || x+xScan >= width || y+yScan < 0 || y+yScan >= height)
                        {
                            continue;
                        }
                        if (waterMap[x+xScan, y+yScan] > 0)
                        {
                            moistureMap[x,y] += 40;
                        }
                    }
                }
                
                //look in reverse wind direction until a hill is found, decrease moisture accordingly
                
                //the very base value is from google for global annual precipitation
                int baseMoisture = 1123;
                double pressureFactor = 4.0;
                
                double moistureMultiplier = 1.0;
                if (pressureMap[x,y] < 0)
                {
                    //low-pressure zones recieve a multiplier of the rainfall
                    moistureMultiplier = 1 + (pressureMap[x,y] * (pressureFactor-1) * -1);
                }
                if (pressureMap[x,y] > 0)
                {
                    //high-pressure zones receive a fraction of the rainfall
                    moistureMultiplier = 1 / (1 + pressureMap[x,y] * (pressureFactor-1));
                }
                
                baseMoisture = (int)(baseMoisture * moistureMultiplier);
                
                double currentMoisture = baseMoisture;
                
                int raindropX = x;
                int raindropY = y;
                int raindropZ = heightMap[x,y] + 1;
                bool raindropDone = false;
                
                while (!raindropDone)
                {
                    for (int i=0; i<Math.Abs(windX); i++)
                    {
                        //if the raindrop went out of bounds, shit i dunno
                        if (raindropX < 0 || raindropX >= width || raindropY < 0 || raindropY >= height)
                        {
                            moistureMap[x,y] += (int)currentMoisture;
                            raindropDone = true;
                            break;
                        }
                        
                        //if we reached the sky we can lock in our bonus
                        if (raindropZ > maxHeight)
                        {
                            moistureMap[x,y] += (int)currentMoisture;
                            raindropDone = true;
                            break;
                        }
                        
                        int effectiveHeight = Math.Max(heightMap[raindropX,raindropY], seaLevel);
                        //if the raindrop is having to penetrate the ground to get here, make it weaker
                        if (effectiveHeight >= raindropZ)
                        {
                            //just splash off the top of water that's fine
                            if (waterMap[raindropX,raindropY] == 1)
                            {
                                raindropDone = true;
                                break;
                            }
                            int difference = effectiveHeight - raindropZ + 1;
                            currentMoisture -= difference * 0.2;
                            
                            //if the raindrop was stopped completely, no reason to continue
                            if (currentMoisture <= 0)
                            {
                                raindropDone = true;
                                break;
                            }
                        }
                        
                        //...and keep going
                        raindropX -= windXDir;
                    }
                    raindropY -= windYDir;
                    raindropZ++;
                }
            }
        }
    }
    
    public static void ComputePlanetProperties(World world, int seed)
    {
        Random propGen = new Random(seed);
        
        double planetHeightMin = 1.4;
        double planetHeightMax = 2.5;
        
        int planetHeight = (int)((propGen.NextDouble() * (planetHeightMax-planetHeightMin) + planetHeightMin) * world.Height);
        world.SetProperty("planetheight", planetHeight);
        
        double equatorMin = world.Height - (0.5*planetHeight);
        double equatorMax = 0.5*planetHeight;
        
        int equator = (int)(propGen.NextDouble() * (equatorMax-equatorMin) + equatorMin);
        world.SetProperty("equator", equator);
    }
    
    public static void ComputeTemperature(World world, int seed)
    {
        double[,] tempMap = world.GetOrAddMap<double>("temperature");
        int[,] heightMap = world.GetMap<int>("height");
        int seaLevel = (int)world.GetProperty("sealevel");
        int equator = (int)world.GetProperty("equator");
        int planetHeight = (int)world.GetProperty("planetheight");
        
        //from roughly the hottest and coldest annual temperature, in kelvin
        //-30C to 30C
        double equatorTemp = 303;
        double poleTemp = 263;
        double tempDistrib = planetHeight/2.0;
        
        world.SetProperty("maxtemp", equatorTemp);
        world.SetProperty("mintemp", poleTemp);
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                double newTemp = TerrainGenDemo.Lerp(equatorTemp, poleTemp, Math.Abs(equator-y) / tempDistrib);
                newTemp -= Math.Max(heightMap[x,y] - seaLevel, 0) * 0.07;
                newTemp = Math.Max(newTemp, 1);
                tempMap[x,y] = newTemp;
            }
        }
    }
    
    public static void FindIslands(World world, int seed)
    {
        int[,] islands = world.GetOrAddMap<int>("islandid");
        int[,] water = world.GetMap<int>("water");
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                islands[x,y] = 0;
            }
        }
        
        int nextIslandId = 1;
        int areaThreshold = 100;
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                if (water[x,y] == 0 && islands[x,y] == 0)
                {
                    bool realIslandFound = FindIsland(x, y, world.Width, world.Height, islands, water, nextIslandId, areaThreshold);
                    if (realIslandFound)
                    {
                        nextIslandId++;
                    }
                }
            }
        }
    }
    
    private static bool FindIsland(int x, int y, int w, int h, int[,] islandMap, int[,] waterMap, int nextIslandId, int areaThreshold)
    {
        Queue<Vector2i> nextCells = new Queue<Vector2i>();
        nextCells.Enqueue(new Vector2i(x, y));
        
        int totalArea = 0;
        
        Console.WriteLine("Exploring island {0}", nextIslandId);
        
        int eastBound, westBound;
        //another prefix traversal, with optimisation to reduce queue utilisation
        while (nextCells.Count != 0)
        {
            Vector2i curCell = nextCells.Dequeue();
            //ignore stuff we already covered
            if (islandMap[curCell.X, curCell.Y] != 0)
            {
                continue;
            }
            islandMap[curCell.X, curCell.Y] = nextIslandId;
            totalArea++;
            
            eastBound = westBound = curCell.X;
            
            while (westBound > 0 && islandMap[westBound-1, curCell.Y] == 0 && waterMap[westBound-1, curCell.Y] == 0)
            {
                westBound--;
            }
            while (eastBound < w-1 && islandMap[eastBound+1, curCell.Y] == 0 && waterMap[eastBound+1, curCell.Y] == 0)
            {
                eastBound++;
            }
            
            for (int scanX=westBound; scanX <= eastBound; scanX++)
            {
                islandMap[scanX, curCell.Y] = nextIslandId;
                if (scanX != curCell.X)
                {
                    totalArea++;
                }
                
                if (curCell.Y > 0 && islandMap[scanX, curCell.Y-1] == 0 && waterMap[scanX, curCell.Y-1] == 0)
                {
                    nextCells.Enqueue(new Vector2i(scanX, curCell.Y-1));
                }
                if (curCell.Y < h-1 && islandMap[scanX, curCell.Y+1] == 0 && waterMap[scanX, curCell.Y+1] == 0)
                {
                    nextCells.Enqueue(new Vector2i(scanX, curCell.Y+1));
                }
            }
        }
        return true;
    }
    
    public static void ComputePressure(World world, int seed)
    {
        int equator = (int)world.GetProperty("equator");
        int planetHeight = (int)world.GetProperty("planetheight");
        double[,] pressureMap = world.GetOrAddMap<double>("atmopressure");
        
        //per hemisphere
        int numCells = 3;
        
        int cellHeight = planetHeight / numCells / 2;
        
        double cellFalloff = 0.65;
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                double yFromEquator = Math.Abs(y-equator);
                pressureMap[x,y] = Math.Cos((yFromEquator/cellHeight)*Math.PI) * -1.0;
                double falloffAmt = yFromEquator / (planetHeight / 0.5);
                pressureMap[x,y] *= (1 - falloffAmt);
                //Console.WriteLine(pressureMap[x,y]);
            }
        }
    }
    
    public static void ComputeFitness(World world, int seed)
    {
        double[,] tempMap = world.GetMap<double>("temperature");
        int[,] moistureMap = world.GetMap<int>("moisture");
        VegetationFitness[,] fitnessMap = world.GetOrAddMap<VegetationFitness>("fitness");
        
        Tuple<int,int> moistureMinMax = TerrainGenDemo.ArrayBounds2D<int>(moistureMap);
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                double temperature = tempMap[x,y];
                double moisture = moistureMap[x,y];
                
                fitnessMap[x,y].Generate(temperature, moisture);
            }
        }
    }
}