#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TiledLib;
#endregion

namespace Hunted
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    public class MapScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D texBG;
        Texture2D texLogo;
        Texture2D mapIcons;

        RenderTarget2D mapRT;

        Map gameMap;
        bool[,] mapFog;
        HeroDude gameHero;

        float scale;

        SpriteFont font;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public MapScreen(Map map, bool[,] fog, HeroDude hero, Texture2D icons)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            gameMap = map;
            mapFog = fog;
            mapIcons = icons;
            gameHero = hero;

            IsPopup = true;
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            texBG = content.Load<Texture2D>("blank");
            font = content.Load<SpriteFont>("hudfont");

            mapRT = new RenderTarget2D(ScreenManager.GraphicsDevice, 600, 600);
            scale = (mapRT.Width/(float)(gameMap.Width*gameMap.TileWidth));
            ScreenManager.GraphicsDevice.SetRenderTarget(mapRT);
            ScreenManager.GraphicsDevice.Clear(Color.Black);
            gameMap.DrawAsMap(ScreenManager.SpriteBatch, scale , mapFog);
            ScreenManager.GraphicsDevice.SetRenderTarget(null);
            //texLogo = content.Load<Texture2D>("paused");
            ScreenManager.Game.ResetElapsedTime();
        }


        public override void HandleInput(GameTime gameTime, InputState input)
        {
            PlayerIndex pi;

            if (input.IsMenuCancel(null, out pi) || input.IsNewKeyPress(Keys.Tab, null, out pi) || input.IsNewButtonPress(Buttons.Back, null, out pi))
                this.ExitScreen();

            base.HandleInput(gameTime, input);
        }

        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 0.3f);

            spriteBatch.Begin();

            spriteBatch.Draw(mapRT, (new Vector2(viewport.Width, viewport.Height) / 2) + new Vector2(0,viewport.Height * TransitionPosition), null, Color.White, 0f, new Vector2(mapRT.Width,mapRT.Height)/2,1f,SpriteEffects.None, 1);
            //spriteBatch.Draw(texBG, fullscreen,
            //                 Color.White * TransitionAlpha * 0.5f);

            //spriteBatch.Draw(texLogo, new Vector2(viewport.Width/2, viewport.Height/3), null,
              //               Color.White * TransitionAlpha, 0f, new Vector2(texLogo.Width/2, texLogo.Height/2), 1f + (TransitionPosition * 10f), SpriteEffects.None, 1);

            Vector2 topLeft = ((new Vector2(viewport.Width, viewport.Height) / 2) + new Vector2(0, viewport.Height * TransitionPosition)) - (new Vector2(mapRT.Width, mapRT.Height) / 2);
            foreach (AIDude e in EnemyController.Instance.Enemies.Where(en => en.Discovered && en.IsGeneral).ToList())
            {
                spriteBatch.Draw(mapIcons, topLeft + (e.Position * scale), new Rectangle(12, 0, 12, 13), Color.White, 0f, new Vector2(6, 6), 1f, SpriteEffects.None, 1);
            }

            spriteBatch.Draw(mapIcons, topLeft+(gameHero.Position * scale), new Rectangle(0, 0, 12, 13), Color.White, gameHero.Rotation - MathHelper.PiOver2, new Vector2(6, 6), 1f, SpriteEffects.None, 1);

            spriteBatch.DrawString(font, "Compounds Discovered: " + gameMap.Compounds.Count(c => c.Discovered) + "/" + gameMap.Compounds.Count, topLeft + new Vector2(0, -30) + new Vector2(1,1), Color.Black * 0.4f * TransitionAlpha);
            spriteBatch.DrawString(font, "Generals Eliminated: " + (3 - EnemyController.Instance.Enemies.Count(e => e.IsGeneral)) + "/" +(3), topLeft + new Vector2(0, -50) + new Vector2(1, 1), Color.Black * 0.4f * TransitionAlpha);
            spriteBatch.DrawString(font, "Compounds Discovered: " + gameMap.Compounds.Count(c => c.Discovered) + "/" + gameMap.Compounds.Count, topLeft + new Vector2(0, -30), Color.White * TransitionAlpha);
            spriteBatch.DrawString(font, "Generals Eliminated: " + (3 - EnemyController.Instance.Enemies.Count(e => e.IsGeneral)) + "/" + (3), topLeft + new Vector2(0, -50), Color.White * TransitionAlpha);

            spriteBatch.End();
        }


        #endregion
    }
}
