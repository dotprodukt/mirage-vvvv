﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

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

        [Output("Node Buffer", IsSingle=true)]
        protected ISpread<DX11Resource<DX11DynamicStructuredBuffer<LBVH.Node>>> FOutput;

        [Output("Box Transforms")]
        protected ISpread<Matrix4x4> FBoxes;

        [Output("Is Valid")]
        protected ISpread<bool> FValid;

        private bool FInvalidate;
        private bool FFirst = true;

        private LBVH.Node[] data;

        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = 1;
            FInvalidate = false;

            if (FOutput[0] == null)
            {
                FOutput[0] = new DX11Resource<DX11DynamicStructuredBuffer<LBVH.Node>>();
            }

            if (FApply[0] || FFirst)
            {
                BVHBuilder builder = new BVHBuilder();
                data = builder.Build(FEntities.ToList());

                FBoxes.SliceCount = data.Length;
                for (int i = 0; i < data.Length; ++i)
                {
                    FBoxes[i] = VMath.Transform((
                        data[i].min + data[i].max)*0.5,
                        data[i].max - data[i].min,
                        Vector3D.Zero);
                }

                FInvalidate = true;
                FFirst = false;
                FOutput.Stream.IsChanged = true;
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (this.FInvalidate)
            {
                Device device = context.Device;

                if (FOutput[0].Contains(context))
                {
                    if (FOutput[0][context].ElementCount != data.Length)
                    {
                        if (data.Length > 0)
                        {
                            FOutput[0].Dispose(context);
                            FOutput[0][context] = new DX11DynamicStructuredBuffer<LBVH.Node>(context, data.Length);
                        }
                    }
                }
                else
                {
                    if (data.Length > 0)
                    {
                        FOutput[0][context] = new DX11DynamicStructuredBuffer<LBVH.Node>(context, data.Length);
                    }
                }

                if (data.Length > 0)
                {
                    FOutput[0][context].WriteData(data);
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
            try
            {
                FOutput[0].Dispose();
            }
            catch
            {

            }
        }
    }
}
