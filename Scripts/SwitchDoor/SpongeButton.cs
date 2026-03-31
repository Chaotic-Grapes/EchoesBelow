using EchoesBelow.Scripts;
using EchoesBelow.Scripts.Audio;
using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using Scripts.BasicTools;
using Scripts.CraftingSystem;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Scripts.SwitchDoor;

public class SpongeBubData
{
    public ulong objID { get; set; }
    public Entity entity { get; set; }
    public bool isTransformed { get; set; }
    public string currentState { get; set; }
    public LayerMask layerMask { get; set; }
    public Entity storedDoor { get; set; }
    public bool isDoorAccessible { get; set; }
    public SpongeBubData(Entity entity, bool isTransformed)
    {
        this.entity = entity;
        this.objID = this.entity.Id;
        this.isTransformed = isTransformed;
    }
}
[Component] public record struct SpongeButtonComponent(bool start, bool awake, bool isTransformed, ulong objID, int inputMSID, int doorSignifier);
[RequireForUpdate<SpongeButtonComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class SpongeButton : SystemBase
{
    public static SpongeButton instance;
    public static Dictionary<ulong, SpongeBubData> instances;

    public AnimState idleState = new AnimState("idleState", 0, 0, 48, 24f, true);
    public AnimState transformState = new AnimState("transformState", 3, 0, 38, 24f, true);
    public AnimState buttonIdleState = new AnimState("buttonIdleState", 6, 0, 60, 24f, true);
    public AnimState buttonPushState = new AnimState("buttonPushState", 10, 0, 10, 24f, true);

    private bool OnAwake(ref bool awakeBool, Entity spongeEntity)
    {
        if (awakeBool == true) return true;
        awakeBool = true;
        //Todo

        instance = this;

        //Initialize our list
        instances = new Dictionary<ulong, SpongeBubData>();

        //End of Start
        return true;
    }
    private bool OnStart(ref bool startBool, Entity spongeEntity, bool isTransformed)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        ref SpongeButtonComponent spButton = ref Entity.FromId(World!, spongeEntity.Id).GetComponent<SpongeButtonComponent>();
        spButton.objID = spongeEntity.Id;

        SpongeBubData bub = new SpongeBubData(spongeEntity, isTransformed);
        instances.Add(spongeEntity.Id, bub);

        if (isTransformed)
        {
            SetAnimState(spongeEntity.Id, World!, buttonIdleState);
            spongeEntity.RemoveComponent<BoxCollider2D>();
            InitBoxCollider(bub);
        }
        else
        {
            SetAnimState(spongeEntity.Id, World!, idleState);
        }



        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        foreach (var gameObject in World!.Query<SpongeButtonComponent>())
        {
            bool awake = gameObject.Component1.awake;
            gameObject.Component1.awake = OnAwake(ref awake, gameObject.Entity);

        }
        foreach (var gameObject in World!.Query<SpongeButtonComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity, gameObject.Component1.isTransformed);
        }

        //Do everyth else
        foreach (var gameObject in World!.Query<SpongeButtonComponent>())
        {

            SpongeBubData sBub= SpongeButton.instances[gameObject.Entity.Id];

            if (sBub.currentState == transformState.name)
            {
                if (Entity.FromId(World!, gameObject.Entity.Id).GetComponent<AnimationState2D>().CurrentFrame >= (transformState.frameLength - 1))
                {
                    sBub.isTransformed = true;

                    SetAnimState(gameObject.Entity.Id, World!, buttonIdleState);

                    InitBoxCollider(sBub);
                }
            }
            else if (sBub.currentState == buttonPushState.name)
            {
                if (!sBub.isDoorAccessible)
                {
                    if (Entity.FromId(World!, gameObject.Entity.Id).GetComponent<AnimationState2D>().CurrentFrame >= (buttonPushState.frameLength - 1))
                    {
                        SetAnimState(gameObject.Entity.Id, World!, buttonIdleState);

                        InitBoxCollider(sBub);
                        sBub.storedDoor = null;
                        sBub.isDoorAccessible = false;
                    }
                }
                else if(sBub.isDoorAccessible)
                {
                    Entity storedDoor = Entity.FromId(World!, sBub.storedDoor.Id);
                    CamFollow.lerpFac = 0.025f;
                    Player.instance.currentPosForCamFollow = storedDoor.GetComponent<LocalTransform>().Position;

                    //If position is within the agreed allowance, stop the tractor beam
                    float camXPos = CamFollow.camPos.X;
                    float Xboundary = storedDoor.GetComponent<LocalTransform>().Position.X;

                    float camYPos = CamFollow.camPos.Y;
                    float yBoundary = storedDoor.GetComponent<LocalTransform>().Position.Y;

                    if ((Xboundary - 0.325f < camXPos && camXPos < Xboundary + 0.325f &&
                        yBoundary - 0.325f < camYPos && camYPos < yBoundary + 0.325f))
                    {
                        AudioManager.instance.PlaySFX("SFX006");
                        ref Active doorActive = ref Entity.FromId(World!, storedDoor.Id).GetComponent<Active>();
                        doorActive.Enabled = false;
                    }

                    if ((Xboundary - 0.125f < camXPos && camXPos < Xboundary + 0.125f &&
                        yBoundary - 0.125f < camYPos && camYPos < yBoundary + 0.125f)
                        && Entity.FromId(World!, gameObject.Entity.Id).GetComponent<AnimationState2D>().CurrentFrame >= (buttonPushState.frameLength - 1))
                    {
                        SetAnimState(gameObject.Entity.Id, World!, buttonIdleState);

                        InitBoxCollider(sBub);
                        sBub.storedDoor = null;
                        sBub.isDoorAccessible = false;
                    }
                }

                
            }
        }
    }

    private static void InitBoxCollider(SpongeBubData sBub)
    {
        ref BoxCollider2D bx = ref sBub.entity.AddComponent<BoxCollider2D>();

        bx.LayerMask = LayerMask.All;
        bx.IsTrigger = false;
        bx.Offset = Vector2.Zero;
        bx.HalfExtents.X = 0.17f;
        bx.HalfExtents.Y = 0.05f;
    }

    public void SetAnimState(ulong objId, World world, AnimState animState)
    {
        instances[objId].currentState = animState.name;

        ref SpriteSheetAnimation2D spr = ref Entity.FromId(world, objId).GetComponent<SpriteSheetAnimation2D>();
        spr.Row = animState.row;
        spr.FrameOffset = animState.frameOffset;
        spr.FrameLength = animState.frameLength;
        spr.FramesPerSecond = animState.fps;
        spr.Loop = animState.isLoop;

        //Zero out the anim
        ref AnimationState2D anim2D = ref Entity.FromId(world, objId).GetComponent<AnimationState2D>();
        anim2D.CurrentFrame = 0;
    }
}

[RequireForUpdate<SpongeButtonComponent>]
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class SpongeTriggerHandler: TriggerSystemBase
{
    Entity otherEntity;
    SpongeBubData bubData;
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        otherEntity = Entity.FromId(World!, evt.OtherEntityId);
        if (self.TryGetComponent<SpongeButtonComponent>(out SpongeButtonComponent spButton) && otherEntity.TryGetComponent<MS_IDComponent>(out MS_IDComponent msIDcomp))
        {

            bubData = SpongeButton.instances[self.Id];
            if(spButton.inputMSID == msIDcomp.msID)
            {
                MS_Manager.instance.SendToPool(otherEntity.Id);


                AudioManager.instance.PlaySFX("SFX006");
                //Do this
                SpongeButton.instance.SetAnimState(bubData.objID, World!, SpongeButton.instance.transformState);

                bubData.layerMask = bubData.entity.GetComponent<BoxCollider2D>().LayerMask;
                bubData.entity.RemoveComponent<BoxCollider2D>();
            }
        }
    }
    protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    {
        otherEntity = Entity.FromId(World!, evt.OtherEntityId);
        if (self.TryGetComponent<SpongeButtonComponent>(out SpongeButtonComponent spButton) && otherEntity.TryGetComponent<MS_IDComponent>(out MS_IDComponent msIDcomp))
        {
            bubData = SpongeButton.instances[self.Id];
            
        }
    }
}

[RequireForUpdate<SpongeButtonComponent>]
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class SpongeCollisionHandler : CollisionSystemBase
{
    Entity otherEntity;
    SpongeBubData bubData; //s
    protected override void OnCollisionEnter(Entity self, CollisionEvent evt)
    {
        otherEntity = Entity.FromId(World!, evt.OtherEntityId);
        if(self.TryGetComponent<SpongeButtonComponent>(out SpongeButtonComponent spButton) && otherEntity.HasComponent<PlayerComponent>())
        {
            if(Player.instance.isDashing && GMath.Abs(Player.instance.player.GetComponent<LinearVelocity2D>().Value.Magnitude) > 0.05f)
            {
                bubData = SpongeButton.instances[self.Id];

                self.RemoveComponent<BoxCollider2D>();

                AudioManager.instance.PlaySFX("SFX012");

                SpongeButton.instance.SetAnimState(bubData.objID, World!, SpongeButton.instance.buttonPushState);

                bubData.storedDoor = null;
                bubData.isDoorAccessible = false;

                foreach (var door in World!.Query<MatchSignifierComponent>())
                {
                    if (door.Component1.signifierID == bubData.entity.GetComponent<SpongeButtonComponent>().doorSignifier
                        && door.Entity.GetComponent<Active>().Enabled)
                    {
                        //Deactivate Door!
                        //AudioManager.instance.PlaySFX("SFX006");
                        //ref Active doorActive = ref Entity.FromId(World!, door.Entity.Id).GetComponent<Active>();
                        //doorActive.Enabled = false;

                        bubData.storedDoor = Entity.FromId(World!, door.Entity.Id);
                        bubData.isDoorAccessible = true;
                    }
                    //else nothin,  no door found
                }

            }
        }
    }
    protected override void OnCollisionExit(Entity self, CollisionExitEvent evt)
    {
        otherEntity = Entity.FromId(World!, evt.OtherEntityId);
        if (self.TryGetComponent<SpongeButtonComponent>(out SpongeButtonComponent spButton) && otherEntity.HasComponent<PlayerComponent>())
        {

        }
    }
}
