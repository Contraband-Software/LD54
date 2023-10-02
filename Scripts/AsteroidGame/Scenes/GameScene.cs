global using LD54.Engine;

namespace LD54.AsteroidGame.Scenes;

using System;
using System.Collections.Generic;
using Engine.Dev;
using Engine.Leviathan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AsteroidGame.GameObjects;
using Engine.Components;
using AsteroidGame.GameObjects;
using Engine;
using LD54.AsteroidGame.GameObjects;
using LD54.Scripts.AsteroidGame.GameObjects;

public class GameScene : Scene
{
    #region PARAMS
    public const float FORCE_LAW = 2.4f;
    public const float GRAVITATIONAL_CONSTANT = 43f;

    public const float BLACK_HOLE_MASS = 100;   // no need to edit, GRAVITATIONAL_CONSTANT is already directly proportional (Satellites are massless)
    public const int SATELLITES = 120;
    public const float SPEED_MULT = 15f;

    public const float MAP_SIZE = 2f;

    public const float MAX_ASTEROID_SPAWN_INVERVAL = 0.01f;
    #endregion

    private int sunLight = -1;

    private Texture2D testObjectTexture;

    List<Texture2D> asteroidTextures;
    private Texture2D asteroidTexture_broken;
    private float countdownToAsteroidSpawn = MAX_ASTEROID_SPAWN_INVERVAL;

    Vector2 windowSize;
    NewtonianSystemObject newtonianSystem;
    GameObject blackHole;

    public GameScene(Game appCtx) : base("GameScene", appCtx)
    {
    }

    public void SpawnAccretionDisk(GameObject parent, Vector2 boundsOffset, Vector2 boundsDimensions, Vector2 blackHole)
    {
        Random rnd = new Random();

        for (int i = 0; i < GameScene.SATELLITES; i++)
        {
            Vector2 startPosition = new Vector2(
                rnd.Next((int)boundsOffset.X, (int)boundsDimensions.X),
                rnd.Next((int)boundsOffset.Y, (int)boundsDimensions.Y));

            Vector2 separation = startPosition - blackHole;
            Vector2 perpendicular = separation.PerpendicularClockwise();
            perpendicular.Normalize();

            // PrintLn(perpendicular.ToString());

            GameObject newSat = new SatelliteObject(
                0,
                new Vector3(perpendicular.X, perpendicular.Y, 0.76f) * GameScene.SPEED_MULT * (1 / MathF.Sqrt(separation.Magnitude())),
                testObjectTexture,
                "satelliteObject_" + i,
                this.app
            );

            parent.AddChild(newSat);
            newSat.SetLocalPosition(startPosition);
        }
    }

    public void SpawnAsteroidDisk(GameObject parent, Vector2 boundsOffset, Vector2 boundsDimensions, Vector2 blackHole)
    {
        Random rnd = new Random();

        for (int i = 0; i < GameScene.SATELLITES; i++)
        {
            Vector2 startPosition = new Vector2(
                rnd.Next((int)boundsOffset.X, (int)boundsDimensions.X),
                rnd.Next((int)boundsOffset.Y, (int)boundsDimensions.Y));

            Vector2 separation = startPosition - blackHole;
            Vector2 perpendicular = separation.PerpendicularClockwise();
            perpendicular.Normalize();

            // PrintLn(perpendicular.ToString());

            GameObject newAst = new Asteroid(
                0,
                new Vector3(
                    perpendicular.X,
                    perpendicular.Y, 0.76f) * GameScene.SPEED_MULT * (1 / MathF.Sqrt(separation.Magnitude())),
                asteroidTextures[rnd.Next(0, 3)],
                asteroidTexture_broken,
                "asteroidObject_" + i,
                this.app
            );

            parent.AddChild(newAst);
            newAst.SetLocalPosition(startPosition);
        }
    }

    public void SpawnAsteroidsTowardsHole(
        GameTime gameTime,
        GameObject parent,
        Vector2 boundsOffset,
        Vector2 boundsDimensions,
        Vector2 blackHole)
    {
        countdownToAsteroidSpawn -= (gameTime.ElapsedGameTime.Milliseconds / 1000f);
        if(countdownToAsteroidSpawn < 0)
        {
            // PrintLn("SPAWNING ASTEROID");
            //reset countdown timer
            Random rnd = new Random();
            countdownToAsteroidSpawn = 1f + (rnd.NextSingle() * (MAX_ASTEROID_SPAWN_INVERVAL - 1f));

            //spawn asteroid heading towards black hole
            //choose random position
            // Generate a random angle in radians
            float angleInRadians = rnd.NextSingle() * 2 * MathF.PI;
            float distAway = 2000f;
            float ranX = distAway * MathF.Cos(angleInRadians);
            float ranY = distAway * MathF.Sin(angleInRadians);

            Vector2 startPosition = new Vector2(ranX, ranY);

            //apply random offset to blackhole position
            float offsetX = rnd.NextSingle() - 0.5f;
            float offsetY = rnd.NextSingle() - 0.5f;
            Vector2 targetPos = blackHole + new Vector2(offsetX, offsetY);

            //find direction from start pos to target pos
            Vector2 direction = (targetPos - startPosition);
            direction.Normalize();

            //make random velocity
            float speed = 1f + (rnd.NextSingle() * 3f);
            Vector2 startVelocity = direction * speed;

            //spawn asteroid
            GameObject newAst = new Asteroid(
                0,
                new Vector3(startVelocity, 0),
                asteroidTextures[rnd.Next(0, 3)],
                asteroidTexture_broken,
                "shooting_asteroidObject_",
                this.app
            );

            parent.AddChild(newAst);
            newAst.SetLocalPosition(startPosition);
        }
    }


    public override void OnLoad(GameObject? parentObject)
    {
        windowSize = this.app.Services.GetService<ILeviathanEngineService>().getWindowSize();
        PrintLn("Screen resolution: " + windowSize);

        // simple scene-wide illumination
        sunLight = this.app.Services.GetService<ILeviathanEngineService>().AddLight(new Vector2(200, 200), new Vector3(10000000, 10000000, 10000000));

        // sim set up
        newtonianSystem = new NewtonianSystemObject(
            GameScene.GRAVITATIONAL_CONSTANT,
            "GravitySimulationObject",
            this.app);
        parentObject.AddChild(newtonianSystem);
        newtonianSystem.SetLocalPosition(new Vector2(150, 150));

        testObjectTexture = this.contentManager.Load<Texture2D>("Sprites/circle");
        Texture2D asteroidTexture1 = this.contentManager.Load<Texture2D>("Sprites/asteroid_1");
        Texture2D asteroidTexture2 = this.contentManager.Load<Texture2D>("Sprites/asteroid_2");
        Texture2D asteroidTexture3 = this.contentManager.Load<Texture2D>("Sprites/asteroid_3");
        asteroidTexture_broken = this.contentManager.Load<Texture2D>("Sprites/asteroid_broken");

        asteroidTextures = new List<Texture2D>() { asteroidTexture1, asteroidTexture2, asteroidTexture3 };

        // player controller

        //DebugPlayer playerd = new(testObjectTexture, "DebugPlayerController", this.app);
        //newtonianSystem.AddChild(playerd);


        // black hole
        Vector2 blackHolePosition = new Vector2(0, 0);
        blackHole = new BlackHole(
            BLACK_HOLE_MASS,
            testObjectTexture,
            "BlackHole",
            this.app
            );
        newtonianSystem.AddChild(blackHole);
        blackHole.SetLocalPosition(blackHolePosition);

        Texture2D shipTexture = this.contentManager.Load<Texture2D>("Sprites/arrow");
        Spaceship player = new Spaceship(blackHole as BlackHole, shipTexture, "player", app);
        player.SetLocalPosition(new Vector2(-300, 150));
        parentObject.AddChild(player);

        // some testing space junk spawning
        SpawnAsteroidDisk(newtonianSystem,
            new Vector2(
                windowSize.X * -MAP_SIZE /2,
                windowSize.Y * -MAP_SIZE /2
            ) + blackHolePosition,
            new Vector2(
                windowSize.X * MAP_SIZE,
                windowSize.Y * MAP_SIZE
            ),
            blackHolePosition
        );
    }

    private bool printed = false;
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (!printed)
        {
            printed = true;
            this.app.Services.GetService<ISceneControllerService>().DebugPrintGraph();
        }

        Vector3 bhPosGlobal = blackHole.GetGlobalPosition();
        Vector2 blackholePosition = new Vector2(bhPosGlobal.X, bhPosGlobal.Y);
        SpawnAsteroidsTowardsHole(
            gameTime,
            this.newtonianSystem,
            new Vector2(
                windowSize.X * -MAP_SIZE / 2,
                windowSize.Y * -MAP_SIZE / 2
            ) + blackholePosition,
            new Vector2(
                windowSize.X * MAP_SIZE,
                windowSize.Y * MAP_SIZE
            ), blackholePosition);
    }

    public override void OnUnload()
    {
        this.app.Services.GetService<ILeviathanEngineService>().removeLight(this.sunLight);
    }
}
