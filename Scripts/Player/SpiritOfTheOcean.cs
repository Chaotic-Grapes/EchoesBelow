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
                if (TutorialController.instance != null)
                {
                    TutorialController.instance.EnableSpiritOfTheOcean();
                }
            }
        }

        foreach (var gameObject in World!.Query<SpiritOfTheOceanComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity);
        }
    }
    public void TheSpiritBeckonsThee(bool isImbueing)
    {
        if (objID == 0) return;

        Entity spiritEntity;
        try
        {
            spiritEntity = Entity.FromId(World!, objID);
        }
        catch
        {
            return;
        }

        if (isImbueing)
        {
            if (spiritEntity.TryGetComponent<ParticleEmitter>(out var prtcleEmit))
            {
                prtcleEmit.EmissionRate = 50f;
            }

            if (spiritEntity.TryGetComponent<Light2D>(out var light2D))
            {
                light2D.Position.Z = 1f;
            }

            if (spiritEntity.HasComponent<SpiritOfTheOceanComponent>())
            {
                spiritEntity.GetComponent<SpiritOfTheOceanComponent>().isEnabled = true;
            }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX("SFX020_SpiritChime");
            }

            timer = 1.5f;

            //housekeeping
            isEnabled = true;
        }
        else
        {
            if (spiritEntity.TryGetComponent<ParticleEmitter>(out var prtcleEmit))
            {
                prtcleEmit.EmissionRate = 0f;
            }

            if (spiritEntity.TryGetComponent<Light2D>(out var light2D))
            {
                light2D.Position.Z = 0f;
            }

            if (Player.instance != null)
            {
                try
                {
                    if (Player.instance.player.Id != 0 && Entity.FromId(World!, Player.instance.player.Id).HasComponent<SpriteRenderer2D>())
                    {
                        ref SpriteRenderer2D spr = ref Player.instance.player.GetComponent<SpriteRenderer2D>();
                        spr.Color = new Color(1f, 1f, 1f, 1f);
                    }
                }
                catch
                {
                    // Ignore transient startup/despawn state.
                }
            }

            //housekeeping
            isEnabled = false;
            if (spiritEntity.HasComponent<SpiritOfTheOceanComponent>())
            {
                spiritEntity.GetComponent<SpiritOfTheOceanComponent>().isEnabled = false;
            }
        }
    }


}
