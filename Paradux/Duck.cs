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

namespace Paradux
{
    public class Duck
    {
        struct ReplayData
        {
            public Vector2 position;
            public int duckFacing;
        }

        ReplayData[] replayData;

        enum ReplayDirection
        {
            Forwards,
            Backwards
        }

        enum State
        {
            Playing,
            Finished,
            Replaying,
            FinishedReplaying,
            Rewinding,
            FinishedRewinding
        }
        private State state;

        

        // This is a texture we can render.
        static Texture2D[] duckTextures = new Texture2D[4];

        public static Duck currentDuck = null;
        
        // Set the coordinates to draw the duck at.
        //Vector2 spritePosition = Vector2.Zero;
        Vector2 lastMovement = Vector2.Zero;
        int duckFacing = 0;
        float duckSpeed = 2.0f;
        Body body;
        Geom geom;
        int player;

        public bool visible;

        public bool sawAnotherDuck;
        int frame;
        int lastFrame;
        Vector2 startPosition;
        Map map;


        static int viewDistance = 5;

        public Duck(int player, Vector2 position, Map map)
        {
            this.player = player;
            body = BodyFactory.Instance.CreateCircleBody(AppFramework.PhysicsSimulator, 14, 1);
            geom = GeomFactory.Instance.CreateCircleGeom(AppFramework.PhysicsSimulator, body, 14, 20);
            geom.Tag = this;
            geom.OnCollision += new Geom.CollisionEventHandler(OnCollision);
            geom.CollisionGroup = 1 + player;
            frame = 0;
            this.Position = position;
            this.startPosition = position;
            this.map = map;
            sawAnotherDuck = false;
            state = State.Playing;

            replayData = new ReplayData[10000];

            visible = false;
        }

        ~Duck()
        {
            System.Console.WriteLine("Destroying duck " + player + ".");
            body = null;
            geom = null;
        }

        private bool OnCollision(Geom geom1, Geom geom2, ContactList contactList)
        {
            Geom other = null;
            if (geom1 == geom)
            {
                other = geom2;
            }
            else if (geom2 == geom)
            {
                other = geom1;
            }
            
            if (other.Tag is Tile)
            {
                return CollideWithTile((Tile)other.Tag);
            }
            else if (other.Tag is Duck)
            {
                return CollideWithDuck((Duck)other.Tag);
            }
            else
            {
                return true;
            }
        }

        private bool CollideWithTile(Tile tile)
        {
            switch (tile.type)
            {
                case TileType.Filled:
                    return true;
                case TileType.Empty:
                case TileType.Start:
                    return false;
                case TileType.End:
                    if (tile.player == player && state == State.Playing)
                        state = State.Finished;
                    return false;
                default:
                    return true;
            }
        }

        private bool CollideWithDuck(Duck duck)
        {
            if (state == State.Playing)
            {
                sawAnotherDuck = true;
            }
            return false;
        }

        public void NewGameReset(Vector2 position, Map map)
        {
            this.Position = position;
            this.startPosition = position;
            this.map = map;
            sawAnotherDuck = false;
            state = State.Playing;
            lastFrame = 0;
            visible = false;
            Reset();
        }

        public void Reset()
        {
            frame = 0;
            body.Position = startPosition;
            geom.CollisionEnabled = true;
            state = State.Replaying;
            sawAnotherDuck = false;
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

            duckTextures[0] = contentManager.Load<Texture2D>("BACK");
            duckTextures[1] = contentManager.Load<Texture2D>("FRONT");
            duckTextures[2] = contentManager.Load<Texture2D>("RIGHT");
            duckTextures[3] = contentManager.Load<Texture2D>("LEFT");
        }

        public static void UnloadContent()
        {
            for (int i = 0; i < 3; i++)
            {
                duckTextures[i] = null;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!visible)
                return;

            SpriteEffects flip = SpriteEffects.None;

            Vector2 offset = new Vector2(duckTextures[duckFacing].Width / 2.0f, duckTextures[duckFacing].Height - 14.0f);

            spriteBatch.Draw(duckTextures[duckFacing], body.Position, null, Color.White, 0, offset, 1.0f, flip, 0);
        }

        private void SetLastMovementDirectionFromKeys()
        {
            lastMovement = Vector2.Zero;
            if (Keyboard.GetState().IsKeyDown((Keys.Up)))
                lastMovement.Y -= 1.0f;
            if (Keyboard.GetState().IsKeyDown((Keys.Down)))
                lastMovement.Y += 1.0f;
            if (Keyboard.GetState().IsKeyDown((Keys.Right)))
                lastMovement.X += 1.0f;
            if (Keyboard.GetState().IsKeyDown((Keys.Left)))
                lastMovement.X -= 1.0f;

            if (lastMovement.LengthSquared() > 0.0f)
                lastMovement.Normalize();
        }

        private void SetFacingDirectionFromLastMovement()
        {
            if (lastMovement.Y > 0)
                duckFacing = 1;
            else if (lastMovement.Y < 0)
                duckFacing = 0;
            else if (lastMovement.X > 0)
                duckFacing = 2;
            else if (lastMovement.X < 0)
                duckFacing = 3;
        }

        private void UpdatePlayerMovement(GameTime gameTime)
        {
            Tile currentTile = map.GetTileForPosition(body.Position);
            currentTile.duck = null;

            SetLastMovementDirectionFromKeys();
            SetFacingDirectionFromLastMovement();

            Vector2 newPos = body.Position + (lastMovement * duckSpeed);
            float height = map.height * Tile.Height;
            float width = map.width * Tile.Width;
            if (!(newPos.X > width || newPos.X < 0.0f || newPos.Y > height || newPos.Y < 0.0f))
            {
                body.Position = newPos;
            }
            replayData[frame].position = body.Position;
            replayData[frame].duckFacing = duckFacing;
            frame += 1;
            lastFrame = frame;

            currentTile = map.GetTileForPosition(body.Position);
            currentTile.Duck(this);
        }

        private void Replay(GameTime gameTime, ReplayDirection direction, int step)
        {
            switch (direction)
            {
                case ReplayDirection.Forwards:
                    if (frame < lastFrame)
                    {
                        body.Position = replayData[frame].position;
                        duckFacing = replayData[frame].duckFacing;
                        frame += step;
                    }
                    else
                    {
                        frame = lastFrame;
                        geom.CollisionEnabled = false;
                        state = State.FinishedReplaying;
                    }
                    break;
                case ReplayDirection.Backwards:
                    if (frame >= 0)
                    {
                        body.Position = replayData[frame].position;
                        duckFacing = replayData[frame].duckFacing;
                        frame -= step;
                    }
                    else
                    {
                        frame = 0;
                        geom.CollisionEnabled = false;
                        state = State.FinishedRewinding;
                    }
                    break;
            }
        }

        private void MoveToCenterOfEndTile(GameTime gameTime)
        {

        }

        public void Update(GameTime gameTime)
        {
            if (!visible)
                return;

            switch (state)
            {
                case State.Playing:
                    UpdatePlayerMovement(gameTime);
                    break;
                case State.Finished:
                    MoveToCenterOfEndTile(gameTime);
                    break;
                case State.Replaying:
                    Replay(gameTime, ReplayDirection.Forwards, 1);
                    CheckDuckVision();
                    break;
                case State.FinishedReplaying:
                    replayData[frame].position = body.Position;
                    replayData[frame].duckFacing = duckFacing;
                    frame += 1;
                    lastFrame = frame;
                    break;
                case State.Rewinding:
                    Replay(gameTime, ReplayDirection.Backwards, 2);
                    break;
            }
        }

        public void StartRewinding()
        {
            state = State.Rewinding;
        }

        public void StartPlaying()
        {
            state = State.Playing;
        }

        public void StartReplaying()
        {
            state = State.Replaying;
        }

        private void CheckDuckVision()
        {
            Tile currentTile = map.GetTileForPosition(body.Position);
            int x = currentTile.x;
            int y = currentTile.y;

            int dX = 0, dY = 0;
            switch (duckFacing)
            {
                case 0:
                    dX = 0; dY = -1;
                    break;
                case 1:
                    dX = 0; dY = 1;
                    break;
                case 2:
                    dX = 1; dY = 0;
                    break;
                case 3:
                    dX = -1; dY = 0;
                    break;
            }

            for (int i = 0; i < viewDistance; i++)
            {
                x += dX;
                y += dY;
                Tile next = map.GetTileAt(x, y);
                if (next == null)
                {
                    break;
                }
                if (next.type == TileType.Filled)
                {
                    break;
                }
                else if (next.type == TileType.Empty)
                {
                    next.Seen();
                    if (next.duck != null)
                    {
                        if (next.duck.player == currentDuck.player)
                        {
                            //seen an earlier duck. It's all over!
                            sawAnotherDuck = true;
                        }
                    }
                }
            }
        }

        public Vector2 Position
        {
            get { return body.Position; }
            set { body.Position = value; }
        }

        public bool Finished
        {
            get { return state == State.Finished; }
        }

        public bool FinishedReplaying
        {
            get { return state == State.FinishedReplaying;  }
        }

        public bool FinishedRewinding
        {
            get { return state == State.FinishedRewinding; }
        }
    }
}
