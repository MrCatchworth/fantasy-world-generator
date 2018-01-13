using System;
using System.Collections.Generic;
using System.Linq;

public class World
{
    public readonly int Width;
    public readonly int Height;
    
    private Dictionary<string, object> _maps;
    private Dictionary<string, double> _properties;
    
    public World(int width, int height)
    {
        Width = width;
        Height = height;
        
        _maps = new Dictionary<string, object>();
        _properties = new Dictionary<string, double>();
    }
    
    public T[,] AddMap<T>(string name)
    {
        T[,] newMap = new T[Width, Height];
        _maps.Add(name, newMap);
        return newMap;
    }
    
    public object GetMapRawCell(string name, int x, int y)
    {
        return ((System.Array)_maps[name]).GetValue(x, y);
    }
    
    public T[,] GetMap<T>(string name)
    {
        return (T[,])_maps[name];
    }
    
    public bool HasMap(string name)
    {
        return _maps.ContainsKey(name);
    }
    
    public T[,] GetOrAddMap<T>(string name)
    {
        if (HasMap(name))
        {
            return GetMap<T>(name);
        }
        else
        {
            return AddMap<T>(name);
        }
    }
    
    public void SetProperty(string name, double value)
    {
        _properties[name] = value;
    }
    
    public double GetProperty(string name)
    {
        return _properties[name];
    }
    
    public string[] MapNames {
        get
        {
            return _maps.Keys.ToArray();
        }
    }
}