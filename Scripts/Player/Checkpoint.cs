using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;
using System.Security.Cryptography;


namespace EchoesBelow.Scripts;

[Component] public record struct CheckPointComponent(bool start);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class Checkpoint : SystemBase
{
    //bool isHit = false;
    public static Checkpoint instance;
    public static Vector2 checkPointPos;
    public Entity checkPoint;
    protected override void OnCreate()
    {
        instance = this;
        //Log("System Checkpoint initialized", LogLevel.Debug);
    }
    private bool OnStart(ref bool startBool, ulong objId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        instance = this;
        checkPoint = Entity.FromId(World!, objId);
        LocalTransform transform = Entity.FromId(World!, objId).GetComponent<LocalTransform>();
        checkPointPos = new Vector2(transform.Position.X, transform.Position.Y);
        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {

        foreach(var gameObject in World!.Query<CheckPointComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity.Id);

            LocalTransform transform = Entity.FromId(World!, gameObject.Entity.Id).GetComponent<LocalTransform>();
            checkPointPos = new Vector2(transform.Position.X, transform.Position.Y);
        }


    }
}

[Component] public record struct CheckPointTriggerComponent();
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class CheckpointTrigger : TriggerSystemBase
{
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        //Log($"self: {Entity.FromId(World!, self.Id).GetComponent<Name>().Value.ToString()} / other: {evt.OtherEntityId} {Entity.FromId(World!, evt.OtherEntityId).GetComponent<Name>().Value.ToString()}");
        
        if (self.HasComponent<CheckPointTriggerComponent>() && other.HasComponent<PlayerComponent>())
        {
        
            Log("Setting Checkpoint . . .");
            foreach (var gameObject in World!.Query<CheckPointTriggerComponent>())
            {
                if(self.Id == gameObject.Entity.Id)
                {
                    ref LocalTransform checkPointTransform = ref Checkpoint.instance.checkPoint.GetComponent<LocalTransform>();
                    checkPointTransform.Position = self.GetComponent<LocalTransform>().Position;

                }
                else if (other.Id == gameObject.Entity.Id)
                {
                    ref LocalTransform checkPointTransform = ref Checkpoint.instance.checkPoint.GetComponent<LocalTransform>();
                    checkPointTransform.Position = other.GetComponent<LocalTransform>().Position;
                }
            }
        }
         



    }
}