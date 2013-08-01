using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted.Weapons
{
    public class SMG : Weapon
    {
        int roundsCount = 0;
        int AIroundscount = 0;

        public SMG(Dude owner)
            : base(owner)
        {
            clipAmmo = 50;
            isAuto = true;
            coolDownTarget = 30;

            sortOrder = 3;
        }

        public override bool Use(GameTime gameTime, Vector2 target, bool trigger, Camera gameCamera, bool canCollide)
        {
            if (!base.Use(gameTime, target, trigger, gameCamera, canCollide)) return false;

            if(owner is AIDude) AIroundscount++;
            roundsCount++;
            if (roundsCount == 2)
            {
                roundsCount = 0;
                if (owner.GetType() == typeof(HeroDude)) owner.Ammo--;
            }

            if (AIroundscount >= 20)
            {
                AIroundscount = 0;
                coolDownTarget = 1000;
            }
            else coolDownTarget = 30;


            AudioController.PlaySFX("smg", 1f, -0.2f,0.2f, owner.Position);
            ProjectileController.Instance.Add(ProjectileType.SMG, owner, muzzlePos, target - muzzlePos, canCollide);

            return true;
        }
    }
}
