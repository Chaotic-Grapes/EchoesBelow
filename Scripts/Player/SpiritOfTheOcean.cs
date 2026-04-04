using EchoesBelow.Scripts;
using EchoesBelow.Scripts.Audio;
using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;


namespace Scripts;

[Component] public record struct SpiritOfTheOceanComponent(bool start, bool isEnabled);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class SpiritOfTheOcean : SystemBase
{
    public static bool isEnabled;
    public static SpiritOfTheOcean instance;
    public static ulong objID;

    float timer = 0f;
    private bool OnStart(ref bool startBool, Entity entity)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        timer = 0f;

        instance = this;
        objID = entity.Id;

        ref SpiritOfTheOceanComponent spirit = ref entity.GetComponent<SpiritOfTheOceanComponent>();
        spirit.isEnabled = false;
        isEnabled = false;

        TheSpiritBeckonsThee(false);

        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {

        if (timer > 0f)
        {
            timer -= Time.DeltaTime;
            if (timer <= 0f) 
            {
                timer = 0f;
                TutorialController.instance.EnableSpiritOfTheOcean();
            }
        }

        

        //Use this
        foreach (var gameObject in World!.Query<SpiritOfTheOceanComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity);

            //Do everyth else


        }
    }
    public void TheSpiritBeckonsThee(bool isImbueing)
    {
        if (isImbueing)
        {
            ref ParticleEmitter prtcleEmit = ref Entity.FromId(World!,objID).GetComponent<ParticleEmitter>();
            prtcleEmit.EmissionRate = 50f;

            ref Light2D light2D = ref Entity.FromId(World!, objID).GetComponent<Light2D>();
            light2D.Position.Z = 1f;

            ref SpriteRenderer2D spr = ref Player.instance.player.GetComponent<SpriteRenderer2D>();
            //spr.Color = new Color(-2.435f, -2.401f, -2.385f,1f);

            AudioManager.instance.PlaySFX("SFX020_SpiritChime");

            timer = 1.5f;

            //housekeeping
            isEnabled = true;
            Entity.FromId(World!, objID).GetComponent<SpiritOfTheOceanComponent>().isEnabled = true;
        }
        else
        {
            ref ParticleEmitter prtcleEmit = ref Entity.FromId(World!, objID).GetComponent<ParticleEmitter>();
            prtcleEmit.EmissionRate = 0f;

            ref Light2D light2D = ref Entity.FromId(World!, objID).GetComponent<Light2D>();
            light2D.Position.Z = 0f;

            ref SpriteRenderer2D spr = ref Player.instance.player.GetComponent<SpriteRenderer2D>();
            spr.Color = new Color(1f, 1f, 1f, 1f);

            //housekeeping
            isEnabled = false;
            Entity.FromId(World!, objID).GetComponent<SpiritOfTheOceanComponent>().isEnabled = false;
        }


    }


}
