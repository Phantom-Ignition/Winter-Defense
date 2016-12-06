using Winter_Defense.Managers;
using Microsoft.Xna.Framework;
using System;

namespace Winter_Defense.Objects
{
    public class PhysicalObject
    {
        //--------------------------------------------------
        // Physics variables

        public float previousBottom;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }
        protected Vector2 _position;

        public Vector2 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }
        protected Vector2 _velocity;

        public bool IsOnGround
        {
            get { return _isOnGround; }
        }
        protected bool _isOnGround;

        protected float _movement;
        protected float _knockbackAcceleration;
        protected float _dyingAcceleration;

        //--------------------------------------------------
        // Constants for controling horizontal movement

        protected const float MoveAcceleration = 13000.0f;
        protected const float GroundDragFactor = 0.48f;
        protected const float AirDragFactor = 0.50f;
        protected const float DyingDragFactor = 0.8f;

        //--------------------------------------------------
        // Constants for controlling vertical movement

        protected const float MaxJumpTime = 0.30f;
        protected const float JumpLaunchVelocity = -2000.0f;
        protected const float GravityAcceleration = 2500.0f;
        protected const float DyingGravityAcceleration = 2500.0f;
        protected const float MaxFallSpeed = 550.0f;
        protected const float JumpControlPower = 0.16f;
        protected const float PlayerSpeed = 0.3f;

        //--------------------------------------------------
        // Jumping states

        protected bool _isJumping;
        protected bool _wasJumping;
        protected float _jumpTime;

        //--------------------------------------------------
        // Move direction

        public enum Direction
        {
            Horizontal,
            Vertical
        }

        //--------------------------------------------------
        // Ignore gravity?

        public bool IgnoreGravity { get; set; }

        //--------------------------------------------------
        // Combat system usage

        protected bool _dying;
        public bool Dying => _dying;

        //--------------------------------------------------
        // Bounding rectangle

        public virtual Rectangle BoundingRectangle => Rectangle.Empty;

        //----------------------//------------------------//

        public virtual void Update(GameTime gameTime)
        {
            ApplyPhysics(gameTime);
            _movement = 0.0f;
            _isJumping = false;
        }

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        protected virtual void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            if (_dying) _velocity.X = _dyingAcceleration * MoveAcceleration * elapsed;
            else _velocity.X += _movement * MoveAcceleration * elapsed;

            UpdateKnockback(elapsed);

            if (!IgnoreGravity)
            {
                var gravity = _dying ? DyingGravityAcceleration : GravityAcceleration;
                _velocity.Y = MathHelper.Clamp(_velocity.Y + gravity * elapsed, -MaxFallSpeed, MaxFallSpeed);
            }
            _velocity.Y = DoJump(_velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (_dying)
                _velocity.X *= DyingDragFactor;
            else if (IsOnGround)
                _velocity.X *= GroundDragFactor;
            else
                _velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            _velocity.X = MathHelper.Clamp(_velocity.X, -MaxMoveSpeed(), MaxMoveSpeed());

            // If the player is now colliding with the level, separate them.
            if (_velocity.X != 0f)
            {
                Position += _velocity.X * Vector2.UnitX * elapsed;
                Position = new Vector2((float)Math.Round(Position.X), Position.Y);
                if (!_dying)
                    HandleCollisions(Direction.Horizontal);
            }

            if (_velocity.Y != 0f)
            {
                Position += _velocity.Y * Vector2.UnitY * elapsed;
                Position = new Vector2(Position.X, (float)Math.Round(Position.Y));
                if (!_dying)
                    HandleCollisions(Direction.Vertical);
            }

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                _velocity.X = 0;

            if (Position.Y == previousPosition.Y)
            {
                _velocity.Y = 0;
                _isJumping = false;
                _jumpTime = 0.0f;
            }

            Position = new Vector2((int)Position.X, (int)Position.Y);
        }

        protected void UpdateKnockback(float elapsed)
        {
            if (_knockbackAcceleration != 0.0f)
            {
                _velocity.X += (_knockbackAcceleration * elapsed);
                _knockbackAcceleration *= 0.9f;
                if (Math.Abs(_knockbackAcceleration) < 500f) _knockbackAcceleration = 0;
            }
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        protected float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (_isJumping)
            {
                // Begin or continue a jump
                if ((!_wasJumping && IsOnGround) || _jumpTime > 0.0f)
                {
                    _jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                // If we are in the ascent of the jump
                if (0.0f < _jumpTime && _jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(_jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    _jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                _jumpTime = 0.0f;
            }
            _wasJumping = _isJumping;

            return velocityY;
        }

        protected void HandleCollisions(Direction direction)
        {
            Rectangle bounds = BoundingRectangle;
            var tileSize = MapManager.Instance.TileSize.X;
            int leftTile = (int)Math.Floor((float)bounds.Left / tileSize);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / tileSize)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / tileSize);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / tileSize)) - 1;

            _isOnGround = false;

            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    Vector2 depth;
                    Rectangle tileBounds = MapManager.Instance.GetTileBounds(x, y);
                    MapManager.TileCollision collision = MapManager.Instance.GetCollision(x, y);

                    if (collision != MapManager.TileCollision.Passable && MapManager.Instance.TileIntersectsPlayer(bounds, MapManager.Instance.GetTileBounds(x, y), direction, out depth))
                    {
                        if (previousBottom <= tileBounds.Top)
                        {
                            _isOnGround = true;
                        }

                        if (collision == MapManager.TileCollision.Block || _isOnGround)
                        {
                            Position += depth;
                            bounds = BoundingRectangle;
                        }
                    }
                }
            }

            previousBottom = bounds.Bottom;
        }

        protected virtual float MaxMoveSpeed()
        {
            return 1750.0f;
        }
    }
}
