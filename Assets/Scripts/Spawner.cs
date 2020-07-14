using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private Wave _currentWave;
    private int _currentWaveNumber;
    private int _enemiesRemainingToSpawn;
    private int _enemiesRemainingAlive;
    private float _nextSpawnTime;

    public Wave[] Waves;
    public Enemy Enemy;


    void Start()
    {
        NextWave();
    }

    void Update()
    {
        if (_enemiesRemainingToSpawn > 0 && Time.time > _nextSpawnTime)
        {
            _enemiesRemainingToSpawn--;
            _nextSpawnTime = Time.time + _currentWave.TimeBetweenSpawn;

            Enemy spawnedEnemy = Instantiate(Enemy, Vector3.zero, Quaternion.identity);
            spawnedEnemy.OnDeath += OnEnemyDeath;
        }
    }

    void OnEnemyDeath()
    {
        _enemiesRemainingAlive--;

        if (_enemiesRemainingAlive == 0)
        {
            NextWave();
        }
    }

    void NextWave()
    {
        _currentWaveNumber++;
        print($"Wave: {_currentWaveNumber}");

        if (_currentWaveNumber - 1 < Waves.Length)
        {
            _currentWave = Waves[_currentWaveNumber - 1];

            _enemiesRemainingToSpawn = _currentWave.EnemyCount;
            _enemiesRemainingAlive = _enemiesRemainingToSpawn;
        }
    }

    [Serializable]
    public class Wave
    {
        public int EnemyCount;
        public float TimeBetweenSpawn;
    }
}