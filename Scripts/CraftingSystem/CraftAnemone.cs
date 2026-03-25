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
using System;


namespace EchoesBelow.Scripts;

[Component] public record struct CraftAnemoneComponent
    (bool start,
    float lerpFacInMiliseconds, 
    float exitSpeed, 
    bool awake, 
    ulong line01, 
    ulong line02, 
    ulong line03, 
    ulong line04, 
    ulong line05, 
    ulong line06, 
    ulong line07, 
    int doorSignifier);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class CraftAnemone : SystemBase
{
    public static Dictionary<ulong, Anemone> instances;
    public static float cameraOffsetY = 3.0f;
    private const float FOVoffset = 151;
    private const float FOVoriginal = 148.0f;
    private const float cameraOriginalY = 0f;
    private const float marginAllowance = 0.25f;

    static bool isKeyPressed_X = false;
    static bool isKeyPressed_E = false;
    static bool isKeyPressed_Space = false;
    //public static bool isEnabled_EInput = true;
    public static bool isLeaving = false;

    #region SystemBehaviours
    private bool OnAwake(ref bool awakeBool, ulong objId) //Onawake must only play once at the beginning per script.
    {
        if (awakeBool == true) return true;
        awakeBool = true;
        //ToDO ONCE! per Script
        isLeaving = false;
        //This effectively executes as many times as there are CraftAnemones. BUT if I place the foreach loop
        //before everything in update. Ultimately this sets something once at the beginning of the script 
        // 1 1 1 1 or 1 or 1 1 1 is effectively 1 in the end. So this can create a List instance once at the start every Scene Load / PlayMode Entrance
        
        NodeLink.instances = new Dictionary<ulong, NodeLinkData>();
        instances = new Dictionary<ulong, Anemone>();
       
        //Migrate these to NodeLink after M5! and incorporate component values instead of storing port bools in NodeLinkData
        foreach (var gameObject in World!.Query<NodeLinkComponent>())
        {
            if (Entity.FromId(World!, gameObject.Entity.Id).GetComponent<NodeLinkComponent>().isRootNode)
            {
                //if root node, the south port is always filled
                NodeLinkData nodeLinkData = new NodeLinkData(World!, gameObject.Entity.Id, 
                                                             Entity.FromId(World!,gameObject.Entity.Id).GetComponent<LocalTransform>().Position, 
                                                             9, false, true, false, false);
                NodeLink.instances.Add(gameObject.Entity.Id, nodeLinkData);
                Log("Created and Added a node");
            }
        }

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
                anemone.rootNodePos = child.GetComponent<LocalTransform>().Position;
                anemone.rootNode = child;

            }
        }

        //Initialize Obj Pools per CraftAnemone Obj

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

                    anemone.sortedChildList.Add(childID);
                }
                id_Iterator++;
            }
        }

        //Add the instance! AFTER everything is set
        instances.Add(objId, anemone);
  
        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        //if (Input.IsKeyPressed(KeyCode.K))
        //{
        //    //foreach (Anemone l in CraftAnemone.instances.Values)
        //    //{
        //    //    Log("++++++++++++++++++++++++++++++++++++++++");
        //    //    Log($"objID stored: {l.objId} name: {l.name}");
        //    //    if (l != null)
        //    //    {
        //    //        Log($"   >>Im not null!! I contain a reference to {l.objId} / count: {l.ms01_ObjectPool.Count + l.ms02_ObjectPool.Count + l.ms03_ObjectPool.Count + l.ms04_ObjectPool.Count + l.ms05_ObjectPool.Count + l.ms06_ObjectPool.Count + l.ms07_ObjectPool.Count}");
        //    //        foreach (List<ulong> i in l.objPools)
        //    //        {
        //    //            foreach (ulong j in i)
        //    //            {
        //    //                Log($"I contain: {Entity.FromId(World!, j).GetComponent<Name>().Value.ToString()}");
        //    //            }
        //    //        }
        //    //    }
        //    //    Log($"______________________________________");
        //    //}


        //}
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

        isKeyPressed_X = Input.IsKeyPressed(KeyCode.X);
        isKeyPressed_Space = Input.IsKeyPressed(KeyCode.Space);
        //if(isEnabled_EInput) 
        isKeyPressed_E = Input.IsKeyPressed(KeyCode.E);

        //Then all Update funcs
        foreach (var gameObject in World!.Query<CraftAnemoneComponent>())
        {
            float lerpFac = gameObject.Component1.lerpFacInMiliseconds / 1000f;
 
            Anemone cr = instances[gameObject.Entity.Id];

            float yBloom = 1.55f;
            //float yWilt = -0.86f; // might be unused but good to know!

            //Inputs
            if (cr.isCaptured)
            {   

                if (isKeyPressed_X)
                {
                    isLeaving = true;

                    //enable player movement and X key for inventory!
                    Player.instance.isEnabled = true;
                    //InventoryController.instance.isEnabled_xInput = true;

                    //This rlly works for dash!
                    //Shoot the player out of the anemone
                    ref LinearVelocity2D lv = ref Player.instance.player.GetComponent<LinearVelocity2D>();
                    lv.Value = new Vector2(0, gameObject.Component1.exitSpeed);

                    //Add the Rigidbody back to the player
                    ref Rigidbody2D rb = ref Entity.FromId(World!, Player.instance.player.Id).AddComponent<Rigidbody2D>();
                    rb.Mass = 1;
                    rb.LinearDamping = 1f;
                    rb.AngularDamping = 2.4f;
                    rb.GravityScale = 1;
                    rb.Flags = 0;

                    cr.isExitingAnemone = true;
                    cr.isEnteredAnemone = false;

                    cr.isCaptured = false;

                    cr.UpdateSelection(World!, 0, new Vector3(0, yBloom, 0));

                    //InventoryController.instance.isEnabled_xInput = true;
                }

                if (InventoryController.scroll != 0)
                {
                    //Log($"Will try to spawn msID: {InventoryController.currentSelected_msID}==============");
                    cr.UpdateSelection(World!, InventoryController.currentSelected_msID, new Vector3(0, yBloom, 0));
                }
             
                if (isKeyPressed_E && NodeLinkTrigger.isAttachable)
                {
                    if (InventoryController.globalInvIterator == 6)
                    {
                        InventoryController.instance.RemoveFromInventory(2, false, new Vector3(100,100, 0), Vector2.Zero);
                    }
                    else if (InventoryController.globalInvIterator == 5)
                    {
                        InventoryController.instance.RemoveFromInventory(1, false, new Vector3(100, 100, 0), Vector2.Zero);
                    }
                    else
                    {
                        InventoryController.instance.RemoveFromInventory(InventoryController.slotInstances[Entity.FromId(World!, 
                                                                         InventoryController.slotObjIds[InventoryController.globalInvIterator]).GetComponent<Name>().Value.ToString()].storedMsId,
                                                                         false, new Vector3(100, 100, 0), Vector2.Zero);
                    }
                    //Update the selection
                    cr.PlaceNodeAndUpdateSelection(World!, InventoryController.currentSelected_msID, new Vector3(0, yBloom, 0));
                }

                if (isKeyPressed_Space)
                {
                    Anemone anemone = instances[gameObject.Entity.Id];
                    Entity rootNode = anemone.rootNode;

                    NodeLinkData rootNodeLinkInstance = NodeLink.instances[rootNode.Id];

                    List<ElementNode> eNodeList = [];
                    rootNodeLinkInstance.node.GetNodeList(rootNodeLinkInstance.node, ref eNodeList);
                    foreach (ElementNode e in eNodeList)
                    {
                        Log($"Name of Node: {e.parent.GetComponent<Name>().Value.ToString()}", LogLevel.Debug);
                    }

                    string queryString = "";
                    rootNodeLinkInstance.node.SearchNode(rootNodeLinkInstance.node, ref queryString);
                    
                    Log("queryString: " +  queryString);

                    string correctString = "";

                    if (gameObject.Component1.line01 > 0) correctString += gameObject.Component1.line01;
                    if (gameObject.Component1.line02 > 0) correctString += gameObject.Component1.line02;
                    if (gameObject.Component1.line03 > 0) correctString += gameObject.Component1.line03;
                    if (gameObject.Component1.line04 > 0) correctString += gameObject.Component1.line04;
                    if (gameObject.Component1.line05 > 0) correctString += gameObject.Component1.line05;
                    if (gameObject.Component1.line07 > 0) correctString += gameObject.Component1.line07;
                    if (gameObject.Component1.line06 > 0) correctString += gameObject.Component1.line06;

                    Log("Correct string: " + correctString);
                    if (queryString == correctString)
                    {
                        //correct
                        AudioManager.instance.PlaySFX("SFX012");

                        foreach(var door in World!.Query<MatchSignifierComponent>())
                        {
                            if(door.Component1.signifierID == gameObject.Component1.doorSignifier)
                            {
                                //Deactivate Door!
                                AudioManager.instance.PlaySFX("SFX006");
                                ref Active doorActive = ref Entity.FromId(World!,door.Entity.Id).GetComponent<Active>();
                                doorActive.Enabled = false;
                            }
                            //else nothin, if no door found
                        }
                    }
                    else
                    {
                        //wrong
                        AudioManager.instance.PlaySFX("SFX013");
                        Log("WRONG", LogLevel.Warning);
                        foreach (ElementNode e in eNodeList)
                        {
                            //for all elementNodes
                            //Log("e.parent = " + e.parent.GetComponent<Name>().Value.ToString());
                            if(e.parent == rootNode)
                            {
                                NodeLinkData nodeLink = NodeLink.instances[e.parent.Id];
                                nodeLink.EnablePort(1);
                                nodeLink.DisablePort(2);
                                nodeLink.EnablePort(3);
                                nodeLink.EnablePort(4);
                                //Log("1) Cleared for root");
                            }
                            else
                            {
                                NodeLinkData nodeLink = NodeLink.instances[e.parent.Id];
                                nodeLink.EnableAllPorts();
                                //Log("2) cleared for everyone else");
                            }

                            //Reset Shapelines of triggers
                            foreach (Entity trigger in e.parent.GetChildren())
                            {
                                //Reset ALL shapelines
                                if (trigger.HasComponent<ShapeLine2D>())
                                {
                                    ref ShapeLine2D shapeLine = ref trigger.GetComponent<ShapeLine2D>();
                                    shapeLine.A = Vector2.Zero;
                                    shapeLine.B = Vector2.Zero;
                                    //Log("Cleared shapelines: ");
                                }
                            }

                            e.ClearNode();

                            //=======================================================
                            if (!e.parent.HasComponent<MS_IDComponent>()) continue;
                            //=======================================================
                            //For non rootnodes
                            int msID = e.parent.GetComponent<MS_IDComponent>().msID;
                            //Log($"Available ID: {msID}", LogLevel.Warning);

                            cr.ResetSelection(World!);

                            //Instantiate the particles
                            if (MS_Manager.instance.TakeFromPool(msID, 
                                rootNode.GetComponent<LocalTransform>().Position + Entity.FromId(World!,gameObject.Entity.Id).GetComponent<LocalTransform>().Position, 
                                new Vector2(GMath.Random(0.5f, 2f), GMath.Random(0.5f, 2f)), 100000f, false) == MS_Manager.instance.emptyId) continue;
                            //Log("LETS GO");
                        }
                        
                    }
                }

            }

            // MOVE TOWARDS ANEMONE
            if (cr.isLerpingToAnemone)
            {
                TractorBeam(CraftAnemoneHandler.capturedEntity, gameObject.Entity.Id, lerpFac);
            }

            //BLOOM THE START NODE

            if (cr.isOpening) //if it hasnt been opened, open the craftanemone start node!
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
    #region AnemoneFuncs
    public void Bloom(Anemone cr, float yOffset ,float lerpFac)
    {
        Entity startNodeEntity = Entity.FromId(World!, cr.rootNode.Id);

        startNodeEntity.GetComponent<Active>().Enabled = true;
        startNodeEntity.GetFirstChild()!.GetComponent<Active>().Enabled = true;

        ref LocalTransform startNodeTransform = ref startNodeEntity.GetComponent<LocalTransform>();

        //Hardcoded transform values

        startNodeTransform.Position = new Vector3(startNodeTransform.Position.X,
                                                  GMath.Lerp(startNodeTransform.Position.Y, yOffset, lerpFac * Time.DeltaTime), 
                                                  startNodeTransform.Position.Z);

        if (startNodeTransform.Position.Y >= yOffset - marginAllowance)
        {
            cr.isOpening = true;
            //cr.isOpened = true;
            //if(cr.isCaptured)
            //cr.UpdateSelection(World!, InventoryController.currentSelected_msID, new Vector3(0, 1.55f, 0));
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

                InventoryController.instance.isEnabled_LMBInput = true;
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
    #endregion
}

[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class CraftAnemoneHandler : TriggerSystemBase
{
    ////this passes information to CraftAnemone class
    public static Entity capturedEntity;
    protected override void OnTriggerStay(Entity self, TriggerEvent evt)
    {
        if (CraftAnemone.isLeaving) return;
        
        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        if (Entity.FromId(World!, self.Id).HasComponent<CraftAnemoneComponent>() && (other.HasComponent<PlayerTriggerComponent>() || other.HasComponent<PlayerComponent>())) { }
        else return;

        if (!Input.IsKeyPressed(KeyCode.E)) return;


        if (other.HasComponent<PlayerComponent>())
        {
            LaunchCrafting(self, other);
        
            //Disable player movement and X key for inventory!
            Player.instance.isEnabled = false;
            InventoryController.instance.isEnabled_LMBInput = false;
            Player.instance.ResetInputs();

            //Remove Rigidbody on the Player!
            Entity.FromId(World!, Player.instance.player.Id).RemoveComponent<Rigidbody2D>();
        }
        

    }
    //Unused for now
    protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    {
        CraftAnemone.isLeaving = false;

        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        if (Entity.FromId(World!, self.Id).HasComponent<CraftAnemoneComponent>() && Entity.FromId(World!, evt.OtherEntityId).HasComponent<PlayerTriggerComponent>()) { }
        else return;


        if (other.HasComponent<PlayerTriggerComponent>())
        {
            InventoryController.instance.isEnabled_LMBInput = true;
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

        CraftAnemone.instances[self.Id].isOpening = true;

        CraftAnemone.instances[self.Id].isLerpingToAnemone = true;
 
        CraftAnemone.instances[self.Id].isEnteredAnemone = true;
        CraftAnemone.instances[self.Id].isExitingAnemone = false;

        //if(CraftAnemone.instances[self.Id].isOpened)
        CraftAnemone.instances[self.Id].UpdateSelection(World!, InventoryController.currentSelected_msID, new Vector3(0, 1.55f, 0));
    }

}
