using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;


namespace VVVV.Mirage.Nodes
{
    [PluginInfo(Name = "Controller", Category = "Animation", Version = "NRT", Help = "Helps coordinate NRT rendering processes.", Tags = "Mirage", Author="dotprodukt")]
    public class NRTControllerNode : IPluginEvaluate
    {
        #region fields & pins
        #region inputs
        [Input("Enabled", DefaultBoolean = false)]
        public ISpread<bool> FEnabled;

        [Input("Auto Advance", DefaultBoolean = true)]
        public ISpread<bool> FAdvance;

        [Input("Reset", IsBang = true)]
        public ISpread<bool> FReset;

        [Input("Restart Frame", IsBang = true)]
        public ISpread<bool> FRestartFrame;

        [Input("Scrub", DefaultValue = 0.0)]
        public ISpread<double> FScrub;

        [Input("Frame Tile Count", DefaultValue = 1.0, MinValue = 1.0)]
        public ISpread<int> FTileCount;

        [Input("Frame Sample Count", DefaultValue = 1.0, MinValue = 1.0)]
        public ISpread<int> FSampleCount;

        [Input("Target Framerate", DefaultValue = 30.0, MinValue = 1.0)]
        public ISpread<double> FFramerate;
        #endregion
        #region outputs
        [Output("Current Frame")]
        public ISpread<int> FCurrentFrame;

        [Output("Tile Index")]
        public ISpread<int> FTileIndex;

        [Output("Sample Index")]
        public ISpread<int> FSampleIndex;

        [Output("Time")]
        public ISpread<double> FTime;

        [Output("Frame Start")]
        public ISpread<bool> FFrameStart;
        private bool lastFrameStart = false;

        [Output("Tile Start")]
        public ISpread<bool> FTileStart;

        [Output("Tile End")]
        public ISpread<bool> FTileEnd;

        [Output("Frame End")]
        public ISpread<bool> FFrameEnd;
        private bool lastFrameEnd = false;
        #endregion
        #endregion

        public void Evaluate(int spreadMax)
        {
        }
    }
}
