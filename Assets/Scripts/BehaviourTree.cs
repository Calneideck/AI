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
        patrolNode.Nodes.Add(new BLeaf(guard.FollowPath, guard.OnFollowPath));
        patrolNode.Nodes.Add(new BLeaf(guard.Search, guard.OnSearch));

        BSelector attack = new BSelector();
        attack.Nodes.Add(new BLeaf(guard.Pursue, guard.OnPursue));
        BSequence outOfAmmo = new BSequence();
        outOfAmmo.Nodes.Add(new BLeaf(guard.FollowPath, guard.OnSeekCover));
        outOfAmmo.Nodes.Add(new BLeaf(guard.Reload, guard.OnSearch));
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
            currentNode++;
            if (currentNode == nodes.Count)
            {
                currentNode = 0;
                return ResultState.SUCCESS;
            }
            else
                return ResultState.RUNNING;
            //return currentNode == nodes.Count ? ResultState.SUCCESS : ResultState.RUNNING;
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
            currentNode++;
            if (currentNode == nodes.Count)
            {
                currentNode = 0;
                return ResultState.FAILURE;
            }
            else
                return ResultState.RUNNING;
            //return currentNode == nodes.Count ? ResultState.FAILURE : ResultState.RUNNING;
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

    private static BLeaf lastRun = null;

    public BLeaf(RunMethod runMethod, OnTransitionMethod onTransition = null)
    {
        this.runMethod = runMethod;
        this.onTransition = onTransition;
    }

    public override ResultState Update()
    {
        if (lastRun == null || lastRun != this)
        {
            lastRun = this;
            if (onTransition != null)
                onTransition();
        }

        return runMethod();
    }
}