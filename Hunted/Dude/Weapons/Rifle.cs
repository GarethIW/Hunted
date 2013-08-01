using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted.Weapons
{
    public class Rifle : Weapon
    {
        int roundsCount = 0;

        public Rifle(Dude owner)
            : base(owner)
        {
            clipAmmo = 50;
            isAuto = true;
            coolDownTarget = 75;
            sortOrder = 4;
        }

        public override bool Use(GameTime gameTime, Vector2 target, bool trigger, Camera gameCamera, bool canCollide)
        {
            if (!trigger)
            {
                roundsCount = 0;
                coolDownTarget = 100;
            }


            if (!base.Use(gameTime, target, trigger, gameCamera, canCollide)) return false;

            roundsCount++;
            if (roundsCount == 3)
            {
                roundsCount = 0;
                coolDownTarget = 600;
            }
            else coolDownTarget = 75;

            if (owner.GetType() == typeof(HeroDude)) owner.Ammo--;


            AudioController.PlaySFX("rifle", 1f, 0f,0.3f, owner.Position);
            ProjectileController.Instance.Add(ProjectileType.Rifle, owner, muzzlePos, target - muzzlePos, canCollide);

            return true;
        }
    }
}
