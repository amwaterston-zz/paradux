using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paradux.Maths
{
    public struct RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public float Left { get { return X; } }
        public float Top { get { return Y; } }
        public float Right { get { return X + Width; } }
        public float Bottom { get { return Y + Height; } }

        #region Constructors

        public RectangleF(Vector2 loc, Vector2 size)
        {
            this.X = loc.X;
            this.Y = loc.Y;
            this.Width = size.X;
            this.Height = size.Y;
        }

        public RectangleF(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        #endregion

        #region Intersect Methods

        public bool Intersects(Rectangle r)
        {
            return !((r.Left > this.Right) || (r.Right < this.Left) || (r.Top > this.Bottom) || (r.Bottom < this.Top));
        }

        public bool Intersects(RectangleF r)
        {
            return !((r.Left > this.Right) || (r.Right < this.Left) || (r.Top > this.Bottom) || (r.Bottom < this.Top));
        }

        #endregion

        #region Contains Methods

        public bool Contains(Point p)
        {
            return !((p.X < this.Left) || (p.Y < this.Top) || (p.X > this.Right) || (p.Y > this.Bottom));
        }

        public bool Contains(Vector2 p)
        {
            return !((p.X < this.Left) || (p.Y < this.Top) || (p.X > this.Right) || (p.Y > this.Bottom));
        }

        public bool Contains(Rectangle r)
        {
            return !((r.Left < this.Left) || (r.Top < this.Top) || (r.Bottom > this.Bottom) || (r.Right > this.Right));
        }

        public bool Contains(RectangleF r)
        {
            return !((r.Left < this.Left) || (r.Top < this.Top) || (r.Bottom > this.Bottom) || (r.Right > this.Right));
        }

        #endregion

        #region Transformation Methods

        public void ResizeToRotation(float rads)
        {
            Vector2 center;
            center.X = this.X + this.Width / 2;
            center.Y = this.Y + this.Height / 2;

            Vector2 topLeft = new Vector2(this.X - center.X, this.Y - center.Y);
            Vector2 topRight = new Vector2(this.Right - center.X, topLeft.Y);
            Vector2 bottomRight = new Vector2(topRight.X, this.Bottom - center.Y);
            Vector2 bottomLeft = new Vector2(topLeft.X, bottomRight.Y);

            double cos = Math.Cos(rads);
            double sin = Math.Sin(rads);

            topLeft = new Vector2((float)(cos * topLeft.X - sin * topLeft.Y), (float)(cos * topLeft.Y + sin * topLeft.X));

            topRight = new Vector2((float)(cos * topRight.X - sin * topRight.Y), (float)(cos * topRight.Y + sin * topRight.X));

            bottomRight = new Vector2((float)(cos * bottomRight.X - sin * bottomRight.Y), (float)(cos * bottomRight.Y + sin * bottomRight.X));

            bottomLeft = new Vector2((float)(cos * bottomLeft.X - sin * bottomLeft.Y), (float)(cos * bottomLeft.Y + sin * bottomLeft.X));

            float minX = Math.Min(topLeft.X, Math.Min(topRight.X, Math.Min(bottomRight.X, bottomLeft.X)));
            float maxX = Math.Max(topLeft.X, Math.Max(topRight.X, Math.Max(bottomRight.X, bottomLeft.X)));

            float minY = Math.Min(topLeft.Y, Math.Min(topRight.Y, Math.Min(bottomRight.Y, bottomLeft.Y)));
            float maxY = Math.Max(topLeft.Y, Math.Max(topRight.Y, Math.Max(bottomRight.Y, bottomLeft.Y)));

            this.X = minX + center.X;
            this.Y = minY + center.Y;
            this.Width = maxX - minX;
            this.Height = maxY - minY;
            
        }

        #region Inflate Methods

        public void Inflate(float amount)
        {
            this.Inflate(amount, amount);
        }

        public void Inflate(Vector2 amount)
        {
            this.Inflate(amount.X, amount.Y);
        }

        public void Inflate (float xAmount, float yAmount)
        {
            this.X -= xAmount * 0.5f;
            this.Y -= yAmount * 0.5f;
            this.Width += xAmount;
            this.Height += yAmount;
        }

        #endregion

        #region Deflate Methods

        public void Deflate(float amount)
        {
            this.Deflate(amount, amount);
        }

        public void Deflate(Vector2 amount)
        {
            this.Deflate(amount.X, amount.Y);
        }

        public void Deflate(float xAmount, float yAmount)
        {
            this.X += xAmount * 0.5f;
            this.Y += yAmount * 0.5f;
            this.Width -= xAmount;
            this.Height -= yAmount;
        }

        #endregion

        #endregion
    }
}
