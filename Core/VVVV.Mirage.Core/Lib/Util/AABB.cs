using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VVVV.Utils.VMath;

namespace VVVV.Mirage.Lib.Util
{
    struct AABB
    {
        public Vector3D Min;
        public Vector3D Max;

        public Vector3D Mean
        {
            get
            {
                return (Min + Max) * 0.5;
            }
        }
        public Vector3D Size
        {
            get
            {
                return Max - Min;
            }
        }

        public AABB(Vector3D[] points)
        {
            this.Min = new Vector3D(0, 0, 0);
            this.Max = new Vector3D(0, 0, 0);
            
            int count = points.Length;
            for (int i = 0; i < count; ++i)
            {
                Vector3D p = points[i];
                if (i == 0)
                {
                    this.Min = p;
                    this.Max = p;
                }
                else
                {
                    this.AssignUnion(p);
                }
            }
        }

        #region Boolean Ops
        public void AssignUnion(Vector3D pt)
        {
            Min.x = VMath.Min(Min.x, pt.x);
            Min.y = VMath.Min(Min.y, pt.y);
            Min.z = VMath.Min(Min.z, pt.z);

            Max.x = VMath.Max(Max.x, pt.x);
            Max.y = VMath.Max(Max.y, pt.y);
            Max.z = VMath.Max(Max.z, pt.z);
        }
        public AABB Union(Vector3D pt)
        {
            AABB b = this; // copy self
            b.AssignUnion(pt);
            return b;
        }

        public void AssignUnion(AABB box)
        {
            Min.x = VMath.Min(Min.x, box.Min.x);
            Min.y = VMath.Min(Min.y, box.Min.y);
            Min.z = VMath.Min(Min.z, box.Min.z);

            Max.x = VMath.Max(Max.x, box.Max.x);
            Max.y = VMath.Max(Max.y, box.Max.y);
            Max.z = VMath.Max(Max.z, box.Max.z);
        }
        public AABB Union(AABB box)
        {
            AABB b = this; // copy self
            b.AssignUnion(box);
            return b;
        }
        #endregion

        public Vector3D NormalizePoint(Vector3D pt)
        {
            return (pt - this.Min) / (this.Max - this.Min);
        }

        static public AABB Transform(AABB box, Matrix4x4 T)
        {
            return new AABB(new Vector3D[]{
                T*box.Min,
                T*new Vector3D( box.Max.x, box.Min.y, box.Min.z ),
                T*new Vector3D( box.Min.x, box.Max.y, box.Min.z ),
                T*new Vector3D( box.Max.x, box.Max.y, box.Min.z ),
                T*new Vector3D( box.Min.x, box.Min.y, box.Max.z ),
                T*new Vector3D( box.Max.x, box.Min.y, box.Max.z ),
                T*new Vector3D( box.Min.x, box.Max.y, box.Max.z ),
                T*box.Max
            });
        }
    }
}
