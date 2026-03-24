using EchoesBelow.Scripts.Audio;
using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using Scripts.CraftingSystem;
using System.Collections;
using System.Collections.Generic;


namespace EchoesBelow.Scripts;
public class ElementNode
{
    public Entity parent { get; set; }
    public ElementNode node_N { get; set; }
    public ElementNode node_S { get; set; }
    public ElementNode node_E { get; set; }
    public ElementNode node_W { get; set; }

    public Vector3 frozenPos { get; set; }

    public int msID {  get; set; }
    public int msID_N { get; set; }
    public int msID_S { get; set; }
    public int msID_E { get; set; }
    public int msID_W { get; set; }

    public ElementNode(int msID, Entity parentEntity, Vector3 frozenPos)
    {
        this.msID = msID;
        parent = parentEntity;
        this.frozenPos = frozenPos;
    }

    public void SetNode(ElementNode node, nodeSelect nodeSelector)
    {
        switch (nodeSelector)
        {
            case nodeSelect.North:
                node_N = node;
                msID_N = node.msID;
             
                break;
            case nodeSelect.South:
                node_S = node;
                msID_S = node.msID;

                break;
            case nodeSelect.East:
                node_E = node;
                msID_E = node.msID;

                break;            
            case nodeSelect.West:
                node_W = node;
                msID_W = node.msID;

                break;
            default:
                break;
        }
    }

    public void SearchNode(ElementNode n, ref string s)
    {
        if (n == null) { return; }

        //Debug.Log("N: " + n.shapeId);//this is where the magic happens
        s += n.msID;
        s += n.msID_N;
        s += n.msID_S;
        s += n.msID_E;
        s += n.msID_W;

        SearchNode(n.node_N, ref s);
        SearchNode(n.node_S, ref s);
        SearchNode(n.node_E, ref s);
        SearchNode(n.node_W, ref s);

    }

    public void GetNodeList(ElementNode n, ref List<ElementNode> e)
    {
        if (n == null) { return; }

        e.Add(n);

        GetNodeList(n.node_N, ref e);
        GetNodeList(n.node_S, ref e);
        GetNodeList(n.node_E, ref e);
        GetNodeList(n.node_W, ref e);

    }

    public void GetmsIDList(ElementNode n, ref List<int> e)
    {
        if (n == null) { return; }
        //DOESNT WORK YET
        e.Add(msID);

        GetmsIDList(n.node_N, ref e);
        GetmsIDList(n.node_S, ref e);
        GetmsIDList(n.node_E, ref e);
        GetmsIDList(n.node_W, ref e);

    }
    public void GetPosOnTree(ElementNode n, ref List<Vector3> PosList, ref Dictionary<Vector3, Entity> VectorDict)
    {
        if (n == null) { return; }

        PosList.Add(frozenPos);
        VectorDict.Add(frozenPos, parent);

        GetPosOnTree(n.node_N, ref PosList, ref VectorDict);
        GetPosOnTree(n.node_S, ref PosList, ref VectorDict);
        GetPosOnTree(n.node_E, ref PosList, ref VectorDict);
        GetPosOnTree(n.node_W, ref PosList, ref VectorDict);
    }

    public void ClearNode()
    {
        node_N = null;
        node_S = null;
        node_E = null;
        node_W = null;

        frozenPos = Vector3.Zero;

        msID_N = 0;
        msID_S = 0;
        msID_E = 0;
        msID_W = 0;
}
}
public enum nodeSelect {
    North = 1, South = 2, East = 3, West = 4
}
