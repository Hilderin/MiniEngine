using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Trees
{
    /// <summary>
    /// KDTree
    /// Reference: https://en.wikipedia.org/wiki/K-d_tree
    /// </summary>
    public class KDTreeVector3
    {
        private Vector3Comparer _comparer = new Vector3Comparer();
        private List<Vector3> _list;
        private KDTreeNode _root;

        /// <summary>
        /// Constructor
        /// </summary>
        public KDTreeVector3(List<Vector3> list)
        {
            _list = list;

            BuildTree();
        }



        /// <summary>
        /// Build the tree
        /// </summary>
        private void BuildTree()
        {

            _root = BuildTreeInternal(0, _list.Count - 1);

        }

        /// <summary>
        /// Build the tree
        /// </summary>
        private KDTreeNode BuildTreeInternal(int startIndex, int stopIndex)
        {
            if (startIndex == stopIndex)
            {
                //Only one element...
                return new KDTreeNode()
                {
                    Axis = -1,
                    StartIndex = startIndex,
                    StopIndex = stopIndex
                };
            }


            GetAxisSplit(startIndex, stopIndex, out int axis, out float splitValue);

            Sort(startIndex, stopIndex, axis);

            int pivot_index = GetPivotIndex(startIndex, stopIndex, axis, splitValue);

            if (pivot_index == 0 || pivot_index == stopIndex)
            {
                //All the tree is on one side... no need to create a node here...
                return new KDTreeNode()
                {
                    Axis = -1,
                    StartIndex = startIndex,
                    StopIndex = stopIndex
                };
            }


            KDTreeNode newNode = new KDTreeNode()
            {
                Axis = axis,
                SplitValue = splitValue
            };

            newNode.Left = BuildTreeInternal(startIndex, pivot_index);
            newNode.Right = BuildTreeInternal(pivot_index + 1, stopIndex);

            return newNode;


        }


        /// <summary>
        /// Sort a section of the list
        /// </summary>
        private int GetPivotIndex(int startIndex, int stopIndex, int axis, float splitValue)
        {
            for (int i = startIndex; i <= stopIndex; i++)
            {
                if (_list[i][axis] >= splitValue)
                    return i;
            }

            //Should not be there...
            throw new InvalidOperationException($"Cannot find pivot index for split value: {splitValue}");
        }

        /// <summary>
        /// Sort a section of the list
        /// </summary>
        private void Sort(int startIndex, int stopIndex, int axis)
        {
            _comparer.Axis = axis;
            _list.Sort(startIndex, stopIndex - startIndex + 1, _comparer);
        }


        /// <summary>
        /// Calculate the axis and the split value to use
        /// </summary>
        private void GetAxisSplit(int startIndex, int stopIndex, out int axis, out float splitValue)
        {
            Vector3 mean = Vector3.Zero;
            Vector3 vars = Vector3.Zero;
            float runc = 1, runs = 1;

            for (int i = startIndex; i <= stopIndex; i++, runc += 1f, runs = 1f / runc)
            {
                Vector3 vector = _list[i];

                for (int k = 0; k < 3; ++k)
                {
                    float delta = vector[k] - mean[k];
                    mean[k] += delta * runs;
                    vars[k] += delta * (vector[k] - mean[k]);
                }
            }


            // split axis is one where the variance is largest
            if (vars.X >= vars.Y && vars.X >= vars.Z)
                //X axis...
                axis = 0;
            else if (vars.Y >= vars.Z)
                //Y axis...
                axis = 1;
            else
                //Z axis
                axis = 2;

            splitValue = mean[axis];
        }


        /// <summary>
        /// KDTreeNode
        /// </summary>
        private class KDTreeNode
        {
            public int Axis;
            public float SplitValue;
            public KDTreeNode Left;
            public KDTreeNode Right;
            public int StartIndex;
            public int StopIndex;
        }

        /// <summary>
        /// Compare 2 vector3
        /// </summary>
        private class Vector3Comparer : IComparer<Vector3>
        {
            public int Axis;

            public int Compare(Vector3 x, Vector3 y)
            {
                return x[Axis].CompareTo(y[Axis]);
            }
        }



    }
}
