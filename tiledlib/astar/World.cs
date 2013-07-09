using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiledLib.AStar
{
    /// <summary>
    /// Author: Roy Triesscheijn (http://www.royalexander.wordpress.com)
    /// Sample World class that only provides 'is free or not' information on a node
    /// </summary>
    public class World
    {
        private bool[, ,] worldBlocked; //extremely simple world where each node can be free or blocked: true=blocked        
        
        //Note: we use Y as height and Z as depth here!
        public int Left { get { return 0; } }
        public int Right { get { return worldBlocked.GetLength(0); } }
        public int Bottom { get { return 0; } }
        public int Top { get { return worldBlocked.GetLength(1); } }
        public int Front { get { return 0; } }
        public int Back { get { return worldBlocked.GetLength(2); } }

        /// <summary>
        /// Creates a 2D world
        /// </summary>        
        public World(int width, int height)
            : this(width, height, 1)
        { }

        /// <summary>
        /// Creates a 3D world
        /// </summary>        
        public World(int width, int height, int depth)
        {
            worldBlocked = new Boolean[width, height, depth];
        }

        /// <summary>
        /// Mark positions in the world als blocked (true) or unblocked (false)
        /// </summary>
        /// <param name="value">use true if you wan't to block the value</param>
        public void MarkPosition(Point3D position, bool value)
        {
            worldBlocked[position.X, position.Y, position.Z] = value;
        }

        /// <summary>
        /// Checks if a position is free or marked (and legal)
        /// </summary>        
        /// <returns>true if the position is free</returns>
        public bool PositionIsFree(Point3D position)
        {
            return position.X >= 0 && position.X < worldBlocked.GetLength(0) &&
                position.Y >= 0 && position.Y < worldBlocked.GetLength(1) &&
                position.Z >= 0 && position.Z < worldBlocked.GetLength(2) &&
                !worldBlocked[position.X, position.Y, position.Z];            
        }
    }
}
