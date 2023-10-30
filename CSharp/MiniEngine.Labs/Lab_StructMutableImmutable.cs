using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Labs
{
    internal class Lab_StructMutableImmuatable
    {
        private MutableStruct _m = new MutableStruct(1, 1);

        private MutableStruct M { get { return _m; } }

        public void Test()
        {


            //Normal ref...
            _m = new MutableStruct(1, 1);
            PassMutableStructRef(ref _m);
            Console.WriteLine($"_m after ref: {_m.X}, expected 2");


            //Updating values using property and method...
            _m = new MutableStruct(1, 1);
            this.M.UpdateValues(2, 2);
            Console.WriteLine($"_m after using property and method: {_m.X}, expected 2");


            //Testing in
            _m = new MutableStruct(1, 1);
            PassMutableStructIn(_m);
            Console.WriteLine($"_m after in: {_m.X}, expected 2");
        }

        private void PassMutableStructRef(ref MutableStruct s)
        {
            s.UpdateValues(2, 0);
        }

        private void PassMutableStructIn(in MutableStruct s)
        {
            s.UpdateValues(2, 0);
        }

        private struct MutableStruct
        {
            public float X;
            public float Y;

            public MutableStruct(float x, float y)
            {
                X = x;
                Y = x;
            }

            public void UpdateValues(float x, float y)
            {
                X = x;
                Y = x;
            }
        }

        private readonly struct ImmutableStruct
        {
            public readonly float X;
            public readonly float Y;

            public ImmutableStruct(float x, float y)
            {
                X = x;
                Y = y;
            }

        }
    }
}
