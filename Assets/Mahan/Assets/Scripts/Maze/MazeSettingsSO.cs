using UnityEngine;

[CreateAssetMenu(fileName = "MazeSettings", menuName = "Maze/Maze Settings")]
public class MazeSettingsSO : ScriptableObject
{
    [Header("Maze Size")]
    [Min(2)] public int width = 12;
    [Min(2)] public int height = 12;

    [Header("Cell Size")]
    [Min(0.5f)] public float cellSize = 4f;

    [Header("Wall Look")]
    [Min(0.5f)] public float wallHeight = 4f;
    [Min(0.05f)] public float wallThickness = 0.35f;
    [Min(0.1f)] public float textureRepeatPerWorldUnit = 0.4f;

    [Header("Materials")]
    public Material wallMaterial;
    public Material floorMaterial;

    [Header("Generation")]
    public bool generateOnStart = true;
    public bool randomSeed = true;
    public int seed = 12345;

    [Header("Floor")]
    public bool createFloor = true;
    public float floorY = 0f;
}