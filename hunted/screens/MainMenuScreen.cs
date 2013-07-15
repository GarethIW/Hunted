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
            MenuEntry continueMenuEntry = new MenuEntry("Continue");
            MenuEntry newMenuEntry = new MenuEntry("New Mission");
            //MenuEntry optionsMenuEntry = new MenuEntry("");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            continueMenuEntry.Selected += ContinueMenuEntrySelected;
            newMenuEntry.Selected += NewMenuEntrySelected;
            //optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            exitMenuEntry.Selected += ExitMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(continueMenuEntry);
            MenuEntries.Add(newMenuEntry);
            //MenuEntries.Add(optionsMenuEntry);
            if (!ScreenManager.IsPhone)
            {
                MenuEntries.Add(exitMenuEntry);
            }

            //texLogo = content.Load<Texture2D>("title");
            //gameFont = content.Load<SpriteFont>("gamefont");

            base.LoadContent();
        }


        #endregion

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
           

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void ContinueMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ExitScreen();
        }

        void NewMenuEntrySelected(object sender, PlayerIndexEventArgs e)
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

           

            base.Draw(gameTime);
        }


        #endregion
    }
}
