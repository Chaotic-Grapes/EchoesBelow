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
[Component] public record struct NodeLinkComponent(bool start, bool isRootNode);
[RequireForUpdate<NodeLinkComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class NodeLink : SystemBase
{
    public static Dictionary<ulong, NodeLinkData> instances;
    //public static Entity currentNodeLinkObj;


    //No OnStart for NodeLink cause this component has weird buggy params
    //Thats in craftanemone
    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        //currentNodeLinkObj = null;

        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        // TODO: Query entities and update components

        foreach (var gameObject in World!.Query<NodeLinkComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start);

            //Use this reference for everything!
            NodeLinkData nodeLink = instances[gameObject.Entity.Id];

        }

    }
}
