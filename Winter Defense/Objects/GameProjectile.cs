using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using System;
using Winter_Defense.Managers;

namespace Winter_Defense.Objects
{
    //--------------------------------------------------
    // Projectile Subject

    public enum ProjectileSubject
    {
        FromPlayer,
        FromEnemy
    }

    public class GameProjectile
    {
        //--------------------------------------------------
        // Gravity

        protected const float GravityAcceleration = 40.0f;

        //--------------------------------------------------
        // Sprite

        private Sprite _sprite;
        public Sprite Sprite => _sprite;

        //--------------------------------------------------
        // Position

        private Vector2 _position;
        public Vector2 Position => _position;
        public Vector2 LastPosition { get; private set; }

        //--------------------------------------------------
        // Distance Traveled

        private float _distanceTraveledX;
        private const float MaxDistanceTraveled = 96.0f;

        //--------------------------------------------------
        // Acceleration

        private Vector2 _acceleration;
        public Vector2 Acceleration
        {
            get { return _acceleration; }
            set
            {
                _acceleration = value;
            }
        }

        //--------------------------------------------------
        // Subject

        private ProjectileSubject _subject;
        public ProjectileSubject Subject
        {
            get { return _subject; }
            set
            {
                _subject = value;
            }
        }

        //--------------------------------------------------
        // Damage

        private int _damage;
        public int Damage { get { return _damage; } }

        //--------------------------------------------------
        // Request Erase & Request Particles

        public bool RequestErase { get; set; }
        public bool RequestParticles { get; set; }

        //--------------------------------------------------
        // Random

        protected Random _rand;

        //--------------------------------------------------
        // Bouding box

        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle((int)_position.X, (int)_position.Y, _sprite.TextureRegion.Width, _sprite.TextureRegion.Height);
            }
        }

        //----------------------//------------------------//

        public GameProjectile(Texture2D texture, Vector2 initialPosition, float dx, float dy, int damage, ProjectileSubject subject)
        {
            _sprite = new Sprite(texture);
            _sprite.OriginNormalized = Vector2.Zero;
            _sprite.Position = initialPosition;
            _position = initialPosition;
            LastPosition = _position;
            _acceleration = new Vector2(dx, dy);
            _damage = damage;
            _subject = subject;
            _rand = new Random();
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            LastPosition = _position;
            
            _position += _acceleration * deltaTime;
            _distanceTraveledX += _acceleration.X * deltaTime;
            if (Math.Abs(_distanceTraveledX) > MaxDistanceTraveled)
            {
                _position.Y += GravityAcceleration * deltaTime;
            }

            _sprite.Position = _position;

            var tileX = (int)(_position.X / MapManager.Instance.TileSize.X);
            var tileY = (int)(_position.Y / MapManager.Instance.TileSize.Y);
            var tileY2 = (int)((_position.Y + Sprite.TextureRegion.Height / 2) / MapManager.Instance.TileSize.Y);
            if (_position.X >= MapManager.Instance.MapWidth || _position.Y >= MapManager.Instance.MapHeight ||
                Position.X + Sprite.TextureRegion.Width <= 0 || Position.Y + Sprite.TextureRegion.Height <= 0 ||
                MapManager.Instance.IsTileBlocked(tileX, tileY))
                Destroy(false);

            if (MapManager.Instance.IsTileBlocked(tileX, tileY2)) Destroy(true);
        }

        public void Destroy(bool showParticles)
        {
            Sprite.Alpha = 0.0f;
            RequestErase = true;
            RequestParticles = showParticles;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite);
        }
    }
}
