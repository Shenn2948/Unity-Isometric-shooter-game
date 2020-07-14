using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform TilePrefab;
    public Transform ObstaclePrefab;
    public Transform NavMeshFloor;
    public Transform NavMeshMaskPrefab;
    public Vector2 MapSize;
    public Vector2 MaxMapSize;

    [Range(0, 1)]
    public float OutlinePercent;

    [Range(0, 1)]
    public float ObstaclePercent;

    public float tileSize;

    private List<Coord> _allTileCoords;
    private Queue<Coord> _shuffledTileCoords;

    public int Seed = 10;
    private Coord _mapCentre;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
    }

    public void GenerateMap()
    {
        ShaffleTileCoords();

        _mapCentre = new Coord((int)MapSize.x / 2, (int)MapSize.y / 2);

        Transform mapHolder = GetRefreshedMapHolder();

        InstantiateTiles(mapHolder);
        InstantiateObstacles(mapHolder);

        NavMeshFloor.localScale = new Vector3(MaxMapSize.x, MaxMapSize.y) * tileSize;
    }

    private Transform GetRefreshedMapHolder()
    {
        string holderName = "Generated Map";
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
        bool[,] obstacleMap = new bool[(int)MapSize.x, (int)MapSize.y];

        int obstacleCount = (int)(MapSize.x * MapSize.y * ObstaclePercent);
        int currentObstacleCount = 0;
        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randCoord = GetRandomCoord();
            obstacleMap[randCoord.X, randCoord.Y] = true;
            currentObstacleCount++;

            if (randCoord != _mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePos = CoordToPosition(randCoord.X, randCoord.Y);
                Transform newObstacle = Instantiate(ObstaclePrefab, obstaclePos + Vector3.up * .5f, Quaternion.identity);
                newObstacle.parent = mapHolder;
                newObstacle.localScale = Vector3.one * (1 - OutlinePercent) * tileSize;
            }
            else
            {
                obstacleMap[randCoord.X, randCoord.Y] = false;
                currentObstacleCount--;
            }
        }
    }

    private void InstantiateTiles(Transform mapHolder)
    {
        for (int x = 0; x < MapSize.x; x++)
        {
            for (int y = 0; y < MapSize.y; y++)
            {
                Vector3 tilePos = CoordToPosition(x, y);
                Transform newTile = Instantiate(TilePrefab, tilePos, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale = Vector3.one * (1 - OutlinePercent) * tileSize;
                newTile.parent = mapHolder;
            }
        }
    }

    private void ShaffleTileCoords()
    {
        _allTileCoords = new List<Coord>();

        for (int x = 0; x < MapSize.x; x++)
        {
            for (int y = 0; y < MapSize.y; y++)
            {
                _allTileCoords.Add(new Coord(x, y));
            }
        }

        _shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(_allTileCoords.ToArray(), Seed));
    }

    private bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];

        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(_mapCentre);

        mapFlags[_mapCentre.X, _mapCentre.Y] = true;
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

        int targetAccessibleTileCount = (int)(MapSize.x * MapSize.y - currentObstacleCount);

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

    private Vector3 CoordToPosition(float x, float y)
    {
        return new Vector3(-MapSize.x / 2 + 0.5f + x, 0, -MapSize.y / 2 + 0.5f + y) * tileSize;
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = _shuffledTileCoords.Dequeue();
        _shuffledTileCoords.Enqueue(randomCoord);

        return randomCoord;
    }

    public readonly struct Coord : IEquatable<Coord>
    {
        public readonly int X;
        public readonly int Y;

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
            return X == other.X &&
                   Y == other.Y;
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
}