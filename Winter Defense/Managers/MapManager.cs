using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Maps.Tiled;
using Winter_Defense.Characters;
using Winter_Defense.Extensions;
using MonoGame.Extended.Maps.Renderers;

namespace Winter_Defense.Managers
{
    class MapManager
    {

        //--------------------------------------------------
        // Singleton

        private static MapManager _instance = null;
        private static readonly object _padlock = new object();

        public static MapManager Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new MapManager();
                    return _instance;
                }
            }
        }

        //--------------------------------------------------
        // Colliders

        private List<Rectangle> _tileColliderBoxes;
        private Texture2D _colliderTexture;

        //--------------------------------------------------
        // Map size

        public int MapWidth => _tiledMap == null ? 0 : _tiledMap.WidthInPixels;
        public int MapHeight => _tiledMap == null ? 0 : _tiledMap.HeightInPixels;

        //--------------------------------------------------
        // First Map

        public static int FirstMap = 1;

        //--------------------------------------------------
        // Tiles stuff

        public TiledMap _tiledMap;
        private IMapRenderer _mapRenderer;
        public Vector2 TileSize;

        public enum TileCollision
        {
            Passable = 0,
            Block = 1,
            Platform = 2
        }

        //----------------------//------------------------//

        private MapManager()
        {
            TileSize = new Vector2(32, 32);
            _tileColliderBoxes = new List<Rectangle>();
            _colliderTexture = new Texture2D(SceneManager.Instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _colliderTexture.SetData<Color>(new Color[] { Color.Red });
        }

        public void LoadMap(ContentManager contentManager, int mapId)
        {
            _tiledMap = contentManager.Load<TiledMap>(String.Format("maps/map{0}", mapId));
            _mapRenderer = new FullMapRenderer(SceneManager.Instance.GraphicsDevice, new MapRendererConfig { DrawObjectLayers = false });
            _mapRenderer.SwapMap(_tiledMap);
            var blockedLayer = (TiledTileLayer)_tiledMap.GetLayer("Block");
            if (blockedLayer == null) return;
            foreach (var tile in blockedLayer.Tiles)
            {
                if (tile.Id != 0)
                {
                    _tileColliderBoxes.Add(new Rectangle(tile.X * (int)TileSize.X, tile.Y * (int)TileSize.Y, (int)TileSize.X, (int)TileSize.Y));
                }
            }
        }

        public void LoadMapWithTransition(int mapId, Action<int> loadCallback)
        {
            SceneManager.Instance.MapTransition(mapId, loadCallback);
        }

        public int GetTileByX(double x)
        {
            return (int)(x / TileSize.X);
        }

        public int GetTileByY(double y)
        {
            return (int)(y / TileSize.Y);
        }

        public TiledTileLayer GetBlockLayer()
        {
            return _tiledMap.GetLayer<TiledTileLayer>("Block");
        }

        public TiledTileLayer GetPlatformLayer()
        {
            return _tiledMap.GetLayer<TiledTileLayer>("Platform");
        }

        public TiledObjectLayer GetObjectGroup(string name)
        {
            return _tiledMap.GetObjectGroup(name);
        }

        public TiledObject GetPlayerSpawn()
        {
            return GetObjectGroup("Player Spawn").Objects[0];
        }

        public bool IsTileBlocked(int x, int y)
        {
            if (y < 0 || y > MapHeight || x < 0 || x > MapWidth) return false;
            var blockLayer = GetBlockLayer();
            if (blockLayer == null) return false;
            if (blockLayer.GetTile(x, y) == null) return false;
            return blockLayer.GetTile(x, y).Id != 0;
        }

        public bool IsTilePlatform(int x, int y)
        {
            if (y < 0 || y > MapHeight || x < 0 || x > MapWidth) return false;
            var platformLayer = GetPlatformLayer();
            if (platformLayer == null) return false;
            if (platformLayer.GetTile(x, y) == null) return false;
            return platformLayer.GetTile(x, y).Id != 0;
        }

        public TileCollision GetCollision(int x, int y)
        {
            if (x < 0 || x >= _tiledMap.Width || IsTileBlocked(x, y))
                return TileCollision.Block;
            if (IsTilePlatform(x, y))
                return TileCollision.Platform;
            return TileCollision.Passable;
        }

        public Rectangle GetTileBounds(int x, int y)
        {
            return new Rectangle(x * (int)TileSize.X, y * (int)TileSize.Y, (int)TileSize.X, (int)TileSize.Y);
        }
        
        public bool TileIntersectsPlayer(Rectangle player, Rectangle block, CharacterBase.Direction direction, out Vector2 depth)
        {
            depth = direction == CharacterBase.Direction.Vertical ? new Vector2(0, player.GetVerticalIntersectionDepth(block)) : new Vector2(player.GetHorizontalIntersectionDepth(block), 0);
            return depth.Y != 0 || depth.X != 0;
        }

        private void DrawTileColliders(SpriteBatch spriteBatch)
        {
            foreach (var collider in _tileColliderBoxes)
            {
                spriteBatch.Draw(_colliderTexture, collider, Color.White * 0.1f);
            }
        }

        public void Draw(Camera2D camera, SpriteBatch spriteBatch)
        {
            _mapRenderer.Draw(camera.GetViewMatrix());
            spriteBatch.Begin(transformMatrix: camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
            if (SceneManager.Instance.DebugMode)
                DrawTileColliders(spriteBatch);
            spriteBatch.End();
        }
    }
}
