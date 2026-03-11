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

public class NodeLinkData : SystemBase
{
    public ulong parentObjId { get; set; }

    public bool port_N_isFilled {  get; set; }
    public bool port_S_isFilled { get; set; }
    public bool port_E_isFilled { get; set; }
    public bool port_W_isFilled { get; set; }

    public Entity Port_N { get; set; }
    public Entity Port_S { get; set; }
    public Entity Port_E { get; set; }
    public Entity Port_W { get; set; }

    public static ulong currentActiveTrigger { get; set; }


    // I can have multiple unique fields in here, these cant be set from the outset
    // But colliders can query and send info to the corresponding CMachineData container
    // Accessing thru ulong ids
    public NodeLinkData(World world, ulong objId, bool port_N_isFilled, bool port_S_isFilled, bool port_E_isFilled, bool port_W_isFilled)
    {
        this.parentObjId = objId;

        //Retrieve the list of children under the comboMachine and sort them appropriately
        List<Entity> rawChildList = Entity.FromId(world, objId).GetChildren();
        foreach (Entity child in rawChildList)
        {
            //Check only CMachine Triggers get thru
            if (!child.TryGetComponent<NodeLinkTriggerComponent>(out NodeLinkTriggerComponent nlTrigger)) continue;

            switch (nlTrigger.NSEW_1234)
            {
                case 1:
                    Port_N = child;
                    break;
                case 2:
                    Port_S = child;
                    break;
                case 3:
                    Port_E = child;
                    break;
                case 4:
                    Port_W = child;
                    break;
                default:
                    //Nothing
                    break;
            }

            ref NodeLinkTriggerComponent nlTrigger2 = ref child.GetComponent<NodeLinkTriggerComponent>();
            nlTrigger2.parentObjId = objId;
        }

        this.port_N_isFilled = port_N_isFilled;
        this.port_S_isFilled = port_S_isFilled;
        this.port_E_isFilled = port_E_isFilled;
        this.port_W_isFilled = port_W_isFilled;
    }
}