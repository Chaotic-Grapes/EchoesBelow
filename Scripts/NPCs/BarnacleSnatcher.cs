using EchoesBelow.Scripts.Audio;
using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections;
using System.Collections.Generic;

namespace EchoesBelow.Scripts;

public enum BarnacleState
{
    Idle = 0,
    Attack = 1
}
public class Barnacle : SystemBase
{
    public ulong objId { get; set; }
    public string name { get; set; }
    public string currentState {  get; set; }
    public World world {  get; set; }

    // I can have multiple unique properties in here, these cant be set from the outset
    // But colliders can query and send info to the corresponding container
    // Accessing thru ulong ids
    public Barnacle(World world, ulong objId, string name)
    {
        //Set everything!
       this.objId = objId;
       this.name = name;

       this.world = world;

    }

    public void SetAnimState(AnimState animState)
    {
        this.currentState = animState.name; 

        ref SpriteSheetAnimation2D spr = ref Entity.FromId(world, objId).GetComponent<SpriteSheetAnimation2D>();
        spr.Row = animState.row;
        spr.FrameOffset = animState.frameOffset;
        spr.FrameLength = animState.frameLength;
        spr.FramesPerSecond = animState.fps;

        //Zero out the anim
        ref AnimationState2D anim2D = ref Entity.FromId(world, objId).GetComponent<AnimationState2D>();
        anim2D.CurrentFrame = 0;
    }
}
[Component(Name = "Barnacle Snatcher")] public record struct BarnacleSnatcherComponent(
    bool start, 
    bool awake
    
    );
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class BarnacleSnatcher : SystemBase
{
    public static Dictionary<ulong, Barnacle> instances = [];

    //Make anim states here
    public static AnimState idleState = new AnimState("idleState", 0, 0, 58, 24f);
    public static AnimState attackState = new AnimState("attackState", 19, 1, 21, 24f);

    private bool OnAwake(ref bool awakeBool)
    {
        if (awakeBool == true) return true;
        awakeBool = true;
        //Todo
        instances = new Dictionary<ulong, Barnacle>();
        //End of Start
        return true;
    }
    private bool OnStart(ref bool startBool, ulong objId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        Barnacle barnacleData = new Barnacle(World!, objId, Entity.FromId(World!, objId).GetComponent<Name>().Value.ToString());
        barnacleData.SetAnimState(idleState);
        instances.Add(objId, barnacleData);
        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        foreach (var gameObject in World!.Query<BarnacleSnatcherComponent>())
        {
            bool awake = gameObject.Component1.awake;
            gameObject.Component1.awake = OnAwake(ref awake);
        }
        foreach (var gameObject in World!.Query<BarnacleSnatcherComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity.Id);
        }
        
        foreach (var gameObject in World!.Query<BarnacleSnatcherComponent>())
        {
            //Do everyth else
            Entity barnacleSnatcher = Entity.FromId(World!, instances[gameObject.Entity.Id].objId);
            Barnacle barnacleData = instances[gameObject.Entity.Id];

            if (barnacleData.currentState == attackState.name && (barnacleSnatcher.GetComponent<AnimationState2D>().CurrentFrame == (attackState.frameLength-1)))
            {
                barnacleData.SetAnimState(idleState);
            }
        }

    }
}
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class BarnacleTriggerHandler : TriggerSystemBase
{
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity selfEntity = Entity.FromId(World!, self.Id);
        Entity otherEntity = Entity.FromId(World!, evt.OtherEntityId);

        if(selfEntity.HasComponent<BarnacleSnatcherComponent>() && otherEntity.HasComponent<PlayerTriggerComponent>())
        {
            
            for (int i = 0;   i <= InventoryController.slotInstances.Count-1; i++)
            {

                Entity slotEntity = Entity.FromId(World!, InventoryController.slotObjIds[i]);
                string slotName = slotEntity.GetComponent<Name>().Value.ToString();
                Slot slotInstance = InventoryController.slotInstances[slotName];
                //If the slot is storing an item, remove the corresponding obj, from Left to right
                if (slotInstance.isStoringItem)
                {
                    //selfEntity.GetComponent<AudioSource>().PlayOnStart = true;
                    AudioManager.instance.PlaySFX("SFX002");
                    BarnacleSnatcher.instances[self.Id].SetAnimState(BarnacleSnatcher.attackState);
                    InventoryController.instance.RemoveFromSlotInInventory(i);
                    break;
                }
                
            }
        }
    }
    protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    {
        Entity selfEntity = Entity.FromId(World!, self.Id);
        Entity otherEntity = Entity.FromId(World!, evt.OtherEntityId);

        if (selfEntity.HasComponent<BarnacleSnatcherComponent>() && otherEntity.HasComponent<PlayerTriggerComponent>())
        {

        }
    }
}

