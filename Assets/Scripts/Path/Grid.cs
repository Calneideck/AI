using UnityEngine;
using System.Collections.Generic;

//***********************************************************************//
// Adapted from Sebastion Lague: https://github.com/SebLague/Pathfinding //
//***********************************************************************//

public class Grid : MonoBehaviour
{
    private static float nodeDiameter;

    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public bool DrawNodes;
    public Node[,] nodeGrid;
    [HideInInspector()]
    public int gridSizeX, gridSizeY;
    public static float nodeRadiusSquared;

    void Awake()
    {
        nodeRadiusSquared = nodeRadius * nodeRadius;
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    public void CreateGrid()
    {
        nodeGrid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckBox(worldPoint, Vector3.one * (nodeRadius - 0.01f), Quaternion.identity, unwalkableMask));
                nodeGrid[x, y] = new Node(walkable, worldPoint, x, y);
            }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                // Diagonal
                //if ((x == -1 && y == -1) || (x == 1 && y == -1) || (x == 1 && y == 1) || (x == -1 && y == 1))
                //    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    if (x == -1 && y == -1)
                        if (!nodeGrid[node.gridX - 1, node.gridY + 0].Walkable && !nodeGrid[node.gridX + 0, node.gridY - 1].Walkable)
                            continue;

                    if (x == 1 && y == -1)
                        if (!nodeGrid[node.gridX + 0, node.gridY - 1].Walkable && !nodeGrid[node.gridX + 1, node.gridY + 0].Walkable)
                            continue;

                    if (x == 1 && y == 1)
                        if (!nodeGrid[node.gridX + 0, node.gridY + 1].Walkable && !nodeGrid[node.gridX + 1, node.gridY + 0].Walkable)
                            continue;

                    if (x == -1 && y == 1)
                        if (!nodeGrid[node.gridX + 0, node.gridY + 1].Walkable && !nodeGrid[node.gridX - 1, node.gridY + 0].Walkable)
                            continue;

                    neighbours.Add(nodeGrid[checkX, checkY]);
                }
            }
        
        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return nodeGrid[x, y];
    }

    public void ChangeNodeStatus(bool walkable, int topLeftX, int topLeftY)
    {
        nodeGrid[topLeftX, topLeftY].walkable = walkable;
        nodeGrid[topLeftX + 1, topLeftY].walkable = walkable;
        nodeGrid[topLeftX, topLeftY - 1].walkable = walkable;
        nodeGrid[topLeftX + 1, topLeftY - 1].walkable = walkable;
    }

    public Node RandomNode()
    {
        while (true)
        {
            Node n = nodeGrid[Random.Range(0, gridSizeX), Random.Range(0, gridSizeY)];
            if (n.walkable)
                return n;
        }
    }

    void OnDrawGizmos()
    {
        if (!DrawNodes)
            return;

        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                Color color = Color.white;
                if (!nodeGrid[x, y].Walkable)
                    color = Color.red;

                Gizmos.color = color;
                Gizmos.DrawWireCube(nodeGrid[x, y].worldPosition, Vector3.one * nodeDiameter);
            }
    }
}
