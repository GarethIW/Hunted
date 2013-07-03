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
        public Texture2D spotBG;
        public Texture2D spotFG;

        RenderTarget2D spotRT;
        BlendState lightsBS;

        public LightsFX LFX;
        public ShadowMapResolver ShadowmapResolver;

        public ShadowCasterMap ShadowMap;
        public RenderTarget2D ScreenLights;
        public RenderTarget2D ScreenGround;

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
            CurrentSunColor = sunColors[8];
            CurrentShadowVect = shadowVects[8];
        }

        public void LoadContent(ContentManager content, GraphicsDevice gd, SpriteBatch sb)
        {
            spotBG = content.Load<Texture2D>("spotbg");
            spotFG = content.Load<Texture2D>("spotfg");

            spotRT = new RenderTarget2D(gd, spotBG.Width, spotBG.Height);

            lightsBS = new BlendState()
            {
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.SourceColor,
                ColorBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
                AlphaBlendFunction = BlendFunction.Add

            };

            LFX = new LightsFX(
                content.Load<Effect>("resolveShadowsEffect"),
                content.Load<Effect>("reductionEffect"),
                content.Load<Effect>("2xMultiBlend"));
            ShadowmapResolver = new ShadowMapResolver(gd, LFX, 256);

            ShadowMap = new ShadowCasterMap(PrecisionSettings.VeryHigh, gd, sb);
            ScreenLights = new RenderTarget2D(gd, gd.Viewport.Width, gd.Viewport.Height);
            ScreenGround = new RenderTarget2D(gd, gd.Viewport.Width, gd.Viewport.Height);
        }

        public void Update(GameTime gameTime, DateTime timeOfDay, SpriteBatch sb, GraphicsDevice gd)
        {
            CalcSunAndShadows(timeOfDay);
            PrepareLights(sb, gd);
        }

        public void PrepareLights(SpriteBatch sb, GraphicsDevice gd)
        {
            gd.SetRenderTarget(spotRT);
            sb.Begin();
            sb.Draw(spotBG, Vector2.Zero, null, Color.White);
            sb.Draw(spotFG, Vector2.Zero, null, Color.White * (1f - (CurrentSunColor.ToVector3().Z)));
            sb.End();
            gd.SetRenderTarget(null);
        }

        public void Draw(SpriteBatch sb, Camera gameCamera)
        {
            sb.Begin(SpriteSortMode.Deferred, lightsBS, SamplerState.PointClamp, null, null, null, gameCamera.CameraMatrix);
            foreach (LightSource ls in LightSources)
            {
                sb.Draw(spotRT, ls.Position, null, Color.White, 0f, new Vector2(spotRT.Width, spotRT.Height) / 2, 1f, SpriteEffects.None, 1);
                sb.Draw(spotRT, ls.Position, null, Color.White*0.995f, 0f, new Vector2(spotRT.Width, spotRT.Height) / 2, 1f, SpriteEffects.None, 1);

            }
            
            sb.End();
            
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
