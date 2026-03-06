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
using System.Collections.Generic;


namespace EchoesBelow.Scripts;

public class Anemone :SystemBase
{
    public ulong objId { get; set; }
    public string name { get; set; }
    public bool isLerpingToAnemone { get; set; }
    public bool isOpened { get; set; }
    public bool isEnteredAnemone {  get; set; }
    public bool isExitingAnemone { get; set; }
    public bool isCaptured { get; set; }
    public Vector3 startNodePos { get; set; }
    public Entity startNode { get; set; }

    //Obj Pool
    public List<ulong> rawChildList { get; set; }

    public List<ulong> ms01_ObjectPool { get; set; }
    public List<ulong> ms02_ObjectPool { get; set; }
    public List<ulong> ms03_ObjectPool { get; set; }
    public List<ulong> ms04_ObjectPool { get; set; }
    public List<ulong> ms05_ObjectPool { get; set; }
    public List<ulong> ms06_ObjectPool { get; set; }
    public List<ulong> ms07_ObjectPool { get; set; }

    public List<ulong>[] objPools;

    private const ulong emptyId = 99999999999;

    // I can have multiple unique fields in here, these cant be set from the outset
    // But colliders can query and send info to the corresponding CMachineData container
    // Accessing thru ulong ids
    public Anemone(ulong objId, string name)
    {
        this.objId = objId;
        this.name = name;
        this.isOpened = false;

        //Obj Pool Assignments
        rawChildList = new List<ulong>();

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

    public ulong UpdateSelection_TakeFromPool(int msID, Vector3 newPos)
    {
        int id_Iterator = 1;
        foreach (List<ulong> objPool in objPools)
        {
            Log("1 - Looking thru pools");
            //Check if obj pool is empty
            if (msID == id_Iterator && objPool.Count > 0)
            {
                Log("2 - Finding in pool");
                ulong pulledObjId = objPool[objPool.Count - 1];
                objPool.Remove(pulledObjId);

                InitPoolObj(newPos, pulledObjId);

                Log($"4 - Taken from Pool {id_Iterator}!", LogLevel.Debug);
                return pulledObjId;
            }
            id_Iterator++;
        }
        Log("Nothing found");
        return emptyId;
    }



    public void SendToPool(ulong returningObjId)
    {
        int id_Iterator = 1;
        foreach (List<ulong> objPool in objPools)
        {
            //check if i++ == target msID
            if (Entity.FromId(World!, returningObjId).GetComponent<CraftMoveComponent>().msID == id_Iterator)
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

    private void InitPoolObj(Vector3 newPos, ulong pulledObjId)
    {
        Log("3 - initialising. . .");
        Entity pulledObj = Entity.FromId(World!, pulledObjId);
        Log("--1");
        //Enable active
        pulledObj.GetComponent<Active>().Enabled = true;
        Log("--2");
        //Set to new transform
        ref LocalTransform pulledObjTransform = ref pulledObj.GetComponent<LocalTransform>();
        pulledObjTransform.Position = newPos;
        Log(">>Tried to initialise objid: " + pulledObjId);
        //Set Everything anew, every field that must be set is set here


    }
    private void ResetPoolObj(ulong returningObjId)
    {
        Entity returningObj = Entity.FromId(World!, returningObjId);

        //Deactivate whatever is necessary


        //Disable active
        returningObj.GetComponent<Active>().Enabled = false;
        //set to original transform
        ref LocalTransform returningObjTransform = ref returningObj.GetComponent<LocalTransform>();
        returningObjTransform.Position = Vector3.Zero;


    }
}
