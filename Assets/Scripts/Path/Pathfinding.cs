using UnityEngine;
using System.Collections.Generic;
using System.Threading;

//***********************************************************************//
// Adapted from Sebastion Lague: https://github.com/SebLague/Pathfinding //
//***********************************************************************//

public class Pathfinding : MonoBehaviour
{
    private Grid grid;
    private Queue<PathInfo> pathsFound = new Queue<PathInfo>();
    private Queue<PathRequest> pathRequests = new Queue<PathRequest>();
    private Thread pathThread;
    private readonly object requestLock = new object();
    private readonly object foundLock = new object();

    public static readonly object gridLock = new object();
    public static Pathfinding instance;

    public struct PathRequest
    {
        public GameObject owner;
        public Vector3 startPos;
        public Vector3 endPos;

        public PathRequest(GameObject owner, Vector3 startPos, Vector3 endPos)
        {
            this.owner = owner;
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }

    public struct PathInfo
    {
        public GameObject unit;
        public Vector3[] path;

        public PathInfo(GameObject unit, Vector3[] path)
        {
            this.unit = unit;
            this.path = path;
        }
    }

    void Awake()
    {
        grid = GetComponent<Grid>();
        instance = this;
    }

    void Update()
    {
        lock (foundLock)
        {
            while (pathsFound.Count > 0)
            {
                PathInfo pathInfo = pathsFound.Dequeue();
                if (pathInfo.unit)
                    pathInfo.unit.GetComponent<Guard>().PathReceived(pathInfo.path);
            }
        }
    }

    /// <summary>
    /// Request a path to a random position
    /// </summary>
    public void RequestPath(GameObject owner)
    {
        if (pathThread == null || !pathThread.IsAlive)
        {
            pathThread = new Thread(new ParameterizedThreadStart(HandleFindPath));
            pathThread.IsBackground = true;
            pathThread.Start(new PathRequest(owner, owner.transform.position, grid.RandomNode().worldPosition));
        }
        else
            lock (requestLock)
            {
                pathRequests.Enqueue(new PathRequest(owner, owner.transform.position, grid.RandomNode().worldPosition));
            }
    }

    /// <summary>
    /// Request a path to a specific location
    /// </summary>
    public void RequestPath(GameObject owner, Vector3 endPos)
    {
        if (pathThread == null || !pathThread.IsAlive)
        {
            pathThread = new Thread(new ParameterizedThreadStart(HandleFindPath));
            pathThread.IsBackground = true;
            pathThread.Start(new PathRequest(owner, owner.transform.position, endPos));
        }
        else
            lock (requestLock)
            {
                pathRequests.Enqueue(new PathRequest(owner, owner.transform.position, endPos));
            }
    }

    private void HandleFindPath(object aPathRequest)
    {
        PathRequest pathRequest = (PathRequest)aPathRequest;
        Vector3[] path;
        lock (gridLock)
        {
            path = FindPath(pathRequest.startPos, pathRequest.endPos);
        }
        lock (foundLock)
        {
            pathsFound.Enqueue(new PathInfo(pathRequest.owner, path));
        }

        lock (requestLock)
        {
            if (pathRequests.Count > 0)
            {
                pathRequest = pathRequests.Dequeue();
                pathThread = new Thread(new ParameterizedThreadStart(HandleFindPath));
                pathThread.IsBackground = true;
                pathThread.Start(pathRequest);
            }
        }
    }

    Vector3[] FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        Heap<Node> openSet = new Heap<Node>((int)Mathf.Ceil(grid.gridSizeX * grid.gridSizeY * 0.5f));
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode).ToArray();

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.Walkable || closedSet.Contains(neighbour))
                    continue;

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }
        }

        return new Vector3[0];
    }

    /// <summary>
    /// Find the closest node to the hider that is out of the seeker's LOS
    /// </summary>
    public Vector3 GetHidingPos(GameObject hider, GameObject seeker, LayerMask wallMask)
    {
        Node startNode = grid.NodeFromWorldPoint(hider.transform.position);
        Heap<Node> openSet = new Heap<Node>((int)Mathf.Ceil(grid.gridSizeX * grid.gridSizeY * 0.5f));
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        int count = 0;

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (Physics.Raycast(currentNode.worldPosition, seeker.transform.position - currentNode.worldPosition, Vector3.Distance(currentNode.worldPosition, seeker.transform.position), wallMask))
                if (++count == 2)
                    return currentNode.worldPosition;

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.Walkable || closedSet.Contains(neighbour))
                    continue;

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = 0;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }
        }

        return Vector3.zero;
    }

    List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.worldPosition);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public Grid NodeGrid
    {
        get { return grid; }
    }
}