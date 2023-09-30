// ReSharper disable MemberCanBePrivate.Global

using System.Linq;

namespace LD54.Engine;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public abstract class GameObject : EngineObject, IUpdateable
{
    #region IUPDATEABLE
    // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
    public bool Enabled { get; } = true;
    public int UpdateOrder { get; }
    public event EventHandler<EventArgs>? EnabledChanged;
    public event EventHandler<EventArgs>? UpdateOrderChanged;
    #endregion
    
    protected GameObject? parent;
    protected readonly List<GameObject> children = new();

    protected Matrix transform;
    protected readonly List<Component> components = new();

    protected GameObject(string name, Game appCtx) : base(name, appCtx)
    {
        this.transform = Matrix.Identity;
    }

    public new virtual void Update(GameTime gameTime)
    {
        this.UpdateComponents(gameTime);
    }

    public override void OnUnload()
    {
        this.UnloadComponents();
    }

    #region SCENE_GRAPH
    // ReSharper disable once MemberCanBeProtected.Global
    public Matrix GetLocalTransform()
    {
        return this.transform;
    }

    // ReSharper disable once MemberCanBeProtected.Global
    public void SetLocalTransform(Matrix transform)
    {
        this.transform = transform;
    }

    public Matrix GetGlobalTransform()
    {
        //                                  |-  Statement null if there is no parent.
        //                                  |                       |-  Null-coalescing operator makes these parenthesis
        //                                  V                       V   evaluate to the identity matrix if the above is null.
        return this.transform * (this.parent?.GetGlobalTransform() ?? Matrix.Identity);
    }

    /// <summary>
    /// Parents the a child to this gameObject.
    /// </summary>
    /// <param name="gameObject"></param>
    public void AddChild(GameObject gameObject)
    {
        gameObject.parent?.RemoveChild(gameObject);
        gameObject.parent = this;
        this.children.Add(gameObject);
    }

    public IEnumerable<GameObject> GetChildren()
    {
        return this.children;
    }

    public void RemoveChild(GameObject gameObject)
    {
        this.children.Remove(gameObject);
        gameObject.parent = null;
    }

    public void ClearChildren()
    {
        foreach (GameObject child in this.children)
        {
            child.parent = null;
        }
        this.children.Clear();
        GC.Collect();
    }
    #endregion

    #region COMPONENT_SYSTEM
    // ReSharper disable once MemberCanBeProtected.Global
    public void AddComponent(Component component)
    {
        this.components.Add(component);
        component.OnLoad(this);
    }

    public void RemoveComponent(Component component)
    {
        component.OnUnload();
        this.components.Remove(component);
    }

    protected void UpdateComponents(GameTime gameTime)
    {
        foreach (Component c in this.components.Where(c => c.Enabled))
        {
            c.Update(gameTime);
        }
    }

    protected void UnloadComponents()
    {
        foreach (Component c in this.components.Where(c => c.Enabled))
        {
            c.OnUnload();
        }
    }

    /// <summary>
    /// This will return the FIRST component of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    // ReSharper disable once UnusedMember.Global
    public Component? GetComponent<T>() where T : Component
    {
        return this.components.FirstOrDefault(c => c.GetType() == typeof(T));
    }

    /// <summary>
    /// This will return ALL components of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<Component> GetAllComponents<T>() where T : Component
    {
        return this.components.Where(c => c.GetType() == typeof(T)).ToList();
    }

    public Component GetComponent<T>(string componentName) where T : Component
    {
        IEnumerable<Component> typeComponents = this.GetAllComponents<T>();
        foreach (Component c in typeComponents.Where(c => c.GetName() == componentName))
        {
            return c;
        }

        throw new ArgumentException("No component with that name on this GameObject");
    }
    #endregion
}
