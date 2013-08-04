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

        int weapon;

        bool vehicle = false;

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
            vehicle = (gameHero.drivingVehicle != null);

            heroHealthTarget = (gameHero.drivingVehicle==null?gameHero.Health:gameHero.drivingVehicle.Health);
            if (heroHealthTarget > 99f && heroHealthTarget < 100f) heroHealthTarget = 100f;

            huntedLevelTarget = gameHero.HuntedLevel.Level;
            huntedLevel = MathHelper.Lerp(huntedLevel, huntedLevelTarget, 0.1f);

            heroHealth = MathHelper.Lerp(heroHealth, heroHealthTarget, 0.1f);

            showAmmo = gameHero.SelectedWeapon > 0;

//            ammo = gameHero.Weapons[gameHero.SelectedWeapon].clipAmmo;
            ammo = gameHero.Ammo;

            weapon = gameHero.Weapons[gameHero.SelectedWeapon].sortOrder;

            timeOfDay = timeofday;
            day = gameDay;

            Ticker.Update(gameTime);
        }

        public void Draw(SpriteBatch sb)
        {
            Rectangle vp = sb.GraphicsDevice.Viewport.Bounds;

            sb.Draw(hudTex, new Vector2(vp.Width - 240, 18), new Rectangle(319,0,230,236), Color.White);

            sb.Draw(hudTex, new Vector2(vp.Width - 238, 120 - (int)huntedLevel), new Rectangle(550, 2 + (100 - (int)huntedLevel), 16, (int)huntedLevel*2), Color.White, 0f, new Vector2(0,0), 1f, SpriteEffects.None, 1);

            sb.DrawString(hudFont, "Day " + day, new Vector2(vp.Width - 216, 221), Color.White);
            sb.DrawString(hudFont, timeOfDay.Hour.ToString("00") + ":" + timeOfDay.Minute.ToString("00"), new Vector2(vp.Width - 24 - hudFont.MeasureString(timeOfDay.Hour.ToString("00") + ":" + timeOfDay.Minute.ToString("00")).X, 221), Color.White);

            sb.Draw(hudTex, new Vector2(30, 28), new Rectangle(vehicle?50:0, 150, 50, 50), Color.White, 0f, new Vector2(25, 25), 1f, SpriteEffects.None, 1);
            sb.Draw(hudTex, new Vector2(30, 28 +26), new Rectangle(weapon * 50, 100, 50, 50), Color.White, 0f, new Vector2(25, 25), 1f, SpriteEffects.None, 1);

            sb.Draw(hudTex, new Vector2(68, 18), new Rectangle(0, 0, 310, 25), Color.White);
            sb.Draw(hudTex, new Vector2(70, 20), new Rectangle(2, 54, (int)((float)(300f/100f) * heroHealth), 16), vehicle?Color.Green:Color.Red);


            //if (showAmmo)
            //{
                sb.Draw(hudTex, new Vector2(68, 18 + 26), new Rectangle(0, 26, 310, 25), Color.White * (showAmmo?1f:0.3f));
                sb.Draw(hudTex, new Vector2(70, 20 + 26), new Rectangle(0, 71, ammo * 3, 16), Color.White * (showAmmo ? 1f : 0.3f));
            //}

            Ticker.Draw(sb, hudFont, new Vector2(20, 70));
        }
    }
}
