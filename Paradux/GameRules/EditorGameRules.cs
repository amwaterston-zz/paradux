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
    class EditorGameRules : GameRules
    {
		private bool mouse_not_down_last_frame;
		private Tile tile_changed_last_frame;
		private MouseState previousMouseState;

        public EditorGameRules(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

		public override void Install(AppFramework app, Map map)
        {
			app.IsMouseVisible = true;
			base.Install(app, map);
			Init();
        }

        public override void Init()
        {
			base.Init();
			camera.Position = new Vector2(300, 200);
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
					tile.InitialisePhysics();
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
					tile.InitialisePhysics();
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
		public override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// Draw the sprite.
			spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None, camera.Transform);
			map.Draw(gameTime, spriteBatch);
			spriteBatch.End();

			base.Draw(gameTime);
		}

        public override void Update(GameTime gameTime)
        {
			UpdateCamera(gameTime);
			UpdateClick(gameTime);
			base.Update(gameTime);
        }
    }
}
