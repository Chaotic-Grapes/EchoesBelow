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
    public Vector3 rootNodePos { get; set; }
    public Entity rootNode { get; set; }

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
            for (int i = 1; i <= objPools.Length; i++)
            {
                Entity queriedObj = Entity.FromId(world, childID);
                if (queriedObj.HasComponent<CraftMoveComponent>() 
                    && queriedObj.GetComponent<CraftMoveComponent>().msID == i
                    && queriedObj.GetComponent<Active>().Enabled)
                {
                    objPools[i-1].Add(queriedObj.Id);
                    queriedObj.GetComponent<CraftMoveComponent>().Enabled = false;
                    break;
                }
            }
            //Reset all children
            if(Entity.FromId(world, childID).HasComponent<CraftMoveComponent>())
            {
                ResetPoolObj(world, childID);
            }

        }

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

                Log($"Initialized {Entity.FromId(world,pulledObjId).GetComponent<Name>().Value.ToString()} in objPool {id_Iterator}!", LogLevel.Debug);
                return;
            }
            id_Iterator++;
            //Log("3 Node");
        }
        //Log("5b Nothing found, out to you");
        return;
    }
    public void ResetSelection(World world)
    {
        //First we iterate thru the sorted child list
        //We reset EVERYTHING, turn off active for active children
        //Then we add any active children back to the relevant lists

        foreach (ulong childID in sortedChildList)
        {
            Entity queriedObj = Entity.FromId(world, childID);

            if (!queriedObj.HasComponent<MS_IDComponent>()) continue;
            Log($"==={queriedObj.GetComponent<Name>().Value.ToString()}==================================");

            //Reset objs

            //Log("Null? " + (NodeLink.instances[queriedObj.Id] == null));
                
            if (!queriedObj.HasComponent<CraftMoveComponent>())
            {
                int msID = queriedObj.GetComponent<MS_IDComponent>().msID;
                objPools[msID-1].Add(queriedObj.Id);
                //Add
                ref LinearVelocity2D lv = ref queriedObj.AddComponent<LinearVelocity2D>();
                ref AngularVelocity2D av = ref queriedObj.AddComponent<AngularVelocity2D>();
                ref Rigidbody2D rb = ref queriedObj.AddComponent<Rigidbody2D>();
                rb.Mass = 1;
                rb.IsKinematic = true;
                rb.LinearDamping = 5f;
                ref CircleCollider2D circCollider = ref queriedObj.AddComponent<CircleCollider2D>();
                circCollider.Radius = 0.3f;
                ref CraftMoveComponent crMove = ref queriedObj.AddComponent<CraftMoveComponent>();
                //Initialize
                Log("HELLO");


                crMove.moveSpeed = 2.5f;
                crMove.maxSpeed = 2.5f;
                crMove.msID = queriedObj.GetComponent<MS_IDComponent>().msID;

                queriedObj.RemoveComponent<NodeLinkComponent>();
            }
            ResetPoolObj(world, childID);

            //foreach(Entity trigger in queriedObj.GetChildren())
            //{
            //    ref ShapeLine2D shapeLine = ref trigger.GetComponent<ShapeLine2D>();
            //    shapeLine.A = Vector2.Zero;
            //    shapeLine.B = Vector2.Zero;
            //}
        }
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

                if (queriedObj.HasComponent<CraftMoveComponent>()
                && queriedObj.GetComponent<CraftMoveComponent>().msID == i
                && queriedObj.GetComponent<Active>().Enabled)
                {
                    FreezeNode(world, queriedObj);
                    break;
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
        if (!AudioManager.sfxEntityDictionary["SFX011"].GetComponent<AudioSource>().PlayOnStart)
        {
            AudioManager.instance.PlaySFX("SFX011");
        }
        else if(!AudioManager.sfxEntityDictionary["SFX011_alt"].GetComponent<AudioSource>().PlayOnStart)
        {
            AudioManager.instance.PlaySFX("SFX011_alt");
        }
        else
        {
            AudioManager.instance.PlaySFX("SFX011_alt2");
        }

        int msID = queriedObj.GetComponent<CraftMoveComponent>().msID;

        //General Initialization
        //queriedObj.RemoveComponent<CraftMoveComponent>();
        //queriedObj.RemoveComponent<Rigidbody2D>();
        //queriedObj.RemoveComponent<CircleCollider2D>();
        //queriedObj.RemoveComponent<LinearVelocity2D>();
        //queriedObj.RemoveComponent<AngularVelocity2D>();

        //NodeLink initialization
        ref NodeLinkComponent nl = ref queriedObj.AddComponent<NodeLinkComponent>();

        nl.start = true;

        //Initialise and add NodeLinkData to the instances list
        NodeLinkData nlD = new NodeLinkData(world, queriedObj.Id, queriedObj.GetComponent<LocalTransform>().Position , msID, false, false, false, false);
        NodeLink.instances.Add(queriedObj.Id, nlD);
        NodeLink.instances[queriedObj.Id].EnableAllPorts();

        //queried obj is the one I want to change
        int fromPort = Entity.FromId(world, NodeLinkData.currentActiveTrigger).GetComponent<CraftPortComponent>().NSEW_1234;

        //NodeLinkData.currentActiveTrigger == the original INFECTOR nodelink so N is N
        //the new NodeLink is queriedobj N is S and E is W

        NodeLinkData currentNodeLinkInstance = NodeLink.instances[NodeLink.currentNodeLinkObj.Id];
        NodeLinkData queriedNodeLinkInstance = NodeLink.instances[queriedObj.Id];

        ElementNode current_node = currentNodeLinkInstance.node;
        ElementNode queried_node = queriedNodeLinkInstance.node;
        switch (fromPort)
        {
            //The corresponding opposite side shld be marked as filled
            case 1: //North N
                currentNodeLinkInstance.DisablePort(1);
                queriedNodeLinkInstance.DisablePort(2);

                current_node.node_N = queried_node;

                current_node.msID_N = queried_node.msID;
                queried_node.msID_S = current_node.msID;

                break;
            case 2: //South S
                currentNodeLinkInstance.DisablePort(2);
                queriedNodeLinkInstance.DisablePort(1);

                current_node.node_S = queried_node;

                current_node.msID_S = queried_node.msID;
                queried_node.msID_N = current_node.msID;

                break;
            case 3: //East E
                currentNodeLinkInstance.DisablePort(3);
                queriedNodeLinkInstance.DisablePort(4);

                current_node.node_E = queried_node;

                current_node.msID_E = queried_node.msID;
                queried_node.msID_W = current_node.msID;

                break;
            case 4: //West W
                currentNodeLinkInstance.DisablePort(4);
                queriedNodeLinkInstance.DisablePort(3);

                current_node.node_W = queried_node;

                current_node.msID_W = queried_node.msID;
                queried_node.msID_E = current_node.msID;

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
        
        foreach(var child in pulledObj.GetChildren())
        {
            ref Active childActive = ref child.GetComponent<Active>();
            childActive.Enabled = true;
        }
 
        //Set to new transform
        ref LocalTransform pulledObjTransform = ref pulledObj.GetComponent<LocalTransform>();
        pulledObjTransform.Position = newPos;


        pulledObj.GetComponent<CraftMoveComponent>().Enabled = true;
        pulledObj.GetComponent<NodeLinkTriggerComponent>().isActiveTrigger = true;
        //Set Everything anew, every field that must be set is set here
    }
    private void ResetPoolObj(World world, ulong returningObjId)
    {
        Entity returningObj = Entity.FromId(world, returningObjId);

        //Deactivate whatever is necessary


        //Disable active
        ref Active active = ref returningObj.GetComponent<Active>();
        active.Enabled = false;

        foreach (var child in returningObj.GetChildren())
        {
            ref Active childActive = ref child.GetComponent<Active>();
            childActive.Enabled = false;
        }
        //set to original transform
        ref LocalTransform returningObjTransform = ref returningObj.GetComponent<LocalTransform>();
        returningObjTransform.Position = Vector3.Zero;


        if(returningObj.HasComponent<CraftMoveComponent>())
        returningObj.GetComponent<CraftMoveComponent>().Enabled = false;
        returningObj.GetComponent<NodeLinkTriggerComponent>().isActiveTrigger = false;
    }
}
