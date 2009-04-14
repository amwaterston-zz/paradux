using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Paradux.GameRules;

using System.Windows.Forms;

using Keys = Microsoft.Xna.Framework.Input.Keys;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace Paradux
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class AppFramework : Microsoft.Xna.Framework.Game
    {
        protected static GraphicsDeviceManager graphics;
        protected static PhysicsSimulator physicsSimulator;
        protected string mapFileName;
        protected Map map;


        GameRules.GameRules currentGameRules = null;
		KeyboardState previousKeyboardState;

        public AppFramework()
        {
            map = new Map();
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            PhysicsSimulator = new PhysicsSimulator(new Vector2(0, 0));
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
			previousKeyboardState = Keyboard.GetState();
            if (mapFileName == null)
            {
                mapFileName = "Content\\default.dux";
            }
            if (!map.KarvoniteIn(mapFileName))
            {
                map.CreateEmptyMap(25, 25);
            }

            currentGameRules = new GameRules.OriginalGameRules(GraphicsDevice);
            currentGameRules.Install(this, map);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Duck.LoadContent(Content);
            Map.LoadContent(Content);
            GameRules.GameRules.LoadContent(Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        protected void SwitchGameRules()
        {
			if (currentGameRules is OriginalGameRules)
			{
				currentGameRules = new EditorGameRules(GraphicsDevice);
			}
			else
			{
				currentGameRules = new OriginalGameRules(GraphicsDevice);
			}

			currentGameRules.Install(this, map);
        }

		private void UpdateLoad()
		{
			OpenFileDialog openDialog = new OpenFileDialog();
			openDialog.AddExtension = true;
			openDialog.DefaultExt = "dux";
			DialogResult dr = openDialog.ShowDialog();
			if (dr == DialogResult.OK)
			{
				if (!map.KarvoniteIn(openDialog.FileName))
				{
					map.CreateEmptyMap(32, 32);
				}
				mapFileName = openDialog.FileName;
				currentGameRules.Install(this, map);
			}
		}

		private void UpdateSave()
		{
			SaveFileDialog saveDialog = new SaveFileDialog();
			saveDialog.AddExtension = true;
			saveDialog.DefaultExt = "dux";
			saveDialog.FileName = mapFileName;
			DialogResult dr = saveDialog.ShowDialog();
			if (dr == DialogResult.OK)
			{
				map.KarvoniteOut(saveDialog.FileName);
			}
		}

		private void CreateNewMap()
		{
			map.CreateEmptyMap(32, 32);
		}

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
			if (Keyboard.GetState().IsKeyDown(Keys.F10) && !previousKeyboardState.IsKeyDown(Keys.F10))
                SwitchGameRules();

			if (Keyboard.GetState().IsKeyDown(Keys.S) && !previousKeyboardState.IsKeyDown(Keys.S))
				UpdateSave();

			if (Keyboard.GetState().IsKeyDown(Keys.L) && !previousKeyboardState.IsKeyDown(Keys.L))
				UpdateLoad();

			if (Keyboard.GetState().IsKeyDown(Keys.N) && !previousKeyboardState.IsKeyDown(Keys.N))
				CreateNewMap();

            if (currentGameRules != null)
            {
                currentGameRules.Update(gameTime);
            }

            PhysicsSimulator.Update(gameTime.ElapsedGameTime.Milliseconds * .001f);

            base.Update(gameTime);
			previousKeyboardState = Keyboard.GetState();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            if (currentGameRules != null)
            {
                currentGameRules.Draw(gameTime);
            }
            base.Draw(gameTime);
        }

        public static PhysicsSimulator PhysicsSimulator
        {
            get { return physicsSimulator; }
            set { physicsSimulator = value; }
        }

        public static GraphicsDeviceManager Graphics
        {
            get { return graphics; }
        }
    }
}
