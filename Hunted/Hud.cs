using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hunted
{
    public class Hud
    {
        public static Hud Instance;

        float heroHealth;
        float heroHealthTarget;

        int ammo;
        bool showAmmo = true;

        float huntedLevel;
        float huntedLevelTarget;

        DateTime timeOfDay;
        int day;

        Texture2D hudTex;
        SpriteFont hudFont;

        public TickerText Ticker;


        public Hud()
        {
            Instance = this;
            Ticker = new TickerText();
        }

        public void LoadContent(ContentManager content)
        {
            hudTex = content.Load<Texture2D>("hud");
            hudFont = content.Load<SpriteFont>("hudfont");
        }

        public void Update(GameTime gameTime, HeroDude gameHero, DateTime timeofday, int gameDay)
        {
            heroHealthTarget = gameHero.Health;
            heroHealth = MathHelper.Lerp(heroHealth, heroHealthTarget, 0.1f);
            if (heroHealth > 99f && heroHealth < 100f) heroHealth = 100f;

            showAmmo = gameHero.SelectedWeapon > 0;

//            ammo = gameHero.Weapons[gameHero.SelectedWeapon].clipAmmo;
            ammo = gameHero.Ammo;


            timeOfDay = timeofday;
            day = gameDay;

            Ticker.Update(gameTime);
        }

        public void Draw(SpriteBatch sb)
        {
            Rectangle vp = sb.GraphicsDevice.Viewport.Bounds;

            sb.Draw(hudTex, new Vector2(vp.Width - 240, 18), new Rectangle(319,0,230,236), Color.White);
            sb.DrawString(hudFont, "Day " + day, new Vector2(vp.Width - 216, 221), Color.White);
            sb.DrawString(hudFont, timeOfDay.Hour.ToString("00") + ":" + timeOfDay.Minute.ToString("00"), new Vector2(vp.Width - 24 - hudFont.MeasureString(timeOfDay.Hour.ToString("00") + ":" + timeOfDay.Minute.ToString("00")).X, 221), Color.White);

            sb.Draw(hudTex, new Vector2(18, 18), new Rectangle(0, 0, 310, 25), Color.White);
            sb.Draw(hudTex, new Vector2(20, 20), new Rectangle(2, 54, (int)((float)(300f/100f) * heroHealth), 16), Color.White);


            if (showAmmo)
            {
                sb.Draw(hudTex, new Vector2(18, 18 + 26), new Rectangle(0, 26, 310, 25), Color.White);
                sb.Draw(hudTex, new Vector2(20, 20 + 26), new Rectangle(0, 71, ammo*3, 16), Color.White);
            }

            Ticker.Draw(sb, hudFont, new Vector2(20, 70));
        }
    }
}
