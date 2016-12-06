using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Winter_Defense.Managers;

namespace Winter_Defense.Characters
{
    //--------------------------------------------------
    // Enemy Type

    public enum EnemyType
    {
        None,
        Ghost,
        Boss
    }

    class EnemyBase : CharacterBase
    {
        //--------------------------------------------------
        // Combat system

        protected EnemyType _enemyType;
        public EnemyType EnemyType => _enemyType;

        protected Rectangle _viewRange;
        public Rectangle ViewRange => _viewRange;
        protected Vector2 _viewRangeOffset;
        protected Vector2 _viewRangeSize;
        private Vector2 _lastPosition;

        private bool _hasViewRange;
        public bool HasViewRange => _hasViewRange;
        protected float _viewRangeCooldown;
        public float ViewRangeCooldown => _viewRangeCooldown;

        //--------------------------------------------------
        // Textures

        private Texture2D _viewRangeTexture;

        //--------------------------------------------------
        // Crystal Position X

        private float _crystalPositionX;

        //----------------------//------------------------//

        public EnemyBase(Texture2D texture) : base(texture)
        {
            _viewRangeTexture = new Texture2D(SceneManager.Instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _viewRangeTexture.SetData(new Color[] { Color.Green });
            _lastPosition = Position;
            _enemyType = EnemyType.None;
            _hasViewRange = false;
            _viewRangeCooldown = 0f;
            _viewRangeOffset = Vector2.Zero;
            _crystalPositionX = SceneManager.Instance.VirtualSize.X / 2;
        }

        public void CreateViewRange()
        {
            var width = ((int)_viewRangeSize.X + CharacterSprite.Collider.BoundingBox.Width / 2) * 2;
            var height = (int)_viewRangeSize.Y;
            _viewRange = new Rectangle(0, 0, width, height);
            _hasViewRange = true;
        }

        public void SetPositionFromGround(Vector2 position)
        {
            Position = new Vector2(position.X - CharacterSprite.Collider.Width / 2,
                position.Y - CharacterSprite.Collider.Height * 2);
        }

        public virtual void PlayerOnSight(Vector2 playerPosition) { }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_lastPosition.X != CharacterSprite.Collider.BoundingBox.X ||
                _lastPosition.Y != CharacterSprite.Collider.BoundingBox.Y)
                UpdateViewRange();

            UpdateMovement();

            CharacterSprite.Effect = _velocity.X > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            UpdateViewRangeCooldown(gameTime);
        }

        private void UpdateViewRangeCooldown(GameTime gameTime)
        {
            if (_viewRangeCooldown > 0f)
                _viewRangeCooldown -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        public void UpdateViewRange()
        {
            _viewRange.X = (CharacterSprite.Collider.BoundingBox.Center.X - _viewRange.Width / 2) + (int)_viewRangeOffset.X;
            _viewRange.Y = (CharacterSprite.Collider.BoundingBox.Y) + (int)_viewRangeOffset.Y;
            _lastPosition.X = CharacterSprite.Collider.BoundingBox.X;
            _lastPosition.Y = CharacterSprite.Collider.BoundingBox.Y;
        }

        private void UpdateMovement()
        {
            _movement = Math.Sign(_crystalPositionX - _position.X);
        }

        public override void DrawColliderBox(SpriteBatch spriteBatch)
        {
            base.DrawColliderBox(spriteBatch);
            DrawViewRange(spriteBatch);
        }

        private void DrawViewRange(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_viewRangeTexture, _viewRange, Color.White * 0.2f);
        }
    }
}
