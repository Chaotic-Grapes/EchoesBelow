/**
 * @Name: Dalton koh, 2403250
 * @email: d.koh@digipen.edu
 * @file    BarnacleSnatcher.cs
 * 
 * @brief   Barnacle snatcher AI, animation, and collision handling.
 */

using EchoesBelow.Scripts.Audio;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Gameplay;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System;
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

    // I can have multiple unique properties in here, these cant be set from the outset
    // But colliders can query and send info to the corresponding container
    // Accessing thru ulong ids
    public Barnacle(ulong objId, string name)
    {
        //Set everything!
       this.objId = objId;
       this.name = name;

        currentState = BarnacleSnatcher.idleState.name;
        SetAnimState(BarnacleSnatcher.idleState);
    }

    public void SetAnimState(AnimState animState)
    {
        foreach (var animator in World!.Query<BarnacleSnatcherComponent, SpriteSheetAnimation2D>())
        {
            //If I find the corresponding obj, change the animstate!
            if(animator.Entity.Id == objId)
            {
                Log($"___Setting for {Entity.FromId(World!, objId).GetComponent<Name>().Value.ToString()}");
                animator.Component2.Row = animState.row;
                animator.Component2.FrameOffset = animState.frameOffset;
                animator.Component2.FrameLength = animState.frameLength;
                animator.Component2.FramesPerSecond = animState.fps;
            }
        }
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
        Log("Awake");
        //instances = new Dictionary<ulong, Barnacle>();
        //Log($" instances count23: {instances.Count()}");
        //End of Start
        return true;
    }
    private bool OnStart(ref bool startBool, ulong objId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        //Log($" instances count2: {instances.Count()}");
        //instances.Add(objId, new Barnacle(objId, Entity.FromId(World!, objId).GetComponent<Name>().Value.ToString()));
        Log("Start");
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
            //Entity barnacleSnatcher = Entity.FromId(World!, instances[gameObject.Entity.Id].objId);
            //Barnacle barnacleData = instances[gameObject.Entity.Id];
            //if (barnacleData.currentState == attackState.name && barnacleSnatcher.GetComponent<AnimationState2D>().CurrentFrame == (attackState.frameLength - 1))
            //{
            //    barnacleData.SetAnimState(idleState);
            //}

        }

    }
}
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class BarnacleTriggerHandler : TriggerSystemBase
{
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        //Log("1");
        Entity selfEntity = Entity.FromId(World!, self.Id);
        Entity otherEntity = Entity.FromId(World!, evt.OtherEntityId);
        //Log($"2 / self: {selfEntity.GetComponent<Name>().Value.ToString()}  /  other: {otherEntity.GetComponent<Name>().Value.ToString()}");
        if(selfEntity.HasComponent<BarnacleSnatcherComponent>() && (otherEntity.HasComponent<PlayerTriggerComponent>()||otherEntity.HasComponent<PlayerComponent>()))
        {
            Log("SNATCH4");
            BarnacleSnatcher.instances[self.Id].SetAnimState(BarnacleSnatcher.attackState);
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

