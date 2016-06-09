using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;

using VVVV.Mirage.Lib.Scene;

namespace VVVV.Mirage.Nodes.Scene
{
    [PluginInfo(Name = "Cons", Category = "Mirage", Version = "Entity", Author = "dotprodukt")]
    public class ConsEntityNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Config("Input Count", DefaultValue = 2, MinValue = 2)]
        protected IDiffSpread<int> FInputCount;

        private List<IIOContainer<Pin<IEntity>>> FInputs = new List<IIOContainer<Pin<IEntity>>>();

        [Output("Output")]
        protected ISpread<ISpread<IEntity>> FOutput;

        [Import()]
        protected IIOFactory FIOFactory;

        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = FInputs.Count;
            for (int i = 0; i < FInputs.Count; ++i)
            {
                if (FInputs[i].IOObject.IsChanged)
                {
                    if (FInputs[i].IOObject.IsConnected)
                    {
                        FOutput[i].SliceCount = this.FInputs[i].IOObject.SliceCount;
                        FOutput[i] = FInputs[i].IOObject;
                    }
                    else
                    {
                        FOutput[i].SliceCount = 0;
                    }
                }
            }
        }

        private void SetInputs()
        {
            if (FInputCount[0] != FInputs.Count)
            {
                if (FInputCount[0] > FInputs.Count)
                {
                    while (FInputCount[0] > FInputs.Count)
                    {
                        InputAttribute attr = new InputAttribute("Input " + Convert.ToString(FInputs.Count + 1));
                        attr.CheckIfChanged = true;
                        IIOContainer<Pin<IEntity>> pin = FIOFactory.CreateIOContainer<Pin<IEntity>>(attr);
                        pin.IOObject.SliceCount = 0;
                        FInputs.Add(pin);
                    }
                }
                else
                {
                    while (FInputCount[0] < FInputs.Count)
                    {
                        FInputs[FInputs.Count - 1].Dispose();
                        FInputs.RemoveAt(FInputs.Count - 1);
                    }
                }
            }
        }

        public void OnImportsSatisfied()
        {
            this.FInputCount.Changed += new SpreadChangedEventHander<int>(FInputCount_Changed);
            this.SetInputs();
        }

        void FInputCount_Changed(IDiffSpread<int> spread)
        {
            this.SetInputs();
        }
    }
}
