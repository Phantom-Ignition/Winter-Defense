using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Winter_Defense.Characters;

namespace Winter_Defense.Managers
{
    class EnemiesSpawnManager
    {
        //--------------------------------------------------
        // Enemy Model

        public struct EnemyModel
        {
            public EnemyType Type;
            public int Side;
        }

        //--------------------------------------------------
        // Queue

        private List<EnemyModel> _queue;
        public List<EnemyModel> Queue => _queue;

        //--------------------------------------------------
        // Time

        private TimeSpan _time;
        public TimeSpan Time => _time;
        
        //--------------------------------------------------
        // Random

        private Random _rand;

        private float _spawnTime;

        //----------------------//------------------------//

        public EnemiesSpawnManager()
        {
            _queue = new List<EnemyModel>();
            _time = new TimeSpan();
            _rand = new Random();
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _spawnTime -= deltaTime;
            if (_spawnTime <= 0.0f)
            {
                Spawn();
                _spawnTime = 10500.0f;
            }
        }

        private void Spawn()
        {
            var model = new EnemyModel
            {
                Type = EnemyType.Ghost,
                Side = _rand.Next(2)
            };
            _queue.Add(model);
        }

        public EnemyModel ShiftModel()
        {
            var model = _queue[0];
            _queue.RemoveAt(0);
            return model;
        }
    }
}
