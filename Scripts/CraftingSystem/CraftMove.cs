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
using System.Collections.Generic;

namespace Scripts.CraftingSystem;

[Component] public record struct CraftMoveComponent(bool start, int msID, bool Enabled, float moveSpeed);
[RequireForUpdate<CraftMoveComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class CraftMove : SystemBase
{
    bool isKeyDown_W = false;
    bool isKeyDown_A = false;
    bool isKeyDown_S = false;
    bool isKeyDown_D = false;
    bool isKeyPressed_Space = false;

    Vector2 moveDir = new Vector2();
    float lerpFac = 0.2f;
    protected override void OnUpdate()
    {
        foreach(var gameObject in World!.Query<CraftMoveComponent>())
        {
            if (!gameObject.Component1.Enabled) continue;


            if (!Player.instance.isEnabled)
            {
                isKeyDown_W = Input.IsKeyDown(KeyCode.W);
                isKeyDown_A = Input.IsKeyDown(KeyCode.A);
                isKeyDown_S = Input.IsKeyDown(KeyCode.S);
                isKeyDown_D = Input.IsKeyDown(KeyCode.D);
                isKeyPressed_Space = Input.IsKeyPressed(KeyCode.Space);
            }

            if (isKeyDown_A || isKeyDown_D || isKeyDown_W || isKeyDown_S)
            {
                
            } // can add an else if in between for spacebar soon
            else
            {
              
            }

            if (isKeyDown_W) moveDir.Y = GMath.Lerp(moveDir.Y, 1, lerpFac);
            if (isKeyDown_S) moveDir.Y = GMath.Lerp(moveDir.Y, -1, lerpFac);
            if (isKeyDown_A) moveDir.X = GMath.Lerp(moveDir.X, -1, lerpFac);
            if (isKeyDown_D) moveDir.X = GMath.Lerp(moveDir.X, 1, lerpFac);
            
            ref LinearVelocity2D lv = ref Entity.FromId(World!,gameObject.Entity.Id).GetComponent<LinearVelocity2D>();
            lv.Value = moveDir.Normalized * gameObject.Component1.moveSpeed * Time.DeltaTime;

        }
    }

    protected override void OnDestroy()
    {
     
    }
}
