using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiledLib
{
    public class LightingEngine
    {
        public static LightingEngine Instance;

        public LightsFX LFX;
        public ShadowMapResolver ShadowmapResolver;

        public ShadowCasterMap ShadowMap;
        public RenderTarget2D ScreenLights;
        public RenderTarget2D ScreenGround;

        public Dictionary<BeamStencilType, Texture2D> BeamStencils = new Dictionary<BeamStencilType, Texture2D>();

        Color[] sunColors = new Color[] {
            new Color(0.2f,0.2f,0.2f), new Color(0.3f,0.2f,0.2f),  new Color(0.4f,0.2f,0.2f), new Color(0.5f,0.3f,0.3f), new Color(0.7f,0.5f,0.4f), new Color(0.8f,0.6f,0.5f),
            new Color(0.9f,0.7f,0.7f), new Color(1f,9f,9f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f),
            new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,9f,9f),
            new Color(0.9f,0.7f,0.6f), new Color(0.8f,0.6f,0.5f), new Color(0.6f,0.5f,0.3f), new Color(0.4f,0.3f,0.2f), new Color(0.2f,0.2f,0.2f), new Color(0.2f,0.2f,0.2f)
        };
        public Color CurrentSunColor;

        Vector2[] shadowVects = new Vector2[] {
            new Vector2(0f,0f), new Vector2(0.0f,0.0f),  new Vector2(0.0f,0.0f),  new Vector2(-0.1f,0.0f),  new Vector2(-0.3f,0.1f),  new Vector2(-0.5f,0.2f), 
             new Vector2(-0.7f,0.3f),  new Vector2(-0.9f,0.4f),  new Vector2(-0.7f,0.5f),  new Vector2(-0.5f,0.7f),  new Vector2(-0.3f,0.8f),  new Vector2(-0.15f,0.9f), 
              new Vector2(0.0f,1f),  new Vector2(0.15f,0.9f),  new Vector2(0.3f,0.8f),  new Vector2(0.5f,0.7f),  new Vector2(0.7f,0.5f),  new Vector2(0.9f,0.4f), 
               new Vector2(0.7f,0.3f),  new Vector2(0.5f,0.2f),  new Vector2(0.3f,0.1f),  new Vector2(0.1f,0.0f),  new Vector2(0.0f,0.0f),  new Vector2(0.0f,0.0f)
        };
        public Vector2 CurrentShadowVect;

        public List<LightSource> LightSources = new List<LightSource>();

        public LightingEngine()
        {
            Instance = this;

            CurrentSunColor = sunColors[8];
            CurrentShadowVect = shadowVects[8];
        }

        public void LoadContent(ContentManager content, GraphicsDevice gd, SpriteBatch sb)
        {

            LFX = new LightsFX(
                content.Load<Effect>("resolveShadowsEffect.mgfxo"),
                content.Load<Effect>("reductionEffect.mgfxo"),
                content.Load<Effect>("2xMultiBlend.mgfxo"));
            ShadowmapResolver = new ShadowMapResolver(gd, LFX, 256);

            ShadowMap = new ShadowCasterMap(PrecisionSettings.VeryHigh, gd, sb);
            ScreenLights = new RenderTarget2D(gd, gd.Viewport.Width, gd.Viewport.Height);
            ScreenGround = new RenderTarget2D(gd, gd.Viewport.Width, gd.Viewport.Height);

            BeamStencils.Add(BeamStencilType.Wide, content.Load<Texture2D>("beamwide"));
            BeamStencils.Add(BeamStencilType.Narrow, content.Load<Texture2D>("beamnarrow"));
        }

        public void Update(GameTime gameTime, DateTime timeOfDay, SpriteBatch sb, GraphicsDevice gd)
        {
            CalcSunAndShadows(timeOfDay);
        }

        public void Draw(SpriteBatch spriteBatch, Camera gameCamera, Map gameMap)
        {
            ShadowMap.StartGeneratingShadowCasteMap(false);
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
                gameMap.DrawLayer(spriteBatch, "Wall", gameCamera, this, Color.Black);
                spriteBatch.End();
            }
            ShadowMap.EndGeneratingShadowCasterMap();

            foreach (LightSource ls in LightSources)
            {
                ShadowmapResolver.ResolveShadows(ShadowMap, ls, PostEffect.LinearAttenuation_BlurHigh, gameCamera);

                spriteBatch.GraphicsDevice.SetRenderTarget(ls.BeamLight);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
                spriteBatch.Draw(ls.PrintedLight, Vector2.Zero, null, Color.White);
                spriteBatch.End();

                if (ls.BeamStencil != null)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null);
                    spriteBatch.Draw(ls.BeamStencil, ls.RenderTargetSize / 2, null, Color.White, ls.Rotation, new Vector2(ls.BeamStencil.Width / 2, ls.BeamStencil.Height / 2), (ls.RenderRadius * 2) / ls.BeamStencil.Width, SpriteEffects.None, 1);
                    spriteBatch.End();
                }
            }

            spriteBatch.GraphicsDevice.SetRenderTarget(ScreenLights);
            {
                spriteBatch.GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
                {
                    foreach (LightSource ls in LightSources)
                    {
                        ls.Draw(spriteBatch);
                    }
                }
                spriteBatch.End();
            }

            spriteBatch.GraphicsDevice.SetRenderTarget(ScreenGround);
            spriteBatch.GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            gameMap.DrawLayer(spriteBatch, "Terrain", gameCamera, this, Color.White);
            gameMap.DrawShadows(spriteBatch, "Wall", gameCamera, this);
            spriteBatch.End();


            
        }

        void CalcSunAndShadows(DateTime t)
        {
            int nexthour = t.Hour + 1;
            int currenthour = t.Hour;
            int min = t.Minute;
            if (nexthour == 24) nexthour = 0;

            float lerpAmount = (1f / 60f) * (float)min;

            CurrentSunColor = Color.Lerp(sunColors[currenthour], sunColors[nexthour], lerpAmount);
            CurrentShadowVect = Vector2.Lerp(shadowVects[currenthour], shadowVects[nexthour], lerpAmount);
        }
    }




}
