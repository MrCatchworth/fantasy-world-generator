using SFML.Graphics;
using SFML.Window;

public class MapVisualisation : Drawable
{
    public delegate void UpdateImage(World w, Image i);
    
    private readonly Sprite MapSprite;
    private readonly Image img;
    private readonly World world;
    private readonly UpdateImage updater;
    public readonly Keyboard.Key ToggleKey;
    public bool Enabled;
    
    public MapVisualisation(World w, Keyboard.Key toggleKey, bool startEnabled, UpdateImage ui)
    {
        int width = w.Width;
        int height = w.Height;
        
        world = w;
        MapSprite = new Sprite(new Texture((uint)width, (uint)height));
        img = new Image((uint)width, (uint)height);
        updater = ui;
        ToggleKey = toggleKey;
        Enabled = startEnabled;
    }
    
    public void UpdateSprite()
    {
        updater(world, img);
        MapSprite.Texture.Update(img);
    }
    
    public void Draw(RenderTarget rt, RenderStates rs)
    {
        if (Enabled)
        {
            rt.Draw(MapSprite, rs);
        }
    }
}