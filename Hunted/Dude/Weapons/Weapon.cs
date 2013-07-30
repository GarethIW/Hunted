using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace Hunted.Weapons
{
    public class Weapon
    {
        internal int clipAmmo;
        internal int maxClipAmmo;
        internal int maxAmmo;

        internal double coolDownTarget;
        internal double coolDown;
        internal bool isAuto;
        internal bool triggerHeld;

        internal Dude owner;

        public Weapon(Dude own)
        {
            owner = own;

            triggerHeld = false;
            coolDown = 0;
        }

        public virtual bool Use(GameTime gameTime, bool trigger, Camera gameCamera, bool canCollide)
        {
            if (!trigger)
            {
                triggerHeld = false;
                return false;
            }

            if (triggerHeld && !isAuto && (owner.GetType() == typeof(HeroDude))) return false;
            triggerHeld = true;

            if (this.GetType() != typeof(Knife))
            {
                //if (clipAmmo <= 0) return false;
                if (owner.Ammo <= 0 && owner.GetType() == typeof(HeroDude)) return false;

                //if (owner.GetType() == typeof(HeroDude)) clipAmmo--;
                if (owner.GetType() == typeof(HeroDude)) owner.Ammo--;
            }

            if (isAuto || (owner.GetType() == typeof(AIDude)))
            {
                coolDown += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (coolDown < coolDownTarget) return false;

                coolDown = 0;
            }

            return true;
        }

       
    }
}
