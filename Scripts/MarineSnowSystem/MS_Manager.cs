using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;
using System.Security.Cryptography;


namespace EchoesBelow.Scripts.MarineSnowSystem;
[Component] public record struct MS_ManagerComponent(

    int msID,
    bool start
    
);

[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class MS_Manager : SystemBase
{
    //default objs like MS_Managers (non obj pool objs will have an msID of 0)
    public static List<ulong> ms01_ObjectPool;
    public static List<ulong> ms02_ObjectPool;
    public static List<ulong> ms03_ObjectPool;
    public static List<ulong> ms04_ObjectPool;
    public static List<ulong> ms05_ObjectPool;
    public static List<ulong> ms06_ObjectPool;
    public static List<ulong> ms07_ObjectPool;

    public static List<ulong>[] objPools;

    public static MS_Manager instance;
    public ulong poolContainerId;

    public static float globalDecayTime;

    public ulong emptyId = 99999999999;
    private Vector3 poolLocation = new Vector3(10000, 10000, 0);

    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        Log("MS MANAGER CREATED");
        //This is only ever called once, so there is only one instance assignment
        //initialize
        instance = this;

        ulong MS_Manager_Id = 0; //default
        //For each MS_Manager only
        //Create the lists
        foreach (var gameObject in World!.Query<MS_ManagerComponent>())
        {
            ulong objID = gameObject.Entity.Id;
            int msID = gameObject.Component1.msID;

            //For MS Manager instance
            poolContainerId = objID;

            ////Log("Initialize Pools ! poolContainerId: " + poolContainerId, LogLevel.Debug);

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

            MS_Manager_Id = objID;
        }

        //Send all found MSnows parented under MSManager into the objpools
        List<Entity> MSchildren = Entity.FromId(World!,MS_Manager_Id).GetChildren();

        //Send Marine Snows to relevant obj pools
        foreach(Entity MS_snow in MSchildren)
        {
            SendToPool(MS_snow.Id);
        }
        Log("MS_MANAGER FULLY INITIALIZED");

        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        foreach (var gameObject in World!.Query<MS_ManagerComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start);

            //Do everyth else


        }
    }
    public ulong TakeFromPool(int msID, Vector3 newPos, float decayTime)
    {
        int id_Iterator = 1;
        foreach(List<ulong>objPool in objPools)
        {
            //Check if obj pool is empty
            if (msID == id_Iterator && objPool.Count > 0)
            {
                ulong pulledObjId = objPool[objPool.Count - 1];
                objPool.Remove(pulledObjId);

                InitPoolObj(newPos, pulledObjId, decayTime);

                ////Log($"Taken from Pool {id_Iterator}!",LogLevel.Debug);
                return pulledObjId;
            }
            id_Iterator++;
        }
        return emptyId;
    }



    public void SendToPool(ulong returningObjId)
    {
        int id_Iterator = 1;
        foreach (List<ulong> objPool in objPools)
        {
            //check if i++ == target msID
            if (Entity.FromId(World!, returningObjId).GetComponent<MS_IDComponent>().msID == id_Iterator)
            {
                //add the obj back into the pool, reset its transforms
                objPool.Add(returningObjId);
                ResetPoolObj(returningObjId);

                //returningEntity.GetComponent<Active>().Enabled = false;

                return;
            }
            id_Iterator++;

        }
        
    }        
    
    private void InitPoolObj(Vector3 newPos, ulong pulledObjId, float decayTime)
    {
        //Set to new transform
        Entity pulledEntity = Entity.FromId(World!, pulledObjId);

        pulledEntity.GetComponent<Active>().Enabled = true;

        ref LocalTransform transform = ref pulledEntity.GetComponent<LocalTransform>();
        transform.Position = newPos;
        //Remove from parent
        //pulledEntity.Detach();

        //Add gravity and forces
        ref Rigidbody2D rb = ref pulledEntity.AddComponent<Rigidbody2D>();
        rb.GravityScale = 0.1f;
        rb.Mass = 1;
        rb.LinearDamping = 1.4f;
        //rb.Flags = 2u;
        rb.Flags |= Rigidbody2D.FLAG_KINEMATIC | Rigidbody2D.FLAG_USE_GRAVITY;

        ref LinearVelocity2D lv = ref pulledEntity.AddComponent<LinearVelocity2D>();
        lv.Value.X = GMath.Random(0.5f, 2f);
        pulledEntity.AddComponent<AngularVelocity2D>();
        //Add Decay Component HARDCODED
        ref MS_DecayComponent decay = ref pulledEntity.AddComponent<MS_DecayComponent>();
        decay.decayTime = decayTime;

    }
    private void ResetPoolObj(ulong objID)
    {
        Entity targetEntity = Entity.FromId(World!, objID);
        ref LocalTransform transform = ref targetEntity.GetComponent<LocalTransform>();
        transform.Position = poolLocation;

        Entity returningEntity = Entity.FromId(World!, objID);
        if (returningEntity.HasComponent<Rigidbody2D>()) returningEntity.RemoveComponent<Rigidbody2D>();
        if (returningEntity.HasComponent<LinearVelocity2D>()) returningEntity.RemoveComponent<LinearVelocity2D>();
        if (returningEntity.HasComponent<AngularVelocity2D>()) returningEntity.RemoveComponent<AngularVelocity2D>();
        if (returningEntity.HasComponent<MS_DecayComponent>()) returningEntity.RemoveComponent<MS_DecayComponent>();

        returningEntity.GetComponent<Active>().Enabled = false;
        //targetEntity.AttachTo(Entity.FromId(World!,poolContainerId));
    }
}
