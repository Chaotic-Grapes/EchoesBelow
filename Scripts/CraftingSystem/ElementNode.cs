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
    public Entity parent { get; private set; }
    public ElementNode node_N { get; private set; }
    public ElementNode node_S { get; private set; }
    public ElementNode node_E { get; private set; }
    public ElementNode node_W { get; private set; }


    public int msID {  get; set; }
    public int msID_N { get; set; }
    public int msID_S { get; set; }
    public int msID_E { get; set; }
    public int msID_W { get; set; }

    public ElementNode(int msID, Entity parentEntity)
    {
        this.msID = msID;
        parent = parentEntity;
    }

    public void SetNode(ElementNode node, nodeSelect nodeSelector)
    {
        switch (nodeSelector)
        {
            case nodeSelect.North:
                node_N = node;
                //Assign each msID ?
                break;
            case nodeSelect.South:
                node_S = node;
                
                break;
            case nodeSelect.East:
                node_E = node;
                
                break;            
            case nodeSelect.West:
                node_W = node;
                
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
}
public enum nodeSelect {
    North = 1, South = 2, East = 3, West = 4
}
