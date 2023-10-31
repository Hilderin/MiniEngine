using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    public struct Vertex
    {
        public Vector3 Pos;
        public Vector3 Color;
        public Vector2 TexCoord;

        public Vertex(float x, float y)
        {
            Pos = new Vector3(x, y, 0.0f);
            Color = Vector3.Zero;
            TexCoord = Vector2.Zero;
        }

        public Vertex(float x, float y, float z)
        {
            Pos = new Vector3(x, y, z);
            Color = Vector3.Zero;
            TexCoord = Vector2.Zero;
        }

        public Vertex(float x, float y, float z, Vector2 texCoord)
        {
            Pos = new Vector3(x, y, z);
            Color = Vector3.Zero;
            TexCoord = texCoord;
        }
    }
}
