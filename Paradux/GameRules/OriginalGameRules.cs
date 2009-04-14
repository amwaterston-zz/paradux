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

namespace Paradux.GameRules
{
    class OriginalGameRules : GameRules
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

        private State state;

        private double stateStart;
        
        private GameTime lastGameTime;

        public OriginalGameRules(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {}

		public override void Install(AppFramework app, Map map)
        {
			app.IsMouseVisible = false;
			base.Install(app, map);
            Init();
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

        public override void Init()
        {
            base.Init();
            state = State.Initial;
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

        public override void Draw(GameTime gameTime)
        {

            switch (state)
            {
                case State.Playing:
                case State.Rewind:
                    DrawWorld(gameTime, spriteBatch);
                    break;
                case State.Fail:
                    DrawWorld(gameTime, spriteBatch);
                    DrawHud(gameTime, spriteBatch);
                    break;
            }

            base.Update(gameTime);
        }

        protected void DrawWorld(GameTime gameTime, SpriteBatch spriteBatch)
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

        protected void DrawHud(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
            string output = "FAIL";
            // Find the center of the string
            Vector2 fontOrigin = font.MeasureString(output) / 2;
            // Draw the string
            Vector2 fontPos = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            spriteBatch.DrawString(font, output, fontPos, Color.LightGreen, 0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            lastGameTime = gameTime;
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
    }
}
