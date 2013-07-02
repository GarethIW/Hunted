using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiledLib
{
    public class LightingEngine
    {
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

        public void Update(GameTime gameTime, ref DateTime timeOfDay)
        {
            timeOfDay = timeOfDay.AddMinutes(gameTime.ElapsedGameTime.TotalSeconds * 100);

            CalcSunAndShadows(timeOfDay);
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

    public enum LightSourceType
    {
        Spot,
        Directional
    }

    public class LightSource
    {
        public LightSourceType Type;
        public Vector2 Position;
        public Vector2 Direction;
    }
}
