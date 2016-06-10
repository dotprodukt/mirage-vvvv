using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

using SlimDX;
using SlimDX.Direct3D11;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

using VVVV.DX11;
using VVVV.DX11.Lib.Devices;
using VVVV.DX11.Nodes;

using VVVV.Mirage.Lib.Util;
using VVVV.Mirage.Lib.Scene;

namespace VVVV.Mirage.Nodes
{
    [PluginInfo(Name = "BVH", Category = "Mirage", Version = "Buffer", Help = "Contructs a LBVH Buffer.", Tags = "", Author = "dotprodukt")]
    public class BVHBuilderNode : IPluginEvaluate, IDX11ResourceProvider, IDisposable
    {
        [Input("Entities")]
        protected ISpread<IEntity> FEntities;

        [Input("Apply", IsBang=true, DefaultValue=1)]
        protected ISpread<bool> FApply;

        [Output("Node Count")]
        protected ISpread<int> FNodeCount;

        [Output("Leaf Count")]
        protected ISpread<int> FLeafCount;

        [Output("Leaf Offset")]
        protected ISpread<int> FLeafOffset;

        [Output("Node Buffer", IsSingle=true)]
        protected ISpread<DX11Resource<DX11DynamicStructuredBuffer<LBVH.Node>>> FNodeBuffer;

        [Output("Transform Buffer", IsSingle = true)]
        protected ISpread<DX11Resource<DX11DynamicStructuredBuffer<Matrix>>> FTransformBuffer;

        [Output("Box Transforms", Order=2)]
        protected ISpread<Matrix4x4> FBoxes;

        [Output("Is Valid", Order=3)]
        protected ISpread<bool> FValid;

        private bool FInvalidate;
        private bool FFirst = true;

        private LBVH.Node[] node_data;
        private Matrix[] transform_data;

        public void Evaluate(int spreadMax)
        {
            FNodeBuffer.SliceCount = 1;
            FTransformBuffer.SliceCount = 1;
            FInvalidate = false;

            if (FNodeBuffer[0] == null)
            {
                FNodeBuffer[0] = new DX11Resource<DX11DynamicStructuredBuffer<LBVH.Node>>();
            }
            if (FTransformBuffer[0] == null)
            {
                FTransformBuffer[0] = new DX11Resource<DX11DynamicStructuredBuffer<Matrix>>();
            }

            //FEntities.Sync();
            if (FApply[0] || FFirst)
            {
                BVHBuilder builder = new BVHBuilder();
                node_data = builder.Build(FEntities.ToList());
                FNodeCount[0] = node_data.Length;
                FLeafCount[0] = FEntities.SliceCount;
                FLeafOffset[0] = FEntities.SliceCount > 0 ? FEntities.SliceCount - 1 : 0;

                if (node_data != null)
                {
                    transform_data = builder.GetTransformsForLastBuild();

                    FBoxes.SliceCount = node_data.Length;
                    for (int i = 0; i < node_data.Length; ++i)
                    {
                        LBVH.Node node = node_data[i];
                        Vector3 min = node_data[i].min;
                        Vector3 max = node_data[i].max;
                        FBoxes[i] = VMath.Transform(
                            (new Vector3D(min.X + max.X, min.Y + max.Y, min.Z + max.Z))*0.5,
                            new Vector3D(max.X - min.X, max.Y - min.Y, max.Z - min.Z),
                            Vector3D.Zero);
                    }
                }
                else
                {
                    FBoxes.SliceCount = 0;
                }

                FInvalidate = true;
                FFirst = false;
                FNodeBuffer.Stream.IsChanged = true;
                FTransformBuffer.Stream.IsChanged = true;
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (FInvalidate)
            {
                Device device = context.Device;
                int len = node_data != null ? node_data.Length : 0;


                if (FNodeBuffer[0].Contains(context))
                {
                    if (FNodeBuffer[0][context].ElementCount != len)
                    {
                        if (len > 0)
                        {
                            FNodeBuffer[0].Dispose(context);
                            FNodeBuffer[0][context] = new DX11DynamicStructuredBuffer<LBVH.Node>(context, len);
                        }
                    }
                }
                else
                {
                    if (len > 0)
                    {
                        FNodeBuffer[0][context] = new DX11DynamicStructuredBuffer<LBVH.Node>(context, len);
                    }
                }

                if (len > 0)
                {
                    FNodeBuffer[0][context].WriteData(node_data);
                }

                len *= 2;

                if (FTransformBuffer[0].Contains(context))
                {
                    if (FTransformBuffer[0][context].ElementCount != len)
                    {
                        if (len > 0)
                        {
                            FTransformBuffer[0].Dispose(context);
                            FTransformBuffer[0][context] = new DX11DynamicStructuredBuffer<Matrix>(context, len);
                        }
                    }
                }
                else
                {
                    if (len > 0)
                    {
                        FTransformBuffer[0][context] = new DX11DynamicStructuredBuffer<Matrix>(context, len);
                    }
                }

                if (len > 0)
                {
                    FTransformBuffer[0][context].WriteData(transform_data);
                }
            }
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            if (force)
            {
                FNodeBuffer[0].Dispose(context);
                FTransformBuffer[0].Dispose(context);
            }
        }

        public void Dispose()
        {
            try
            {
                FNodeBuffer[0].Dispose();
                FTransformBuffer[0].Dispose();
            }
            catch
            {

            }
        }
    }
}
