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

namespace Paradux
{
    public class Camera
    {
        #region Members
        private Vector2 position;
        private Vector2 zoom;
        private Vector2 offset;
        private float rotation;
        private Matrix matrix;
        private GraphicsDevice graphicsDevice;
        private bool transformNeedsUpdating;
        private Maths.RectangleF limits;
        private bool useLimits;
        #endregion

        #region Constructor
        public Camera(GraphicsDevice graphicsDevice)
        {
            position = new Vector2(0, 0);
            matrix = new Matrix();
            offset = Vector2.Zero;
            rotation = 0.0f;
            zoom = Vector2.One;
            this.graphicsDevice = graphicsDevice;
            useLimits = false;
            UpdateTransform();
        }
        #endregion

        public void UpdateTransform()
        {
            Vector3 matrixRotOrigin = new Vector3(position + offset, 0);
            matrix =
                Matrix.CreateTranslation(-matrixRotOrigin) *
                Matrix.CreateScale(zoom.X, zoom.Y, 1.0f) *
                Matrix.CreateRotationZ(rotation) *
                Matrix.CreateTranslation(new Vector3((graphicsDevice.Viewport.Width / 2),(graphicsDevice.Viewport.Height / 2), 0.0f));
            transformNeedsUpdating = false;
        }

        public void Reset()
        {
            zoom = Vector2.One;
            rotation = 0.0f;
            position = Vector2.Zero;
        }

        public void Update(GameTime gameTime)
        {
            if (transformNeedsUpdating)
            {
                UpdateTransform();
            }
            if (useLimits && !Limits.Contains(Bounds))
            {
                if (Limits.Left > Bounds.Left)
                {
                    position.X += (Limits.Left - Bounds.Left);
                }
                if (Limits.Right < Bounds.Right)
                {
                    position.X -= (Bounds.Right - Limits.Right);
                }
                if (Limits.Top > Bounds.Top)
                {
                    position.Y += (Limits.Top - Bounds.Top);
                }
                if (Limits.Bottom < Bounds.Bottom)
                {
                    position.Y -= (Bounds.Bottom - Limits.Bottom);
                }
                UpdateTransform();
            }
        }

        #region Accessors
        public Matrix Transform
        {
            get { return matrix; }
        }

        public Maths.RectangleF Bounds
        {
            get
            {
                float zoomedWidth = (graphicsDevice.Viewport.Width) / zoom.X;
                float zoomedHeight = (graphicsDevice.Viewport.Height) / zoom.Y;
                return new Maths.RectangleF(position.X - (zoomedWidth / 2.0f), position.Y - (zoomedHeight / 2.0f), zoomedWidth, zoomedHeight);
            }
        }

        public Maths.RectangleF Limits
        {
            get { return limits; }
            set { limits = value; }
        }

        public bool UseLimits
        {
            get { return useLimits; }
            set
            {
                useLimits = value;
                transformNeedsUpdating = true;
            }
        }

        public Vector2 Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                transformNeedsUpdating = true;
            }
        }

        public float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                transformNeedsUpdating = true;
            }
        }

        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                transformNeedsUpdating = true;
            }
        }

        public Vector2 TopLeft
        {
            get { return new Vector2(Bounds.Left, Bounds.Top); }
        }
        #endregion
    }
}
