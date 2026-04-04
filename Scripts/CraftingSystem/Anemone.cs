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
    public bool isWonSpirit {  get; set; }
    public Vector3 rootNodePos { get; set; }
    public Entity rootNode { get; set; }

    //Anim
    public string currentState { get; set; }
    public Entity[] anemoneSprites { get; set; }

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

        //Anim-Centric Array, stores 2
        anemoneSprites = new Entity[2];

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
                    && queriedObj.HasComponent<LinearVelocity2D>()
                    && queriedObj.GetComponent<CraftMoveComponent>().msID == i
                    && queriedObj.GetComponent<Active>().Enabled)
                {
                    objPools[i-1].Add(queriedObj.Id);
                    queriedObj.GetComponent<CraftMoveComponent>().Enabled = false;
                    break;
                }
            }
            //Reset all children
            if(Entity.FromId(world, childID).HasComponent<CraftMoveComponent>() 
            && Entity.FromId(world, childID).HasComponent<LinearVelocity2D>())
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
            for (int i = 1; i <= objPools.Length; i++)
            {
                Entity queriedObj = Entity.FromId(world, childID);
                if (queriedObj.HasComponent<CraftMoveComponent>()
                    && !queriedObj.HasComponent<LinearVelocity2D>()
                    && queriedObj.GetComponent<CraftMoveComponent>().msID == i
                    && queriedObj.GetComponent<Active>().Enabled)
                {
                    Log($"Adding {queriedObj.GetComponent<Name>().Value.ToString()} back to pool");
                    objPools[i - 1].Add(queriedObj.Id);
                    queriedObj.GetComponent<CraftMoveComponent>().Enabled = false;
                    break;
                }
            }

            ResetPoolObj(world, childID);
            Log("Reset!");

            Entity child = Entity.FromId(world, childID);
            if (!child.HasComponent<LinearVelocity2D>()) child.AddComponent<LinearVelocity2D>();
            if (!child.HasComponent<AngularVelocity2D>()) child.AddComponent<AngularVelocity2D>();
        }
    }
    public void PlaceNodeAndUpdateSelection(World world, int msID, Vector3 newPos)
    {
        //Log("START ==============");
        //First we iterate thru the sorted child list
        //We detach craftmove and DONT send it back to the pool
      
        foreach (ulong childID in sortedChildList)
        {
            if(Entity.FromId(world, childID).HasComponent<CraftMoveComponent>()
            && Entity.FromId(world, childID).GetComponent<Active>().Enabled
            && Entity.FromId(world, childID).HasComponent<LinearVelocity2D>())
            {
                Entity queriedObj = Entity.FromId(world, childID);
                FreezeNode(world, queriedObj, NodeLinkTrigger.selectedPort);
                Log("Frozen!");
            }
            else if (Entity.FromId(world, childID).HasComponent<CraftMoveComponent>() 
                  && Entity.FromId(world, childID).HasComponent<LinearVelocity2D>())
            {
                ResetPoolObj(world, childID);
            }
        }
            
        //Spawn based on the iterator
        //Skip the spawning step if msID is 0 , i.e empty slot
        if (msID == 0) return;
        int id_Iterator = 1;

        TutorialController.instance.EnableCoralBuilderConfirm();
        foreach (List<ulong> objPool in objPools)
        {
            Log($"msID == idIterator: {msID == id_Iterator}");
            Log($"objpoolcount > 0: {objPool.Count > 0}");
            Log($"stores item? : {InventoryController.slotInstances[Entity.FromId(world, InventoryController.slotObjIds[InventoryController.globalInvIterator]).GetComponent<Name>().Value.ToString()].isStoringItem}");

            //Check if obj pool is empty
            if (msID == id_Iterator && objPool.Count > 0
                && InventoryController.slotInstances[Entity.FromId(world, InventoryController.slotObjIds[InventoryController.globalInvIterator]).GetComponent<Name>().Value.ToString()].isStoringItem)
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

    private void FreezeNode(World world, Entity queriedObj, Entity selectedPort)
    {
        //if (!AudioManager.sfxEntityDictionary["UI003"].GetComponent<AudioSource>().PlayOnStart)
        //{
        //    AudioManager.instance.PlaySFX("UI003");
        //}
        //else if (!AudioManager.sfxEntityDictionary["UI004"].GetComponent<AudioSource>().PlayOnStart)
        //{
        //    AudioManager.instance.PlaySFX("UI004");
        //}

        int audioRandomiser = GMath.Random(1, 10);

        switch (audioRandomiser)
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                //AudioManager.instance.PlaySFX("UI004");
                if (!AudioManager.sfxEntityDictionary["UI004"].GetComponent<AudioSource>().PlayOnStart)
                {
                    AudioManager.instance.PlaySFX("UI004");
                }
                else
                {
                    AudioManager.instance.PlaySFX("UI004_alt");
                }
                break;
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
                //AudioManager.instance.PlaySFX("UI003");
                if (!AudioManager.sfxEntityDictionary["UI003"].GetComponent<AudioSource>().PlayOnStart)
                {
                    AudioManager.instance.PlaySFX("UI003");
                }
                else
                {
                    AudioManager.instance.PlaySFX("UI003_alt");
                }
                break;
        }

        int msID = queriedObj.GetComponent<CraftMoveComponent>().msID;

        //General Initialization
        //queriedObj.RemoveComponent<CraftMoveComponent>();

        //change crm
        ref CraftMoveComponent crM = ref queriedObj.GetComponent<CraftMoveComponent>();
        crM.Enabled = false;

        //queriedObj.RemoveComponent<CraftMoveComponent>();
        //queriedObj.RemoveComponent<Rigidbody2D>();
        queriedObj.RemoveComponent<LinearVelocity2D>();
        queriedObj.RemoveComponent<AngularVelocity2D>();

        ref NodeLinkTriggerComponent nlt = ref queriedObj.GetComponent<NodeLinkTriggerComponent>();
        nlt.isActiveTrigger = false;

        //NodeLink initialization

        //Initialise and add NodeLinkData to the instances list
        NodeLinkData nlD = new NodeLinkData(world, queriedObj.Id, queriedObj.GetComponent<LocalTransform>().Position , msID, false, false, false, false);
        NodeLink.instances.Add(queriedObj.Id, nlD);
        NodeLink.instances[queriedObj.Id].EnableAllPorts();

        int fromPort = NodeLinkTrigger.portOrientation;

        NodeLinkData nodeLink_nonPlayerControlled = NodeLink.instances[NodeLinkTrigger.nonPlayerControlledEntity.Id];
        NodeLinkData nodeLink_playerControlled = NodeLink.instances[queriedObj.Id];

        ElementNode current_node = nodeLink_nonPlayerControlled.node;
        ElementNode queried_node = nodeLink_playerControlled.node;

        switch (fromPort)
        {
            //The corresponding opposite side shld be marked as filled
            case 1: //North N
                nodeLink_nonPlayerControlled.DisablePort(1);
                nodeLink_playerControlled.DisablePort(2);
               
                current_node.node_N = queried_node;
               
                current_node.msID_N = queried_node.msID;
                queried_node.msID_S = current_node.msID;
                break;
            case 2: //South S
                nodeLink_nonPlayerControlled.DisablePort(2);
                nodeLink_playerControlled.DisablePort(1);

                current_node.node_S = queried_node;

                current_node.msID_S = queried_node.msID;
                queried_node.msID_N = current_node.msID;

                break;
            case 3: //East E
                nodeLink_nonPlayerControlled.DisablePort(3);
                nodeLink_playerControlled.DisablePort(4);

                current_node.node_E = queried_node;

                current_node.msID_E = queried_node.msID;
                queried_node.msID_W = current_node.msID;

                break;
            case 4: //West W
                nodeLink_nonPlayerControlled.DisablePort(4);
                nodeLink_playerControlled.DisablePort(3);

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


        if (pulledObj.HasComponent<CraftMoveComponent>())
        {
            pulledObj.GetComponent<CraftMoveComponent>().Enabled = true;
        }
        if (pulledObj.HasComponent<NodeLinkTriggerComponent>())
        {
            pulledObj.GetComponent<NodeLinkTriggerComponent>().isActiveTrigger = true;
        }

        //Set Everything anew, every field that must be set is set here
        ref LinearVelocity2D lv = ref pulledObj.GetComponent<LinearVelocity2D>();
        lv.Value = Vector2.Zero;
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


        if (returningObj.HasComponent<CraftMoveComponent>())
        {
            returningObj.GetComponent<CraftMoveComponent>().Enabled = false;
        }
        if (returningObj.HasComponent<NodeLinkTriggerComponent>())
        {
            returningObj.GetComponent<NodeLinkTriggerComponent>().isActiveTrigger = false;
        }
        if (returningObj.HasComponent<LinearVelocity2D>())
        {
            ref LinearVelocity2D lv = ref returningObj.GetComponent<LinearVelocity2D>();
            lv.Value = Vector2.Zero;
        }
    }
}
