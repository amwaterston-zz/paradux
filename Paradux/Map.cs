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
using MP.Karvonite;

namespace Paradux
{
    public class Map
    {
        public int width;
        public int height;
        public Tile[] map;
        private Dictionary<int, Tile> startPositions;
        private Dictionary<int, Tile> endPositions;

        public Map()
        {
            startPositions = new Dictionary<int, Tile>();
            endPositions = new Dictionary<int, Tile>();
        }

        public void Reset()
        {
            Tile.Reset();
        }

        public void LoadMap(Map newMap)
        {
            width = newMap.width;
            height = newMap.height;

            startPositions = new Dictionary<int, Tile>();
            map = new Tile[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[y * width + x] = new Tile(newMap.map[y * width + x].type, newMap.map[y * width + x].position, newMap.map[y * width + x].size, newMap.map[y * width + x].player, x, y);
                    if (map[y * width + x].type == TileType.Start)
                    {
                        if (!startPositions.ContainsKey(map[y * width + x].player))
                        {
                            startPositions[map[y * width + x].player] = map[y * width + x];
                        }
                    }
                    if (map[y * width + x].type == TileType.End)
                    {
                        if (!endPositions.ContainsKey(map[y * width + x].player))
                        {
                            endPositions[map[y * width + x].player] = map[y * width + x];
                        }
                    }
                    map[y * width + x].InitialisePhysics();
                }
            }
        }

        public void KarvoniteOut(string filename)
        {
            ObjectSpace objectSpace = new ObjectSpace("MapModel.kvtmodel,Paradux", filename);
            objectSpace.CreateObjectLibrary(true); 
            // Open ObjectSpace (empty up to this point) 
            objectSpace.Open(); 
            objectSpace.Add(this); 
            // Ensures changes 
            objectSpace.Save();
            objectSpace.Close();
        }

        public bool KarvoniteIn(string filename)
        {
            AppFramework.PhysicsSimulator.Clear();
            ObjectSpace objectSpace = new ObjectSpace("MapModel.kvtmodel,Paradux", filename);
            Map newMap = null;

            try
            {
                objectSpace.Open();
                newMap = objectSpace.GetFirstObject<Map>();
            }
            catch (System.Exception)
            {
                return false;
            }
            finally
            {
                objectSpace.Close();
            }

            if (newMap == null)
                return false;

            LoadMap(newMap);
            
            
            return true;
        }

        public void CreateEmptyMap(int width, int height)
        {
            AppFramework.PhysicsSimulator.Clear();
            this.width = width;
            this.height = height;
            map = new Tile[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 size = new Vector2(Tile.Width, Tile.Height);
                    Vector2 position = new Vector2(x * size.X, y * size.Y);
                    map[y * width + x] = new Tile(0, position, size, 0, x, y);
                    if (y * width + x < 16)
                    {
                        startPositions[y * width + x] = map[y * width + x];
                    }
                    else if (y * width + x > 32)
                    {
                        endPositions[y * width + x - 16] = map[y * width + x];
                    }
                }
            }
        }

        public Tile GetTileAt(int x, int y)
        {
            if (y >= height || x >= width || x < 0 || y < 0)
                return null;

            return map[y * width + x];
        }

        public Tile GetTileForPosition(Vector2 position)
        {
            int x = (int)((position.X + (Tile.Width / 2.0f)) / Tile.Width);
            int y = (int)((position.Y + (Tile.Width / 2.0f)) / Tile.Height);
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return map[y * width + x];
            }
            return null;
        }

        public Tile GetStartPosition(int i)
        {
            if (startPositions.ContainsKey(i))
            {
                return startPositions[i];
            }
            return null;
        }

        public Tile GetEndPosition(int i)
        {
            if (endPositions.ContainsKey(i))
            {
                return endPositions[i];
            }
            return null;
        }

        public int NumberOfStartPositions
        {
            get { return startPositions.Count; }
        }

        /// <summary>
        /// Load all of the static graphics content for this class.
        /// </summary>
        /// <param name="contentManager">The content manager to load with.</param>
        public static void LoadContent(ContentManager contentManager)
        {
            Tile.LoadContent(contentManager);
        }

        public static void UnloadContent()
        {
            Tile.UnloadContent();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[y * width + x].Draw(gameTime, spriteBatch);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (Tile tile in map)
            {
                tile.seen = false;
            }
        }
    }
}
