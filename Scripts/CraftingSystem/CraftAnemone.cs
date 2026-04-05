using EchoesBelow.Scripts.Audio;
using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using Scripts;
using Scripts.CraftingSystem;
using Scripts.Level_Toys;
using Scripts.Menu;
using System;
using System.Collections;
using System.Collections.Generic;


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
    public static CraftAnemone instance;

    public static Dictionary<ulong, Anemone> instances;
    public static float cameraOffsetY = 3.0f;
    private const float FOVoffset = 151;
    private const float FOVoriginal = 148.0f;
    private const float cameraOriginalY = 0f;
    private const float marginAllowance = 0.25f;

    public static float gloablLightOriginalIntensity;

    public static bool isRMB_pressed = false;
    public static bool isLMB_pressed = false;
    static bool isKeyPressed_Space = false;
    //public static bool isEnabled_EInput = true;
    public static bool isLeaving = false;

    public static List<Entity> listOfContacts;

    //Declare AnimStates
    //These states are static in a way, presets shared by all CraftAnemones
    //SetAnimState however, is per instance of CraftAnemone
    public static AnimState idleEnterState = new AnimState("idleEnterState", 0, 0, 19, 30, true);
    public static AnimState idleState = new AnimState("idleState", 2, 0, 67, 24, true);

    public static AnimState vibrateEnterState = new AnimState("vibrateEnterState", 7, 0, 6, 24, true);
    public static AnimState vibrateState = new AnimState("vibrateState", 8, 0, 10, 24, true);
    public static AnimState vibrateExitState = new AnimState("vibrateExitState", 7, 11, 6, 48, true);

    public static AnimState releaseState = new AnimState("releaseState", 10, 0, 6, 24, true);
    public static AnimState captureState = new AnimState("captureState", 9, 0, 6, 24, true);
    public static AnimState closedStillState = new AnimState("closedStillState", 9, 5, 1, 24, false);


    #region SystemBehaviours
    private bool OnAwake(ref bool awakeBool, ulong objId) //Onawake must only play once at the beginning per script.
    {
        if (awakeBool == true) return true;
        awakeBool = true;

        instance = this;

        //ToDO ONCE! per Script
        isLeaving = false;
        //This effectively executes as many times as there are CraftAnemones. BUT if I place the foreach loop
        //before everything in update. Ultimately this sets something once at the beginning of the script 
        // 1 1 1 1 or 1 or 1 1 1 is effectively 1 in the end. So this can create a List instance once at the start every Scene Load / PlayMode Entrance

        NodeLink.instances = new Dictionary<ulong, NodeLinkData>();
        instances = new Dictionary<ulong, Anemone>();
        listOfContacts = new List<Entity>();

        //Migrate these to NodeLink after M5! and incorporate component values instead of storing port bools in NodeLinkData
        foreach (var gameObject in World!.Query<NodeLinkComponent>())
        {
            if (Entity.FromId(World!, gameObject.Entity.Id).GetComponent<NodeLinkComponent>().isRootNode)
            {
                //if root node, the south port is always filled
                NodeLinkData nodeLinkData = new NodeLinkData(World!, gameObject.Entity.Id,
                                                             Entity.FromId(World!, gameObject.Entity.Id).GetComponent<LocalTransform>().Position,
                                                             9, false, true, false, false);
                NodeLink.instances.Add(gameObject.Entity.Id, nodeLinkData);
                //Log("Created and Added a node");
            }
        }


        foreach(var waterCurr in World!.Query<AddCurrentComponent>())
        {
            waterCurr.Entity.RemoveComponent<ShapeCircle2D>();
        }

        return true;
    }
    private bool OnStart(ref bool startBool, ulong objId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        //creates a new anemone container per CraftAnemone detected
        Anemone anemone = new Anemone(objId, Entity.FromId(World!, objId).GetComponent<Name>().Value.ToString());

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
            //If child does not have a craftmove, skip! but add the anim objects
            if (!child.TryGetComponent<CraftMoveComponent>(out CraftMoveComponent crMove))
            {
                if (!child.HasComponent<NodeLinkComponent>())
                {
                    //Log("Found my anims!")
                    int i = 0;
                    foreach (Entity grandChild in child.GetChildren())
                    {
                        if (grandChild.HasComponent<SpriteSheetAnimation2D>())
                        {
                            anemone.anemoneSprites[i++] = grandChild;
                            //Log("Anim Anemone Sprite Found: " + grandChild.GetComponent<Name>().Value.ToString());
                        }
                    }
                }
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

        //Set Anims
        SetAnimState(idleState, objId, World!);

        foreach (var light in World!.Query<Light2D>())
        {
            if (light.Entity.GetComponent<Light2D>().LightType == Light2D.Type.Directional)
            {
                gloablLightOriginalIntensity = light.Entity.GetComponent<Light2D>().Intensity;
            }
        }
        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
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

        if (!PauseMenuController.instance.isPausable) return;

        isRMB_pressed = Input.IsMousePressed(1) || Input.IsGamepadButtonPressed(0,GamepadButton.B);
        isKeyPressed_Space = Input.IsKeyPressed(KeyCode.Space) || Input.IsGamepadButtonPressed(0, GamepadButton.Y);
        //if(isEnabled_EInput) 
        isLMB_pressed = Input.IsMousePressed(0) || Input.IsGamepadButtonPressed(0, GamepadButton.A);

        //Then all Update funcs
        foreach (var gameObject in World!.Query<CraftAnemoneComponent>())
        {
            float lerpFac = gameObject.Component1.lerpFacInMiliseconds / 1000f;

            Anemone cr = instances[gameObject.Entity.Id];
            //Log("Currentstate: " + cr.currentState);
            float yBloom = 1.55f;
            //float yWilt = -0.86f; // might be unused but good to know!

            //Inputs
            if (cr.isCaptured)
            {

                if (isRMB_pressed)
                {
                    ////Might swap out for idle? or set a timer for releaseState in Update
                    //SetAnimState(releaseState, cr.objId, World!);
                    ExitAnemone(gameObject, cr, yBloom);

                    //InventoryController.instance.isEnabled_xInput = true;
                }

                if (InventoryController.mouseScroll != 0)
                {
                    //Log($"Will try to spawn msID: {InventoryController.currentSelected_msID}==============");
                    cr.UpdateSelection(World!, InventoryController.currentSelected_msID, new Vector3(0, yBloom, 0));
                }

                if (isLMB_pressed && NodeLinkTrigger.isAttachable)
                {
                    if (InventoryController.globalInvIterator == 6)
                    {
                        InventoryController.instance.RemoveFromInventory(2, false, new Vector3(100, 100, 0), Vector2.Zero);
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

                    //cr.UpdateSelection(World!, InventoryController.currentSelected_msID, new Vector3(0, yBloom, 0));
                }

                if (isKeyPressed_Space)
                {
                    Anemone anemone = instances[gameObject.Entity.Id];
                    Entity rootNode = anemone.rootNode;

                    NodeLinkData rootNodeLinkInstance = NodeLink.instances[rootNode.Id];

                    List<ElementNode> elementsInTreeList = [];
                    rootNodeLinkInstance.node.GetNodeList(rootNodeLinkInstance.node, ref elementsInTreeList);
                    //foreach (ElementNode elementInTree in elementsInTreeList)
                    //{
                    //    Log($"Name of Node: {elementInTree.Entity.GetComponent<Name>().Value.ToString()}", LogLevel.Debug);
                    //}

                    string queryString = "";
                    rootNodeLinkInstance.node.SearchNode(rootNodeLinkInstance.node, ref queryString);

                    Log("queryString: " + queryString);

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
                        SpiritOfTheOcean.instance.TheSpiritBeckonsThee(true);
                        cr.isWonSpirit = true;
                    }
                    else
                    {
                        //wrong
                        AudioManager.instance.PlaySFX("SFX013");
                        Log("WRONG", LogLevel.Warning);
                        foreach (ElementNode elementInTree in elementsInTreeList)
                        {

                            foreach (Entity port in elementInTree.Entity.GetChildren())
                            {
                                //Reset ALL shapelines
                                if (port.HasComponent<ShapeLine2D>())
                                {
                                    ref ShapeLine2D shapeLine = ref port.GetComponent<ShapeLine2D>();
                                    shapeLine.A = Vector2.Zero;
                                    shapeLine.B = Vector2.Zero;
                                }
                            }
                            //Instantiate the particles

                            if (elementInTree.Entity.HasComponent<CraftMoveComponent>()
                            && !elementInTree.Entity.HasComponent<NodeLinkComponent>())
                            {
                                int msID = elementInTree.Entity.GetComponent<CraftMoveComponent>().msID;

                                //Log("msID from tree: " + elementInTree.Entity.GetComponent<Name>().Value.ToString() + " / msid: " + msID);

                                if (msID == 1 || msID == 2)
                                {
                                    MS_Manager.instance.TakeFromPool(msID,
                                    rootNode.GetComponent<LocalTransform>().Position + Entity.FromId(World!, gameObject.Entity.Id).GetComponent<LocalTransform>().Position,
                                    new Vector2(GMath.Random(0.5f, 2f), GMath.Random(0.5f, 2f)), 15f, false);
                                }
                                else
                                {
                                    MS_Manager.instance.TakeFromPool(msID,
                                    rootNode.GetComponent<LocalTransform>().Position + Entity.FromId(World!, gameObject.Entity.Id).GetComponent<LocalTransform>().Position,
                                    new Vector2(GMath.Random(0.5f, 2f), GMath.Random(0.5f, 2f)), 100000f, false);
                                }
                            }
                        }

                        cr.ResetSelection(World!);

                        //Identical to Start function, recreate and re-initialize the list of instances of NodeLinks
                        NodeLink.instances = new Dictionary<ulong, NodeLinkData>();
                        listOfContacts = new List<Entity>();

                        //Find the RootNode, and begin again
                        foreach (var gameObject2 in World!.Query<NodeLinkComponent>())
                        {
                            if (Entity.FromId(World!, gameObject2.Entity.Id).GetComponent<NodeLinkComponent>().isRootNode)
                            {
                                //if root node, the south port is always filled
                                NodeLinkData nodeLinkData = new NodeLinkData(World!, gameObject2.Entity.Id,
                                                                             Entity.FromId(World!, gameObject2.Entity.Id).GetComponent<LocalTransform>().Position,
                                                                             9, false, true, false, false);
                                NodeLink.instances.Add(gameObject2.Entity.Id, nodeLinkData);
                                //Log("ReCreated and ReAdded a node");
                            }
                        }
                    }

                    ExitAnemone(gameObject, cr, yBloom);

                }

            }

            // MOVE TOWARDS ANEMONE
            if (cr.isLerpingToAnemone)
            {
                TractorBeam(CraftAnemoneHandler.capturedEntity, gameObject.Entity.Id, lerpFac);
            }

            //BLOOM THE START NODE

            if (cr.isOpening) //if it hasnt been opened, open the craftanemone start node!
                Bloom(cr, yBloom, lerpFac * 0.8f);
            //else if (!isOpened) Bloom with yWilt; 
            //if you want it to wilt, plug in the yWilt value!


            //CHANGE CAMERA
            if (cr.isEnteredAnemone && !instances[gameObject.Entity.Id].isExitingAnemone)
            {
                TransitionCamera(instances[gameObject.Entity.Id], lerpFac * 1.6f, cameraOffsetY, FOVoffset);

                //ResetGlobalLight(lerpFac);

            }
            if (cr.isExitingAnemone && !instances[gameObject.Entity.Id].isEnteredAnemone)
            {
                ResetCamera(instances[gameObject.Entity.Id], lerpFac * 1.6f);
            }

            if (cr.isWonSpirit && isLeaving)
            {
                CamFollow.instance.CamShake(true, 0.056f);
                if (Input.IsGamepadConnected(0))
                {
                    Input.SetGamepadVibration(0, 0.8f, 0.8f);
                }
            }

            //AnimCentric
            AnimManager(cr, gameObject.Entity.Id, lerpFac);
        }
    }

    private void ExitAnemone(GrapeEngine.Scripting.Internal.Query.QueryResult<CraftAnemoneComponent> gameObject, Anemone cr, float yBloom)
    {
        AudioManager.instance.PlaySFX("SFX010_Track01");
        //Might swap out for idle? or set a timer for releaseState in Update
        SetAnimState(releaseState, cr.objId, World!);

        ref ZIndex2D zIndex = ref Player.instance.player.GetComponent<ZIndex2D>();
        zIndex.ZOrder = 0;

        isLeaving = true;

        //Reinstate list of contacts
        listOfContacts.Clear();

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
                                                  GMath.Lerp(startNodeTransform.Position.Y, yOffset, lerpFac * 2.5f * Time.DeltaTime), 
                                                  startNodeTransform.Position.Z);

        if (startNodeTransform.Position.Y >= yOffset - marginAllowance)
        {
            cr.isOpening = true;
            //cr.isOpened = true;
            //if(cr.isCaptured)
            //cr.UpdateSelection(World!, InventoryController.currentSelected_msID, new Vector3(0, 1.55f, 0));
        }
    }
    public void TransitionCamera(Anemone cr, float lerpFac, float offSetY, float offSetFOV)
    {
        foreach (var camera in World!.Query<Camera3D>())
        {

            ref LocalTransform cameraTransform = ref Entity.FromId(World!, camera.Entity.Id).GetComponent<LocalTransform>();

            //Lerp transform to 2.2 on positive y
            //And change FOV to 141
            cameraTransform.Position = new Vector3(cameraTransform.Position.X, GMath.Lerp(cameraTransform.Position.Y, offSetY, lerpFac * Time.DeltaTime), cameraTransform.Position.Z);
            Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV = GMath.Lerp(Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV, offSetFOV, lerpFac * Time.DeltaTime);

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

                //InventoryController.instance.isEnabled_RMBInput = true;
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
        playerTransform.Position = new Vector3(GMath.Lerp(playerTransform.Position.X, transform.Position.X, lerpFac * 11.75f *Time.DeltaTime),
                                                       GMath.Lerp(playerTransform.Position.Y, transform.Position.Y + 0.56f, lerpFac * 11.75f *Time.DeltaTime), 0);
    
        //If position is within the agreed allowance, stop the tractor beam
        float playerXpos = player.GetComponent<LocalTransform>().Position.X;
        float Xboundary = transform.Position.X;
     
        float playerYpos = player.GetComponent<LocalTransform>().Position.Y;
        float yBoundary = transform.Position.Y + 0.56f;

        float allowance = 0.05f;

        if (Xboundary - allowance < playerXpos && playerXpos < Xboundary + allowance &&
            yBoundary - allowance < playerYpos && playerYpos < yBoundary + allowance)
        {
            ref ZIndex2D zIndex = ref player.GetComponent<ZIndex2D>();
            zIndex.ZOrder = -3;


            instances[objId].isLerpingToAnemone = false;
            instances[objId].isCaptured = true;
        }
    }
    private void TransitGlobalLight(float lerpFac)
    {
        foreach (var light in World!.Query<Light2D>())
        {
            if (light.Entity.GetComponent<Light2D>().LightType == Light2D.Type.Directional)
            {
                ref Light2D l = ref light.Entity.GetComponent<Light2D>();
                l.Intensity = GMath.Lerp(l.Intensity, CraftAnemone.gloablLightOriginalIntensity-3f, lerpFac * 20f * Time.DeltaTime);
            }
        }
    }

    private void ResetGlobalLight(float lerpFac)
    {
        foreach (var light in World!.Query<Light2D>())
        {
            if (light.Entity.GetComponent<Light2D>().LightType == Light2D.Type.Directional)
            {
                ref Light2D l = ref light.Entity.GetComponent<Light2D>();
                l.Intensity = GMath.Lerp(l.Intensity, CraftAnemone.gloablLightOriginalIntensity, lerpFac * 20f * Time.DeltaTime);
            }
        }
    }

    #endregion

    #region AnimStateHandler

    private void AnimManager(Anemone cr, ulong objID, float lerpFac)
    {
        if (cr.currentState == vibrateExitState.name)
        {
            ResetCamera(cr, lerpFac * 20f);

            //ResetGlobalLight(lerpFac);

            if (cr.anemoneSprites[0].GetComponent<AnimationState2D>().CurrentFrame >= (vibrateExitState.frameLength - 1))
            {
                SetAnimState(idleEnterState, cr.objId, World!);
            }
        }
        else if (cr.currentState == vibrateEnterState.name)
        {
            TransitionCamera(instances[objID], lerpFac * 4f, 0f, 143);

            //TransitGlobalLight(lerpFac);

            if (cr.anemoneSprites[0].GetComponent<AnimationState2D>().CurrentFrame >= (vibrateEnterState.frameLength - 1))
            {
                SetAnimState(vibrateState, cr.objId, World!);
            }
        }
        else if (cr.currentState == releaseState.name)
        {
            InventoryController.instance.isEnabled_vomitInput = false;
            if (cr.anemoneSprites[0].GetComponent<AnimationState2D>().CurrentFrame >= (releaseState.frameLength-1))
            {
                SetAnimState(idleEnterState, cr.objId, World!);
                InventoryController.instance.isEnabled_vomitInput = true;
            }
        }
        else if (cr.currentState == idleEnterState.name)
        {
            if (cr.anemoneSprites[0].GetComponent<AnimationState2D>().CurrentFrame >= (idleEnterState.frameLength - 1))
            {
                SetAnimState(idleState, cr.objId, World!);
            }
        }
        else if (cr.currentState == captureState.name)
        {
            if (cr.anemoneSprites[0].GetComponent<AnimationState2D>().CurrentFrame >= (captureState.frameLength - 1))
            {
                TutorialController.instance.EnableCoralBuilderNavigate();
                SetAnimState(closedStillState, cr.objId, World!);
            }
        }
    }

    public void SetAnimState(AnimState animState, ulong objID, World world)
    {
        Anemone cr = instances[objID];

        cr.currentState = animState.name;

        foreach(Entity anemoneSprite in cr.anemoneSprites) SetAnimForBothAnemoneEntities(animState, world, anemoneSprite.Id);
    }

    private static void SetAnimForBothAnemoneEntities(AnimState animState, World world, ulong anemoneID)
    {
        ref SpriteSheetAnimation2D spr = ref Entity.FromId(world, anemoneID).GetComponent<SpriteSheetAnimation2D>();
        spr.Row = animState.row;
        spr.FrameOffset = animState.frameOffset;
        spr.FrameLength = animState.frameLength;
        spr.FramesPerSecond = animState.fps;
        spr.Loop = animState.isLoop;

        //Zero out the anim
        ref AnimationState2D anim2D = ref Entity.FromId(world, anemoneID).GetComponent<AnimationState2D>();
        anim2D.CurrentFrame = 0;
    }
    #endregion




}

[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class CraftAnemoneHandler : TriggerSystemBase
{
    ////this passes information to CraftAnemone class
    public static Entity capturedEntity;

    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        if (Entity.FromId(World!, self.Id).HasComponent<CraftAnemoneComponent>() && (other.HasComponent<PlayerTriggerComponent>() || other.HasComponent<PlayerComponent>())) { }
        else return;
        Log("Begin Vibration");
        
        if(!CraftAnemone.isLeaving)
        CraftAnemone.instance.SetAnimState(CraftAnemone.vibrateEnterState, self.Id, World!);

        TutorialController.instance.EnableCoralBuilderEntry();

    }


    protected override void OnTriggerStay(Entity self, TriggerEvent evt)
    {
        if (CraftAnemone.isLeaving) return;

        

        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        if (Entity.FromId(World!, self.Id).HasComponent<CraftAnemoneComponent>() && (other.HasComponent<PlayerTriggerComponent>() || other.HasComponent<PlayerComponent>())) { }
        else return;
       
        

        CamFollow.instance.CamShake(true, 0.0085f);
        if (Input.IsGamepadConnected(0))
        {
            Input.SetGamepadVibration(0,0.8f,0.8f);
        }
        
       
        if (!CraftAnemone.isLMB_pressed) return;
 
        if (other.HasComponent<PlayerComponent>())
        {
            Log("ENTERING");
            LaunchCrafting(self, other);

            CamFollow.instance.CamShake(false, 0f);

            //Disable player movement and X key for inventory!
            Player.instance.isEnabled = false;
            InventoryController.instance.isEnabled_vomitInput = false;
            Player.instance.ResetInputs();

            //Remove Rigidbody on the Player!
            Entity.FromId(World!, Player.instance.player.Id).RemoveComponent<Rigidbody2D>();
        }
        

    }
    //Unused for now
    protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    {
        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        if (Entity.FromId(World!, self.Id).HasComponent<CraftAnemoneComponent>() && (other.HasComponent<PlayerTriggerComponent>() || other.HasComponent<PlayerComponent>())) { }
        else return;
        Log("Exitting");
        CraftAnemone.isLeaving = false;

        CamFollow.instance.CamShake(false, 0f);

        if (!CraftAnemone.instances[self.Id].isLerpingToAnemone && !CraftAnemone.instances[self.Id].isExitingAnemone)
        {
            CraftAnemone.instance.SetAnimState(CraftAnemone.vibrateExitState, self.Id, World!);
        }

    }

    private void LaunchCrafting(Entity self, Entity other)
    {
        CraftAnemone.instance.SetAnimState(CraftAnemone.captureState, self.Id, World!);
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
        CraftAnemone.instances[self.Id].isWonSpirit = false;

        //if(CraftAnemone.instances[self.Id].isOpened)
        if(InventoryController.slotInstances[Entity.FromId(World!, InventoryController.slotObjIds[InventoryController.globalInvIterator]).GetComponent<Name>().Value.ToString()].isStoringItem)
        {
            CraftAnemone.instances[self.Id].UpdateSelection(World!, InventoryController.currentSelected_msID, new Vector3(0, 1.55f, 0));
        }
        
    }

}
