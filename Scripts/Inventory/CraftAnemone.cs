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

public class CraftAnemoneData
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

 


    // I can have multiple unique fields in here, these cant be set from the outset
    // But colliders can query and send info to the corresponding CMachineData container
    // Accessing thru ulong ids
    public CraftAnemoneData(ulong objId, string name)
    {
        this.objId = objId;
        this.name = name;
        this.isOpened = false;
    }
}
[Component] public record struct CraftAnemoneComponent(bool start, float lerpFacInMiliseconds, float exitSpeed);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class CraftAnemone : SystemBase
{
    public static Dictionary<ulong, CraftAnemoneData> instances;
    private const float cameraOffsetY = 3.0f;
    private const float FOVoffset = 151;
    private const float FOVoriginal = 128.5f;
    private const float cameraOriginalY = 0f;
    private const float marginAllowance = 0.25f;

    static bool isKeyPressed_X = false;

    private bool OnStart(ref bool startBool, ulong objId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        //Initialize all CraftAnemoneData per CraftAnemone Objs
        instances = new Dictionary<ulong, CraftAnemoneData>();

        foreach (var gameObject in World!.Query<CraftAnemoneComponent>())
        {
            // Process component
            //creates a new craftAnemoneData container per CraftAnemone detected
            CraftAnemoneData craftAnemone = new CraftAnemoneData(gameObject.Entity.Id, Entity.FromId(World!, gameObject.Entity.Id).GetComponent<Name>().Value.ToString());
            instances.Add(gameObject.Entity.Id, craftAnemone);

            //Assign to that specific anemone
            CraftAnemoneData craftAnemoneInstance = instances[gameObject.Entity.Id];
            craftAnemoneInstance.isLerpingToAnemone = false;
            craftAnemoneInstance.isOpened = false;

            //craftAnemone.startNodePos = Entity.FromId(World!, gameObject.Entity.Id);

            //Assign the appropriate start pos
            foreach (Entity child in Entity.FromId(World!, gameObject.Entity.Id).GetChildren())
            {
                if (child.TryGetComponent<MatchSignifierComponent>(out MatchSignifierComponent mSignifier) && mSignifier.signifierID == 466)
                {
                    craftAnemone.startNodePos = child.GetComponent<LocalTransform>().Position;
                    craftAnemoneInstance.startNode = child;

                }
            }



        }

        //Initialize Obj Pools per CraftAnemone Obj
        Log($"Init Pool. . . =======(obj id : {objId})==================================================================");

        CraftAnemone_ObjPool pool = new CraftAnemone_ObjPool(objId);

        //Get a raw list of all children under the craftAnemone
        foreach (Entity child in Entity.FromId(World!, pool.objId).GetChildren())
        {
            
            //If child does not have a craftmove, skip!
            if (!child.TryGetComponent<CraftMoveComponent>(out CraftMoveComponent crMove))
            {
            }
            else
            {
                pool.rawChildList.Add(child.Id);
            }

                
        }

        foreach (ulong childID in pool.rawChildList)
        {
            int id_Iterator = 1;
            foreach (List<ulong> objPool in pool.objPools)
            {
                //check if i++ == target msID
                if (Entity.FromId(World!, childID).GetComponent<CraftMoveComponent>().msID == id_Iterator)
                {
                    //add the obj back into the pool, reset its transforms
                    objPool.Add(childID);
                    Log($"{Entity.FromId(World!, childID).GetComponent<Name>().Value.ToString()} is added to objPool[{id_Iterator}]");
                }
                id_Iterator++;

            }
        }
        Log("Complete==============================================================================================");


        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        //Use this
        foreach (var gameObject in World!.Query<CraftAnemoneComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity.Id);

            float lerpFac = gameObject.Component1.lerpFacInMiliseconds / 1000f;

            //Inputs
            if (instances[gameObject.Entity.Id].isCaptured)
            {
                isKeyPressed_X = Input.IsKeyPressed(KeyCode.X);

                if(isKeyPressed_X)
                {
                    //ensable player movement and X key for inventory!
                    Player.instance.isEnabled = true;
                    InventoryController.instance.isEnabled_xInput = true;
                    
                    //This rlly works for dash!
                    //Shoot the player out of the anemone
                    ref LinearVelocity2D lv = ref Player.instance.player.GetComponent<LinearVelocity2D>();
                    lv.Value = new Vector2(0, gameObject.Component1.exitSpeed);

                    instances[gameObject.Entity.Id].isExitingAnemone = true;
                    instances[gameObject.Entity.Id].isEnteredAnemone = false;

                    instances[gameObject.Entity.Id].isCaptured = false;
                }
            }


            ////Do everyth else
            if (instances[gameObject.Entity.Id].isLerpingToAnemone)
            {
                TractorBeam(CraftAnemoneHandler.capturedEntity, gameObject.Entity.Id, lerpFac);
            }

            float yBloom = 1.55f;
            float yWilt = -0.86f; // might be unused but good to know!

            if (instances[gameObject.Entity.Id].isOpened) //if it hasnt been opened, open the craftanemone start node!
            Bloom(instances[gameObject.Entity.Id], yBloom,lerpFac * 0.8f);
            //else if (!isOpened) Bloom with yWilt; 
            //if you want it to wilt, plug in the yWilt value!

            if (instances[gameObject.Entity.Id].isEnteredAnemone && !instances[gameObject.Entity.Id].isExitingAnemone)
            {
                TransitionCamera(instances[gameObject.Entity.Id], lerpFac*1.6f);
            }
            if (instances[gameObject.Entity.Id].isExitingAnemone && !instances[gameObject.Entity.Id].isEnteredAnemone)
            {
                ResetCamera(instances[gameObject.Entity.Id], lerpFac * 1.6f);
            }
        }
    }
    public void Bloom(CraftAnemoneData cr, float yOffset ,float lerpFac)
    {
        Entity startNodeEntity = Entity.FromId(World!, cr.startNode.Id);

        startNodeEntity.GetComponent<Active>().Enabled = true;
        startNodeEntity.GetFirstChild()!.GetComponent<Active>().Enabled = true;

        ref LocalTransform startNodeTransform = ref startNodeEntity.GetComponent<LocalTransform>();

        //Hardcoded transform values

        startNodeTransform.Position = new Vector3(startNodeTransform.Position.X,
                                                  GMath.Lerp(startNodeTransform.Position.Y, yOffset, lerpFac), 
                                                  startNodeTransform.Position.Z);

        if (startNodeTransform.Position.Y >= yOffset - marginAllowance)
        {
            cr.isOpened = true;
        }
    }
    public void TransitionCamera(CraftAnemoneData cr, float lerpFac)
    {
        foreach (var camera in World!.Query<Camera3D>())
        {

            ref LocalTransform cameraTransform = ref Entity.FromId(World!, camera.Entity.Id).GetComponent<LocalTransform>();

            //Lerp transform to 2.2 on positive y
            //And change FOV to 141
            cameraTransform.Position = new Vector3(cameraTransform.Position.X, GMath.Lerp(cameraTransform.Position.Y, cameraOffsetY, lerpFac), cameraTransform.Position.Z);
            Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV = GMath.Lerp(Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV, FOVoffset, lerpFac);

            if (cameraTransform.Position.Y >= cameraOffsetY - marginAllowance && Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV >= FOVoffset - marginAllowance)
            {   //When Done entering
                cr.isEnteredAnemone = false;
                cr.isExitingAnemone = false;
                Log("Done Entering!");
            }

        }
    }
    public void ResetCamera(CraftAnemoneData cr, float lerpFac)
    {
        foreach (var camera in World!.Query<Camera3D>())
        {

            ref LocalTransform cameraTransform = ref Entity.FromId(World!, camera.Entity.Id).GetComponent<LocalTransform>();

            //Lerp transform to 2.2 on positive y
            //And change FOV to 141
            cameraTransform.Position = new Vector3(cameraTransform.Position.X, GMath.Lerp(cameraTransform.Position.Y, cameraOriginalY, lerpFac), cameraTransform.Position.Z);
            Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV = GMath.Lerp(Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV, FOVoriginal, lerpFac);

            if (cameraTransform.Position.Y <= cameraOriginalY + marginAllowance && Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV <= FOVoriginal + marginAllowance)
            {   //When done exiting
                cr.isEnteredAnemone = false;
                cr.isExitingAnemone = false;
                Log("Done Exiting!");
            }


        }
    }
    public void TractorBeam(Entity e, ulong objId, float lerpFac)
    {
        Entity player = Entity.FromId(World!, e.Id);
        Entity anemone = Entity.FromId(World!, objId);

        ref LocalTransform playerTransform = ref player.GetComponent<LocalTransform>();
        ref LocalTransform transform = ref anemone.GetComponent<LocalTransform>();

        //Zero out angular and linearVelocities
        ref LinearVelocity2D playerLinearVelocity = ref player.GetComponent<LinearVelocity2D>();
        ref AngularVelocity2D playerAngularVelocity = ref player.GetComponent<AngularVelocity2D>();
        playerAngularVelocity.Value = 0;
        playerLinearVelocity.Value = Vector2.Zero;

        //Zero out rotation, and reset!
        playerTransform.Rotation = Quaternion.Identity;

        //Interpolate towards anemone!
        playerTransform.Position = new Vector3(GMath.Lerp(playerTransform.Position.X, transform.Position.X, lerpFac * 1.75f),
                                                       GMath.Lerp(playerTransform.Position.Y, transform.Position.Y, lerpFac * 1.75f), 0);

        //If position is within the agreed allowance, stop the tractor beam
        float playerXpos = player.GetComponent<LocalTransform>().Position.X;
        float Xboundary = transform.Position.X;

        float playerYpos = player.GetComponent<LocalTransform>().Position.Y;
        float yBoundary = transform.Position.Y;

        if (Xboundary - 0.125f < playerXpos && playerXpos < Xboundary + 0.125f &&
            yBoundary - 0.125f < playerYpos && playerYpos < yBoundary + 0.125f)
        {
            instances[objId].isLerpingToAnemone = false;
            instances[objId].isCaptured = true;
        }
    }
}

[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class CraftAnemoneHandler : TriggerSystemBase
{
    ////this passes information to CraftAnemone class
    public static Entity capturedEntity;
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        if (Entity.FromId(World!, self.Id).HasComponent<CraftAnemoneComponent>()) { }
        else return;

        if (other.HasComponent<PlayerComponent>())
        {
            LaunchCrafting(self, other);

            //Disable player movement and X key for inventory!
            Player.instance.isEnabled = false;
            InventoryController.instance.isEnabled_xInput = false;
            Player.instance.ResetInputs();
        }
        

    }
    //Unused for now
    //protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    //{
    //    Entity other = Entity.FromId(World!, evt.OtherEntityId);

    //    if (Entity.FromId(World!, self.Id).HasComponent<CraftAnemoneComponent>()) { }
    //    else return;


    //    if (other.HasComponent<PlayerTriggerComponent>())
    //    {

    //    }

    //}

    private void LaunchCrafting(Entity self, Entity other)
    {
        //Force set
        CraftAnemone.instances[self.Id].isEnteredAnemone = false;
        CraftAnemone.instances[self.Id].isExitingAnemone = false;


        AudioManager.instance.PlaySFX("SFX09");
        //This is everything that happens when a player is captured by the anemone
        capturedEntity = other;

        CraftAnemone.instances[self.Id].isOpened = true;

        CraftAnemone.instances[self.Id].isLerpingToAnemone = true;
 
        CraftAnemone.instances[self.Id].isEnteredAnemone = true;
        CraftAnemone.instances[self.Id].isExitingAnemone = false;
  
    }

}


public class CraftAnemone_ObjPool
{
    public List<ulong> rawChildList { get; set; }

    public List<ulong> ms01_ObjectPool { get; set; }
    public List<ulong> ms02_ObjectPool { get; set; }
    public List<ulong> ms03_ObjectPool { get; set; }
    public List<ulong> ms04_ObjectPool { get; set; }
    public List<ulong> ms05_ObjectPool { get; set; }
    public List<ulong> ms06_ObjectPool { get; set; }
    public List<ulong> ms07_ObjectPool { get; set; }

    public List<ulong>[] objPools;

    public ulong objId { get; set; }

    public CraftAnemone_ObjPool(ulong objId)
    {
        this.objId = objId;

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
}
