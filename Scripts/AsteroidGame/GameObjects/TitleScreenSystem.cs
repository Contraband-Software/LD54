namespace LD54.AsteroidGame.GameObjects;

using Engine.Leviathan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class TitleScreenSystem : GameObject
{
    private ILeviathanEngineService? render;

    private SpriteFont titleFont;
    private SpriteFont subTitleFont;
    private Texture2D rock;

    private readonly int showTime = 0;
    private float timeShowed = 0;

    public TitleScreenSystem(int showTime, SpriteFont titleFont, SpriteFont subtitleFont, Texture2D rock, Game appCtx) : base("TitleScreenObject", appCtx)
    {
        this.titleFont = titleFont;
        this.subTitleFont = subtitleFont;
        this.showTime = showTime;
        this.rock = rock;
    }

    private LeviathanUIElement gameTitle;
    private LeviathanUIElement gameSubtitle1;
    private LeviathanUIElement gameSubtitle2;
    private LeviathanUIElement rockIcon;
    public override void OnLoad(GameObject? parentObject)
    {
        render = this.app.Services.GetService<ILeviathanEngineService>();

        Vector2 offset = new Vector2(0, -300);

        {
            Matrix fuckingTitlePosition = this.GetGlobalTransform();

            Vector2 titlePos = this.render.getWindowSize() / 2f;
            string titleText = "EVENT HORIZON";
            float titleScale = 1;
            fuckingTitlePosition.Translation += new Vector3(titlePos - titleFont.MeasureString(titleText) / 2 * titleScale + offset, 0);
            gameTitle = new LeviathanUIElement(this.app, fuckingTitlePosition, new Vector2(titleScale), titleText, this.titleFont, Color.White);
        }
        {
            Matrix fuckingTitlePosition = this.GetGlobalTransform();

            Vector2 subTitle1Pos = this.render.getWindowSize() / 2f + new Vector2(0, 100);
            string subTitle1Text = "By Contraband Studio, 2023, for Ludum Dare";
            fuckingTitlePosition.Translation += new Vector3(subTitle1Pos - subTitleFont.MeasureString(subTitle1Text) / 2 + offset, 0);
            gameSubtitle1 = new LeviathanUIElement(this.app, fuckingTitlePosition, new Vector2(1), subTitle1Text, this.subTitleFont, Color.White);
        }
        {
            Matrix fuckingTitlePosition = this.GetGlobalTransform();

            Vector2 subTitle1Pos = this.render.getWindowSize() / 2f + new Vector2(0, 200);
            string subTitle1Text = "Made with the GreenRock Engine";
            fuckingTitlePosition.Translation += new Vector3(subTitle1Pos - subTitleFont.MeasureString(subTitle1Text) / 2 + offset, 0);
            gameSubtitle2 = new LeviathanUIElement(this.app, fuckingTitlePosition, new Vector2(1), subTitle1Text, this.subTitleFont, Color.White);
        }
        {
            Matrix fuckingTitlePosition = this.GetGlobalTransform();

            Vector2 size = new Vector2(rock.Width, this.rock.Height);

            Vector2 subTitle1Pos = this.render.getWindowSize() / 2f + new Vector2(0, 400);
            fuckingTitlePosition.Translation += new Vector3(subTitle1Pos - size / 2 + offset, 0);
            rockIcon = new LeviathanUIElement(this.app, fuckingTitlePosition, size, rock);
        }
    }

    private bool showedTitle = false;
    private bool showedEngine = false;
    private bool showedStudio = false;
    private bool showedrock = false;
    public override void Update(GameTime gameTime)
    {
        float increment = (float)this.showTime / 5f;

        timeShowed = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000f;

        if (this.timeShowed > increment && !showedTitle)
        {
            showedTitle = true;
            this.render.addUISprite(gameTitle);
        } else if (this.timeShowed > increment * 2 && !showedStudio)
        {
            showedStudio = true;
            this.render.addUISprite(gameSubtitle1);

        } else if (this.timeShowed > increment * 3 && !showedEngine)
        {
            showedEngine = true;
            this.render.addUISprite(gameSubtitle2);
            this.render.addUISprite(this.rockIcon);
        } else if (this.timeShowed > increment * 4)
        {
            this.app.Services.GetService<ISceneControllerService>().ChangeScene("GameScene");
        }

        base.Update(gameTime);
    }

    public override void OnUnload()
    {
        this.render.removeUISprite(gameTitle);
        this.render.removeUISprite(gameSubtitle1);
        this.render.removeUISprite(gameSubtitle2);
        this.render.removeUISprite(this.rockIcon);

        base.OnUnload();
    }
}
