#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace Hunted
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    public class MainMenuScreen : MenuScreen
    {
        #region Initialization

        ContentManager content;

        float bgAlpha = 1f;

        double bgFadeTime = 0;

        Texture2D texLogo;
        SpriteFont gameFont;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("Main Menu", 0)
        {
            this.IsPopup = true;
            this.TransitionOnTime = TimeSpan.FromMilliseconds(2000);
            this.TransitionOffTime = TimeSpan.FromMilliseconds(1000);
        }

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Hunted.Content");

            // Create our menu entries.
            MenuEntry campaignGameMenuEntry = new MenuEntry("Let's Go");
            MenuEntry aboutGameMenuEntry = new MenuEntry("Rea");
            MenuEntry optionsMenuEntry = new MenuEntry("OPTIONS");
            MenuEntry exitMenuEntry = new MenuEntry("I'm Out");

            // Hook up menu event handlers.
            campaignGameMenuEntry.Selected += CampaignGameMenuEntrySelected;
            aboutGameMenuEntry.Selected += AboutGameMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            exitMenuEntry.Selected += ExitMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(campaignGameMenuEntry);
           // MenuEntries.Add(aboutGameMenuEntry);
            //MenuEntries.Add(optionsMenuEntry);
            if (!ScreenManager.IsPhone)
            {
                MenuEntries.Add(exitMenuEntry);
            }

            texLogo = content.Load<Texture2D>("title");
            gameFont = content.Load<SpriteFont>("gamefont");

            base.LoadContent();
        }


        #endregion

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            bgFadeTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (bgFadeTime > 1000)
                if (bgAlpha > 0f) bgAlpha -= 0.001f;

            if(IsExiting)
                if (bgAlpha > 0f) bgAlpha -= 0.05f;

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void CampaignGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ExitScreen();
        }

        void AboutGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            //ScreenManager.AddScreen(new AboutScreen(), e.PlayerIndex);
        }


        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            //ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }

        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void ExitMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }


        /// <summary>
        /// When the user cancels the main menu, we exit the game.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToBlack(bgAlpha);

            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(texLogo, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width/2, ScreenManager.GraphicsDevice.Viewport.Height/3) , null, Color.White * TransitionAlpha, 0f, new Vector2(texLogo.Width, texLogo.Height) / 2, 1f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.DrawString(gameFont, "One-level Prototype for #1GAM 2013", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 3) + new Vector2(0, 40), Color.White * TransitionAlpha, 0f, gameFont.MeasureString("One-level Prototype for #1GAM 2013") / 2, 1f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.DrawString(gameFont, "onegameamonth.com/garethiw", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 3) + new Vector2(0, 60), Color.White * TransitionAlpha, 0f, gameFont.MeasureString("onegameamonth.com/garethiw") / 2, 1f, SpriteEffects.None, 1);
            ScreenManager.SpriteBatch.DrawString(gameFont, "Controls: A/D - move left/right, Space - use GravPad, Left Mouse - SHOOT STUFF", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 3) + new Vector2(0, 85), Color.White * TransitionAlpha, 0f, gameFont.MeasureString("Controls: A/D - move left/right, Space - use GravPad, Left Mouse - SHOOT STUFF") / 2, 0.8f, SpriteEffects.None, 1);

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion
    }
}
