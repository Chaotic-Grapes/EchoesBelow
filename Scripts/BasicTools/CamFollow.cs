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
    static bool start;

    public static CamFollow instance;

    //const float shakeIntensity = 0.056f;

    static Vector2 focusPos;
    public static Vector3 camPos;
    public static float originalLerp = 0.1f;
    public static float lerpFac = 0.1f;

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
        start = OnStart(ref start);

        focusPos = new Vector2(Player.instance.currentPosForCamFollow.X, Player.instance.currentPosForCamFollow.Y);
        foreach(var gameObject in World!.Query<CamFollowComponent, LocalTransform>())
        {
            Entity entity = Entity.FromId(World!, gameObject.Entity.Id);
            ref LocalTransform transform = ref gameObject.Component2;

            transform.Position = new Vector3(GMath.Lerp(transform.Position.X, playerPos.X, lerpFac),
                                             GMath.Lerp(transform.Position.Y, playerPos.Y, lerpFac), transform.Position.Z);

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
    public void CamShake(bool isShaking, float intensity)
    {
        if (isShaking)
        {
            float xOffset = GMath.Random(-intensity, intensity);
            float yOffset = GMath.Random(-intensity, intensity);

            foreach (var cam in World!.Query<Camera3D>())
            {
                ref LocalTransform transform = ref cam.Entity.GetParent()!.GetComponent<LocalTransform>();
                transform.Position = new Vector3(xOffset, yOffset, 0f);
            }
        }
        else
        {
            foreach (var cam in World!.Query<Camera3D>())
            {
                ref LocalTransform transform = ref cam.Entity.GetParent()!.GetComponent<LocalTransform>();
                transform.Position = new Vector3(0f, 0f, 0f);
            }
        }

    }

}
