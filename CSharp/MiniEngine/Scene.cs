using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    /// <summary>
    /// Scene that contains objects to render
    /// </summary>
    public class Scene
    {

        /// <summary>
        /// List of children game objects
        /// </summary>
        private List<GameObject> _children = new List<GameObject>();


        /// <summary>
        /// List of children game objects
        /// </summary>
        public List<GameObject> Children { get { return _children; } }


        /// <summary>
        /// Current camera
        /// </summary>
        public CameraObject Camera;

        /// <summary>
        /// Basic scene
        /// </summary>
        public Scene()
        {
            InitEmptyScene();
        }


        /// <summary>
        /// Add a game object
        /// </summary>
        public void Add(GameObject obj)
        {
            _children.Add(obj);
        }

        /// <summary>
        /// Remove a game object
        /// </summary>
        public void Remove(GameObject obj)
        {
            _children.Remove(obj);
        }

        /// <summary>
        /// Clear everything on the scene (it's a reset)
        /// </summary>
        public void Clear()
        {
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                _children[i].Destroy();
            }


            InitEmptyScene();

        }

        /// <summary>
        /// Create a new camera
        /// </summary>
        private void InitEmptyScene()
        {
            Camera = new CameraObject();
            
        }

    }
}
