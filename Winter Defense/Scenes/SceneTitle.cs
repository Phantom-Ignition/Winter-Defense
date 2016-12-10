using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winter_Defense.Managers;
using MonoGame.Extended.ViewportAdapters;
using Winter_Defense.Extensions;
using MonoGame.Extended.BitmapFonts;
using Microsoft.Xna.Framework.Input;

namespace Winter_Defense.Scenes
{
    class SceneTitle : SceneBase
    {
        //--------------------------------------------------
        // Texture

        private Texture2D _backgroundTexture;
        private Texture2D _gameTitleTexture;

        //--------------------------------------------------
        // Positions

        private Vector2 _titlePosition;
        private float _titlePositionFloat;

        private Vector2 _pressAnyKeyPosition;
        private const string PressAnyKeyText = "Press Start";
        private float _pressAnyKeyAlpha;
        private float _pressAnyKeyTick;

        //----------------------//------------------------//

        public override void LoadContent()
        {
            base.LoadContent();

            _backgroundTexture = ImageManager.loadScene("sceneTitle", "Background");
            _gameTitleTexture = ImageManager.loadScene("sceneTitle", "GameTitle");

            _titlePosition = new Vector2(213 - _gameTitleTexture.Width / 2, 58);
            _pressAnyKeyPosition = new Vector2(213 - SceneManager.Instance.GameFont.MeasureString(PressAnyKeyText).Width / 2, 180);

            SoundManager.StartBgm(SoundManager.BGMType.Default);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateSprites(gameTime);
            UpdateInput();
        }

        private void UpdateSprites(GameTime gameTime)
        {
            var elapsedTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            _titlePositionFloat = (int)MathUtils.SinInterpolation(-3, 3, elapsedTime / 15);
            _pressAnyKeyTick -= deltaTime;
            if (_pressAnyKeyTick <= 0.0f)
            {
                _pressAnyKeyAlpha = _pressAnyKeyAlpha == 1.0f ? 0.0f : 1.0f;
                _pressAnyKeyTick = 500.0f;
            }
        }

        private void UpdateInput()
        {
            if (InputManager.Instace.KeyPressed(Keys.Enter, Keys.Space))
            {
                SoundManager.PlayConfirmSe();
                SceneManager.Instance.ChangeScene("SceneMap");
            }
        }

        public override void Draw(SpriteBatch spriteBatch, ViewportAdapter viewportAdapter)
        {
            base.Draw(spriteBatch, viewportAdapter);

            spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix(), samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);
            spriteBatch.Draw(_gameTitleTexture, _titlePosition + _titlePositionFloat * Vector2.UnitY, Color.White);
            spriteBatch.DrawString(SceneManager.Instance.GameFont, PressAnyKeyText, _pressAnyKeyPosition, Color.White * _pressAnyKeyAlpha);
            spriteBatch.End();
        }
    }
}
