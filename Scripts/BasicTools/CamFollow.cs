using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;

namespace EchoesBelow.Scripts;

[Component] public record struct CamFollowComponent();
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class CamFollow : SystemBase
{
    const float shakeMinMax = 0.234f;

    static Vector2 focusPos;
    public static Vector3 camPos;
    public static float originalLerp = 0.1f;
    public static float lerpFac = 0.1f;
    protected override void OnCreate()
    {
        //Log("System CamFollow initialized");
        
    }

    protected override void OnUpdate()
    {
        focusPos = new Vector2(Player.instance.currentPosForCamFollow.X, Player.instance.currentPosForCamFollow.Y);
        foreach(var gameObject in World!.Query<CamFollowComponent, LocalTransform>())
        {
            Entity entity = Entity.FromId(World!, gameObject.Entity.Id);
            ref LocalTransform transform = ref gameObject.Component2;

            transform.Position = new Vector3(GMath.Lerp(transform.Position.X, focusPos.X, lerpFac),
                                             GMath.Lerp(transform.Position.Y, focusPos.Y, lerpFac), transform.Position.Z);

            camPos = transform.Position;

            //Reset Camera Lerp when other scripts are done w the camera essentially
            float Xboundary = transform.Position.X;
            float yBoundary = transform.Position.Y;

            Vector3 playerPos = Player.instance.player.GetComponent<LocalTransform>().Position;

            if ((Xboundary - 0.125f < playerPos.X && playerPos.X < Xboundary + 0.125f &&
                yBoundary - 0.125f < playerPos.Y && playerPos.Y < yBoundary + 0.125f))
            {
                lerpFac = originalLerp;
            }

        }
    }
    public void CamShake(bool isShaking)
    {
        if (isShaking)
        {
            float xOffset = GMath.Random(-shakeMinMax, shakeMinMax);
            float yOffset = GMath.Random(-shakeMinMax, shakeMinMax);

            foreach (var cam in World!.Query<Camera3D>())
            {
                ref LocalTransform transform = ref cam.Entity.GetComponent<LocalTransform>();
                transform.Position = new Vector3(xOffset, yOffset, 0f);
            }
        }
        else
        {
            foreach (var cam in World!.Query<Camera3D>())
            {
                ref LocalTransform transform = ref cam.Entity.GetComponent<LocalTransform>();
                transform.Position = Vector3.Zero;
            }
        }

    }

}
