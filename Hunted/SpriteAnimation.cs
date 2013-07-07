using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hunted
{
    public class SpriteAnimation
    {
        public int NumFrames;
        public int CurrentFrame;
        public double TargetFrameTime;
        public double CurrentFrameTime;

        public int YOffset;

        public Rectangle CellRect;

        bool HasRestFrame;

        public SpriteAnimation(int numFrames, int targetFrameTime, int yOffset, Rectangle cellSize, bool hasRestFrame)
        {
            NumFrames = numFrames;
            TargetFrameTime = targetFrameTime;
            YOffset = yOffset;

            HasRestFrame = hasRestFrame;

            CurrentFrame = 0;
            CurrentFrameTime = 0;

            CellRect = cellSize;
            CellRect.Y = YOffset * cellSize.Height;
        }

        public void Update(GameTime gameTime)
        {
            CurrentFrameTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (CurrentFrameTime >= TargetFrameTime)
            {
                CurrentFrameTime = 0;
                CurrentFrame++;
               
                if (CurrentFrame >= NumFrames) CurrentFrame = 0;

            }

            CellRect.X = CellRect.Width * CurrentFrame;
        }

        public void Reset()
        {
            if (HasRestFrame) CurrentFrame = NumFrames;
            else CurrentFrame = 0;
            CurrentFrameTime = 0;
            CellRect.X = CellRect.Width * CurrentFrame;

        }
    }
}
