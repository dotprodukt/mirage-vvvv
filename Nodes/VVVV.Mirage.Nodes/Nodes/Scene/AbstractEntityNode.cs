using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

using VVVV.Mirage.Lib.Util;
using VVVV.Mirage.Lib.Scene;

namespace VVVV.Mirage.Nodes.Scene
{
    [PluginInfo(Name = "Entity", Category = "Mirage", Version = "Abstract", Help = "Create an abstract scene entity.", Tags = "", Author = "dotprodukt")]
    public class AbstractEntityNode : IPluginEvaluate
    {
        [Input("Type")]
        protected ISpread<int> FType;
        [Input("Bounds Min")]
        protected ISpread<Vector3D> FMin;
        [Input("Bounds Max")]
        protected ISpread<Vector3D> FMax;
        [Input("Transform")]
        protected ISpread<Matrix4x4> FTransform;

        [Output("Entity")]
        protected ISpread<AbstractEntity> FOutput;

        private int LastSliceCount = 0;

        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; ++i)
            {
                if (FOutput[i] == null || i>=LastSliceCount)
                    FOutput[i] = new AbstractEntity();

                FOutput[i].Type = FType[i];
                FOutput[i].Bounds = new AABB(FMin[i], FMax[i]);
                FOutput[i].Transform = FTransform[i];
            }

            LastSliceCount = FOutput.SliceCount;
        }
    }
}
