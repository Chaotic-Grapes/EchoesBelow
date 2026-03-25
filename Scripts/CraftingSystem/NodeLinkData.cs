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

public class NodeLinkData : SystemBase
{
    public ulong parentObjId { get; set; }

    public Vector3 frozenPos { get; set; }

    public bool port_N_isFilled {  get; set; }
    public bool port_S_isFilled { get; set; }
    public bool port_E_isFilled { get; set; }
    public bool port_W_isFilled { get; set; }

    public ElementNode node { get; set; }

    public static ulong currentActivePort { get; set; }


    // I can have multiple unique fields in here, these cant be set from the outset
    // But colliders can query and send info to the corresponding CMachineData container
    // Accessing thru ulong ids
    public NodeLinkData(World world, ulong objId, Vector3 frozenPos, int msID, bool port_N_isFilled, bool port_S_isFilled, bool port_E_isFilled, bool port_W_isFilled)
    {
        this.parentObjId = objId;
        this.frozenPos = frozenPos;

        this.port_N_isFilled = port_N_isFilled;
        this.port_S_isFilled = port_S_isFilled;
        this.port_E_isFilled = port_E_isFilled;
        this.port_W_isFilled = port_W_isFilled;

        node = new ElementNode(msID, Entity.FromId(world, objId), this.frozenPos);

       
    }

    public void EnableAllPorts()
    {

        port_N_isFilled = false;
        port_S_isFilled = false;
        port_E_isFilled = false;
        port_W_isFilled = false;

        Log("Added 4 Triggers!!!!!!");
    }

    public void DisableAllPorts()
    {
        port_N_isFilled = true;
        port_S_isFilled = true;
        port_E_isFilled = true;
        port_W_isFilled = true;

        Log("Removed 4 Triggers!!!!!");
    }
    public void EnablePort(int NSEW_1234)
    {
        //enable a specific collider
        switch (NSEW_1234)
        {
            case (int)nodeSelect.North:
                port_N_isFilled = false;
                break;
            case (int)nodeSelect.South:
                port_S_isFilled = false;
                break;
            case (int)nodeSelect.East:
                port_E_isFilled = false;
                break;
            case (int)nodeSelect.West:
                port_W_isFilled = false;
                break;
            default:
                break;

        }

        Log("Added a Trigger");
    }

    public void DisablePort(int NSEW_1234)
    {
        switch (NSEW_1234)
        {
            case (int)nodeSelect.North:
                Log("Removing . . . ");
                Log("Removed...");
                port_N_isFilled = true;
                Log("Port bool checked: " + port_N_isFilled);
                break;
            case (int)nodeSelect.South:
                port_S_isFilled = true;
                break;
            case (int)nodeSelect.East:
                port_E_isFilled = true;
                break;
            case (int)nodeSelect.West:
                port_W_isFilled = true;
                break;
            default:
                break;

        }
        Log("Removed a trigger");
    }

}