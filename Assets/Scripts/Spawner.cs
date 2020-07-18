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
    private MapGenerator _map;
    private LivingEntity _playerEntity;
    private Transform _playerT;

    private float _timeBetweenCampingChecks = 2f;
    private float _campThresholdDistance = 1.5f;
    private float _nextCampCheckTime;
    private Vector3 _campPositionOld;
    private bool _isCamping;
    private bool _isDisabled;

    public event Action<int> OnNewWave;

    public bool DeveloperMode;

    public Wave[] Waves;
    public Enemy Enemy;

    private void Start()
    {
        _playerEntity = FindObjectOfType<Player>();
        _playerEntity.OnDeath += OnPlayerDeath;
        _playerT = _playerEntity.transform;

        _nextCampCheckTime = _timeBetweenCampingChecks + Time.deltaTime;
        _campPositionOld = _playerT.position;

        _map = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    private void Update()
    {
        if (!_isDisabled)
        {
            if (Time.time > _nextCampCheckTime)
            {
                _nextCampCheckTime = Time.time + _timeBetweenCampingChecks;

                _isCamping = (Vector3.Distance(_playerT.position, _campPositionOld) < _campThresholdDistance);
                _campPositionOld = _playerT.position;
            }

            if ((_enemiesRemainingToSpawn > 0 || _currentWave.Infinite) && Time.time > _nextSpawnTime)
            {
                _enemiesRemainingToSpawn--;
                _nextSpawnTime = Time.time + _currentWave.TimeBetweenSpawn;

                StartCoroutine("SpawnEnemy");
            }
        }

        if (DeveloperMode)
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                StopCoroutine("SpawnEnemy");
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    Destroy(enemy.gameObject);
                }

                NextWave();
            }
        }
    }

    private IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = _map.GetRandomOpenTile();
        if (_isCamping)
        {
            spawnTile = _map.GetTileFromPosition(_playerT.position);
        }

        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = Color.white;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(Enemy, spawnTile.position + Vector3.up, Quaternion.identity);
        spawnedEnemy.OnDeath += OnEnemyDeath;

        spawnedEnemy.SetCharacteristics(_currentWave);
    }

    private void OnEnemyDeath()
    {
        _enemiesRemainingAlive--;

        if (_enemiesRemainingAlive == 0)
        {
            NextWave();
        }
    }

    private void OnPlayerDeath()
    {
        _isDisabled = true;
    }

    private void ResetPlayerPosition()
    {
        _playerT.position = _map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    private void NextWave()
    {
        if (_currentWaveNumber > 0)
        {
            AudioManager.instance.PlaySound("Level completed");
        }

        _currentWaveNumber++;

        if (_currentWaveNumber - 1 < Waves.Length)
        {
            _currentWave = Waves[_currentWaveNumber - 1];

            _enemiesRemainingToSpawn = _currentWave.EnemyCount;
            _enemiesRemainingAlive = _enemiesRemainingToSpawn;

            OnNewWave?.Invoke(_currentWaveNumber);

            ResetPlayerPosition();
        }
    }

    [Serializable]
    public class Wave
    {
        public bool Infinite;
        public int EnemyCount;
        public float TimeBetweenSpawn;

        public float MoveSpeed;
        public int HitsToKillPlayer;
        public float EnemyHealth;
        public Color SkinColor;
    }
}