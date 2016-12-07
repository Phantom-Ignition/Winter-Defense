using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        private Rectangle _domeBackFrame;
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

        //----------------------//------------------------//

        public GameCrystal(Vector2 position, Texture2D texture)
        {
            _position = position;
            _crystalPosition = new Vector2(_position.X, _position.Y - 3);
            _texture = texture;
            _alpha = 1.0f;

            _domeBackFrame = new Rectangle(0, 0, 96, 128);
            _domeFrontFrame = new Rectangle(192, 0, 96, 128);
            _crystalFrame = new Rectangle(96, 0, 96, 128);

            _boundingBox = new Rectangle((int)position.X + 6, (int)position.Y, _crystalFrame.Width - 12, _crystalFrame.Height);

            _colliderTexture = new Texture2D(SceneManager.Instance.GraphicsDevice, 1, 1);
            _colliderTexture.SetData(new Color[] { Color.Blue });

            _lives = 5;
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.TotalGameTime.TotalMilliseconds / 20;
            _crystalPosition.Y = (int)MathUtils.SinInterpolation(_position.Y - 3, _position.Y + 5, deltaTime);
            UpdateImmunityAnimation(gameTime);
        }

        public void OnDamage()
        {
            _immunityAnimation = true;
            _lives--;
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
            spriteBatch.Draw(_texture, _position, _domeBackFrame, Color.White * _alpha);
            spriteBatch.Draw(_texture, _crystalPosition, _crystalFrame, Color.White * _alpha);
            spriteBatch.Draw(_texture, _position, _domeFrontFrame, Color.White * _alpha);
        }

        public void DrawCollider(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_colliderTexture, BoudingBox, Color.White * 0.5f);
        }
        #endregion
    }
}
