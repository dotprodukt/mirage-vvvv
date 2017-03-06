using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.DX11.Lib.Effects;

namespace VVVV.Mirage.Lib.Scene
{
    //[PluginInfo(Name = "EntityNode", Category = "Mirage", Version = "", Author = "dotprodukt")]
    public class BaseEntityNode : IPluginBase, IPluginEvaluate, IDisposable, IPartImportsSatisfiedNotification, IDX11ShaderNodeWrapper
    {
        public void Evaluate(int SpreadMax)
        {

        }

        public void Dispose()
        {

        }
    }
}
