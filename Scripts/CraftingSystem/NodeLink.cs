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
                Log($"I have: {i.objId}");
            }
        }

        Log($"I am a RootNode1? {Entity.FromId(World!,objId).GetComponent<NodeLinkComponent>().isRootNode1}");
        Log($"I am a RootNode2? {Entity.FromId(World!, objId).GetComponent<NodeLinkComponent>().isRootNode2}");
        Log($"I am a RootNode3? {Entity.FromId(World!, objId).GetComponent<NodeLinkComponent>().isRootNode3}");
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


    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        //Log($"{Entity.FromId(World!, self.Id).GetComponent<Name>().Value.ToString()} triggered by {Entity.FromId(World!, evt.OtherEntityId).GetComponent<Name>().Value.ToString()}", LogLevel.Warning);
        //Filter out all non NodeLink Trigger, SELF = NodeLinkTrigger, EVT = CraftMove particle
        if (Entity.FromId(World!, self.Id).HasComponent<NodeLinkTriggerComponent>() && Entity.FromId(World!, evt.OtherEntityId).HasComponent<CraftMoveComponent>())
        {
            Entity parentObj = Entity.FromId(World!, Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().parentObjId);
            ////Use this reference for everything
            NodeLinkData nodeLink = NodeLink.instances[parentObj.Id];

            //TODO
            ref NodeLinkComponent nl = ref Entity.FromId(World!, nodeLink.objId).GetComponent<NodeLinkComponent>();

            //Which Port did I pass by?
            Log($"NodeLink Data Port: " + Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().NSEW_1234);
            Log($"N:{NodeLink.instances[parentObj.Id].port_N_isFilled}  / S:{NodeLink.instances[parentObj.Id].port_S_isFilled}  / E:{NodeLink.instances[parentObj.Id].port_E_isFilled}  / W:{NodeLink.instances[parentObj.Id].port_W_isFilled}");
        }

    }

    protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    {

    }


}