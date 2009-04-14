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

using System.Windows.Forms;

using Keys = Microsoft.Xna.Framework.Input.Keys;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace Paradux
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Paradux : Application
    {
        enum State
        {
            Initial,
            Intro,
            Playing,
            Fail,
            Rewind,
            Restart,
            Win
        }

        Duck[] ducks;
        int player = -1;

        string mapFileName;
        private State state;

        private double stateStart;

        private GameTime lastGameTime;

        public Paradux(string mapFileName)
        {
            this.mapFileName = mapFileName;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            if (mapFileName == null)
            {
                mapFileName = "Content\\default.dux";
            }
            if (!map.KarvoniteIn(mapFileName))
            {
                map.CreateEmptyMap(25, 25);
            }
            state = State.Initial;
        }

        protected void NewGame()
        {
            player = -1;
            if (ducks == null)
                ducks = new Duck[16];

            for (int i = 0; i < 16; i++)
            {
                Duck d = ducks[i];
                if (d == null)
                {
                    if (i < map.NumberOfStartPositions)
                    {
                        d = new Duck(i, map.GetStartPosition(i).position, map);
                        d.Reset();
                    }
                }
                else
                {
                    d.NewGameReset(map.GetStartPosition(i).position, map);
                }
            }

            NextDuck();
            ChangeState(State.Playing);
            camera.Reset();
            map.Reset();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        private void UpdateLoad(GameTime gametime)
        {
            if (Keyboard.GetState().IsKeyDown((Keys.L)))
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.AddExtension = true;
                openDialog.DefaultExt = "dux";
                DialogResult dr = openDialog.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    if (!map.KarvoniteIn(openDialog.FileName))
                    {
                        map.CreateEmptyMap(25, 25);
                    }
                    mapFileName = openDialog.FileName;
                    ChangeState(State.Initial);
                }
            }
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            lastGameTime = gameTime;
            UpdateLoad(gameTime);
            switch (state)
            {
                case State.Initial:
                    NewGame();
                    ChangeState(State.Intro);
                    break;
                case State.Intro:
                    ChangeState(State.Playing);
                    break;
                case State.Playing:
                    {
                        map.Update(gameTime);
                        foreach (Duck duck in ducks)
                        {
                            if (duck != null)
                            {
                                duck.Update(gameTime);

                                if (duck.sawAnotherDuck)
                                {
                                    ChangeState(State.Fail);
                                }
                            }
                        }
                        camera.Position = ducks[player].Position;
                        if (ducks[player].Finished)
                        {
                            foreach (Duck duck in ducks)
                            {
                                if (duck != null)
                                {
                                    duck.Reset();
                                }
                            }
                            NextDuck();
                        }
                    }
                    break;
                case State.Fail:
                    {
                        if (gameTime.TotalRealTime.TotalSeconds - stateStart < 2.0f)
                        {
                            camera.Zoom += new Vector2(0.01f, 0.02f);
                            camera.Rotation += 0.001f;
                        }
                        else if (gameTime.TotalRealTime.TotalSeconds - stateStart > 5.0f)
                        {
                            foreach (Duck duck in ducks)
                            {
                                if (duck != null)
                                {
                                    duck.StartRewinding();
                                }
                            }
                            camera.Reset();
                            ChangeState(State.Rewind);
                        }
                    }
                    break;
                case State.Rewind:
                    {
                        map.Update(gameTime);
                        bool bAllFinished = true;
                        foreach (Duck duck in ducks)
                        {
                            if (duck != null)
                            {
                                duck.Update(gameTime);
                                bAllFinished &= duck.FinishedRewinding;
                            }
                        }
                        camera.Position = ducks[player].Position;
                        if (bAllFinished)
                        {
                            foreach (Duck duck in ducks)
                            {
                                if (duck != null)
                                {
                                    duck.Reset();
                                }
                            }
                            map.Reset();
                            ducks[player].StartPlaying();
                            ChangeState(State.Playing);
                        }
                    }
                    break;
                case State.Restart:
                    {
                        NewGame();
                    }
                    break;
           }
            
            base.Update(gameTime);
        }

        private void ChangeState(State newState)
        {
            state = newState;
            if (lastGameTime != null)
                stateStart = lastGameTime.TotalRealTime.TotalSeconds;
        }

        private void NextDuck()
        {
            player++;
            ducks[player] = new Duck(player, map.GetStartPosition(player).position, map);
            ducks[player].visible = true;
            Duck.currentDuck = ducks[player];
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            switch (state)
            {
                case State.Playing:
                case State.Rewind:
                    DrawWorld(gameTime);
                    break;
                case State.Fail:
                    DrawWorld(gameTime);
                    DrawHud(gameTime);
                    break;
 
            }
            base.Draw(gameTime);
        }

        protected void DrawWorld(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None, camera.Transform);
            foreach (Duck duck in ducks)
            {
                if (duck != null)
                    duck.Draw(gameTime, spriteBatch);
            }
            map.Draw(gameTime, spriteBatch);
            spriteBatch.End();
        }

        protected void DrawHud(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            string output = "FAIL";
            // Find the center of the string
            Vector2 fontOrigin = font.MeasureString(output) / 2;
            // Draw the string
            Vector2 fontPos = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);
            spriteBatch.DrawString(font, output, fontPos, Color.LightGreen, 0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.End();
        }
    }
}
