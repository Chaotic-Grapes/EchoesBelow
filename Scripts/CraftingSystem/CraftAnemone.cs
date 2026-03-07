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

[Component] public record struct CraftAnemoneComponent(bool start, float lerpFacInMiliseconds, float exitSpeed, bool awake);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class CraftAnemone : SystemBase
{
    public static Dictionary<ulong, Anemone> instances;
    private const float cameraOffsetY = 3.0f;
    private const float FOVoffset = 151;
    private const float FOVoriginal = 128.5f;
    private const float cameraOriginalY = 0f;
    private const float marginAllowance = 0.25f;

    static bool isKeyPressed_X = false;

    #region SystemBehaviours
    private bool OnAwake(ref bool awakeBool, ulong objId) //Onawake must only play once at the beginning per script.
    {
        if (awakeBool == true) return true;
        awakeBool = true;
        //ToDO ONCE! per Script

        //This effectively executes as many times as there are CraftAnemones. BUT if I place the foreach loop
        //before everything in update. Ultimately this sets something once at the beginning of the script 
        // 1 1 1 1 or 1 or 1 1 1 is effectively 1 in the end. So this can create a List instance once at the start every Scene Load / PlayMode Entrance
        instances = new Dictionary<ulong, Anemone>();

        return true;
    }
    private bool OnStart(ref bool startBool, ulong objId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        //creates a new anemone container per CraftAnemone detected
        Anemone anemone = new Anemone(objId, Entity.FromId(World!,objId).GetComponent<Name>().Value.ToString());

        //Assign the appropriate start pos
        foreach (Entity child in Entity.FromId(World!, objId).GetChildren())
        {
            if (child.TryGetComponent<MatchSignifierComponent>(out MatchSignifierComponent mSignifier) && mSignifier.signifierID == 466)
            {
                anemone.startNodePos = child.GetComponent<LocalTransform>().Position;
                anemone.startNode = child;

            }
        }

        //Initialize Obj Pools per CraftAnemone Obj
        Log($"Init Pool. . . =======(obj id : {objId})===================================");

        //Get a raw list of all children under the craftAnemone
        foreach (Entity child in Entity.FromId(World!, objId).GetChildren())
        {
            //If child does not have a craftmove, skip!
            if (!child.TryGetComponent<CraftMoveComponent>(out CraftMoveComponent crMove))
            {
            }
            else
            {
                anemone.rawChildList.Add(child.Id);
            }
        }

        //Sort everything
        foreach (ulong childID in anemone.rawChildList)
        {
            int id_Iterator = 1;
            foreach (List<ulong> ms_objPool in anemone.objPools)
            {
                //check if i++ == target msID
                if (Entity.FromId(World!, childID).GetComponent<CraftMoveComponent>().msID == id_Iterator)
                {
                    //add the obj back into the pool, reset its transforms
                    ms_objPool.Add(childID);
                }
                id_Iterator++;
            }
        }
        foreach(List<ulong> objPool in anemone.objPools)
        {
            foreach (ulong item in objPool)
            {
                Log($"I contain: {Entity.FromId(World!, item).GetComponent<Name>().Value.ToString()}");
            }
        }
        //Add the instance! AFTER everything is set
        instances.Add(objId, anemone);
        
        Log($"Complete===================(instances.Count: {instances.Count})========");
        //End of Start
        return true;
    }
    protected override void OnUpdate()
    { 
        if (Input.IsKeyPressed(KeyCode.K))
        {
            foreach (Anemone l in CraftAnemone.instances.Values)
            {
                Log("++++++++++++++++++++++++++++++++++++++++");
                Log($"objID stored: {l.objId} name: {l.name}");
                if (l != null)
                {
                    Log($"   >>Im not null!! I contain a reference to {l.objId} / count: {l.ms01_ObjectPool.Count + l.ms02_ObjectPool.Count + l.ms03_ObjectPool.Count + l.ms04_ObjectPool.Count + l.ms05_ObjectPool.Count + l.ms06_ObjectPool.Count + l.ms07_ObjectPool.Count}");
                    foreach (List<ulong> i in l.objPools)
                    {
                        foreach (ulong j in i)
                        {
                            Log($"I contain: {Entity.FromId(World!, j).GetComponent<Name>().Value.ToString()}");
                        }
                    }
                }
                Log($"______________________________________");
            }
        }
        //Call OnAwake 1st
        foreach (var gameObject in World!.Query<CraftAnemoneComponent>())
        {
            bool awake = gameObject.Component1.awake;
            gameObject.Component1.awake = OnAwake(ref awake, gameObject.Entity.Id);
        }
        //Call OnStart 2nd - These MUST be called separately
        foreach (var gameObject in World!.Query<CraftAnemoneComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity.Id);
        }

        //Then all Update funcs
        foreach (var gameObject in World!.Query<CraftAnemoneComponent>())
        {
            float lerpFac = gameObject.Component1.lerpFacInMiliseconds / 1000f;
 
            Anemone cr = instances[gameObject.Entity.Id];

            //Inputs
            if (cr.isCaptured)
            {
                isKeyPressed_X = Input.IsKeyPressed(KeyCode.X);

                if (isKeyPressed_X)
                {
                    //enable player movement and X key for inventory!
                    Player.instance.isEnabled = true;
                    //InventoryController.instance.isEnabled_xInput = true;

                    //This rlly works for dash!
                    //Shoot the player out of the anemone
                    ref LinearVelocity2D lv = ref Player.instance.player.GetComponent<LinearVelocity2D>();
                    lv.Value = new Vector2(0, gameObject.Component1.exitSpeed);

                    cr.isExitingAnemone = true;
                    cr.isEnteredAnemone = false;

                    cr.isCaptured = false;
                }

                if (InventoryController.isPressed_Q)
                {
                    Log($"Will try to spawn msID: {InventoryController.currentSelected_msID}==============");
                    //Take from Pool, if InventoryController.currentSelected_msID == 0, ignore it
                    if (InventoryController.currentSelected_msID != 0)
                    {
                        cr.UpdateSelection_TakeFromPool(InventoryController.currentSelected_msID, new Vector3(4, 0, 0));
                        Log("Updated Selection");
                    }

                }

            }

            // MOVE TOWARDS ANEMONE
            if (cr.isLerpingToAnemone)
            {
                TractorBeam(CraftAnemoneHandler.capturedEntity, gameObject.Entity.Id, lerpFac);
            }

            //BLOOM THE START NODE
            float yBloom = 1.55f;
            float yWilt = -0.86f; // might be unused but good to know!
            if (cr.isOpened) //if it hasnt been opened, open the craftanemone start node!
            Bloom(cr, yBloom,lerpFac * 0.8f);
            //else if (!isOpened) Bloom with yWilt; 
            //if you want it to wilt, plug in the yWilt value!


            //CHANGE CAMERA
            if (cr.isEnteredAnemone && !instances[gameObject.Entity.Id].isExitingAnemone)
            {
                TransitionCamera(instances[gameObject.Entity.Id], lerpFac * 1.6f);
            }
            if (cr.isExitingAnemone && !instances[gameObject.Entity.Id].isEnteredAnemone)
            {
                ResetCamera(instances[gameObject.Entity.Id], lerpFac * 1.6f);
            }
        }
    }
    #endregion

    public void Bloom(Anemone cr, float yOffset ,float lerpFac)
    {
        Entity startNodeEntity = Entity.FromId(World!, cr.startNode.Id);

        startNodeEntity.GetComponent<Active>().Enabled = true;
        startNodeEntity.GetFirstChild()!.GetComponent<Active>().Enabled = true;

        ref LocalTransform startNodeTransform = ref startNodeEntity.GetComponent<LocalTransform>();

        //Hardcoded transform values

        startNodeTransform.Position = new Vector3(startNodeTransform.Position.X,
                                                  GMath.Lerp(startNodeTransform.Position.Y, yOffset, lerpFac * Time.DeltaTime), 
                                                  startNodeTransform.Position.Z);

        if (startNodeTransform.Position.Y >= yOffset - marginAllowance)
        {
            cr.isOpened = true;
        }
    }
    public void TransitionCamera(Anemone cr, float lerpFac)
    {
        foreach (var camera in World!.Query<Camera3D>())
        {

            ref LocalTransform cameraTransform = ref Entity.FromId(World!, camera.Entity.Id).GetComponent<LocalTransform>();

            //Lerp transform to 2.2 on positive y
            //And change FOV to 141
            cameraTransform.Position = new Vector3(cameraTransform.Position.X, GMath.Lerp(cameraTransform.Position.Y, cameraOffsetY, lerpFac * Time.DeltaTime), cameraTransform.Position.Z);
            Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV = GMath.Lerp(Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV, FOVoffset, lerpFac * Time.DeltaTime);

            if (cameraTransform.Position.Y >= cameraOffsetY - marginAllowance && Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV >= FOVoffset - marginAllowance)
            {   //When Done entering
                cr.isEnteredAnemone = false;
                cr.isExitingAnemone = false;
                Log("Done Entering!");
            }

        }
    }
    public void ResetCamera(Anemone cr, float lerpFac)
    {
        foreach (var camera in World!.Query<Camera3D>())
        {

            ref LocalTransform cameraTransform = ref Entity.FromId(World!, camera.Entity.Id).GetComponent<LocalTransform>();

            //Lerp transform to 2.2 on positive y
            //And change FOV to 141

            cameraTransform.Position = new Vector3(cameraTransform.Position.X, GMath.Lerp(cameraTransform.Position.Y, cameraOriginalY, lerpFac * Time.DeltaTime), cameraTransform.Position.Z);
            
            Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV = GMath.Lerp(Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV, FOVoriginal, lerpFac * Time.DeltaTime);

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
        Entity craftAnemone = Entity.FromId(World!, objId);
   
        ref LocalTransform playerTransform = ref player.GetComponent<LocalTransform>();
        ref LocalTransform transform = ref craftAnemone.GetComponent<LocalTransform>();
     
        //Zero out angular and linearVelocities
        ref LinearVelocity2D playerLinearVelocity = ref player.GetComponent<LinearVelocity2D>();
        ref AngularVelocity2D playerAngularVelocity = ref player.GetComponent<AngularVelocity2D>();
        playerAngularVelocity.Value = 0;
        playerLinearVelocity.Value = Vector2.Zero;
      
        //Zero out rotation, and reset!
        playerTransform.Rotation = Quaternion.Identity;
       
        //Interpolate towards anemone!
        playerTransform.Position = new Vector3(GMath.Lerp(playerTransform.Position.X, transform.Position.X, lerpFac * 1.75f *Time.DeltaTime),
                                                       GMath.Lerp(playerTransform.Position.Y, transform.Position.Y, lerpFac * 1.75f *Time.DeltaTime), 0);
    
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
    protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    {
        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        if (Entity.FromId(World!, self.Id).HasComponent<CraftAnemoneComponent>()) { }
        else return;


        if (other.HasComponent<PlayerTriggerComponent>())
        {
            InventoryController.instance.isEnabled_xInput = true;
        }

    }

    private void LaunchCrafting(Entity self, Entity other)
    {
        //Force set
        CraftAnemone.instances[self.Id].isEnteredAnemone = false;
        CraftAnemone.instances[self.Id].isExitingAnemone = false;


        AudioManager.instance.PlaySFX("SFX009");
        //This is everything that happens when a player is captured by the anemone
        capturedEntity = other;

        CraftAnemone.instances[self.Id].isOpened = true;

        CraftAnemone.instances[self.Id].isLerpingToAnemone = true;
 
        CraftAnemone.instances[self.Id].isEnteredAnemone = true;
        CraftAnemone.instances[self.Id].isExitingAnemone = false;
    }

}
