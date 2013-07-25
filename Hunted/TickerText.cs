using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Hunted
{
    public class TickerLine
    {
        public string Text;
        public double Life;
        public int CurrentChar;
        public StringBuilder CurrentText;
    }

    public class TickerText
    {
        private List<TickerLine> Lines = new List<TickerLine>();

        private int CurrentLine=-1;
        private double animTime;

        private string EmptyString = string.Empty;


        public TickerText()
        {
           
        }

        

        public void AddLine(string Text)
        {
            TickerLine newLine = new TickerLine();
            newLine.Text = Text;
            newLine.Life = 10000;
            newLine.CurrentChar = -1;
            newLine.CurrentText = new StringBuilder();
            Lines.Add(newLine);

            //for (int i = 0; i < 5; i++)
            //{
            //    if (Lines[i].Life == 0)
            //    {
            //        Lines[i].Life = 10000;
            //        Lines[i].Text = Text;
            //        Lines[i].CurrentChar = -1;
            //        break;
            //    }
            //}
        }

        public void Update(GameTime gameTime)
        {
            foreach(TickerLine l in Lines)
            {
                if (l.CurrentChar < l.Text.Length - 1)
                {
                    animTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (animTime >= 50)
                    {
                        l.CurrentChar++;
                        l.CurrentText.Append(l.Text[l.CurrentChar]);
                        //Audio.Audio.TickerBlip();
                        animTime = 0;
                    }
                    if (l.CurrentChar == l.Text.Length - 1)
                    {
                        animTime = 0;
                    }

                    break;
                }
            }

            foreach(TickerLine l in Lines)
            {
                if (l.Life > 0)
                {
                    if (l.CurrentChar == l.Text.Length - 1)
                    {
                        l.Life -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    }
                }
            }

            Lines.RemoveAll(line => line.Life <= 0);
        }

        public void Clear()
        {
            Lines.Clear();

            //CurrentLine = -1;
            //animTime = 0;
            //for (int i = 0; i < 5; i++)
            //{
                
            //        Lines[i].Life = 0;
            //        Lines[i].Text = EmptyString;
            //        Lines[i].CurrentText.Length = 0;
            //        Lines[i].CurrentChar = -1;
                  
            //}
        }

        public void Draw(SpriteBatch batch, SpriteFont font, Vector2 pos)
        {
            //batch.Begin();

            Vector2 ScreenPos = pos;
            foreach (TickerLine l in Lines)
            {
                if (l.CurrentChar>-1)
                {
                    batch.DrawString(font,
                                     l.CurrentText,
                                     ScreenPos + Vector2.One, Color.Black);
                    batch.DrawString(font,
                                     l.CurrentText,
                                     ScreenPos, Color.White);
                    
                }
                ScreenPos.Y += 25;
            }

           // batch.End();
        }

    }
}