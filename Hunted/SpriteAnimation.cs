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

        public bool ReverseLoop;

        public int YOffset;
        public int XOffset;

        public Rectangle CellRect;

        bool HasRestFrame;
        int dir;

        public SpriteAnimation(int numFrames, int targetFrameTime, int xOffset, int yOffset, Rectangle cellSize, bool hasRestFrame, bool reverseLoop)
        {
            NumFrames = numFrames;
            TargetFrameTime = targetFrameTime;
            XOffset = xOffset;
            YOffset = yOffset;

            HasRestFrame = hasRestFrame;

            CurrentFrame = 0;
            CurrentFrameTime = 0;

            CellRect = cellSize;
            CellRect.Y = YOffset * cellSize.Height;

            ReverseLoop = reverseLoop;
            dir = 1;
        }

        public void Update(GameTime gameTime)
        {
            CurrentFrameTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (CurrentFrameTime >= TargetFrameTime)
            {
                CurrentFrameTime = 0;
                CurrentFrame+=dir;

                if (!ReverseLoop)
                {
                    if (CurrentFrame >= NumFrames) CurrentFrame = 0;
                }
                else
                {
                    if (CurrentFrame >= NumFrames)
                    {
                        dir = -dir;
                        CurrentFrame = NumFrames - 2;
                    }
                    if(CurrentFrame<0)
                    {
                        dir = -dir;
                        CurrentFrame = 1;
                    }

                    
                }

            }

            CellRect.X = CellRect.Width * (CurrentFrame + XOffset);
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
