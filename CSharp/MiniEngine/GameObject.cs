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
    public abstract class GameObject : WorldTransform
    {
        /// <summary>
        /// Parent Game Object
        /// </summary>
        private GameObject _parent;

        /// <summary>
        /// Components
        /// </summary>
        private List<GameComponent> _components = new List<GameComponent>();

        /// <summary>
        /// Current Context
        /// </summary>
        public Context Context => Context.Current;

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
            if(component.Parent != this)
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
