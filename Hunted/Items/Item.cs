using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hunted
{
    public enum ItemType
    {
        Health,
        Ammo,
        CompoundMap,
        GeneralMap
    }

    public class Item
    {
        public ItemType Type;
        public Vector2 Position;
        public bool Active;

        public float Rotation;

        public Item(ItemType type, Vector2 pos)
        {
            Type = type;
            Position = pos;
            Active = true;
            Rotation = (float)Helper.Random.NextDouble() * MathHelper.Pi;
        }

        public void Update(GameTime gameTime)
        {
            Rotation += 0.01f;
        }

    }
}
