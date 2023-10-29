using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// A game component that can be place on a GameObject
    /// </summary>
    public abstract class GameComponent
    {
        private GameObject _parent;

        /// <summary>
        /// Parent GameObject
        /// </summary>
        public GameObject Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (_parent != null)
                {
                    _parent = null;
                    _parent.RemoveComponent(this);
                }
                if (value != null)
                {
                    _parent.AddComponent(this);
                }
            }
        }

        /// <summary>
        /// Destruction of the GameComponent
        /// </summary>
        public void Destroy()
        {
            OnDestroy();

            if (_parent != null)
            {
                _parent.RemoveComponent(this);
                _parent = null;
            }
        }

        /// <summary>
        /// On destruction
        /// </summary>
        protected virtual void OnDestroy()
        {

        }


        /// <summary>
        /// Parent GameObject
        /// </summary>
        internal void SetParentInternal(GameObject gameObject)
        {
            _parent = gameObject;
        }

    }
}
