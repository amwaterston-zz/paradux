using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using Paradux.Maths;

namespace Paradux
{
    public enum TileType
    {
        Empty,
        Filled,
        Start,
        End,
        NumberOfTypes
    }

    public class Tile
    {
        //public int type;
        public Vector2 position;
        public Vector2 size;
        public TileType type;
        public int player;
        public int x, y;
        public bool seen;
        public Duck duck;

        private Geom geom;
        private Body body;

        private static SpriteFont font;
        private static Texture2D[] mapTextures = new Texture2D[4];
        private static List<Tile> seenTiles = new List<Tile>();

        public Tile()
        {}

        public Tile(TileType type, Vector2 position, Vector2 size, int player, int x, int y)
        {
            this.position = position;
            this.size = size;
            this.type = type;
            this.player = player;
            this.x = x;
            this.y = y;
            seen = false;
            duck = null;
            InitialisePhysics();
        }

        public static void Reset()
        {
            foreach (Tile t in seenTiles)
            {
                t.seen = false;
                t.duck = null;
            }
            seenTiles.Clear();
        }

        public void InitialisePhysics()
        {
			if (geom != null)
			{
				AppFramework.PhysicsSimulator.Remove(geom);
				geom = null;
			}
			if (body != null)
			{
				AppFramework.PhysicsSimulator.Remove(body);
				body = null;
			}

            if (type == TileType.Filled)
            {
                body = BodyFactory.Instance.CreateRectangleBody(AppFramework.PhysicsSimulator, size.X, size.Y, 1);
                body.IsStatic = true;
                geom = GeomFactory.Instance.CreateRectangleGeom(AppFramework.PhysicsSimulator, body, size.X, size.Y, 20);
                body.Position = position;
                geom.CollisionGroup = 100;
            }
			else if (type == TileType.End)
			{
				body = BodyFactory.Instance.CreateRectangleBody(AppFramework.PhysicsSimulator, size.X / 4.0f, size.Y / 4.0f, 1);
				body.IsStatic = true;
				geom = GeomFactory.Instance.CreateRectangleGeom(AppFramework.PhysicsSimulator, body, size.X / 4.0f, size.Y / 4.0f, 20);
				body.Position = position;
				geom.CollisionResponseEnabled = false;
				geom.Tag = this;
				geom.CollisionGroup = 100;
			}
        }

        /// <summary>
        /// Load all of the static graphics content for this class.
        /// </summary>
        /// <param name="contentManager">The content manager to load with.</param>
        public static void LoadContent(ContentManager contentManager)
        {
            // safety-check the parameters
            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }
            mapTextures[0] = contentManager.Load<Texture2D>("white");
            mapTextures[1] = contentManager.Load<Texture2D>("black");
            mapTextures[2] = contentManager.Load<Texture2D>("start");
            mapTextures[3] = contentManager.Load<Texture2D>("end");

            font = contentManager.Load<SpriteFont>("TileFont");
        }

        public static void UnloadContent()
        {
            for (int i = 0; i < mapTextures.Length; i++)
            {
                mapTextures[i] = null;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            int tile_to_draw = 0;
            Color colour_of_tile = Color.White;

            switch (type)
            {
                case TileType.Start:
                    {
                        colour_of_tile = Color.AliceBlue;
                        tile_to_draw = 0;
                    }
                    break;
                case TileType.End:
                    {
                        colour_of_tile = Color.Firebrick;
                        tile_to_draw = 0;
                    }
                    break;
                case TileType.Empty:
                    if (seen)
                        colour_of_tile = Color.Purple;

                    tile_to_draw = 0;
                    break;
                case TileType.Filled:
                    tile_to_draw = 1;
                    break;

            }
            Vector2 offset = size / 2.0f;
            spriteBatch.Draw(mapTextures[tile_to_draw], position, null, colour_of_tile, 0, offset, 1.0f, SpriteEffects.None, 1);
            if (type == TileType.End || type == TileType.Start)
            {
                string output = "" + player;
                Vector2 origin = font.MeasureString(output) / 2.0f;
                spriteBatch.DrawString(font, output, position, Color.Black, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            }
        }

        public RectangleF Bounds
        {
            get { return new RectangleF(position - (size / 2.0f), size); }
        }

        public static int Height
        {
            get { return mapTextures[0].Height; }
        }
        public static int Width
        {
            get { return mapTextures[0].Width; }
        }

        public void Seen()
        {
            seenTiles.Add(this);
            seen = true;
        }

        public void Duck(Duck duck)
        {
            seenTiles.Add(this);
            this.duck = duck;
        }
    }
}
