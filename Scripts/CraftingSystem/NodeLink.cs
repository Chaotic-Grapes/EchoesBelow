using EchoesBelow.Scripts;
using EchoesBelow.Scripts.Audio;
using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;

namespace Scripts.CraftingSystem;

/// <summary>
/// System that processes entities with specific components.
/// This is a pure ECS system: it queries entities and updates their components.
/// </summary>
[Component] public record struct NodeLinkComponent(bool start, bool isRootNode1, bool isRootNode2, bool isRootNode3, bool isRootNode4);
[RequireForUpdate<NodeLinkComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class NodeLink : SystemBase
{
    public static Dictionary<ulong, NodeLinkData> instances;
    public static Entity currentNodeLinkObj;

    private bool OnStart(ref bool startBool, ulong objId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        Log("Combo Machine Alive!");

        instances = new Dictionary<ulong, NodeLinkData>();
        //Log("1");
        //Execute once for all instances
        foreach (var gameObject in World!.Query<NodeLinkComponent>())
        {
            // Process component
            //creates a new combination machine per NodeLinkComponent detected

            //in editor, use isRootNode1, it will point to isRootNode4 ! Weird and buggy Engine behaviour sadly
            if(Entity.FromId(World!, objId).GetComponent<NodeLinkComponent>().isRootNode4)
            {
                //if root node, the south port is always filled
                NodeLinkData nodeLinkData = new NodeLinkData(World!, gameObject.Entity.Id, false, true, false, false);
                instances.Add(gameObject.Entity.Id, nodeLinkData);
            }
            else
            {
                //default unfilled?
                NodeLinkData nodeLinkData = new NodeLinkData(World!, gameObject.Entity.Id, false, false, false, false);
                instances.Add(gameObject.Entity.Id, nodeLinkData);
            }


            Log("Added!");
            foreach (NodeLinkData i in instances.Values)
            {
                Log($"I have: {i.parentObjId}");
            }
        }

        Log($"I am a RootNode4? {Entity.FromId(World!, objId).GetComponent<NodeLinkComponent>().isRootNode4}");
        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        // TODO: Query entities and update components

        foreach (var gameObject in World!.Query<NodeLinkComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity.Id);
        }

        foreach (var gameObject in World!.Query<NodeLinkComponent>())
        {
            //Use this reference for everything!
            NodeLinkData nodeLink = instances[gameObject.Entity.Id];

        }

    }
}
[Component] public record struct NodeLinkTriggerComponent(int NSEW_1234, ulong parentObjId);
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class NodeLinkTrigger
{
    //A simple identifier for CMachineTriggers, North is 1, South is 2, East is 3 and West is 4
}

[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class NodeLinkTriggerHandler : TriggerSystemBase
{


    protected override void OnTriggerStay(Entity self, TriggerEvent evt)
    {
        //Log($"{Entity.FromId(World!, self.Id).GetComponent<Name>().Value.ToString()} triggered by {Entity.FromId(World!, evt.OtherEntityId).GetComponent<Name>().Value.ToString()}", LogLevel.Warning);
        //Filter out all non NodeLink Trigger, SELF = NodeLinkTrigger, EVT = CraftMove particle
        if (Entity.FromId(World!, self.Id).HasComponent<NodeLinkTriggerComponent>() && Entity.FromId(World!, evt.OtherEntityId).HasComponent<CraftMoveComponent>())
        {
            //Enable the E key
            CraftAnemone.isEnabled_EInput = true;

            Entity nodeTriggerObj = Entity.FromId(World!, self.Id);
            Entity parentObj = Entity.FromId(World!, Entity.FromId(World!, self.Id).GetParent()!.Id);
            Entity playerMSobj = Entity.FromId(World!, evt.OtherEntityId);

            //Very important, asign this for everyone
            NodeLink.currentNodeLinkObj = parentObj;

            NodeLinkData nodeLinkData = NodeLink.instances[NodeLink.currentNodeLinkObj.Id];

            NodeLinkData.currentActiveTrigger = self.Id;

            //Check if ports are filled
            //If 'x' port is filled AND the corresponding 'x' trigger is queried here, return
            if (nodeLinkData.port_N_isFilled && Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().NSEW_1234 == 1) return;
            if (nodeLinkData.port_S_isFilled && Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().NSEW_1234 == 2) return;
            if (nodeLinkData.port_E_isFilled && Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().NSEW_1234 == 3) return;
            if (nodeLinkData.port_W_isFilled && Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().NSEW_1234 == 4) return;

            DrawLink(nodeTriggerObj, parentObj, playerMSobj);

            //Which Port did I pass by?
            //Log($"NodeLink Data Port: " + Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().NSEW_1234);
            //Log($"N:{NodeLink.instances[NodeLink.currentNodeLinkObj.Id].port_N_isFilled}  / S:{NodeLink.instances[NodeLink.currentNodeLinkObj.Id].port_S_isFilled}  / E:{NodeLink.instances[NodeLink.currentNodeLinkObj.Id].port_E_isFilled}  / W:{NodeLink.instances[NodeLink.currentNodeLinkObj.Id].port_W_isFilled}");
        }

    }
    protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    {
        if (Entity.FromId(World!, self.Id).HasComponent<NodeLinkTriggerComponent>() && Entity.FromId(World!, evt.OtherEntityId).HasComponent<CraftMoveComponent>())
        {
            //Disable the E key
            CraftAnemone.isEnabled_EInput = false;

            Entity parentObj = Entity.FromId(World!, Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().parentObjId);
            Entity nodeTriggerObj = Entity.FromId(World!, self.Id);

            NodeLinkData nodeLinkData = NodeLink.instances[NodeLink.currentNodeLinkObj.Id];

            NodeLinkData.currentActiveTrigger = 9999; //default empty

            //Check if ports are filled
            //If 'x' port is filled AND the corresponding 'x' trigger is queried here, return
            if (nodeLinkData.port_N_isFilled && Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().NSEW_1234 == 1) return;
            if (nodeLinkData.port_S_isFilled && Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().NSEW_1234 == 2) return;
            if (nodeLinkData.port_E_isFilled && Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().NSEW_1234 == 3) return;
            if (nodeLinkData.port_W_isFilled && Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().NSEW_1234 == 4) return;

            ResetLink(nodeTriggerObj);
            Log("Exitted");
        }
    }
    private void DrawLink(Entity nodeTriggerObj, Entity parentObj, Entity playerMSobj)
    {
        //Retrieve positions
        Vector2 nodeTriggerPos = new Vector2(nodeTriggerObj.GetComponent<LocalTransform>().Position.X, nodeTriggerObj.GetComponent<LocalTransform>().Position.Y);
        Vector2 parentObjPos = new Vector2(parentObj.GetComponent<LocalTransform>().Position.X, parentObj.GetComponent<LocalTransform>().Position.Y);
        Vector2 playerMSPos = new Vector2(playerMSobj.GetComponent<LocalTransform>().Position.X, playerMSobj.GetComponent<LocalTransform>().Position.Y);

        //Retrieve Shapeline, and map the position of the MS snow into parentObj's local space
        ref ShapeLine2D lineRenderer = ref nodeTriggerObj.GetComponent<ShapeLine2D>();
        lineRenderer.A = new Vector2(0 - nodeTriggerPos.X, 0 - nodeTriggerPos.Y);
        lineRenderer.B = new Vector2(playerMSPos.X - parentObjPos.X - nodeTriggerPos.X, playerMSPos.Y - parentObjPos.Y - nodeTriggerPos.Y);
    }
    private void ResetLink(Entity nodeTriggerObj)
    {
        Vector2 nodeTriggerPos = new Vector2(nodeTriggerObj.GetComponent<LocalTransform>().Position.X, nodeTriggerObj.GetComponent<LocalTransform>().Position.Y);

        //Retrieve Shapeline
        ref ShapeLine2D lineRenderer = ref nodeTriggerObj.GetComponent<ShapeLine2D>();
        lineRenderer.A = new Vector2(0 - nodeTriggerPos.X, 0 - nodeTriggerPos.Y);
        lineRenderer.B = new Vector2(0 - nodeTriggerPos.X, 0 - nodeTriggerPos.Y);
    }
}