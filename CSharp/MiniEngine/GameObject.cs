using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// A GameObject that can be placed in a scene
    /// </summary>
    public abstract class GameObject
    {
        /// <summary>
        /// OnLocationChanged
        /// </summary>
        public event OnTransformChangedHandler OnChanged
        {
            add { Transform.OnChanged += value; }
            remove { Transform.OnChanged -= value; }
        }

        /// <summary>
        /// Components
        /// </summary>
        private List<GameComponent> _components = new List<GameComponent>();

        /// <summary>
        /// Transform
        /// </summary>
        public WorldTransform Transform { get; private set; } = new WorldTransform();

        /// <summary>
        /// Current Context
        /// </summary>
        public Context Context => Context.Current;

        /// <summary>
        /// Location
        /// </summary>
        public Vector3 Location { get { return Transform.Location; } set { Transform.Location = value; } }

        /// <summary>
        /// Scale
        /// </summary>
        public Vector3 Scale { get { return Transform.Scale; } set { Transform.Scale = value; } }

        /// <summary>
        /// Scale
        /// </summary>
        public Rotator3 Rotation { get { return Transform.Rotation; } set { Transform.Rotation = value; } }


        /// <summary>
        /// Add a new component to the game object
        /// </summary>
        public T AddComponent<T>() where T : GameComponent, new()
        {
            
            T newComponent = new();

            newComponent.SetParentInternal(this);

            _components.Add(newComponent);

            return newComponent;

        }

        /// <summary>
        /// Add a component to the game object
        /// </summary>
        public T AddComponent<T>(T component) where T : GameComponent
        {
            AddComponent(component);

            return component;

        }

        /// <summary>
        /// Add a component to the game object
        /// </summary>
        public GameComponent AddComponent(GameComponent component)
        {
            if (component.Parent != null)
                throw new InvalidOperationException("Cannot add a component already attached to a GameObject.");

            component.SetParentInternal(this);

            _components.Add(component);

            return component;

        }

        /// <summary>
        /// Remove a component to the game object
        /// </summary>
        public void RemoveComponent(GameComponent component)
        {
            if (component.Parent != this)
                throw new InvalidOperationException("The component does not belong to this GameObject.");

            component.SetParentInternal(null);

            _components.Remove(component);

        }

        /// <summary>
        /// Destroy the GameObject
        /// </summary>
        public void Destroy()
        {
            for (int i = _components.Count - 1; i >= 0; i--)
            {
                _components[i].Destroy();
            }

            OnDestroy();
        }

        /// <summary>
        /// On destruction
        /// </summary>
        protected virtual void OnDestroy()
        {

        }
    }
}
