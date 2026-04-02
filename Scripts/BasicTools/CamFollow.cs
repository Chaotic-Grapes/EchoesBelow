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
    static Vector2 playerPos;
    static Vector3 _prevListenerPos;
    static bool _hasPrevListenerPos;
    protected override void OnCreate()
    {
        //Log("System CamFollow initialized");
    }

    protected override void OnUpdate()
    {
        playerPos = new Vector2(Player.instance.currentPos.X, Player.instance.currentPos.Y);
        foreach(var gameObject in World!.Query<CamFollowComponent, LocalTransform>())
        {
            Entity entity = Entity.FromId(World!, gameObject.Entity.Id);
            ref LocalTransform transform = ref gameObject.Component2;

            transform.Position = new Vector3(GMath.Lerp(transform.Position.X, playerPos.X, 0.1f),
                                             GMath.Lerp(transform.Position.Y, playerPos.Y, 0.1f), transform.Position.Z);

            float dt = Time.DeltaTime;
            if (dt <= 0.0f) dt = 0.0001f;

            // Drive the 3D audio listener from the camera so panning matches screen X (camera center).
            Vector3 listenerPos = new Vector3(transform.Position.X, transform.Position.Y, 0.0f);
            Vector3 listenerVel = _hasPrevListenerPos ? (listenerPos - _prevListenerPos) / dt : Vector3.Zero;
            _prevListenerPos = listenerPos;
            _hasPrevListenerPos = true;

            GrapeEngine.Scripting.Services.Audio.SetListener(
                listenerPos,
                listenerVel,
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 1.0f));
        }
    }

}
