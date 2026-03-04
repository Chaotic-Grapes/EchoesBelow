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

[Component] public record struct CombinationMachineComponent(float num);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class CombinationMachine : SystemBase
{
    public static Dictionary<ulong, CMachineData> instances;
    protected override void OnCreate()
    {
        Log("Combo Machine Alive!");

        instances = new Dictionary<ulong, CMachineData>();
        Log("1");
        //Execute once for all instances
        foreach (var gameObject in World!.Query<CombinationMachineComponent>())
        {
            // Process component
            //creates a new combination machine per CombinationMachineComponent detected
            CMachineData cm = new CMachineData(gameObject.Entity.Id, Entity.FromId(World!, gameObject.Entity.Id).GetComponent<Name>().Value.ToString());
            instances.Add(gameObject.Entity.Id, cm);
        }
        Log("CombinationMachines are created");
        Log("What is happening...");
    }

    protected override void OnUpdate()
    {
        // TODO: Query entities and update components
        // Example:
        //Log("HELLO!!!2");
        foreach (var gameObject in World!.Query<CombinationMachineComponent>())
        {
           
            string name = instances[gameObject.Entity.Id].name;
            Log("My NEW name is" + name);
            


        }
    }

    protected override void OnDestroy()
    {
        Log("System CombinationMachine destroyed");
    }
}

public class CMachineData
{
    public ulong objId { get; set; }
    public string name { get; set; }
    public bool port_N { get; set; }
    public bool port_S { get; set; }
    public bool port_E { get; set; }
    public bool port_W { get; set; }


    // I can have multiple unique fields in here, these cant be set from the outset
    // But colliders can query and send info to the corresponding CMachineData container
    // Accessing thru ulong ids
    public CMachineData(ulong objId, string name)
    {
        this.objId = objId;
        this.name = name;

        //initialize all ports w a false value
        port_N = false;
        port_S = false;
        port_E = false;
        port_W = false;
    }
}