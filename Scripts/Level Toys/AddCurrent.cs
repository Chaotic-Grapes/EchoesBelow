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
using Scripts.CraftingSystem;
using System.Collections;
using System.Collections.Generic;

namespace Scripts.Level_Toys;

/// <summary>
/// System that processes entities with specific components.
/// This is a pure ECS system: it queries entities and updates their components.
/// </summary>
[Component] public record struct AddCurrentComponent(float pushSpeed, float currentDirX, float currentDirY, bool start);
[RequireForUpdate<AddCurrentComponent>]
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]

public class AddCurrent : TriggerSystemBase
{
    //protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    //{
    //    //Add to lv of the passing obj
    //    Entity selfEntity = Entity.FromId(World!, self.Id);
    //    Entity otherEntity = Entity.FromId(World!, evt.OtherEntityId);

    //    if (selfEntity.HasComponent<AddCurrentComponent>() && (otherEntity.HasComponent<PlayerTriggerComponent>() || otherEntity.HasComponent<MS_IDComponent>()))
    //    {

    //        foreach (var gameObject in World!.Query<AddCurrentComponent>())
    //        {
    //            //If the id does not match, skip this obj
    //            if (gameObject.Entity.Id != self.Id) continue;
    //            Entity playerEntity = Entity.FromId(World!, Player.instance.player.Id);
    //            ref LinearVelocity2D lv = ref playerEntity.GetComponent<LinearVelocity2D>();
    //            lv.Value = Vector2.Zero;
    //            //Zero out rigidbody, av and lv, with an exception for the player
    //            //if (otherEntity.HasComponent<PlayerTriggerComponent>())
    //            //{
    //            //    foreach (var gameObject2 in World!.Query<PlayerComponent>())
    //            //    {
    //            //        Entity playerEntity = Entity.FromId(World!, gameObject2.Entity.Id);
    //            //        ref LinearVelocity2D lv = ref playerEntity.GetComponent<LinearVelocity2D>();
    //            //        lv.Value = Vector2.Zero;
    //            //    }
    //            //}
    //            //else
    //            //{
    //            //    ref LinearVelocity2D lv = ref otherEntity.GetComponent<LinearVelocity2D>();
    //            //    lv.Value = Vector2.Zero;
    //            //}
    //        }
    //    }
    //}
    protected override void OnTriggerStay(Entity self, TriggerEvent evt)
    {
        //Add to lv of the passing obj
        Entity selfEntity = Entity.FromId(World!, self.Id);
        Entity otherEntity = Entity.FromId(World!, evt.OtherEntityId);

        if (selfEntity.HasComponent<AddCurrentComponent>() && (otherEntity.HasComponent<PlayerTriggerComponent>() || otherEntity.HasComponent<MS_IDComponent>()))
        {

            foreach (var gameObject in World!.Query<AddCurrentComponent>())
            {
                //If the id does not match, skip this obj
                if (gameObject.Entity.Id != self.Id) continue;
                Entity playerEntity = Entity.FromId(World!, Player.instance.player.Id);
                ref Rigidbody2D rb = ref playerEntity.GetComponent<Rigidbody2D>();
                ref LinearVelocity2D lv = ref playerEntity.GetComponent<LinearVelocity2D>();
                ref AngularVelocity2D av = ref playerEntity.GetComponent<AngularVelocity2D>();

                lv.Value += new Vector2(gameObject.Component1.currentDirX * gameObject.Component1.pushSpeed * Time.DeltaTime, gameObject.Component1.currentDirY * gameObject.Component1.pushSpeed * Time.DeltaTime);
                lv.Value = new Vector2(GMath.Clamp(lv.Value.X, -3f, 3f), GMath.Clamp(lv.Value.Y, -3f, 3f));
            }
        }


    }

    //protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    //{
    //    //restore obj values
    //    Entity selfEntity = Entity.FromId(World!, self.Id);
    //    Entity otherEntity = Entity.FromId(World!, evt.OtherEntityId);
    //    if (selfEntity.HasComponent<AddCurrentComponent>() && (otherEntity.HasComponent<PlayerTriggerComponent>() || otherEntity.HasComponent<MS_IDComponent>()))
    //    {

    //        foreach (var gameObject in World!.Query<AddCurrentComponent>())
    //        {
    //            //If the id does not match, skip this obj
    //            if (gameObject.Entity.Id != self.Id) continue;
    //            Entity playerEntity = Entity.FromId(World!, Player.instance.player.Id);
    //            ref LinearVelocity2D lv = ref playerEntity.GetComponent<LinearVelocity2D>();
    //            lv.Value = Vector2.Zero;
    //        }
    //    }
    //}


}

[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class AddCurrentUtility : SystemBase
{
    private bool OnStart(ref bool startBool, Entity obj)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        float objAngle = Quat2EulerAxisZ(obj.GetComponent<LocalTransform>().Rotation);
        ref AddCurrentComponent ac = ref obj.GetComponent<AddCurrentComponent>();
        Vector2 currentDir = new Vector2(GMath.Cos(objAngle + (90 * GMath.Deg2Rad)), GMath.Cos(objAngle));

        ac.currentDirX = currentDir.X;
        ac.currentDirY = currentDir.Y;
   
        ref ParticleEmitter pe = ref obj.GetComponent<ParticleEmitter>();
   
        pe.GravityX = currentDir.X * 4.3f;
        pe.GravityY = currentDir.Y * 4.3f;
        pe.EmissionAngle = objAngle * GMath.Deg2Rad;

        //Log($"currentDir){currentDir} objAngle){objAngle} ");


        ref ShapeBox2D shapeBox = ref obj.GetComponent<ShapeBox2D>();
        shapeBox.Filled = false;

        //Log("PARTICLE SET for : " + obj.GetComponent<Name>().Value.ToString());
        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        foreach (var gameObject in World!.Query<AddCurrentComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, Entity.FromId(World!, gameObject.Entity.Id));

            //Do everyth else
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