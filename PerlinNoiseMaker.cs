using System;

public class PerlinNoiseMaker
{
    private static int hash32shift(int key)
    {
      key = ~key + (key << 15); // key = (key << 15) - key - 1;
      key = key ^ (key >> 12);
      key = key + (key << 2);
      key = key ^ (key >> 4);
      key = key * 2057; // key = (key + (key << 3)) + (key << 11);
      key = key ^ (key >> 16);
      return key;
    }
    
    private static double MyRandom(int x, int y, int seed)
    {
        //return hash32shift(seed+hash32shift(x+hash32shift(y))) / 1000.0;
        //return hash32shift(seed+hash32shift(x+hash32shift(y))) % 10000;
        return hash32shift(seed+hash32shift(x+hash32shift(y))) / (double)int.MaxValue;
    }
    
    private static int Interpolate(int from, int to, double x)
    {
        double ft = x * Math.PI;
        double f = (1.0 - Math.Cos(ft)) * 0.5;
        
        return (int)(from*(1.0-f) + to*f);
    }
    
    public static void ApplyNoise (World w, int seed)
    {
        //wavelength, amplitude
        Tuple<int,int>[] octaveParams = new Tuple<int,int>[]
        {
            Tuple.Create(160,500),
            Tuple.Create(60,200),
            Tuple.Create(50,100),
            Tuple.Create(15,50),
            Tuple.Create(7,25),
            Tuple.Create(3,12)
        };
        
        int[,] map = w.GetOrAddMap<int>("height");
        
        int max = 0;
        int min = 0;
        
        //for every pixel on the map
        for (int x=0; x<w.Width; x++)
        {
            for (int y=0; y<w.Height; y++)
            {
                //value to add up the octaves before applying to map
                int total = 0;
                
                //for every octave to apply to the pixel
                for (int octave=0; octave<octaveParams.Length; octave++)
                { 
                    int wavelength = octaveParams[octave].Item1;
                    int amplitude = octaveParams[octave].Item2;
                    
                    int leftX = (x/wavelength)*wavelength;
                    int rightX = leftX+wavelength;
                    
                    int topY = (y/wavelength)*wavelength;
                    int botY = topY+wavelength;
                    
                    int xFractional = x-leftX;
                    int yFractional = y-topY;
                    
                    int topLeft = (int)(MyRandom(leftX, topY, seed) * amplitude);
                    int topRight = (int)(MyRandom(rightX, topY, seed) * amplitude);
                    int botLeft = (int)(MyRandom(leftX, botY, seed) * amplitude);
                    int botRight = (int)(MyRandom(rightX, botY, seed) * amplitude);
                    
                    int topInter = Interpolate(topLeft, topRight, (double)xFractional/wavelength);
                    int botInter = Interpolate(botLeft, botRight, (double)xFractional/wavelength);
                    
                    total += Interpolate(topInter, botInter, (double)yFractional/wavelength);
                }
                
                map[x,y] = total;
            }
        }
        
        //make a bias for the edge of the map to be lower, tending to form continents
        
        int xCentre = w.Width/2;
        int yCentre = w.Height/2;
        for (int x=0; x<w.Width; x++)
        {
            for (int y=0; y<w.Height; y++)
            {
                int xDif = xCentre-x;
                int yDif = yCentre-y;
                double distFromCentre = Math.Sqrt((xDif*xDif)+(yDif*yDif));
                map[x,y] -= (int)(distFromCentre * 0.9);
                max = Math.Max(map[x,y], max);
                min = Math.Min(map[x,y], min);
            }
        }
        
        w.SetProperty("minheight", min);
        w.SetProperty("maxheight", max);
    }
}