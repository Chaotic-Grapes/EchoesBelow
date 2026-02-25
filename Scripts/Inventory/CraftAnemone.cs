using EchoesBelow.Scripts.Audio;
using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;

namespace EchoesBelow.Scripts.BasicTools;

[Component] public record struct CraftAnemoneComponent(bool start, float lerpFacInMiliseconds);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class CraftAnemone : SystemBase
{
    public static bool isCaptured;
    public static CraftAnemone instance;
    protected override void OnCreate()
    {
        
    }
    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        isCaptured = false;
        instance = this;

        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        //Use this
        foreach (var gameObject in World!.Query<CraftAnemoneComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start);

            float lerpFac = gameObject.Component1.lerpFacInMiliseconds / 1000f;

            //Do everyth else
            if (isCaptured) TractorBeam(CraftAnemoneHandler.capturedEntity, gameObject.Entity.Id, lerpFac);

        }
    }
    public void TractorBeam(Entity e, ulong objId, float lerpFac)
    {
        Log("AHH");
        Entity capturedEntity = Entity.FromId(World!, e.Id);
        Entity anemone = Entity.FromId(World!, objId);

        ref LocalTransform capturedEntityTransform = ref capturedEntity.GetComponent<LocalTransform>();
        ref LocalTransform transform = ref anemone.GetComponent<LocalTransform>();

        capturedEntityTransform.Position = new Vector3(GMath.Lerp(capturedEntityTransform.Position.X, transform.Position.X, 0.04f),
                                                       GMath.Lerp(capturedEntityTransform.Position.Y, transform.Position.Y, 0.04f), lerpFac);

        //If my speed is near 0, means Im close to the checkpoint
        //RESPAWN
        float playerXpos = capturedEntity.GetComponent<LocalTransform>().Position.X;
        float checkPXpos = transform.Position.X;

        float playerYpos = capturedEntity.GetComponent<LocalTransform>().Position.Y;
        float checkPYpos = transform.Position.Y;

        if (checkPXpos - 0.125f < playerXpos && playerXpos < checkPXpos + 0.125f &&
            checkPYpos - 0.125f < playerYpos && playerYpos < checkPYpos + 0.125f)
        {
            capturedEntity.GetComponent<Active>().Enabled = true;

            isCaptured = false;
        }
        Log("Woahh");
    }
}

[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class CraftAnemoneHandler : TriggerSystemBase
{
    public static Entity capturedEntity;
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity other = Entity.FromId(World!, evt.OtherEntityId);
        
        if (Entity.FromId(World!, self.Id).HasComponent<CraftAnemoneComponent>()) Log("I am a cnidarian and I'm proud!");
        else return;

        if (other.HasComponent<PlayerComponent>())
        {
            Log("he has a player and now Im in the beam");
            capturedEntity = other;
            CraftAnemone.isCaptured = true;
        }
    }
}
