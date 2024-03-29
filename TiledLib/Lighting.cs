﻿using Microsoft.Xna.Framework;
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

        RenderTarget2D[] PrintedLight = new RenderTarget2D[20];
        RenderTarget2D[] BeamLight = new RenderTarget2D[20];

        public Dictionary<BeamStencilType, Texture2D> BeamStencils = new Dictionary<BeamStencilType, Texture2D>();
        public Dictionary<SpotStencilType, Tuple<Texture2D, Texture2D, RenderTarget2D>> SpotStencils = new Dictionary<SpotStencilType, Tuple<Texture2D, Texture2D, RenderTarget2D>>();

        BlendState spotBS;

        Color[] sunColors = new Color[] {
            new Color(0.3f,0.3f,0.3f), new Color(0.3f,0.3f,0.3f), new Color(0.3f,0.3f,0.3f),  new Color(0.3f,0.3f,0.3f), new Color(0.4f,0.3f,0.3f), new Color(0.7f,0.5f,0.4f), 
            new Color(0.9f,0.7f,0.7f), new Color(1f,9f,9f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f),
            new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,1f,1f), new Color(1f,9f,9f),
            new Color(0.9f,0.7f,0.6f), new Color(0.8f,0.6f,0.5f), new Color(0.6f,0.5f,0.3f), new Color(0.4f,0.3f,0.3f), new Color(0.3f,0.3f,0.3f), new Color(0.3f,0.3f,0.3f)
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
            for (int i = 0; i < PrintedLight.Length; i++)
            {
                PrintedLight[i] = new RenderTarget2D(gd, 750, 750);
                BeamLight[i] = new RenderTarget2D(gd, 750, 750);
            }
#if OPENGL
            LFX = new LightsFX(
                content.Load<Effect>("resolveShadowsEffect.mgfxo"),
                content.Load<Effect>("reductionEffect.mgfxo"),
                content.Load<Effect>("2xMultiBlend.mgfxo"));
#else
            LFX = new LightsFX(
                content.Load<Effect>("resolveShadowsEffect"),
                content.Load<Effect>("reductionEffect"),
                content.Load<Effect>("2xMultiBlend"));
#endif
            ShadowmapResolver = new ShadowMapResolver(gd, LFX, 128);

            ShadowMap = new ShadowCasterMap(PrecisionSettings.VeryHigh, gd, sb);
            ScreenLights = new RenderTarget2D(gd, gd.Viewport.Width, gd.Viewport.Height);
            ScreenGround = new RenderTarget2D(gd, gd.Viewport.Width, gd.Viewport.Height);

            BeamStencils.Add(BeamStencilType.Wide, content.Load<Texture2D>("lights/beamwide"));
            BeamStencils.Add(BeamStencilType.Narrow, content.Load<Texture2D>("lights/beamnarrow"));

            SpotStencils.Add(SpotStencilType.Full, Tuple.Create<Texture2D, Texture2D, RenderTarget2D>(content.Load<Texture2D>("lights/spotbg"), content.Load<Texture2D>("lights/spotfg"), new RenderTarget2D(gd, content.Load<Texture2D>("lights/spotbg").Width, content.Load<Texture2D>("lights/spotbg").Height)));
            SpotStencils.Add(SpotStencilType.Half, Tuple.Create<Texture2D, Texture2D, RenderTarget2D>(content.Load<Texture2D>("lights/spotbg"), content.Load<Texture2D>("lights/spothalf"), new RenderTarget2D(gd, content.Load<Texture2D>("lights/spotbg").Width, content.Load<Texture2D>("lights/spotbg").Height)));

            spotBS = new BlendState()
            {
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.SourceColor,
                ColorBlendFunction = BlendFunction.Add,
            };
        }

        public void Update(GameTime gameTime, DateTime timeOfDay, SpriteBatch sb, GraphicsDevice gd)
        {
            CalcSunAndShadows(timeOfDay);
            foreach (LightSource ls in LightSources)
            {
                ls.Color = Color.White * (1f - (CurrentSunColor.ToVector3().Z));
            }
            PrepareSpotLights(sb, gd);
        }

        void PrepareSpotLights(SpriteBatch sb, GraphicsDevice gd)
        {
            foreach (KeyValuePair<SpotStencilType, Tuple<Texture2D, Texture2D, RenderTarget2D>> kvp in SpotStencils)
            {
                gd.SetRenderTarget(kvp.Value.Item3);
                //gd.Clear(new Color(0.25f,0.25f,0.25f));
                sb.Begin();
                sb.Draw(kvp.Value.Item1, Vector2.Zero, null, Color.White);
                sb.Draw(kvp.Value.Item2, Vector2.Zero, null, Color.White * (1f - ((CurrentSunColor.ToVector3().X))));
                sb.End();
                gd.SetRenderTarget(null);
            }
        }

        public void DrawPhase1(SpriteBatch spriteBatch, Camera gameCamera)
        {
            int rtnum = 0;

            foreach (LightSource ls in LightSources.Where(src => src.SpotStencil==null && src.Active).OrderBy(src => (src.Position - gameCamera.Position).Length()))
            {
                if (!(ls.Position.X > (gameCamera.Position.X - (gameCamera.Width)) && ls.Position.X < (gameCamera.Position.X + (gameCamera.Width)) &&
                   ls.Position.Y > (gameCamera.Position.Y - (gameCamera.Height)) && ls.Position.Y < (gameCamera.Position.Y + (gameCamera.Height)))) continue;

                ShadowmapResolver.ResolveShadows(ShadowMap, ls, PrintedLight[rtnum], PostEffect.LinearAttenuation_BlurHigh, gameCamera);

                spriteBatch.GraphicsDevice.SetRenderTarget(BeamLight[rtnum]);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
                spriteBatch.Draw(PrintedLight[rtnum], Vector2.Zero, null, Color.White);
                spriteBatch.End();
                if (ls.BeamStencil != null)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
                    spriteBatch.Draw(ls.BeamStencil, ls.RenderTargetSize / 2, null, Color.White, ls.Rotation, new Vector2(ls.BeamStencil.Width / 2, ls.BeamStencil.Height / 2), (ls.RenderRadius * 2) / ls.BeamStencil.Width, SpriteEffects.None, 1);
                    spriteBatch.End();
                }

                rtnum++;
                if (rtnum == PrintedLight.Length) break;
            }

            //spriteBatch.GraphicsDevice.SetRenderTarget(BeamLight);

            //foreach (LightSource ls in LightSources.Where(src => src.SpotStencil == null))
            //{
            //    if (!(ls.Position.X > (gameCamera.Position.X - (gameCamera.Width * 2)) && ls.Position.X < (gameCamera.Position.X + (gameCamera.Width * 2)) &&
            //       ls.Position.Y > (gameCamera.Position.Y - (gameCamera.Height * 2)) && ls.Position.Y < (gameCamera.Position.Y + (gameCamera.Height * 2)))) continue;

            //    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            //    spriteBatch.Draw(PrintedLight, Vector2.Zero, null, Color.White);
            //    spriteBatch.End();

            //    if (ls.BeamStencil != null)
            //    {
            //        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            //        spriteBatch.Draw(ls.BeamStencil, ls.RenderTargetSize / 2, null, Color.White, ls.Rotation, new Vector2(ls.BeamStencil.Width / 2, ls.BeamStencil.Height / 2), (ls.RenderRadius * 2) / ls.BeamStencil.Width, SpriteEffects.None, 1);
            //        spriteBatch.End();
            //    }
            //}

            rtnum = 0;

            spriteBatch.GraphicsDevice.SetRenderTarget(ScreenLights);
            {
                spriteBatch.GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
                {
                    foreach (LightSource ls in LightSources.Where(src => src.SpotStencil == null && src.Active).OrderBy(src => (src.Position - gameCamera.Position).Length()))
                    {
                        if (!(ls.Position.X > (gameCamera.Position.X - (gameCamera.Width)) && ls.Position.X < (gameCamera.Position.X + (gameCamera.Width)) &&
                            ls.Position.Y > (gameCamera.Position.Y - (gameCamera.Height)) && ls.Position.Y < (gameCamera.Position.Y + (gameCamera.Height)))) continue;

                        ls.Draw(spriteBatch, BeamLight[rtnum]);

                        rtnum++;
                        if (rtnum == PrintedLight.Length) break;
                    }
                }
                spriteBatch.End();

                
            }

            spriteBatch.GraphicsDevice.SetRenderTarget(ScreenGround);
            spriteBatch.GraphicsDevice.Clear(Color.Black);
            
        }

        public void DrawPhase2(SpriteBatch spriteBatch)
        {

            LFX.PrintLightsOverTexture(null, spriteBatch, spriteBatch.GraphicsDevice, ScreenLights, ScreenGround, 0.4f * (1f - (CurrentSunColor.ToVector3().X)));
        }

        public void DrawSpots(SpriteBatch spriteBatch, Camera gameCamera, Map gameMap)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, spotBS, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateScale(gameCamera.Zoom) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height / 2, 0));
            foreach (LightSource ls in LightSources.Where(src => src.SpotStencil != null && src.Active))
            {
                if (!(ls.Position.X > (gameCamera.Position.X - (gameCamera.Width)) && ls.Position.X < (gameCamera.Position.X + (gameCamera.Width)) &&
                      ls.Position.Y > (gameCamera.Position.Y - (gameCamera.Height)) && ls.Position.Y < (gameCamera.Position.Y + (gameCamera.Height)))) continue;

                ls.Draw(spriteBatch, BeamLight[0]);
                //ls.Draw(spriteBatch);

            }
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

        public void RemoveSource(LightSource ls)
        {
            LightSources.Remove(ls);
            ls = null;
        }
    }




}
