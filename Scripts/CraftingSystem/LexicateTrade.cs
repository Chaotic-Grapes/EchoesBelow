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

[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class Lexicate : SystemBase
{
    private bool OnStart(ref bool startBool, Entity LexicateEntity)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        ref LexicateTradeComponent lxT = ref LexicateEntity.GetComponent<LexicateTradeComponent>();
        lxT.objID = LexicateEntity.Id;

        Log("Once every start per obj");
        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        //Use this
        foreach (var gameObject in World!.Query<LexicateTradeComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity);

            //Do everyth else


        }
    }
}
[Component] public record struct LexicateTradeComponent(int msID_in, int msID_out, int doorSignifier, float vomitSpeed, ulong objID, bool start);
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class LexicateTrade : TriggerSystemBase
{
   
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity selfEntity = Entity.FromId(World!, self.Id);
        Entity otherEntity = Entity.FromId(World!, evt.OtherEntityId);

        if(selfEntity.HasComponent<LexicateTradeComponent>() && otherEntity.HasComponent<MS_IDComponent>())
        {
            foreach (var gameObject in World!.Query<LexicateTradeComponent, Active>())
            {
                if (gameObject.Component1.objID != self.Id) continue;


                if(otherEntity.GetComponent<MS_IDComponent>().msID == gameObject.Component1.msID_in)
                {
                    AudioManager.instance.PlaySFX("SFX010_Track01");

                    //Finding the local up angle?
                    float eulerAngle = Quat2EulerAxisZ(selfEntity.GetComponent<LocalTransform>().Rotation);
                    Vector2 localUp = new Vector2(GMath.Cos(eulerAngle + (90 * GMath.Deg2Rad)), GMath.Cos(eulerAngle));
                    if (-0.0001f < localUp.X && localUp.X < 0.0001f && 0.9999f < localUp.Y && localUp.Y < 1.0001f) localUp = new Vector2(0, 1);

                    //declaring my values
                    Vector2 trajectory = localUp.Normalized * gameObject.Component1.vomitSpeed;
                    Vector3 newPos = selfEntity.GetComponent<LocalTransform>().Position;

                    //send and remove an obj from the pool into the world
                    MS_Manager.instance.SendToPool(otherEntity.Id);
                    MS_Manager.instance.TakeFromPool(gameObject.Component1.msID_out, newPos, trajectory, 100000f, true);

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
                    Log($"Throwin it back to ya from {Entity.FromId(World!,self.Id).GetComponent<Name>().Value.ToString()}");

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
}
