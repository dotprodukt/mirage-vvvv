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

        public uint LeafCount { get; private set; }
        public uint InternalCount { get; private set; }
        public uint Count
        {
            get
            {
                return InternalCount + LeafCount;
            }
        }

        public uint LeafOffset
        {
            get
            {
                return InternalCount;
            }
        }
        public uint InternalOffset
        {
            get
            {
                return 0;
            } 
        }

        private uint internalIndex;

        public Node[] Nodes { get; private set; }

        public LBVH(uint capacity)
        {
            LeafCount = capacity;
            InternalCount = capacity > 0 ? capacity - 1 : 0;
            Nodes = new Node[Count];
            internalIndex = 0;
        }

        public void Reset()
        {
            internalIndex = 0;
        }

        public uint GetNextInternalNodeIndex()
        {
            return internalIndex++;
        }
    }
}