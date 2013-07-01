using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hunted
{
    class Compound
    {
        public Vector2 Position;
        public Rectangle Bounds;
        public Rectangle InnerBounds;

        public List<Building> Buildings = new List<Building>();
    }

    public enum BuildingType
    {
        Building,
        Helipad,
        Carpark
    }

    class Building
    {
        public BuildingType Type;
        public Rectangle Rect;
    }
}
