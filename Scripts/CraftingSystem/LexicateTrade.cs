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

namespace Scripts.CraftingSystem;

public class LexicateData
{
    public ulong objID {  get; set; }
    public Entity output { get; set; }
    
    public string currentState { get; set; }


    //For Vomitting
    public int throwing_msID { get; set; }
    public Vector3 throwing_newPos { get; set; }
    public Vector2 throwing_trajectory { get; set; }
    public float throwing_decayTime { get; set; }

    public LexicateData(ulong objID, World world)
    {
        this.objID = objID;

        //store the only child, the output obj
        output = Entity.FromId(world, objID).GetFirstChild()!;
    }
}

[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class Lexicate : SystemBase
{
    public AnimState idleState;
    public AnimState spitState;
    public static Lexicate instance;
    private bool OnAwake(ref bool awakeBool, Entity LexicateEntity)
    {
        if (awakeBool == true) return true;
        awakeBool = true;
        //Todo

        instance = this;

        //Initialize our list
        LexicateTrade.instances = new Dictionary<ulong, LexicateData>();

        idleState = new AnimState("idleState", 3, 0, 96, 24, true);
        spitState = new AnimState("spitState", 0, 0, 40, 24, true);

        Log("Lotsa times every awake per obj");
        //End of Start
        return true;
    }
    private bool OnStart(ref bool startBool, Entity LexicateEntity)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo


        ref LexicateTradeComponent lxT = ref LexicateEntity.GetComponent<LexicateTradeComponent>();
        lxT.objID = LexicateEntity.Id;

        LexicateData lx = new LexicateData(lxT.objID, World!);
        LexicateTrade.instances.Add(lx.objID, lx);
        Log("Once every start per obj / Count: " + LexicateTrade.instances.Count);

        SetAnimState(LexicateEntity.Id, World!, idleState);

        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        foreach (var gameObject in World!.Query<LexicateTradeComponent>())
        {
            bool awake = gameObject.Component1.awake;
            gameObject.Component1.awake = OnAwake(ref awake, gameObject.Entity);

        }
        foreach (var gameObject in World!.Query<LexicateTradeComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity);
        }

        //Do everyth else
        foreach (var gameObject in World!.Query<LexicateTradeComponent>())
        {
            LexicateData lx = LexicateTrade.instances[gameObject.Entity.Id];

            if (lx.currentState == spitState.name)
            {
                if (Entity.FromId(World!,gameObject.Entity.Id).GetComponent<AnimationState2D>().CurrentFrame >= (spitState.frameLength - 1))
                {
                    SetAnimState(lx.objID, World!, idleState);
                    MS_Manager.instance.TakeFromPool(lx.throwing_msID, lx.throwing_newPos, lx.throwing_trajectory, 100000f, true);
                }
            }
        }
    }

    public void SetAnimState(ulong objId, World world, AnimState animState)
    {
        LexicateTrade.instances[objId].currentState = animState.name;

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
[Component] public record struct LexicateTradeComponent(int msID_in, int msID_out, int doorSignifier, float vomitSpeed, ulong objID, bool start, bool awake);
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class LexicateTrade : TriggerSystemBase
{
    public static Dictionary<ulong, LexicateData> instances;
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity selfEntity = Entity.FromId(World!, self.Id);
        Entity otherEntity = Entity.FromId(World!, evt.OtherEntityId);

        Entity outputEntity = instances[self.Id].output;

        if (selfEntity.HasComponent<LexicateTradeComponent>() && otherEntity.HasComponent<MS_IDComponent>())
        {
            foreach (var gameObject in World!.Query<LexicateTradeComponent, Active>())
            {
                if (gameObject.Component1.objID != self.Id) continue;


                if(otherEntity.GetComponent<MS_IDComponent>().msID == gameObject.Component1.msID_in)
                {
                    //AudioManager.instance.PlaySFX("SFX010_Track01");

                    //Finding the local up angle?
                    float eulerAngle = Quat2EulerAxisZ(selfEntity.GetComponent<LocalTransform>().Rotation) + (90f*GMath.Rad2Deg);
                    Vector2 localUp = new Vector2(GMath.Cos(eulerAngle + (90 * GMath.Deg2Rad)), GMath.Cos(eulerAngle));
                    if (-0.0001f < localUp.X && localUp.X < 0.0001f && 0.9999f < localUp.Y && localUp.Y < 1.0001f) localUp = new Vector2(0, 1);

                    //declaring my values
                    Vector2 trajectory = localUp.Normalized * gameObject.Component1.vomitSpeed;
                    Vector3 newPos = ApplyRotationToVector(outputEntity.GetComponent<LocalTransform>().Position, selfEntity.GetComponent<LocalTransform>().Rotation) + selfEntity.GetComponent<LocalTransform>().Position;

                    //send and remove an obj from the pool into the world

                    MS_Manager.instance.SendToPool(otherEntity.Id);
                    Lexicate.instance.SetAnimState(self.Id, World!, Lexicate.instance.spitState);
                    //Delay this

                    LexicateData lx = LexicateTrade.instances[self.Id];
                    lx.throwing_msID = gameObject.Component1.msID_out;
                    lx.throwing_newPos = newPos;
                    lx.throwing_trajectory = trajectory;
                    lx.throwing_decayTime = 100000f;

                    //MS_Manager.instance.TakeFromPool(gameObject.Component1.msID_out, newPos, trajectory, 100000f, true);

                    //Deprecated
                    //foreach (var door in World!.Query<MatchSignifierComponent>())
                    //{
                    //    if (door.Component1.signifierID == gameObject.Component1.doorSignifier)
                    //    {
                    //        //Deactivate Door!
                    //        AudioManager.instance.PlaySFX("SFX006");
                    //        ref Active doorActive = ref Entity.FromId(World!, door.Entity.Id).GetComponent<Active>();
                    //        doorActive.Enabled = false;
                    //    }
                    //    //else nothin,  no door found
                    //}
                    //Log($"Throwin it back to ya from {Entity.FromId(World!,self.Id).GetComponent<Name>().Value.ToString()}");
                    //Log($"{Entity.FromId(World!, self.Id).GetComponent<Name>().Value.ToString()}'s child is {instances[self.Id].output.GetComponent<Name>().Value.ToString()}");

                }



            }
        }
    }
    private float Quat2EulerAxisZ(Quaternion quat)
    {
        //To find out how
        //Search up Conversion of ZYX Quaternion to Euler Angle (z-yaw)
        float x = quat.X;
        float y = quat.Y;
        float z = quat.Z;
        float w = quat.W;

        float a = 2 * (w * z + x * y);
        float b = 1 - (2 * ((y * y) + (z * z)));
        float outAngle = GMath.Atan2(a, b);
        return outAngle;
    }

    private Vector3 ApplyRotationToVector(Vector3 vector, Quaternion quat)
    {

        Vector3 u = new Vector3(quat.X, quat.Y, quat.Z);
        float s = quat.W;
        // v' = v + 2*u x (s*v + u x v)

        return vector + 2.0f * Cross3D(u, (quat.W * vector + Cross3D(u, vector)));
    }

    public Vector3 Cross3D(Vector3 left, Vector3 right)
    {
        Vector3 result = new Vector3();
        result.X = left.Y * right.Z - left.Z * right.Y;
        result.Y = left.Z * right.X - left.X * right.Z;
        result.Z = left.X * right.Y - left.Y * right.X;
        return result;
    }
}
