using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using Winter_Defense.Extensions;
using Winter_Defense.Managers;

namespace Winter_Defense.Objects
{
    class GameCrystal
    {
        //--------------------------------------------------
        // Position

        private Vector2 _position;
        private Vector2 _crystalPosition;

        //--------------------------------------------------
        // Texture

        private Texture2D _texture;

        //--------------------------------------------------
        // Frames

        private Rectangle[] _domeBackFrames;
        private int[] _domeLivesIndex;
        private Rectangle _domeFrontFrame;
        private Rectangle _crystalFrame;

        //--------------------------------------------------
        // Graphics

        private float _alpha;

        private int _immunityTick;
        private float _immunityTimeElapsed;
        private bool _immunityAnimation;
        private const float ImmunityMaxTime = 0.2f;

        //--------------------------------------------------
        // Lives

        private int _lives;
        public int Lives => _lives;

        //--------------------------------------------------
        // Bouding Box

        private Rectangle _boundingBox;
        public Rectangle BoudingBox => _boundingBox;

        private Texture2D _colliderTexture;

        //--------------------------------------------------
        // Dying

        private bool _dying;
        public bool Dying => _dying;

        //--------------------------------------------------
        // Request Erase

        private bool _requestErase;
        public bool RequestErase => _requestErase;

        //--------------------------------------------------
        // Sound Effects

        private SoundEffect _hitSe;
        private SoundEffect _glassSe;

        //----------------------//------------------------//

        public GameCrystal(Vector2 position, Texture2D texture)
        {
            _position = position;
            _crystalPosition = new Vector2(_position.X, _position.Y - 3);
            _texture = texture;
            _alpha = 1.0f;

            _domeBackFrames = new Rectangle[]
            {
                new Rectangle(0, 0, 96, 128),
                new Rectangle(0, 128, 96, 128),
                new Rectangle(96, 128, 96, 128),
                new Rectangle(192, 128, 96, 128),
            };
            _domeLivesIndex = new int[] { 0, 1, 1, 2, 3, 3 };
            _domeFrontFrame = new Rectangle(192, 0, 96, 128);
            _crystalFrame = new Rectangle(96, 0, 96, 128);

            _boundingBox = new Rectangle((int)position.X + 6, (int)position.Y, _crystalFrame.Width - 12, _crystalFrame.Height);

            _colliderTexture = new Texture2D(SceneManager.Instance.GraphicsDevice, 1, 1);
            _colliderTexture.SetData(new Color[] { Color.Blue });

            _lives = 5;

            _hitSe = SoundManager.LoadSe("Hit");
            _glassSe = SoundManager.LoadSe("Glass");
        }

        public void Update(GameTime gameTime)
        {
            if (_dying)
            {
                _alpha -= (float)gameTime.ElapsedGameTime.TotalSeconds / 3;
                _requestErase = _alpha <= 0.0f;
                return;
            }

            var deltaTime = (float)gameTime.TotalGameTime.TotalMilliseconds / 20;
            _crystalPosition.Y = (int)MathUtils.SinInterpolation(_position.Y - 3, _position.Y + 5, deltaTime);

            UpdateImmunityAnimation(gameTime);
        }

        public void OnDamage()
        {
            if (_lives > 0)
            {
                _immunityAnimation = true;
                _lives--;
                _hitSe.PlaySafe();
            }
            if (!_dying && _lives <= 0)
            {
                _dying = true;
                _glassSe.PlaySafe();
            }
        }

        private void UpdateImmunityAnimation(GameTime gameTime)
        {
            if (!_immunityAnimation) return;
             _alpha = _immunityTick == 0 ? 1f : 0.2f;
            _immunityTimeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_immunityTimeElapsed > ImmunityMaxTime)
            {
                _immunityAnimation = false;
                _immunityTick = 0;
                _immunityTimeElapsed = 0.0f;
                _alpha = 1.0f;
            }
            else
            {
                _immunityTick = _immunityTick == 0 ? 1 : 0;
            }
        }

        #region Draw
        public void Draw(SpriteBatch spriteBatch)
        {
            var domeFrame = _domeBackFrames[_domeLivesIndex[Math.Max(5 - _lives, 0)]];
            spriteBatch.Draw(_texture, _crystalPosition, _crystalFrame, Color.White * _alpha);
            spriteBatch.Draw(_texture, _position, domeFrame, Color.White * _alpha);
            spriteBatch.Draw(_texture, _position, _domeFrontFrame, Color.White * _alpha);
        }

        public void DrawCollider(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_colliderTexture, BoudingBox, Color.White * 0.5f);
        }
        #endregion
    }
}
