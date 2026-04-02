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

/// <summary>
/// System that processes entities with specific components.
/// This is a pure ECS system: it queries entities and updates their components.
/// </summary>
[Component] public record struct LexicateTradeComponent(int msID_in, int msID_out, int doorSignifier, float vomitSpeed);
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class LexicateTrade : TriggerSystemBase
{
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity selfEntity = Entity.FromId(World!, self.Id);
        Entity otherEntity = Entity.FromId(World!, evt.OtherEntityId);

        if(selfEntity.HasComponent<LexicateTradeComponent>() && otherEntity.HasComponent<MS_IDComponent>())
        {
            foreach (var gameObject in World!.Query<LexicateTradeComponent, Active, ProximityCullingComponent>())
            {
                if (gameObject.Component2.Enabled == false) continue;
                //skip inactive due to proximity

                if(otherEntity.GetComponent<MS_IDComponent>().msID == gameObject.Component1.msID_in)
                {
                    AudioManager.instance.PlaySFX("SFX010");

                    float eulerAngle = Quat2EulerAxisZ(selfEntity.GetComponent<LocalTransform>().Rotation);
                    Vector2 localUp = new Vector2(GMath.Cos(eulerAngle + (90 * GMath.Deg2Rad)), GMath.Cos(eulerAngle));
                    if (-0.0001f < localUp.X && localUp.X < 0.0001f && 0.9999f < localUp.Y && localUp.Y < 1.0001f) localUp = new Vector2(0, 1);

                    Vector2 trajectory = localUp.Normalized * gameObject.Component1.vomitSpeed;
                    Vector3 newPos = selfEntity.GetComponent<LocalTransform>().Position;

                    MS_Manager.instance.SendToPool(otherEntity.Id);
                    MS_Manager.instance.TakeFromPool(gameObject.Component1.msID_out, newPos, trajectory, 100000f, true);

                    foreach (var door in World!.Query<MatchSignifierComponent>())
                    {
                        if (door.Component1.signifierID == gameObject.Component1.doorSignifier)
                        {
                            //Deactivate Door!
                            AudioManager.instance.PlaySFX("SFX006");
                            ref Active doorActive = ref Entity.FromId(World!, door.Entity.Id).GetComponent<Active>();
                            doorActive.Enabled = false;
                        }
                        //else nothin,  no door found
                    }


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
