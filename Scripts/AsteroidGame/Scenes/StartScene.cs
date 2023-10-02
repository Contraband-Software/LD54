namespace LD54.AsteroidGame.Scenes;

using Engine.Leviathan;
using Microsoft.Xna.Framework;
using Engine;
using GameObjects;
using Microsoft.Xna.Framework.Graphics;

public class StartScene : Scene
{
    private readonly float showTime = 0;
    private float timeShowed = 0;

    private ILeviathanEngineService? render;
    private ISceneControllerService? scene;

    public StartScene(float showTime, Game appCtx) : base("StartScene", appCtx)
    {
        this.showTime = showTime;
    }

    public override void OnLoad(GameObject? parentObject)
    {
        render = this.app.Services.GetService<ILeviathanEngineService>();
        scene = this.app.Services.GetService<ISceneControllerService>();

        GameObject titleUI = new TitleScreenSystem(this.contentManager.Load<SpriteFont>("Fonts/TitleFont"), this.app);
        parentObject.AddChild(titleUI);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        timeShowed += gameTime.TotalGameTime.Seconds;

        if (this.timeShowed > this.showTime)
        {
            this.scene.ChangeScene("GameScene");
        }
    }

    public override void OnUnload()
    {

    }
}
