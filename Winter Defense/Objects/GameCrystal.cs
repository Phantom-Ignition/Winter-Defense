using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winter_Defense.Extensions;

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

        //----------------------//------------------------//

        public GameCrystal(Vector2 position, Texture2D texture)
        {
            _position = position;
            _crystalPosition = new Vector2(_position.X, _position.Y - 3);
            _texture = texture;

            _domeBackFrame = new Rectangle(0, 0, 96, 128);
            _domeFrontFrame = new Rectangle(192, 0, 96, 128);
            _crystalFrame = new Rectangle(96, 0, 96, 128);
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.TotalGameTime.TotalMilliseconds / 20;
            _crystalPosition.Y = (float)MathUtils.SinInterpolation(_position.Y - 3, _position.Y + 5, deltaTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, _domeBackFrame, Color.White);
            spriteBatch.Draw(_texture, _crystalPosition, _crystalFrame, Color.White);
            spriteBatch.Draw(_texture, _position, _domeFrontFrame, Color.White);
        }
    }
}
