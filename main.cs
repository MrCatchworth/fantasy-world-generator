using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using System.Diagnostics;

public class TerrainGenDemo
{
    public static Tuple<T,T> ArrayBounds2D<T>(T[,] array) where T:IComparable<T>
    {
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        
        if (width <= 0 || height <= 0)
        {
            throw new Exception("Can't get the bounds of an empty array");
        }
        
        T min = array[0,0];
        T max = array[0,0];
        
        for (int x=0; x<width; x++)
        {
            for (int y=0; y<height; y++)
            {
                if (array[x,y].CompareTo(max) > 0)
                    max = array[x,y];
                if (array[x,y].CompareTo(min) < 0)
                    min = array[x,y];
            }
        }
        
        return new Tuple<T,T>(min, max);
    }
    
    public static double Lerp(double a, double b, double t)
    {
        /*
        double min, max;
        if (a < b)
        {
            min = a;
            max = b;
        }
        else
        {
            max = a;
            min = b;
        }
        */
        double min = a;
        double max = b;
        
        return min + (max-min)*t;
    }
    
    public static Color ColorLerp(Color c1, Color c2, double t)
    {
        byte r = (byte)Lerp(c1.R, c2.R, t);
        byte g = (byte)Lerp(c1.G, c2.G, t);
        byte b = (byte)Lerp(c1.B, c2.B, t);
        byte a = (byte)Lerp(c1.A, c2.A, t);
        
        return new Color(r,g,b,a);
    }
    
    public static void OnClose(object sender, EventArgs e)
    {
        RenderWindow w = (RenderWindow)sender;
        w.Close();
    }
    
    public static void OnKeyPress(object sender, KeyEventArgs e)
    {
        if (e.Code == Keyboard.Key.R)
        {
            Console.WriteLine("Generating new map");
            //GenerateWorldAndSprites();
            RefreshWorldNew();
        }
        
        foreach (MapVisualisation mv in MapLayers)
        {
            if (e.Code == mv.ToggleKey)
            {
                mv.Enabled = !mv.Enabled;
            }
        }
    }
    
    
    
    private static double GetVegetationFitness(int moisture, double temperature, int bestTemperature)
    {
        Tuple<int,int> moistureMinMax = ArrayBounds2D<int>(world.GetMap<int>("moisture"));
        int moistureMin = moistureMinMax.Item1;
        int moistureMax = moistureMinMax.Item2;
        
        double maxFitness = 100.0;
        
        //reduction of fitness caused by 1-kelvin difference in temperature
        //let's say plants can't stand below -10C or above 40C average, where 15C is best, so 25C should be 100% reduction
        double tempFactor = 4.0;
        double perfectTemp = 288.0;
        maxFitness -= Math.Abs(perfectTemp - temperature) * tempFactor;
        maxFitness = Math.Max(maxFitness, 0.0);
        
        //more moisture is better right
        //moisture doesn't correspond to a real world quantity yet so it's arbitrary
        double neededMoisture = 0.4 * (moistureMax - moistureMin);
        double goodMoistureRange = moistureMax - neededMoisture;
        maxFitness *= (moisture - neededMoisture) / goodMoistureRange;
        maxFitness = Math.Max(maxFitness, 0.0);
        
        return maxFitness;
    }
    
    //represent a world's height and water map with nice colours
    private static Image GetWorldImage()
    {
        int[,] map = world.GetMap<int>("height");
        int[,] waterMap = world.GetMap<int>("water");
        int[,] moistureMap = world.GetMap<int>("moisture");
        double[,] tempMap = world.GetMap<double>("temperature");
        
        Image returnImage = new Image((uint)map.GetLength(0), (uint)map.GetLength(1));
        
        int min = (int)world.GetProperty("minheight");
        int max = (int)world.GetProperty("maxheight");
        int seaLevel = (int)world.GetProperty("sealevel");
        int maxTemp = (int)world.GetProperty("maxtemp");
        
        int dif = max-min;
        
        Color poorVegColor = new Color(188,167,122,255);
        Color richVegColor = new Color(77,158,58);
        double maxVegFitness = 500;
        
        for (int x=0; x<world.Width; x++)
        {
            for (int y=0; y<world.Height; y++)
            {
                int effectiveHeight = Math.Max(map[x,y], seaLevel);
                
                int heightFromMin = effectiveHeight - min;
                byte brightness = (byte)((double)heightFromMin/dif * 255);
                
                double vegFitness = GetVegetationFitness(moistureMap[x,y], tempMap[x,y], maxTemp/2);
                
                Color pixCol = new Color(0,0,0,255);
                
                if (waterMap[x,y] > 0)
                {
                    byte a = (byte)Math.Max(brightness, (byte)80);
                    pixCol = new Color(0, a, a, 255);
                }
                //else if (tempMap[x,y] < 70 && moistureMap[x,y] > 100)
                else if (vegFitness < 200 && tempMap[x,y] < 100 && moistureMap[x,y] > 100)
                {
                    pixCol = ColorLerp(new Color(128,128,128,255), Color.White, (double)heightFromMin/dif);
                }
                else
                {
                    //default: vegetation tile
                    vegFitness = Math.Min(Math.Max(vegFitness, 0), maxVegFitness);
                    pixCol = ColorLerp(poorVegColor, richVegColor, vegFitness/maxVegFitness);
                    
                    //pixCol = ColorLerp(pixCol, Color.White, (double)heightFromSea / seaDif * 0.1);
                }
                returnImage.SetPixel((uint)x, (uint)y, pixCol);
            }
        }
        
        return returnImage;
    }
    
    private static void CreateVisualisations()
    {
        MapLayers.Add(new MapVisualisation(
            world,
            Keyboard.Key.F,
            true,
            Updaters.FitnessImage
        ));
        MapLayers.Add(new MapVisualisation(
            world,
            Keyboard.Key.F,
            true,
            Updaters.HeightShadeImage
        ));
        MapLayers.Add(new MapVisualisation(
            world,
            Keyboard.Key.B,
            true,
            Updaters.IsolineImage
        ));
        
        MapLayers.Add(new MapVisualisation(
            world,
            Keyboard.Key.T,
            false,
            Updaters.TempImage
        ));
        
        MapLayers.Add(new MapVisualisation(
            world,
            Keyboard.Key.M,
            false,
            Updaters.MoistureImage
        ));
        
        MapLayers.Add(new MapVisualisation(
            world,
            Keyboard.Key.I,
            false,
            Updaters.IslandImage
        ));
        
        MapLayers.Add(new MapVisualisation(
            world,
            Keyboard.Key.P,
            false,
            Updaters.PressureImage
        ));
    }
    
    private static void RefreshWorldNew()
    {
        gen.Reset();
        gen.Run(seedRandom.Next());
        
        foreach (MapVisualisation mv in MapLayers)
        {
            mv.UpdateSprite();
        }
    }
    
    private static List<MapVisualisation> MapLayers = new List<MapVisualisation>();
    private static Random seedRandom = new Random();
    private static RenderWindow gameWindow;
    private static World world;
    private static WorldGenerator gen;
    
    public static void Main(string[] args)
    {
        gameWindow = new RenderWindow(new VideoMode(1024,700), "Terrain Generator");
        gameWindow.Closed += new EventHandler(OnClose);
        gameWindow.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPress);
        
        world = new World((int)gameWindow.Size.X, (int)gameWindow.Size.Y);
        
        CreateVisualisations();
        
        gen = new WorldGenerator(world);
        gen.AddStep(new GeneratorStep(
            gen,
            "perlin",
            new string[0],
            PerlinNoiseMaker.ApplyNoise
        ));
        gen.AddStep(new GeneratorStep(
            gen,
            "planetprops",
            new string[0],
            GenStepLibrary.ComputePlanetProperties
        ));
        gen.AddStep(new GeneratorStep(
            gen,
            "seawater",
            new string[] {"perlin"},
            GenStepLibrary.ComputeSeaWater
        ));
        gen.AddStep(new GeneratorStep(
            gen,
            "rainfall",
            new string[] {"perlin", "seawater", "pressure"},
            GenStepLibrary.ComputeRainfall
        ));
        gen.AddStep(new GeneratorStep(
            gen,
            "temperature",
            new string[] {"perlin", "seawater", "planetprops"},
            GenStepLibrary.ComputeTemperature
        ));
        gen.AddStep(new GeneratorStep(
            gen,
            "findislands",
            new string[] {"seawater"},
            GenStepLibrary.FindIslands
        ));
        gen.AddStep(new GeneratorStep(
            gen,
            "pressure",
            new string[] {"planetprops"},
            GenStepLibrary.ComputePressure
        ));
        gen.AddStep(new GeneratorStep(
            gen,
            "fitness",
            new string[] {"rainfall", "temperature", "seawater"},
            GenStepLibrary.ComputeFitness
        ));
        
        RefreshWorldNew();
        
        Text cellValueText = new Text();
        cellValueText.CharacterSize = 15;
        cellValueText.Color = new Color(255,255,255,255);
        cellValueText.Font = new Font("arial.ttf");
        string[] mapNames = world.MapNames;
        string[] cellValueLines = new string[mapNames.Length];
        
        while(gameWindow.IsOpen)
        {
            gameWindow.DispatchEvents();
            gameWindow.Clear();
            
            foreach (MapVisualisation mv in MapLayers)
            {
                if (mv.Enabled)
                {
                    mv.Draw(gameWindow, RenderStates.Default);
                }
            }
            
            Vector2i mousePos = Mouse.GetPosition(gameWindow);
            if (mousePos.X >= 0 && mousePos.X < world.Width && mousePos.Y >= 0 && mousePos.Y < world.Height)
            {
                for (int i=0; i<mapNames.Length; i++)
                {
                    object thisCell = world.GetMapRawCell(mapNames[i], mousePos.X, mousePos.Y);
                    
                    string valueString;
                    if (thisCell.GetType().IsPrimitive)
                    {
                        valueString = String.Format("{0:F2}", thisCell);
                    }
                    else
                    {
                        valueString = thisCell.ToString();
                    }
                    cellValueLines[i] = mapNames[i] + "~ " + valueString;
                }
                cellValueText.DisplayedString = string.Join("\n", cellValueLines);
                
                cellValueText.Color = new Color(0,0,0,255);
                cellValueText.Position = new Vector2f(mousePos.X+10, mousePos.Y+1);
                cellValueText.Draw(gameWindow, RenderStates.Default);
                
                cellValueText.Color = new Color(255,255,255,255);
                cellValueText.Position -= new Vector2f(1,1);
                cellValueText.Draw(gameWindow, RenderStates.Default);
            }
            
            gameWindow.Display();
        }
    }
}