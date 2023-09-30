namespace LD54.Engine.Leviathan;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class LevithanSprite
{
    private string colorPath;
    private string normalPath;
    public Texture2D color;
    public Texture2D normal;
    private Matrix transform;
    public Point size;
    public bool useNormal = false;
    private Game game;
    public LevithanSprite(Game game, Matrix transform,Point size, string colorPath, string normalPath = ""){
        this.colorPath = colorPath;
        this.normalPath = normalPath;
        if(normalPath != "")
        {
            useNormal = true;
        }
        this.transform = transform;
        this.game = game;
        this.size = size;
    }
    protected void LoadContent()
    {
        color = game.Content.Load<Texture2D>(colorPath);
        if(useNormal) {
            normal = game.Content.Load<Texture2D>(normalPath);
        }
    }

    public void setTransform(Matrix transform)
    {
        this.transform = transform;
    }
    public Vector3 getPosition()
    {
        return this.transform.Translation;
    }
    public Vector2 getPositionXY()
    {
        return new Vector2(this.transform.Translation.X, this.transform.Translation.Y);
    }
    public void setPosition(Vector3 position) {
        this.transform.Translation = position;
    }
    public void translatePosition(Vector3 translation)
    {
        this.transform.Translation += translation;
    }
}