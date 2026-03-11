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

    public void EnableAllPorts()
    {
        InitBoxCollider(ref Port_N.AddComponent<BoxCollider2D>(), (int)nodeSelect.North);
        InitBoxCollider(ref Port_S.AddComponent<BoxCollider2D>(), (int)nodeSelect.South);
        InitBoxCollider(ref Port_E.AddComponent<BoxCollider2D>(), (int)nodeSelect.East);
        InitBoxCollider(ref Port_W.AddComponent<BoxCollider2D>(), (int)nodeSelect.West);

        port_N_isFilled = false;
        port_S_isFilled = false;
        port_E_isFilled = false;
        port_W_isFilled = false;

        Log("Added 4 Triggers");
    }

    public void DisableAllPorts()
    {
        Port_N.RemoveComponent<BoxCollider2D>();
        Port_S.RemoveComponent<BoxCollider2D>();
        Port_E.RemoveComponent<BoxCollider2D>();
        Port_W.RemoveComponent<BoxCollider2D>();

        port_N_isFilled = true;
        port_S_isFilled = true;
        port_E_isFilled = true;
        port_W_isFilled = true;

        Log("Removed 4 Triggers");
    }
    public void EnablePort(int NSEW_1234)
    {
        //enable a specific collider
        switch (NSEW_1234)
        {
            case (int)nodeSelect.North:
                InitBoxCollider(ref Port_N.AddComponent<BoxCollider2D>(), (int)nodeSelect.North);
                port_N_isFilled = false;
                break;
            case (int)nodeSelect.South:
                InitBoxCollider(ref Port_S.AddComponent<BoxCollider2D>(), (int)nodeSelect.South);
                port_S_isFilled = false;
                break;
            case (int)nodeSelect.East:
                InitBoxCollider(ref Port_E.AddComponent<BoxCollider2D>(), (int)nodeSelect.East);
                port_E_isFilled = false;
                break;
            case (int)nodeSelect.West:
                InitBoxCollider(ref Port_W.AddComponent<BoxCollider2D>(), (int)nodeSelect.West);
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
                Port_N.RemoveComponent<BoxCollider2D>();
                port_N_isFilled = true;
                break;
            case (int)nodeSelect.South:
                Port_S.RemoveComponent<BoxCollider2D>();
                port_S_isFilled = true;
                break;
            case (int)nodeSelect.East:
                Port_E.RemoveComponent<BoxCollider2D>();
                port_E_isFilled = true;
                break;
            case (int)nodeSelect.West:
                Port_W.RemoveComponent<BoxCollider2D>();
                port_W_isFilled = true;
                break;
            default:
                break;

        }
        Log("Removed a trigger");
    }
    private void InitBoxCollider(ref BoxCollider2D bx, int NSEW_1234)
    {
        bx.IsTrigger = true;
        bx.HalfExtents = new Vector2(0.8f, 0.3f);

        switch (NSEW_1234)
        {
            case (int)nodeSelect.North:
            case (int)nodeSelect.South:
                bx.Rotation = 1.571f;
                break;
            case (int)nodeSelect.East:
            case (int)nodeSelect.West:
                //nothin
                break;
        }
    }


}