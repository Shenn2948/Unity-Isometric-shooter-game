using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] Maps;
    public int MapIndex;

    public Transform TilePrefab;
    public Transform ObstaclePrefab;
    public Transform NavMeshFloor;
    public Transform NavMeshMaskPrefab;
    public Vector2 MaxMapSize;

    [Range(0, 1)]
    public float OutlinePercent;

    private Map _currentMap;

    public float TileSize;

    private List<Coord> _allTileCoords;
    private Queue<Coord> _shuffledTileCoords;
    private Queue<Coord> _shuffledOpenTileCoords;
    private Transform[,] _tileMap;

    private void Awake()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    private void Update()
    {
    }

    public void GenerateMap()
    {
        _currentMap = Maps[MapIndex];
        GetComponent<BoxCollider>().size = new Vector3(_currentMap.MapSize.X * TileSize, .5f, _currentMap.MapSize.Y * TileSize);

        _tileMap = new Transform[_currentMap.MapSize.X, _currentMap.MapSize.Y];

        ShaffleTileCoords();

        Transform mapHolder = GetRefreshedMapHolder();

        InstantiateTiles(mapHolder);
        InstantiateObstacles(mapHolder);

        CreateNavMeshMask(mapHolder);
    }

    private void OnNewWave(int waveNumber)
    {
        MapIndex = waveNumber - 1;
        GenerateMap();
    }

    private void CreateNavMeshMask(Transform mapHolder)
    {
        Transform maskLeft = Instantiate(NavMeshMaskPrefab, Vector3.left * (_currentMap.MapSize.X + MaxMapSize.x) / 4 * TileSize, Quaternion.identity);
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((MaxMapSize.x - _currentMap.MapSize.X) / 2, 1, _currentMap.MapSize.Y) * TileSize;

        Transform maskRight = Instantiate(NavMeshMaskPrefab, Vector3.right * (_currentMap.MapSize.X + MaxMapSize.x) / 4 * TileSize, Quaternion.identity);
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((MaxMapSize.x - _currentMap.MapSize.X) / 2, 1, _currentMap.MapSize.Y) * TileSize;

        Transform maskTop = Instantiate(NavMeshMaskPrefab, Vector3.forward * (_currentMap.MapSize.Y + MaxMapSize.y) / 4 * TileSize, Quaternion.identity);
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(MaxMapSize.x, 1, (MaxMapSize.y - _currentMap.MapSize.Y) / 2) * TileSize;

        Transform maskBottom = Instantiate(NavMeshMaskPrefab, Vector3.back * (_currentMap.MapSize.Y + MaxMapSize.y) / 4 * TileSize, Quaternion.identity);
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(MaxMapSize.x, 1, (MaxMapSize.y - _currentMap.MapSize.Y) / 2) * TileSize;

        NavMeshFloor.localScale = new Vector3(MaxMapSize.x, MaxMapSize.y) * TileSize;
    }

    private Transform GetRefreshedMapHolder()
    {
        const string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        return mapHolder;
    }

    private void InstantiateObstacles(Transform mapHolder)
    {
        System.Random rand = new System.Random(_currentMap.Seed);

        bool[,] obstacleMap = new bool[_currentMap.MapSize.X, _currentMap.MapSize.Y];

        int obstacleCount = (int)(_currentMap.MapSize.X * _currentMap.MapSize.Y * _currentMap.ObstaclePercent);
        int currentObstacleCount = 0;

        List<Coord> allOpenCoords = new List<Coord>(_allTileCoords);

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randCoord = GetRandomCoord();
            obstacleMap[randCoord.X, randCoord.Y] = true;
            currentObstacleCount++;

            if (randCoord != _currentMap.MapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(_currentMap.MinObstacleHeight, _currentMap.MaxObstacleHeight, (float)rand.NextDouble());
                Vector3 obstaclePos = CoordToPosition(randCoord.X, randCoord.Y);

                Transform newObstacle = Instantiate(ObstaclePrefab, obstaclePos + Vector3.up * obstacleHeight / 2, Quaternion.identity);
                newObstacle.parent = mapHolder;
                float obstacleSize = (1 - OutlinePercent) * TileSize;
                newObstacle.localScale = new Vector3(obstacleSize, obstacleHeight, obstacleSize);

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);

                float colorPercent = randCoord.Y / (float)_currentMap.MapSize.Y;
                obstacleMaterial.color = Color.Lerp(_currentMap.ForegroundColor, _currentMap.BackgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randCoord);
            }
            else
            {
                obstacleMap[randCoord.X, randCoord.Y] = false;
                currentObstacleCount--;
            }
        }

        _shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), _currentMap.Seed));
    }

    private void InstantiateTiles(Transform mapHolder)
    {
        for (int x = 0; x < _currentMap.MapSize.X; x++)
        {
            for (int y = 0; y < _currentMap.MapSize.Y; y++)
            {
                Vector3 tilePos = CoordToPosition(x, y);
                Transform newTile = Instantiate(TilePrefab, tilePos, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale = Vector3.one * (1 - OutlinePercent) * TileSize;
                newTile.parent = mapHolder;

                _tileMap[x, y] = newTile;
            }
        }
    }

    private void ShaffleTileCoords()
    {
        _allTileCoords = new List<Coord>();

        for (int x = 0; x < _currentMap.MapSize.X; x++)
        {
            for (int y = 0; y < _currentMap.MapSize.Y; y++)
            {
                _allTileCoords.Add(new Coord(x, y));
            }
        }

        _shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(_allTileCoords.ToArray(), _currentMap.Seed));
    }

    private bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];

        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(_currentMap.MapCentre);

        mapFlags[_currentMap.MapCentre.X, _currentMap.MapCentre.Y] = true;
        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.X + x;
                    int neighbourY = tile.Y + y;
                    if (x == 0 || y == 0)
                    {
                        if (IsInsideMap(obstacleMap, neighbourX, neighbourY))
                        {
                            if (NotChecked(obstacleMap, mapFlags, neighbourX, neighbourY))
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = _currentMap.MapSize.X * _currentMap.MapSize.Y - currentObstacleCount;

        return targetAccessibleTileCount == accessibleTileCount;
    }

    private static bool NotChecked(bool[,] obstacleMap, bool[,] mapFlags, int neighbourX, int neighbourY)
    {
        return !mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY];
    }

    private static bool IsInsideMap(bool[,] obstacleMap, int neighbourX, int neighbourY)
    {
        return neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1);
    }

    private Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-_currentMap.MapSize.X / 2f + 0.5f + x, 0, -_currentMap.MapSize.Y / 2f + 0.5f + y) * TileSize;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / TileSize + (_currentMap.MapSize.X - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / TileSize + (_currentMap.MapSize.Y - 1) / 2f);
        x = Mathf.Clamp(x, 0, _tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, _tileMap.GetLength(1) - 1);
        return _tileMap[x, y];
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = _shuffledTileCoords.Dequeue();
        _shuffledTileCoords.Enqueue(randomCoord);

        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = _shuffledOpenTileCoords.Dequeue();
        _shuffledOpenTileCoords.Enqueue(randomCoord);
        return _tileMap[randomCoord.X, randomCoord.Y];
    }

    [Serializable]
    public struct Coord : IEquatable<Coord>
    {
        public int X;
        public int Y;

        public Coord(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is Coord coord && Equals(coord);
        }

        public bool Equals(Coord other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            int hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.X == c2.X && c1.Y == c2.Y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    [Serializable]
    public class Map
    {
        public Coord MapSize;

        public Coord MapCentre =>
            new Coord(MapSize.X / 2, MapSize.Y / 2);


        [Range(0, 1)]
        public float ObstaclePercent;

        public int Seed;
        public float MinObstacleHeight;
        public float MaxObstacleHeight;
        public Color ForegroundColor;
        public Color BackgroundColor;
    }
}