using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
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
        // Sprite

        private Sprite _sprite;
        public Sprite Sprite { get { return _sprite; } }

        //--------------------------------------------------
        // Position

        private Vector2 _position;
        public Vector2 Position { get { return _position; } }
        public Vector2 LastPosition { get; private set; }

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
        // Request erase

        public bool RequestErase { get; set; }

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
            LastPosition = _position;
            _position += _acceleration;
            _sprite.Position = _position;
            var tileX = (int)(_position.X / MapManager.Instance.TileSize.X);
            var tileY = (int)(_position.Y / MapManager.Instance.TileSize.Y);
            if (_position.X >= MapManager.Instance.MapWidth || _position.Y >= MapManager.Instance.MapHeight ||
                Position.X + Sprite.TextureRegion.Width <= 0 || Position.Y + Sprite.TextureRegion.Height <= 0)
                Destroy(false);

            if (MapManager.Instance.IsTileBlocked(tileX, tileY))
            {
                Destroy();
            }
        }

        public void Destroy(bool showParticles = true)
        {
            Sprite.Alpha = 0.0f;
            RequestErase = true;
        }
    }
}
