using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private MazeSettingsSO settings;

    private const int North = 0;
    private const int East = 1;
    private const int South = 2;
    private const int West = 3;

    private Cell[,] cells;
    private Transform generatedRoot;

    private class Cell
    {
        public bool visited;
        public bool[] walls = new bool[4] { true, true, true, true };
    }

    private struct WallSpan
    {
        public int startX;
        public int startY;
        public int length;
        public bool horizontal;

        public WallSpan(int startX, int startY, int length, bool horizontal)
        {
            this.startX = startX;
            this.startY = startY;
            this.length = length;
            this.horizontal = horizontal;
        }
    }

    private void Start()
    {
        if (settings != null && settings.generateOnStart)
        {
            GenerateMaze();
        }
    }

    [ContextMenu("Generate Maze")]
    public void GenerateMaze()
    {
        if (settings == null)
        {
            Debug.LogError("MazeGenerator: No MazeSettingsSO assigned.", this);
            return;
        }

        if (settings.width < 2 || settings.height < 2)
        {
            Debug.LogError("MazeGenerator: Width and Height must be at least 2.", this);
            return;
        }

        ClearMaze();
        CreateData();
        RunBacktracker();
        BuildMaze();
    }

    [ContextMenu("Clear Maze")]
    public void ClearMaze()
    {
        Transform oldRoot = transform.Find("GeneratedMaze");
        if (oldRoot != null)
        {
            if (Application.isPlaying)
            {
                Destroy(oldRoot.gameObject);
            }
            else
            {
                DestroyImmediate(oldRoot.gameObject);
            }
        }
    }

    private void CreateData()
    {
        cells = new Cell[settings.width, settings.height];

        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                cells[x, y] = new Cell();
            }
        }
    }

    private void RunBacktracker()
    {
        int seedToUse = settings.randomSeed ? Random.Range(int.MinValue, int.MaxValue) : settings.seed;
        System.Random rng = new System.Random(seedToUse);

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int current = new Vector2Int(0, 0);
        cells[current.x, current.y].visited = true;
        int visitedCount = 1;
        int totalCells = settings.width * settings.height;

        while (visitedCount < totalCells)
        {
            List<int> availableDirections = GetUnvisitedDirections(current);

            if (availableDirections.Count > 0)
            {
                int dir = availableDirections[rng.Next(availableDirections.Count)];
                Vector2Int next = GetNeighbor(current, dir);

                RemoveWallBetween(current, next, dir);

                stack.Push(current);
                current = next;
                cells[current.x, current.y].visited = true;
                visitedCount++;
            }
            else if (stack.Count > 0)
            {
                current = stack.Pop();
            }
            else
            {
                break;
            }
        }
    }

    private List<int> GetUnvisitedDirections(Vector2Int cell)
    {
        List<int> dirs = new List<int>();

        if (cell.y + 1 < settings.height && !cells[cell.x, cell.y + 1].visited) dirs.Add(North);
        if (cell.x + 1 < settings.width && !cells[cell.x + 1, cell.y].visited) dirs.Add(East);
        if (cell.y - 1 >= 0 && !cells[cell.x, cell.y - 1].visited) dirs.Add(South);
        if (cell.x - 1 >= 0 && !cells[cell.x - 1, cell.y].visited) dirs.Add(West);

        return dirs;
    }

    private Vector2Int GetNeighbor(Vector2Int cell, int dir)
    {
        switch (dir)
        {
            case North: return new Vector2Int(cell.x, cell.y + 1);
            case East: return new Vector2Int(cell.x + 1, cell.y);
            case South: return new Vector2Int(cell.x, cell.y - 1);
            case West: return new Vector2Int(cell.x - 1, cell.y);
            default: return cell;
        }
    }

    private void RemoveWallBetween(Vector2Int current, Vector2Int next, int dir)
    {
        cells[current.x, current.y].walls[dir] = false;

        switch (dir)
        {
            case North:
                cells[next.x, next.y].walls[South] = false;
                break;
            case East:
                cells[next.x, next.y].walls[West] = false;
                break;
            case South:
                cells[next.x, next.y].walls[North] = false;
                break;
            case West:
                cells[next.x, next.y].walls[East] = false;
                break;
        }
    }

    private void BuildMaze()
    {
        generatedRoot = new GameObject("GeneratedMaze").transform;
        generatedRoot.SetParent(transform, false);

        if (settings.createFloor)
        {
            CreateFloor();
        }

        bool[,] horizontalWalls = new bool[settings.width, settings.height + 1];
        bool[,] verticalWalls = new bool[settings.width + 1, settings.height];

        for (int x = 0; x < settings.width; x++)
        {
            for (int y = 0; y < settings.height; y++)
            {
                if (cells[x, y].walls[North]) horizontalWalls[x, y + 1] = true;
                if (cells[x, y].walls[South]) horizontalWalls[x, y] = true;
                if (cells[x, y].walls[West]) verticalWalls[x, y] = true;
                if (cells[x, y].walls[East]) verticalWalls[x + 1, y] = true;
            }
        }

        List<WallSpan> spans = new List<WallSpan>();
        spans.AddRange(MergeHorizontal(horizontalWalls));
        spans.AddRange(MergeVertical(verticalWalls));

        foreach (WallSpan span in spans)
        {
            CreateWallSegment(span);
        }
    }

    private List<WallSpan> MergeHorizontal(bool[,] horizontalWalls)
    {
        List<WallSpan> spans = new List<WallSpan>();

        for (int y = 0; y < settings.height + 1; y++)
        {
            int x = 0;
            while (x < settings.width)
            {
                if (!horizontalWalls[x, y])
                {
                    x++;
                    continue;
                }

                int startX = x;
                int length = 1;
                x++;

                while (x < settings.width && horizontalWalls[x, y])
                {
                    length++;
                    x++;
                }

                spans.Add(new WallSpan(startX, y, length, true));
            }
        }

        return spans;
    }

    private List<WallSpan> MergeVertical(bool[,] verticalWalls)
    {
        List<WallSpan> spans = new List<WallSpan>();

        for (int x = 0; x < settings.width + 1; x++)
        {
            int y = 0;
            while (y < settings.height)
            {
                if (!verticalWalls[x, y])
                {
                    y++;
                    continue;
                }

                int startY = y;
                int length = 1;
                y++;

                while (y < settings.height && verticalWalls[x, y])
                {
                    length++;
                    y++;
                }

                spans.Add(new WallSpan(x, startY, length, false));
            }
        }

        return spans;
    }

    private void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(generatedRoot, false);

        float mazeWidth = settings.width * settings.cellSize;
        float mazeHeight = settings.height * settings.cellSize;

        floor.transform.localPosition = new Vector3(
            mazeWidth * 0.5f,
            settings.floorY - 0.05f,
            mazeHeight * 0.5f
        );

        floor.transform.localScale = new Vector3(mazeWidth, 0.1f, mazeHeight);

        if (settings.floorMaterial != null)
        {
            Renderer renderer = floor.GetComponent<Renderer>();
            renderer.sharedMaterial = settings.floorMaterial;
        }
    }

    private void CreateWallSegment(WallSpan span)
    {
        float lengthWorld = span.length * settings.cellSize;
        float halfHeight = settings.wallHeight * 0.5f;

        GameObject root = new GameObject(span.horizontal ? "Wall_H" : "Wall_V");
        root.transform.SetParent(generatedRoot, false);

        if (span.horizontal)
        {
            float centerX = (span.startX * settings.cellSize) + (lengthWorld * 0.5f);
            float z = span.startY * settings.cellSize;

            root.transform.localPosition = new Vector3(centerX, settings.floorY + halfHeight, z);
            root.transform.localRotation = Quaternion.identity;
        }
        else
        {
            float x = span.startX * settings.cellSize;
            float centerZ = (span.startY * settings.cellSize) + (lengthWorld * 0.5f);

            root.transform.localPosition = new Vector3(x, settings.floorY + halfHeight, centerZ);
            root.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        }

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(lengthWorld, settings.wallHeight, settings.wallThickness);
        collider.center = Vector3.zero;

        CreateQuadSide(root.transform, "Front", Vector3.forward, lengthWorld);
        CreateQuadSide(root.transform, "Back", Vector3.back, lengthWorld);
    }

    private void CreateQuadSide(Transform parent, string quadName, Vector3 localForwardOffset, float lengthWorld)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = quadName;
        quad.transform.SetParent(parent, false);

        float halfThickness = settings.wallThickness * 0.5f;
        quad.transform.localPosition = localForwardOffset * halfThickness;

        if (localForwardOffset == Vector3.back)
        {
            quad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        quad.transform.localScale = new Vector3(lengthWorld, settings.wallHeight, 1f);

        Collider quadCollider = quad.GetComponent<Collider>();
        if (quadCollider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(quadCollider);
            }
            else
            {
                DestroyImmediate(quadCollider);
            }
        }

        MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
        if (settings.wallMaterial != null)
        {
            renderer.sharedMaterial = settings.wallMaterial;
        }

        ApplyTextureTiling(renderer, lengthWorld);
    }

    private void ApplyTextureTiling(Renderer renderer, float lengthWorld)
    {
        if (renderer == null)
            return;

        float repeatX = Mathf.Max(1f, lengthWorld * settings.textureRepeatPerWorldUnit);

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);

        if (renderer.sharedMaterial != null)
        {
            if (renderer.sharedMaterial.HasProperty("_MainTex"))
            {
                block.SetVector("_MainTex_ST", new Vector4(repeatX, 1f, 0f, 0f));
            }

            if (renderer.sharedMaterial.HasProperty("_BaseMap"))
            {
                block.SetVector("_BaseMap_ST", new Vector4(repeatX, 1f, 0f, 0f));
            }
        }

        renderer.SetPropertyBlock(block);
    }
}