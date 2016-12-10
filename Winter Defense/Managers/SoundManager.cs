using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Winter_Defense.Managers
{
    public static class SoundManager
    {
        //--------------------------------------------------
        // BGMType

        public enum BGMType
        {
            None,
            Default,
            Gameover
        }

        private static BGMType _bgmType;

        //--------------------------------------------------
        // Content Manager

        private static Dictionary<string, SoundEffect> _seCache = new Dictionary<string, SoundEffect>();
        private static ContentManager _contentManager = new ContentManager(SceneManager.Instance.Content.ServiceProvider, "Content");

        //--------------------------------------------------
        // SEs

        private static SoundEffect _confirmSe;

        //--------------------------------------------------
        // BGMs

        private static Song _defaultSong;
        private static Song _gameoverSong;

        //--------------------------------------------------
        // For debug purposes

        private static bool _soundOn = true;

        //----------------------//------------------------//

        public static void Initialize()
        {
            _confirmSe = LoadSe("Confirm");
            _defaultSong = LoadBgm("TheLift");
            _gameoverSong = LoadBgm("Ossuary6Air");
            MediaPlayer.IsRepeating = true;
        }

        public static Song LoadBgm(string filename)
        {
            return _contentManager.Load<Song>("sounds/bgm/" + filename);
        }

        public static SoundEffect LoadSe(string filename)
        {
            if (!_seCache.ContainsKey(filename))
            {
                try
                {
                    _seCache[filename] = _contentManager.Load<SoundEffect>("sounds/se/" + filename);
                }
                catch (Exception ex)
                {
                    _seCache[filename] = null;
                    Debug.WriteLine(ex.ToString());
                }
            }
            return _seCache[filename];
        }

        public static void StartBgm(BGMType bgmType)
        {
            if (_soundOn)
            {
                if (bgmType == BGMType.Default && _bgmType != BGMType.Default)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(_defaultSong);
                    _bgmType = BGMType.Default;
                }
                else if (bgmType == BGMType.Gameover && _bgmType != BGMType.Gameover)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(_gameoverSong);
                    _bgmType = BGMType.Gameover;
                }
                MediaPlayer.Volume = 1.0f;
            }
        }

        public static void SetBgmVolume(float volume)
        {
            if (_soundOn)
            {
                MediaPlayer.Volume = volume;
            }
        }

        public static void PlaySafe(this SoundEffect se)
        {
            if (_soundOn)
                se?.Play();
        }

        public static void PlaySafe(this SoundEffectInstance seInstance)
        {
            if (_soundOn)
                seInstance?.Play();
        }

        public static void PlayConfirmSe()
        {
            _confirmSe.PlaySafe();
        }

        public static void Dispose()
        {
            _confirmSe.Dispose();
        }
    }
}
