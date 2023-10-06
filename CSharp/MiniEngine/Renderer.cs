using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.OpenGL;

namespace MiniEngine
{
    /// <summary>
    /// Renderer
    /// </summary>
    public class Renderer
    {
        /// <summary>
        /// Context
        /// </summary>
        private Context _context;

        ///// <summary>
        ///// Materials
        ///// </summary>
        //private List<Material> _materials = new List<Material>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Renderer(Context context)
        {
            _context = context;

        }

        /// <summary>
        /// Initialize the renderer
        /// </summary>
        public void Init()
        {
            GL.glEnable(GL.GL_CULL_FACE);
            GL.glFrontFace(GL.GL_CW);
            GL.glCullFace(GL.GL_BACK);
            GL.glEnable(GL.GL_DEPTH_TEST);


            //Set the clear color to black... just because, black is nice!
            GL.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        }

        ///// <summary>
        ///// Add new material
        ///// </summary>
        //public void AddMaterial(Material mat)
        //{
        //    mat.Compile();

        //    _materials.Add(mat);

        //}

    }
}
