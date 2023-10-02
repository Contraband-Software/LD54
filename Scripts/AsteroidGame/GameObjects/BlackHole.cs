namespace LD54.AsteroidGame.GameObjects;

using Engine.Collision;
using Engine.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class BlackHole : GameObject
{
    private Texture2D? texture;

    private CircleColliderComponent collider;

    public float Mass { get; set; } = 1000;

    public BlackHole(float mass, Texture2D texture, string name, Game appCtx) : base(name, appCtx)
    {
        this.Mass = mass;

        this.texture = texture;
    }

    public override void OnLoad(GameObject? parentObject)
    {
        float scaleDivider = 1f;

        //SpriteRendererComponent src = new SpriteRendererComponent("Sprite1", this.app);
        Vector3 textureSize = new Vector3((float)this.texture.Width / scaleDivider, (float)this.texture.Height / scaleDivider, 0f);
        //Matrix transform = this.GetGlobalTransform();
        //transform.Translation -= textureSize / 2f;
        //src.LoadSpriteData(
        //    transform,
        //    new Vector2(textureSize.X, textureSize.Y),
        //    this.texture,
        //    null);
        //src.Offset = textureSize / -2f;
        //this.AddComponent(src);

        RigidBodyComponent rb = new RigidBodyComponent("BlackHoleRB", this.app);
        rb.Mass = this.Mass;
        rb.Static = true;
        this.AddComponent(rb);

        collider = new CircleColliderComponent(textureSize.X / 2, textureSize / -2f, "BlackHoleCollider", this.app);
        this.collider.isTrigger = true;
        this.collider.TriggerEvent += this.EatIt;
        this.collider.DebugMode = true;
        this.AddComponent(collider);

        BlackHoleComponent bh = new BlackHoleComponent("BlackHoleDrawing", this.app);
        this.AddComponent(bh);

    }

    public int DestroyedObjects = 0;

    private void EatIt(ColliderComponent other)
    {
        //PrintLn("");
        //PrintLn("Eaten Object: " + other.GetGameObject().GetName());
        DestroyedObjects++;
        this.app.Services.GetService<ISceneControllerService>().DestroyObject(other.GetGameObject());
        //PrintLn("Blackholed Objects: " + this.DestroyedObjects);
    }
}
