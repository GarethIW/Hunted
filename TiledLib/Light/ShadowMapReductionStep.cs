using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace TiledLib
{
    public class ShadowMapReductionStep
    {
        public int ReductionPower;
        public RenderTarget2D RenderTarget;

        public ShadowMapReductionStep(int reductionFactor, RenderTarget2D renderTarget)
        {
            this.ReductionPower = reductionFactor;
            this.RenderTarget = renderTarget;
        }
    }
}