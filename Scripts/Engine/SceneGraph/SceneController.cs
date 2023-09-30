namespace LD54.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class RootGameObject : GameObject
{
    public RootGameObject(string name, Game appCtx) : base(name, appCtx)
    {

    }

    public override void OnLoad(GameObject parentObject) => throw new System.NotImplementedException();

    public override void Update(GameTime gameTime) => throw new System.NotImplementedException();

    public override void OnUnload() => throw new System.NotImplementedException();
}

public interface ISceneControllerService
{
    public void DebugPrintGraph();

    public RootGameObject GetSceneRoot();
    public RootGameObject GetPersistentGameObject();

    public Scene? GetCurrentScene();
    public void AddScene(Scene scene);
    public void ChangeScene(string next);
}

public class SceneController : GameComponent, ISceneControllerService
{
    private readonly List<Scene> scenes = new();

    private Scene? activeScene;
    private Scene? nextScene;

    private readonly RootGameObject rootGameObject;
    public RootGameObject GetSceneRoot()
    {
        return this.rootGameObject;
    }
    private readonly RootGameObject persistantGameObject;
    public RootGameObject GetPersistentGameObject()
    {
        return this.persistantGameObject;
    }

    public SceneController(Game game) : base(game)
    {
        this.rootGameObject = new RootGameObject(".Root", game);
        this.persistantGameObject = new RootGameObject(".PersistentRoot", game);
    }

    /// <summary>
    /// Updates the current scene as well as handles any requested scene transitions
    /// </summary>
    /// <param name="gameTime"></param>
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (this.nextScene != null)
        {
            this.TransitionScene();
        }

        if(this.activeScene != null)
        {
            this.UpdateGameObjects(gameTime);
        }
    }

    public void DestroyObject(GameObject gameObject)
    {
        gameObject.OnUnload();
        this.UnloadChildren(gameObject);
        gameObject.GetParent().RemoveChild(gameObject);
    }

    #region SCENE_API
    /// <summary>
    /// Prints a text-version of the scene tree (has indentation for children)
    /// </summary>
    public void DebugPrintGraph()
    {
        this.PrintChildren(this.rootGameObject, 0);
        this.PrintChildren(this.persistantGameObject, 0);
    }
    private void PrintChildren(GameObject gameObject, int depth)
    {
        string space = "";
        for (int i = 0; i < depth; i++)
        {
            space += "   ";
        }
        PrintLn(space + gameObject.GetName());

        IEnumerable<GameObject> g = gameObject.GetChildren();
        foreach (GameObject child in g.ToList())
        {
            this.PrintChildren(child, ++depth);
        }
    }

    public Scene? GetCurrentScene()
    {
        return this.activeScene;
    }

    /// <summary>
    /// Registers a scene. Must be used before trying to load said scene.
    /// </summary>
    /// <param name="scene"></param>
    public void AddScene(Scene scene)
    {
        this.scenes.Add(scene);
    }

    public void ChangeScene(string next)
    {
        Scene nextScene = this.GetSceneByName(next);

        if(this.activeScene != nextScene)
        {
            this.nextScene = nextScene;
        }
    }
    #endregion

    #region SCENE_CONTROL
    private Scene GetSceneByName(string scene)
    {
        foreach (Scene s in this.scenes)
        {
            if (s.GetName() == scene)
            {
                return s;
            }
        }

        throw new ArgumentException("No such scene with that name");
    }

    private void TransitionScene()
    {
        if(this.activeScene != null)
        {
            this.UnloadChildren(this.rootGameObject);
            // this should unload all the monogame assets from the previous scene
            this.activeScene.OnUnload();

            // this should delete the entire scene graph from the previous scene
            this.rootGameObject.ClearChildren();
        }

        //  Perform a garbage collection to ensure memory is cleared
        GC.Collect();

        this.activeScene = this.nextScene;

        this.nextScene = null;

        // guaranteed to be not null by ChangeScene function not having a nullable (?) parameter
        this.activeScene?.OnLoad(this.rootGameObject);
    }
    #endregion

    #region SCENE_GRAPH_CONTROL
    private void UpdateGameObjects(GameTime gameTime)
    {
        this.UpdateChildren(this.rootGameObject, gameTime);
        this.UpdateChildren(this.persistantGameObject, gameTime);
    }

    private void UpdateChildren(GameObject gameObject, GameTime gameTime)
    {
        IEnumerable<GameObject> g = gameObject.GetChildren();

        foreach (GameObject child in g.ToList().Where(child => child.Enabled))
        {
            if (!child.Initialized)
            {
                child.OnLoad(gameObject);
                child.Initialized = true;
            }
            child.Update(gameTime);
            this.UpdateChildren(child, gameTime);
        }
    }
    private void UnloadChildren(GameObject gameObject)
    {
        IEnumerable<GameObject> g = gameObject.GetChildren();

        foreach (GameObject child in g.ToList().Where(child => child.Enabled))
        {
            child.OnUnload();
            this.UnloadChildren(child);
        }
    }
    #endregion
}
