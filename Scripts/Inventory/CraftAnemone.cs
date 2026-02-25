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

[Component] public record struct CraftAnemoneComponent(bool start);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class CraftAnemone : SystemBase
{
    protected override void OnCreate()
    {
        
    }
    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo



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

            //Do everyth else


        }
    }
    public void PullPlayer()
    {

    }
}

[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class CraftAnemoneHandler : TriggerSystemBase
{

    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity other = Entity.FromId(World!, evt.OtherEntityId);


        if (Entity.FromId(World!, self.Id).TryGetComponent<CraftAnemoneComponent>(out CraftAnemoneComponent craftAnemone))
        {
            Log("I am a cnidarian and I'm proud!");
        }
        else
        {
            //Log("Trigger: I DONT have a Squidward!");
            return;
        }

    }
}
