using UnityEngine;
using System.Collections.Generic;

public class BehaviourTree : MonoBehaviour
{
    private BSequence patrolNode = new BSequence();
    private BSequence seekNode = new BSequence();
    private BSelector engageNode = new BSelector();

    public Guard guard;
    public Patrol patrol;
    public Seek seek;
    public Engage engage;

    public BLeaf lastRun = null;

    private void Start()
    {
        patrolNode.Nodes.Add(new BLeaf(this, patrol.FollowPath, patrol.OnFollowPath));
        patrolNode.Nodes.Add(new BLeaf(this, patrol.Search, guard.OnSearch));

        BSelector investigate = new BSelector();
        investigate.Nodes.Add(new BLeaf(this, seek.Follow, seek.OnFollow));
        investigate.Nodes.Add(new BLeaf(this, patrol.FollowPath, seek.OnCheckLocation));
        seekNode.Nodes.Add(investigate);
        seekNode.Nodes.Add(new BLeaf(this, patrol.Search, guard.OnSearch));

        BSelector attack = new BSelector();
        attack.Nodes.Add(new BLeaf(this, engage.Pursue, engage.OnPursue));
        BSequence outOfAmmo = new BSequence();
        outOfAmmo.Nodes.Add(new BLeaf(this, engage.SeekCover, engage.OnSeekCover));
        outOfAmmo.Nodes.Add(new BLeaf(this, engage.Reload, guard.OnSearch));
        attack.Nodes.Add(outOfAmmo);
        engageNode.Nodes.Add(attack);
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
        foreach (BNode child in nodes)
            child.Reset();
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
        if (result == ResultState.FAILURE)
            currentNode = 0;

        if (result == ResultState.SUCCESS)
        {
            currentNode++;
            if (currentNode == nodes.Count)
            {
                currentNode = 0;
                return ResultState.SUCCESS;
            }
            else
                return ResultState.RUNNING;
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
        if (result == ResultState.SUCCESS)
            currentNode = 0;

        if (result == ResultState.FAILURE)
        {
            currentNode++;
            if (currentNode == nodes.Count)
            {
                currentNode = 0;
                return ResultState.FAILURE;
            }
            else
                return ResultState.RUNNING;
        }
        else
            return result;
    }
}

public class BLeaf : BNode
{
    public delegate ResultState RunMethod();
    public delegate void OnTransitionMethod();
    public RunMethod runMethod;
    public OnTransitionMethod onTransition;

    private BehaviourTree ownerTree;

    public BLeaf(BehaviourTree ownerTree, RunMethod runMethod, OnTransitionMethod onTransition = null)
    {
        this.ownerTree = ownerTree;
        this.runMethod = runMethod;
        this.onTransition = onTransition;
    }

    public override ResultState Update()
    {
        // Run a method when transitioning to a new Leaf Node
        if (ownerTree.lastRun == null || ownerTree.lastRun != this)
        {
            ownerTree.lastRun = this;
            if (onTransition != null)
                onTransition();
        }

        return runMethod();
    }
}