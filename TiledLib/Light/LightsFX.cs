using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TiledLib
{
    public class LightsFX
    {
        public Effect ResolveShadowsEffect;
        public Effect ReductionEffect;
        public Effect LightBlender;

        public LightsFX(Effect resolveShadowsEffect, Effect reductionEffect, Effect blender)
        {
            this.ResolveShadowsEffect = resolveShadowsEffect;
            this.ReductionEffect = reductionEffect;
            this.LightBlender = blender;
        }

        public void PrintLightsOverTexture(RenderTarget2D renderTarget, SpriteBatch spriteBatch, GraphicsDevice graphics, Texture2D light, Texture2D underlyingTexture, float mixFactor0to1)
        {
            this.LightBlender.Parameters["MixFactor"].SetValue(mixFactor0to1);
            graphics.SetRenderTarget(renderTarget);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, this.LightBlender);
            graphics.Textures[1] = light;
            graphics.Textures[1].GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            spriteBatch.Draw(underlyingTexture, Vector2.Zero, Color.White);
            spriteBatch.End();

        }

        public void PrintLightsPortionOverTexture(RenderTarget2D renderTarget, SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Texture2D light, Vector2 portionStart, Vector2 portionSize, float portionScale, Texture2D underlyingTexture, float mixFactor0to1)
        {
            if (portionScale <= 0)
                portionScale = 1f;

            // Portion must be translated in screen coordinates to HLSL coordinates
            Vector4 portionHLSL = Vector4.Zero;
            portionHLSL.X = portionStart.X / light.Width;
            portionHLSL.Y = portionStart.Y / light.Height;
            portionHLSL.Z = portionSize.X / light.Width;
            portionHLSL.W = portionSize.Y / light.Height;
            this.LightBlender.Parameters["MixFactor"].SetValue(mixFactor0to1);
            this.LightBlender.Parameters["Portion"].SetValue(portionHLSL);
            this.LightBlender.Parameters["PortionScale"].SetValue(portionScale);
            graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, this.LightBlender);
            graphics.GraphicsDevice.Textures[1] = light;
            graphics.GraphicsDevice.Textures[1].GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            spriteBatch.Draw(underlyingTexture, Vector2.Zero, Color.White);
            spriteBatch.End();
            graphics.GraphicsDevice.SetRenderTarget(null);
        }

        public void PrintLightsPortionOverTextureWithMask(RenderTarget2D renderTarget, SpriteBatch spriteBatch, GraphicsDeviceManager graphics, Texture2D light, Vector2 portionStart, Vector2 portionSize, float portionScale, Texture2D underlyingTexture, float mixFactor0to1)
        {
            if (portionScale <= 0)
                portionScale = 1f;

            // Portion must be translated in screen coordinates to HLSL coordinates
            Vector4 portionHLSL = Vector4.Zero;
            portionHLSL.X = portionStart.X / light.Width;
            portionHLSL.Y = portionStart.Y / light.Height;
            portionHLSL.Z = portionSize.X / light.Width;
            portionHLSL.W = portionSize.Y / light.Height;
            this.LightBlender.Parameters["MixFactor"].SetValue(mixFactor0to1);
            this.LightBlender.Parameters["Portion"].SetValue(portionHLSL);
            this.LightBlender.Parameters["PortionScale"].SetValue(portionScale);
            graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, this.LightBlender);
            graphics.GraphicsDevice.Textures[1] = light;
            graphics.GraphicsDevice.Textures[1].GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            spriteBatch.Draw(underlyingTexture, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
    }
}
