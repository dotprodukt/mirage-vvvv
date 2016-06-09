using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VVVV.Utils.VMath;

using VVVV.Mirage.Lib.Util;

namespace VVVV.Mirage.Lib.Scene
{
    public interface IEntity
    {
        Matrix4x4 Transform { get; set; }
        AABB Bounds { get; set; }
        int Type { get; set; }
    }
}
