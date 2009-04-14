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
    public class GameRules
    {
        protected static PhysicsSimulator physicsSimulator;

        protected GraphicsDeviceManager graphics;
        protected SpriteBatch spriteBatch;

        protected Map map;
        protected Camera camera;
        protected static SpriteFont font;

        public GraphicsDevice graphicsDevice;

        public GameRules(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public virtual void Init()
        {
			if (spriteBatch == null)
			{
				spriteBatch = new SpriteBatch(GraphicsDevice);
			}
            if (camera == null)
            {
                camera = new Camera(GraphicsDevice);
                camera.Reset();
            }
        }

        public virtual void Draw(GameTime gameTime)
        { }
        public virtual void Update(GameTime gameTime)
        {
            camera.Update(gameTime);
        }
        public virtual void Uninstall()
        { }
        public virtual void Install(AppFramework app, Map map)
        { 
            this.map = map;
        }
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }

        public static void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("Font");
        }
    }
}
