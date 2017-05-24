using UnityEngine;
using System.Collections.Generic;

public class BehaviourTree : MonoBehaviour
{
    private BSequence patrolNode = new BSequence();
    private BSequence seekNode = new BSequence();
    private BSelector engageNode = new BSelector();

    private Guard guard;

    private void Start()
    {
        guard = GetComponent<Guard>();
        patrolNode.Nodes.Add(new BLeaf(guard.FollowPath));
        patrolNode.Nodes.Add(new BLeaf(guard.Search));
    }

    #region Getters
    public BNode Patrol
    {
        get { return patrolNode; }
    }

    public BNode Seek
    {
        get { return seekNode; }
    }

    public BNode Engage
    {
        get { return engageNode; }
    }
    #endregion
}

public abstract class BNode
{
    public enum ResultState { RUNNING, SUCCESS, FAILURE }

    //protected BNode parent;
    protected List<BNode> nodes = new List<BNode>();
    protected int currentNode;

    public abstract ResultState Update();

    public void Reset()
    {
        currentNode = 0;
        foreach (BNode node in nodes)
            node.Reset();
    }

    public List<BNode> Nodes
    {
        get { return nodes; }
    }
}

public class BSequence : BNode
{
    public override ResultState Update()
    {
        ResultState result = nodes[currentNode].Update();
        if (result == ResultState.SUCCESS)
        {
            currentNode += 1;
            return currentNode == nodes.Count ? ResultState.SUCCESS : ResultState.RUNNING;
        }
        else
            return result;
    }
}

public class BSelector : BNode
{
    public override ResultState Update()
    {
        ResultState result = nodes[currentNode].Update();
        if (result == ResultState.FAILURE)
        {
            currentNode += 1;
            return currentNode == nodes.Count ? ResultState.FAILURE : ResultState.RUNNING;
        }
        else
            return result;
    }
}

public class BLeaf : BNode
{
    public delegate ResultState RunMethod();
    private RunMethod runMethod;

    public BLeaf(RunMethod runMethod)
    {
        this.runMethod = runMethod;
    }

    public override ResultState Update()
    {
        return runMethod();
    }
}