using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TiledLib
{
    public enum LightAreaQuality
    {
        VeryLow,
        Low,
        Middle,
        High,
        VeryHigh
    }

    public enum LightSourceType
    {
        Spot,
        Directional
    }

    public class LightSource
    {
        private GraphicsDevice graphics;

        public LightSourceType Type;

        public RenderTarget2D PrintedLight;
        public RenderTarget2D BeamLight;
        public Vector2 Position { get; set; }
        public Vector2 RenderTargetSize { get; set; }
        public Vector2 Size { get; set; }
        private float qualityRatio;
        public Color Color;

        public Texture2D BeamStencil;

        public int Radius;
        public float RenderRadius;

        public Vector2 PrintPosition
        {
            get { return this.Position - new Vector2(this.Radius, this.Radius); }
        }

        public LightSource(LightSourceType type, GraphicsDevice graphics, int radius, LightAreaQuality quality, Color color, Texture2D beamStencil)
        {
            switch (quality)
            {
                case LightAreaQuality.VeryLow:
                    this.qualityRatio = 0.1f;
                    break;
                case LightAreaQuality.Low:
                    this.qualityRatio = 0.25f;
                    break;
                case LightAreaQuality.Middle:
                    this.qualityRatio = 0.5f;
                    break;
                case LightAreaQuality.High:
                    this.qualityRatio = 0.75f;
                    break;
                case LightAreaQuality.VeryHigh:
                    this.qualityRatio = 1f;
                    break;
            }
            this.Type = type;
            this.graphics = graphics;
            this.Radius = radius;
            this.RenderRadius = (float)radius * this.qualityRatio;
            float baseSize = (float)this.Radius * 2f;
            this.Size = new Vector2(baseSize);
            baseSize *= this.qualityRatio;
            this.RenderTargetSize = new Vector2(baseSize);
            PrintedLight = new RenderTarget2D(graphics, (int)baseSize, (int)baseSize);
            BeamLight = new RenderTarget2D(graphics, (int)baseSize, (int)baseSize);
            this.Color = color;
            BeamStencil = beamStencil;
        }

        public Vector2 ToRelativePosition(Vector2 worldPosition)
        {
            return worldPosition - (Position - RenderTargetSize * 0.5f);
        }

        public Vector2 RelativeZero
        {
            get
            {
                return new Vector2(Position.X - this.Radius, Position.Y - this.Radius);
            }
        }

        public Vector2 RelativeZeroHLSL(ShadowCasterMap shadowMap)
        {
            Vector2 sizedRelativeZero = this.RelativeZero * shadowMap.PrecisionRatio;
            float shadowmapRelativeZeroX = sizedRelativeZero.X / shadowMap.Size.X;
            shadowmapRelativeZeroX -= (shadowmapRelativeZeroX % shadowMap.PixelSizeHLSL.X) * shadowMap.PrecisionRatio;
            float shadowmapRelativeZeroY = sizedRelativeZero.Y / shadowMap.Size.Y;
            shadowmapRelativeZeroY -= (shadowmapRelativeZeroY % shadowMap.PixelSizeHLSL.Y) * shadowMap.PrecisionRatio;
            return new Vector2(shadowmapRelativeZeroX, shadowmapRelativeZeroY);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int size = (int)(this.Radius * 2f);
            spriteBatch.Draw(this.BeamLight, new Rectangle((int)this.PrintPosition.X, (int)this.PrintPosition.Y, size, size), this.Color);
        }

        public void Draw(SpriteBatch spriteBatch, byte opacity)
        {
            Color colorA = this.Color;
            colorA.A = opacity;
            int size = (int)(this.Radius * 2f);
            spriteBatch.Draw(this.PrintedLight, new Rectangle((int)this.PrintPosition.X, (int)this.PrintPosition.Y, size, size), colorA);
        }

        public void Draw(SpriteBatch spriteBatch, Color color, byte opacity)
        {
            color.A = opacity;
            int size = (int)(this.Radius * 2f);
            spriteBatch.Draw(this.PrintedLight, new Rectangle((int)this.PrintPosition.X, (int)this.PrintPosition.Y, size, size), color);
        }
    }
}
