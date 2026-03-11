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

public class Anemone :SystemBase
{
    public ulong objId { get; set; }
    public string name { get; set; }
    public bool isLerpingToAnemone { get; set; }
    public bool isOpening { get; set; }
    public bool isEnteredAnemone {  get; set; }
    public bool isExitingAnemone { get; set; }
    public bool isCaptured { get; set; }
    public bool isOpened { get; set; }
    public Vector3 startNodePos { get; set; }
    public Entity startNode { get; set; }

    //Obj Pool
    public List<ulong> rawChildList { get; set; }
    public List<ulong> sortedChildList { get; set; }

    public List<ulong> ms01_ObjectPool { get; set; }
    public List<ulong> ms02_ObjectPool { get; set; }
    public List<ulong> ms03_ObjectPool { get; set; }
    public List<ulong> ms04_ObjectPool { get; set; }
    public List<ulong> ms05_ObjectPool { get; set; }
    public List<ulong> ms06_ObjectPool { get; set; }
    public List<ulong> ms07_ObjectPool { get; set; }

    public List<ulong>[] objPools;

    private const ulong emptyId = 99999999999;

    // I can have multiple unique properties in here, these cant be set from the outset
    // But colliders can query and send info to the corresponding container
    // Accessing thru ulong ids
    public Anemone(ulong objId, string name)
    {
        //Set everything!
        this.objId = objId;
        this.name = name;
        this.isOpening = false;
        this.isLerpingToAnemone = false;
        this.isOpening = false;
        this.isEnteredAnemone = false;
        this.isExitingAnemone = false;
        this.isCaptured = false;
        this.isOpened = false;

        //Obj Pool Assignments
        rawChildList = new List<ulong>();
        sortedChildList = new List<ulong>();

        ms01_ObjectPool = new List<ulong>();
        ms02_ObjectPool = new List<ulong>();
        ms03_ObjectPool = new List<ulong>();
        ms04_ObjectPool = new List<ulong>();
        ms05_ObjectPool = new List<ulong>();
        ms06_ObjectPool = new List<ulong>();
        ms07_ObjectPool = new List<ulong>();

        objPools = new List<ulong>[7];
        objPools[0] = ms01_ObjectPool;
        objPools[1] = ms02_ObjectPool;
        objPools[2] = ms03_ObjectPool;
        objPools[3] = ms04_ObjectPool;
        objPools[4] = ms05_ObjectPool;
        objPools[5] = ms06_ObjectPool;
        objPools[6] = ms07_ObjectPool;
    }

    public void UpdateSelection(World world, int msID, Vector3 newPos)
    {
        //Log("START ==============");
        //First we iterate thru the sorted child list
        //We reset EVERYTHING, turn off active for active children
        //Then we add any active children back to the relevant lists
        foreach (ulong childID in sortedChildList)
        {
            //Log("1 Node==============================================");
            for (int i = 1; i < 8; i++)
            {
                //Log("2 Node");
                Entity queriedObj = Entity.FromId(world, childID);
                //Log($"3 Node: {queriedObj.GetComponent<Name>().Value.ToString()}");
                //if (!queriedObj.HasComponent<CraftMoveComponent>()) continue;
                if (queriedObj.TryGetComponent<CraftMoveComponent>(out CraftMoveComponent crM) && queriedObj.GetComponent<CraftMoveComponent>().msID == i)
                {
                    //Log("4 Node");
                    if (queriedObj.GetComponent<Active>().Enabled)
                    {
                        //Log("5 Node");
                        objPools[i-1].Add(queriedObj.Id);
                        //Log("6 Node");
                        queriedObj.GetComponent<CraftMoveComponent>().Enabled = false;
                        Log($"Added {Entity.FromId(world, queriedObj.Id).GetComponent<Name>().Value.ToString()} in objPool {i-1}", LogLevel.Warning);
                        break;
                    }
                }
            }
            //Reset all children
            if(Entity.FromId(world, childID).TryGetComponent<CraftMoveComponent>(out CraftMoveComponent crM2))
            {
                ResetPoolObj(world, childID);
                //Log($"Obj: {Entity.FromId(world,childID).GetComponent<Name>().Value.ToString()} : Reset>>>");
            }

        }

        //Skip the spawning step if msID is 0 , i.e empty slot
        if (msID == 0) return;
        //Log("1 Node");
        int id_Iterator = 1;
        foreach (List<ulong> objPool in objPools)
        {
            //Log("2 Node");
            //Check if obj pool is empty
            if (msID == id_Iterator && objPool.Count > 0)
            {
                
                ulong pulledObjId = objPool[objPool.Count - 1];
                objPool.Remove(pulledObjId);

                Entity.FromId(world, pulledObjId).GetComponent<CraftMoveComponent>().Enabled = true;

                InitPoolObj(world, newPos, pulledObjId);

                Log($"Initialized {Entity.FromId(world,pulledObjId).GetComponent<Name>().Value.ToString()} in objPool {id_Iterator}!", LogLevel.Debug);
                return;
            }
            id_Iterator++;
            //Log("3 Node");
        }
        //Log("5b Nothing found, out to you");
        return;
    }

    public void PlaceNodeAndUpdateSelection(World world, int msID, Vector3 newPos)
    {
        //Log("START ==============");
        //First we iterate thru the sorted child list
        //We detach craftmove and DONT send it back to the pool

        foreach (ulong childID in sortedChildList)
        {
            for (int i = 1; i < 8; i++)
            {
                Entity queriedObj = Entity.FromId(world, childID);

                //if (!queriedObj.HasComponent<CraftMoveComponent>()) continue;
                if (queriedObj.TryGetComponent<CraftMoveComponent>(out CraftMoveComponent crM) && queriedObj.GetComponent<CraftMoveComponent>().msID == i)
                {
                    if (queriedObj.GetComponent<Active>().Enabled)
                    {
                        FreezeNode(world, queriedObj);
                        break;
                    }
                }
            }
            if (Entity.FromId(world, childID).TryGetComponent<CraftMoveComponent>(out CraftMoveComponent crM2))
            {
                ResetPoolObj(world, childID);
            }
        }

        //Spawn based on the iterator
        //Skip the spawning step if msID is 0 , i.e empty slot
        if (msID == 0) return;

        int id_Iterator = 1;
        foreach (List<ulong> objPool in objPools)
        {
            //Check if obj pool is empty
            if (msID == id_Iterator && objPool.Count > 0)
            {

                ulong pulledObjId = objPool[objPool.Count - 1];
                objPool.Remove(pulledObjId);

                Entity.FromId(world, pulledObjId).GetComponent<CraftMoveComponent>().Enabled = true;

                InitPoolObj(world, newPos, pulledObjId);

                return;
            }
            id_Iterator++;
        }

        return;
    }

    private void FreezeNode(World world, Entity queriedObj)
    {
        queriedObj.RemoveComponent<CraftMoveComponent>();
        queriedObj.RemoveComponent<Rigidbody2D>();
        queriedObj.RemoveComponent<CircleCollider2D>();
        queriedObj.RemoveComponent<LinearVelocity2D>();
        queriedObj.RemoveComponent<AngularVelocity2D>();

        ref NodeLinkComponent nl = ref queriedObj.AddComponent<NodeLinkComponent>();

        nl.start = true;

        //Initialise and add NodeLinkData to the instances list
        NodeLinkData nlD = new NodeLinkData(world, queriedObj.Id, false, false, false, false);
        NodeLink.instances.Add(queriedObj.Id, nlD);

        NodeLinkData nodeLinkData = NodeLink.instances[queriedObj.Id];

        //queried obj is the one I want to change
        int fromPort = Entity.FromId(world, NodeLinkData.currentActiveTrigger).GetComponent<NodeLinkTriggerComponent>().NSEW_1234;

        switch (fromPort)
        {
            //The corresponding opposite side shld be marked as filled
            case 1: //North N
                //NodeLink.instances[NodeLink.currentNodeLinkObj.Id].port_N_isFilled = true;
                nodeLinkData.port_S_isFilled = true;
                break;
            case 2: //South S
                nodeLinkData.port_N_isFilled = true;
                break;
            case 3: //East E
                nodeLinkData.port_W_isFilled = true;
                break;
            case 4: //West W
                nodeLinkData.port_E_isFilled = true;
                break;
            default:
                break;
        }
    }

    public void InitPoolObj(World world, Vector3 newPos, ulong pulledObjId)
    {
      
        Entity pulledObj = Entity.FromId(world, pulledObjId);

        //Enable active
        ref Active active = ref pulledObj.GetComponent<Active>();
        active.Enabled = true;

        //Zero out the Linear Velocity before we do any other initialization of parameters
        ref LinearVelocity2D lv = ref pulledObj.GetComponent<LinearVelocity2D>();
        lv.Value = Vector2.Zero;

        //Set to new transform
        ref LocalTransform pulledObjTransform = ref pulledObj.GetComponent<LocalTransform>();
        pulledObjTransform.Position = newPos;


        pulledObj.GetComponent<CraftMoveComponent>().Enabled = true;
        //Set Everything anew, every field that must be set is set here
    }
    private void ResetPoolObj(World world, ulong returningObjId)
    {
        Entity returningObj = Entity.FromId(world, returningObjId);

        //Deactivate whatever is necessary


        //Disable active
        ref Active active = ref returningObj.GetComponent<Active>();
        active.Enabled = false;
        //set to original transform
        ref LocalTransform returningObjTransform = ref returningObj.GetComponent<LocalTransform>();
        returningObjTransform.Position = Vector3.Zero;

        ref LinearVelocity2D lv = ref returningObj.GetComponent<LinearVelocity2D>();
        lv.Value = Vector2.Zero;

        returningObj.GetComponent<CraftMoveComponent>().Enabled = false;
    }
}
