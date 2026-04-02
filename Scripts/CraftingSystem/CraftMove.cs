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

[Component] public record struct CraftMoveComponent(bool start, int msID, bool Enabled, float moveSpeed, float maxSpeed);
[RequireForUpdate<CraftMoveComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class CraftMove : SystemBase
{
    bool isKeyDown_W = false;
    bool isKeyDown_A = false;
    bool isKeyDown_S = false;
    bool isKeyDown_D = false;
    bool isKeyPressed_Space = false;

    Vector2 moveDir;
    private const float lerpFac = 0.75f;

    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        moveDir = Vector2.Zero;

        //End of Start
        return true;
    }

    protected override void OnUpdate()
    {


        foreach (var gameObject in World!.Query<CraftMoveComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start);

            //Do everyth else
            //Log($"{Entity.FromId(World!, gameObject.Entity.Id).GetComponent<Name>().Value.ToString()} / Layer id: {Entity.FromId(World!, gameObject.Entity.Id).GetComponent<Layer>().Id}");
        }
        foreach (var gameObject in World!.Query<CraftMoveComponent>())
        {
            if (!gameObject.Component1.Enabled) continue;

            if (!Player.instance.isEnabled && !Input.IsGamepadConnected(0))
            {
                isKeyDown_W = Input.IsKeyDown(KeyCode.W);
                isKeyDown_A = Input.IsKeyDown(KeyCode.A);
                isKeyDown_S = Input.IsKeyDown(KeyCode.S);
                isKeyDown_D = Input.IsKeyDown(KeyCode.D);
                isKeyPressed_Space = Input.IsKeyPressed(KeyCode.Space);
            }
            else if(!Player.instance.isEnabled && Input.IsGamepadConnected(0))
            {
                isKeyPressed_Space = Input.IsGamepadButtonDown(0, GamepadButton.Y);
            }

            if (Input.IsGamepadConnected(0))
            {
                moveDir.Y = -Input.GetGamepadAxis(0, GamepadAxis.LeftY);
                moveDir.X = Input.GetGamepadAxis(0, GamepadAxis.LeftX);
            }


            if (isKeyDown_W) moveDir.Y = GMath.Lerp(moveDir.Y, 1, lerpFac);
            if (isKeyDown_S) moveDir.Y = GMath.Lerp(moveDir.Y, -1, lerpFac);
            if (isKeyDown_A) moveDir.X = GMath.Lerp(moveDir.X, -1, lerpFac);
            if (isKeyDown_D) moveDir.X = GMath.Lerp(moveDir.X, 1, lerpFac);
            moveDir.X = GMath.Lerp(moveDir.X, 0, lerpFac / 2);
            moveDir.Y = GMath.Lerp(moveDir.Y, 0, lerpFac / 2);


            Vector2 moveDirNormalized = Vector2.Zero;
            //NaN protection for normalization
            if (Input.IsGamepadConnected(0))
            {
                moveDirNormalized = moveDir;
            }
            else
            {
                moveDirNormalized = (-0.0001f <= moveDir.X && moveDir.X <= 0.0001f && -0.0001f <= moveDir.Y && moveDir.Y <= 0.0001f) ? Vector2.Zero : moveDir.Normalized;
            }
              

            ref LinearVelocity2D lv = ref Entity.FromId(World!,gameObject.Entity.Id).GetComponent<LinearVelocity2D>();
            //lv.Value = moveDirNormalized * gameObject.Component1.moveSpeed * Time.DeltaTime;

            lv.Value.X += moveDirNormalized.X * gameObject.Component1.moveSpeed * Time.DeltaTime;
            lv.Value.Y += moveDirNormalized.Y * gameObject.Component1.moveSpeed * Time.DeltaTime;

            //Clamping these values to a maxSpeed
            lv.Value.X = GMath.Clamp(lv.Value.X, -gameObject.Component1.maxSpeed, gameObject.Component1.maxSpeed);
            lv.Value.Y = GMath.Clamp(lv.Value.Y, -gameObject.Component1.maxSpeed, gameObject.Component1.maxSpeed);

            Player.instance.currentPosForCamFollow = gameObject.Entity.GetComponent<LocalTransform>().Position 
                + gameObject.Entity.GetParent()!.GetComponent<LocalTransform>().Position + new Vector3(0,-CraftAnemone.cameraOffsetY,0);


            if((Vector3.Zero - gameObject.Entity.GetComponent<LocalTransform>().Position).Magnitude > 10f)
            {
                ref LocalTransform transform = ref gameObject.Entity.GetComponent<LocalTransform>();
                transform.Position = Vector3.Zero;
            }
        }
    }
}
