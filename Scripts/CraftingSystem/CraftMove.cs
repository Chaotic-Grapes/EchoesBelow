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
using System.Numerics;

namespace Scripts.CraftingSystem;

[Component] public record struct CraftMoveComponent(bool start, int msID);
[RequireForUpdate<CraftMoveComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class CraftMove : SystemBase
{


    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo


        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        // TODO: Query entities and update components
        // Example:
        //Log("HELLO!!!2");
        foreach (var gameObject in World!.Query<CraftMoveComponent>())
        {
       
        }
    }

    protected override void OnDestroy()
    {
     
    }
}
