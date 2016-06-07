using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.DX11.Nodes;
using VVVV.Mirage.Lib.Util;
using VVVV.Mirage.Lib.Scene;

using SlimDX.Direct3D11;

using VVVV.DX11;
using VVVV.DX11.Lib.Devices;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
//using SlimDX;
namespace VVVV.Mirage.Nodes
{
    [PluginInfo(Name = "BVH", Category = "DX11", Version = "Buffer", Help = "Contructs a LBVH Buffer", Tags = "", Author = "dotprodukt")]
    class BVHBuilderNode : IPluginEvaluate, IDX11ResourceProvider, IDisposable
    {
        [Input("Entities")]
        protected ISpread<IEntity> FEntities;

        [Input("Apply", IsBang=true, DefaultValue=1)]
        protected ISpread<bool> FApply;

        [Output("Node Buffer", IsSingle=true)]
        protected ISpread<DX11Resource<DX11DynamicStructuredBuffer<LBVH.Node>>> FOutput;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        private bool FInvalidate;
        private bool FFirst = true;



        public void Evaluate(int spreadMax)
        {
            this.FOutput.SliceCount = 1;
            this.FInvalidate = false;

            if (this.FOutput[0] == null)
            {
                this.FOutput[0] = new DX11Resource<DX11DynamicStructuredBuffer<LBVH.Node>>();
            }

            if (this.FApply[0] || this.FFirst)
            {


                this.FInvalidate = true;
                this.FFirst = false;
                this.FOutput.Stream.IsChanged = true;
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (this.FInvalidate)
            {
                Device device = context.Device;

                if (this.FOutput[0].Contains(context))
                {
                    
                }
                else
                {
                    FOutput[0][context] = new DX11DynamicStructuredBuffer<LBVH.Node>(context,10);
                }
            }
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            if (force)
            {
                FOutput[0].Dispose(context);
            }
        }

        public void Dispose()
        {
            
        }
    }
}
