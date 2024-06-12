using Unity.Mathematics;

public partial struct GridIndexer
{
    public readonly int width;
    public readonly int height;
    public readonly float cellSize;
    private readonly float3 playerPosition;
    private readonly quaternion playerRotation;

    public GridIndexer(int width, int height, float cellSize, float3 playerPosition, quaternion playerRotation)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.playerPosition = playerPosition;
        this.playerRotation = playerRotation;
    }

    public int this[float3 worldPosition]
    {
        get
        {
            float3 relativePosition = math.rotate(math.inverse(playerRotation), worldPosition - playerPosition);
            int gridX = (int)(math.floor(relativePosition.x / cellSize) + width / 2);
            int gridZ = (int)(math.floor(relativePosition.z / cellSize) + height / 2);
            return gridZ * width + gridX;
        }
    }

    public float3 this[int flattenedIndex]
    {
        get
        {
            int gridZ = flattenedIndex / width;
            int gridX = flattenedIndex % width;
            float relativeX = (gridX - width / 2) * cellSize;
            float relativeZ = (gridZ - height / 2) * cellSize;
            float3 relativePosition = new float3(relativeX, 0f, relativeZ);
            return playerPosition + math.rotate(playerRotation, relativePosition);
        }
    }
}
