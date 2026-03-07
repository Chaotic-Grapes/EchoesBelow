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
    public bool isOpened { get; set; }
    public bool isEnteredAnemone {  get; set; }
    public bool isExitingAnemone { get; set; }
    public bool isCaptured { get; set; }
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
        this.isOpened = false;
        this.isLerpingToAnemone = false;
        this.isOpened = false;
        this.isEnteredAnemone = false;
        this.isExitingAnemone = false;
        this.isCaptured = false;

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

    public ulong UpdateSelection(World world, int msID, Vector3 newPos)
    {

        //shld check inside each child obj , maybe after the raw List is sorted, store that list of items
        // order of foreach sortedList > check against every objpool in objpools > do the query , so 3 levels of foreach and 1 conditional operator
        //foreach(ulong childID in sortedChildList)
        //{
        //    int id_Iterator2 = 1;
        //    foreach (List<ulong> objPool in objPools)
        //    {
        //        Log("1a");
       
        //        Entity queriedObj = Entity.FromId(world, childID);
         
        //        if (queriedObj.GetComponent<CraftMoveComponent>().msID == id_Iterator2)
        //        {
        //            Log("3a");
        //            if (queriedObj.GetComponent<Active>().Enabled)
        //            {
        //                Log("4a");
        //                objPool.Add(queriedObj.Id);
        //                ResetPoolObj(world, queriedObj.Id);
        //            }
        //            Log("5a");
        //        }
        //        Log("6a");
    
        //        id_Iterator2++;
        //    }
        //}
        
        
        




        int id_Iterator = 1;
        foreach (List<ulong> objPool in objPools)
        {
            Log("1b - Looking thru pools");
            //Check if obj pool is empty
            if (msID == id_Iterator && objPool.Count > 0)
            {
                Log("2b - Finding in pool");
                ulong pulledObjId = objPool[objPool.Count - 1];
                objPool.Remove(pulledObjId);

                InitPoolObj(world, newPos, pulledObjId);

                Log($"4b - Taken from Pool {id_Iterator}!", LogLevel.Debug);
                return pulledObjId;
            }
            id_Iterator++;
        }
        Log("5b Nothing found");
        return emptyId;
    }



    public void SendToPool(World world, ulong returningObjId)
    {
        int id_Iterator = 1;
        foreach (List<ulong> objPool in objPools)
        {
            //check if i++ == target msID
            if (Entity.FromId(World!, returningObjId).GetComponent<CraftMoveComponent>().msID == id_Iterator)
            {
                //add the obj back into the pool, reset its transforms
                objPool.Add(returningObjId);
                ResetPoolObj(world, returningObjId);

                //returningEntity.GetComponent<Active>().Enabled = false;

                return;
            }
            id_Iterator++;

        }

    }

    public void InitPoolObj(World world, Vector3 newPos, ulong pulledObjId)
    {
      
        Entity pulledObj = Entity.FromId(world, pulledObjId);

        //Enable active
        ref Active active = ref pulledObj.GetComponent<Active>();
        active.Enabled = true;

        //Set to new transform
        ref LocalTransform pulledObjTransform = ref pulledObj.GetComponent<LocalTransform>();
        pulledObjTransform.Position = newPos;

        //Set Everything anew, every field that must be set is set here
    }
    private void ResetPoolObj(World world, ulong returningObjId)
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
