using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public virtual bool Use(GameTime gameTime, bool trigger)
        {
            if (!trigger)
            {
                triggerHeld = false;
                return false;
            }

            if (triggerHeld && !isAuto) return false;
            triggerHeld = true;

            if (clipAmmo <= 0) return false;

            coolDown += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (coolDown < coolDownTarget) return false;

            coolDown = 0;

            return true;
        }

       
    }
}
