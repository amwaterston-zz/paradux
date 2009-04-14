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
    public class DuxEd : Application
    {
        private bool mouse_not_down_last_frame;
        private Tile tile_changed_last_frame;
        private MouseState previousMouseState;
        private string mapFileName;

        public DuxEd(string mapFileName)
        {
            mouse_not_down_last_frame = true;
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
            IsMouseVisible = true;
            base.Initialize();
            camera.Position = new Vector2(300, 200);

            if (mapFileName == null || !map.KarvoniteIn(mapFileName))
            {
                map.CreateEmptyMap(32, 32);
            }
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

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateCamera(gameTime);
            UpdateClick(gameTime);
            UpdateSave(gameTime);
            UpdateLoad(gameTime);
        }

        private void UpdateSave(GameTime gametime)
        {
            if (Keyboard.GetState().IsKeyDown((Keys.S)))
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
                }
            }
        }

        private void UpdateClick(GameTime gameTime)
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Vector2 position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                Tile tile = map.GetTileForPosition(position + camera.TopLeft);
                if (tile != tile_changed_last_frame)
                {
                    mouse_not_down_last_frame = true;
                }
                if (mouse_not_down_last_frame && tile != null)
                {
                    tile.type += 1;
                    if (tile.type == TileType.NumberOfTypes)
                    {
                        tile.type = TileType.Empty;
                    }
                    tile_changed_last_frame = tile;
                }
                mouse_not_down_last_frame = false;
            }
            else if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                Vector2 position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                Tile tile = map.GetTileForPosition(position + camera.TopLeft);
                if (tile != tile_changed_last_frame)
                {
                    mouse_not_down_last_frame = true;
                }
                if (mouse_not_down_last_frame && tile != null)
                {
                    if (tile.type != TileType.End && tile.type != TileType.Start)
                    {
                        tile.type = TileType.Start;
                        tile.player = 0;
                    }
                    else
                    {
                        tile.player += 1;
                        tile.player %= 16;
                    }
                    tile_changed_last_frame = tile;
                }
                mouse_not_down_last_frame = false;
            }
            else
            {
                mouse_not_down_last_frame = true;
                tile_changed_last_frame = null;
            }

            previousMouseState = Mouse.GetState();
        }

        private void UpdateCamera(GameTime gameTime)
        {
            Vector2 lastMovement = Vector2.Zero;
            float cameraSpeed = 2.0f;
            if (Keyboard.GetState().IsKeyDown((Keys.Up)))
            {
                lastMovement.Y -= 1.0f;
            }
            if (Keyboard.GetState().IsKeyDown((Keys.Down)))
            {
                lastMovement.Y += 1.0f;
            }
            if (Keyboard.GetState().IsKeyDown((Keys.Right)))
            {
                lastMovement.X += 1.0f;
            }
            if (Keyboard.GetState().IsKeyDown((Keys.Left)))
            {
                lastMovement.X -= 1.0f;
            }
            if (lastMovement.LengthSquared() > 0.0f)
                lastMovement.Normalize();

            camera.Position += lastMovement * cameraSpeed;
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the sprite.
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None, camera.Transform);
            map.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
