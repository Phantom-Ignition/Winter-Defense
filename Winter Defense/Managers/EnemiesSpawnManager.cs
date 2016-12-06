using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using System;
using System.Collections.Generic;
using Winter_Defense.Characters;

namespace Winter_Defense.Managers
{
    //--------------------------------------------------
    // Side

    public enum Side
    {
        Left,
        Right
    }

    class EnemiesSpawnManager
    {
        //--------------------------------------------------
        // Enemy Model

        public struct EnemyModel
        {
            public EnemyType Type;
            public Side Side;
        }

        //--------------------------------------------------
        // Queue

        private List<EnemyModel> _queue;
        public List<EnemyModel> Queue => _queue;

        //--------------------------------------------------
        // Time stuff

        private const float SpawnInterval = 1500.0f;
        private float _currentSpawnInterval;

        //--------------------------------------------------
        // Spawn Rules

        private List<List<EnemyModel>> _spawnRules;

        private List<EnemyType> _waveSpawnQueue;
        private int _ghostCount;
        private int _fireGhostCount;
        private int _plantGhostCount;
        private int _trueGhostCount;

        //--------------------------------------------------
        // Waves

        private int _currentWave;
        private bool _waveCompleted;
        public bool WaveCompleted => _waveCompleted;

        //--------------------------------------------------
        // Random

        private Random _rand;

        //--------------------------------------------------
        // Active

        private bool _active;
        public bool Active => _active;

        //----------------------//------------------------//

        public EnemiesSpawnManager()
        {
            _queue = new List<EnemyModel>();
            _waveSpawnQueue = new List<EnemyType>();
            _rand = new Random();

            CreateSpawnRules();
            InitEnemiesCount();
        }

        private void CreateSpawnRules()
        {
            _spawnRules = new List<List<EnemyModel>>
            {
                // Wave #1
                new List<EnemyModel> {
                    new  EnemyModel
                    {
                        Type = EnemyType.Ghost,
                        Side = Side.Right
                    },
                    new  EnemyModel
                    {
                        Type = EnemyType.FireGhost,
                        Side = Side.Right
                    },
                    new  EnemyModel
                    {
                        Type = EnemyType.PlantGhost,
                        Side = Side.Right
                    },
                    new  EnemyModel
                    {
                        Type = EnemyType.TrueGhost,
                        Side = Side.Right
                    }
                },
                // Wave #2
                new List<EnemyModel> {
                    new  EnemyModel
                    {
                        Type = EnemyType.Ghost,
                        Side = Side.Left
                    },
                    new  EnemyModel
                    {
                        Type = EnemyType.Ghost,
                        Side = Side.Left
                    },
                    new  EnemyModel
                    {
                        Type = EnemyType.FireGhost,
                        Side = Side.Left
                    },
                    new  EnemyModel
                    {
                        Type = EnemyType.FireGhost,
                        Side = Side.Left
                    }
                }
            };
        }

        private void InitEnemiesCount()
        {
            _ghostCount = 3;
            _fireGhostCount = 2;
            _plantGhostCount = 2;
            _trueGhostCount = 1;
        }

        public void Start()
        {
            _active = true;
        }

        public void StartNextWave()
        {
            _currentWave++;
            _active = true;
        }

        public EnemyModel ShiftModelFromQueue()
        {
            var model = _queue[0];
            _queue.RemoveAt(0);
            return model;
        }

        public void Update(GameTime gameTime)
        {
            if (!_active) return;

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            _currentSpawnInterval += deltaTime;
            if (_currentSpawnInterval >= SpawnInterval)
            {
                Spawn();
                _currentSpawnInterval = 0;
            }
        }

        private void Spawn()
        {
            var model = GetNextModel();
            _queue.Add(model);
        }

        private EnemyModel GetNextModel()
        {
            if (_spawnRules.Count > 0)
            {
                var model = _spawnRules[0][0];
                _spawnRules[0].RemoveAt(0);
                if (_spawnRules[0].Count == 0)
                {
                    _spawnRules.RemoveAt(0);
                    CompleteWave();
                }
                return model;
            }

            return new EnemyModel
            {
                Type = GetNextWaveEnemy(),
                Side = GetRandomSide()
            };
        }

        private void CompleteWave()
        {
            _waveCompleted = true;
            _active = false;
            if (_spawnRules.Count == 0)
            {
                GenerateWave();
            }
        }

        private EnemyType GetNextWaveEnemy()
        {
            var enemy = _waveSpawnQueue[0];
            _waveSpawnQueue.RemoveAt(0);
            if (_waveSpawnQueue.Count == 0)
            {
                CompleteWave();
                GenerateWave();
            }
            return enemy;
        }

        private void GenerateWave()
        {
            _waveSpawnQueue.Clear();
            for (var i = 0; i < _ghostCount; i++)
                _waveSpawnQueue.Add(EnemyType.Ghost);
            for (var i = 0; i < _fireGhostCount; i++)
                _waveSpawnQueue.Add(EnemyType.FireGhost);
            for (var i = 0; i < _plantGhostCount; i++)
                _waveSpawnQueue.Add(EnemyType.PlantGhost);
            for (var i = 0; i < _trueGhostCount; i++)
                _waveSpawnQueue.Add(EnemyType.TrueGhost);
            _waveSpawnQueue.Shuffle(_rand);
        }

        private Side GetRandomSide()
        {
            return _rand.Next(2) == 0 ? Side.Left : Side.Right;
        }
    }
}
