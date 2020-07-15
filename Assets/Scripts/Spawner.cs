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
    private MapGenerator map;
    private LivingEntity playerEntity;
    private Transform playerT;

    private float timeBetweenCampingChecks = 2f;
    private float campThresholdDistance = 1.5f;
    private float nextCampCheckTime;
    private Vector3 campPositionOld;
    private bool isCamping;
    private bool isDisabled;

    public event Action<int> OnNewWave;

    public Wave[] Waves;
    public Enemy Enemy;

    private void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerEntity.OnDeath += OnPlayerDeath;
        playerT = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.deltaTime;
        campPositionOld = playerT.position;

        map = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    private void Update()
    {
        if (!isDisabled)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;

                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerT.position;
            }

            if (_enemiesRemainingToSpawn > 0 && Time.time > _nextSpawnTime)
            {
                _enemiesRemainingToSpawn--;
                _nextSpawnTime = Time.time + _currentWave.TimeBetweenSpawn;

                StartCoroutine(SpawnEnemy());
            }
        }
    }

    private IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping)
        {
            spawnTile = map.GetTileFromPosition(playerT.position);
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
        isDisabled = true;
    }

    private void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    private void NextWave()
    {
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
        public int EnemyCount;
        public float TimeBetweenSpawn;
    }
}