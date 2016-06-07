using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VVVV.Utils.VMath;

namespace VVVV.Mirage.Lib.Util
{
    public class LBVH
    {
        public struct Node
        {
            public Vector3D min;
            public int childA;
            public Vector3D max;
            public int childB;
        }

        public uint LeafCount { public get; private set; }
        public uint InternalCount { public get; private set; }
        public uint Count
        {
            public get
            {
                return InternalCount + LeafCount;
            }
        }

        public uint LeafOffset
        {
            public get
            {
                return InternalCount;
            }
        }
        public uint InternalOffset
        {
            public get
            {
                return 0;
            } 
        }

        private uint availInternalNodes;

        public Node[] Nodes { public get; private set; }

        public LBVH(uint capacity)
        {
            LeafCount = capacity;
            InternalCount = capacity > 0 ? capacity - 1 : 0;
            Nodes = new Node[Count];
            availInternalNodes = InternalCount;
        }

        public void Reset()
        {
            availInternalNodes = InternalCount;
        }

        public uint GetNextInternalNodeIndex()
        {
            return availInternalNodes++;
        }
    }
}