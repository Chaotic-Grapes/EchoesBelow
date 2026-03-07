using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;


namespace EchoesBelow.Scripts.MarineSnowSystem;

[Component] public record struct MS_IDComponent(int msID, int color, bool start, float collisionCooldown);
[RequireForUpdate<MS_IDComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class MS_ID : SystemBase
{
    //This is just a container of useful fields for Marine Snow
    // .color will contain a range of values between 0-4 
    public static MS_ID instance;
    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        instance = this;

        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        foreach(var gameObject in World!.Query<MS_IDComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start);
        }

        foreach (var gameObject in World!.Query<MS_IDComponent>())
        {
            if(gameObject.Component1.collisionCooldown >= 0f)
            {
                gameObject.Component1.collisionCooldown -= Time.DeltaTime;
                gameObject.Component1.collisionCooldown = GMath.Clamp(gameObject.Component1.collisionCooldown, 0f, 100f); //hardcoded limit
            }
        }
    }

    public void SetCooldown(float cooldown, ulong objId)
    {
        foreach (var gameObject in World!.Query<MS_IDComponent>())
        {
            if(gameObject.Entity.Id == objId)
            {
                gameObject.Component1.collisionCooldown = cooldown;
            }
        }
    }
}
