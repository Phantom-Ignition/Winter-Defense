using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winter_Defense.Managers;

namespace Winter_Defense.Objects
{
    class GameHud
    {
        //--------------------------------------------------
        // Textures

        private Texture2D _backgroundTexture;
        private Texture2D _itemsTexture;

        //--------------------------------------------------
        // Frames

        private Rectangle _backgroundFrame;
        private Rectangle _lifeItemFrame;
        private Rectangle _ammoItemFrame;

        //--------------------------------------------------
        // Wave Text

        private Vector2 _wavePosition;
        private BitmapFont _waveFont;

        //--------------------------------------------------
        // Data

        private int _lives;
        private int _ammo;
        private int _wave;

        //----------------------//------------------------//

        public GameHud()
        {
            _backgroundTexture = ImageManager.loadScene("sceneMap", "HudBackground");
            _itemsTexture = ImageManager.loadScene("sceneMap", "HudItems");

            _backgroundFrame = new Rectangle(0, 0, 427, 48);
            _lifeItemFrame = new Rectangle(0, 0, 15, 16);
            _ammoItemFrame = new Rectangle(17, 1, 14, 14);

            _waveFont = SceneManager.Instance.Content.Load<BitmapFont>("fonts/SneakAttack");
            _wavePosition = new Vector2(341, 26);
        }

        public void SetData(int lives, int ammo, int wave)
        {
            _lives = lives;
            _ammo = ammo;
            _wave = wave;
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_backgroundTexture, _backgroundFrame, Color.White);
            
            for (var i = 0; i < _lives; i++)
            {
                var position = new Vector2(59 + i * 22, 22);
                spriteBatch.Draw(_itemsTexture, position, _lifeItemFrame, Color.White);
            }

            for (var i = 0; i < _ammo; i++)
            {
                var position = new Vector2(180 + i * 21, 23);
                spriteBatch.Draw(_itemsTexture, position, _ammoItemFrame, Color.White);
            }

            var waveText = _wave.ToString();
            var textSize = _waveFont.MeasureString(waveText);
            var wavePosition = new Vector2(textSize.Width / 2, textSize.Height / 2);
            spriteBatch.DrawString(_waveFont, _wave.ToString(), _wavePosition - wavePosition, Color.White);
        }
    }
}
