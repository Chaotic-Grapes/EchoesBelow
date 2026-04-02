using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;

namespace EchoesBelow.Scripts.Audio;

[Component] public record struct AudioManagerComponent(bool start, float bgmStartVolume);
[RequireForUpdate<AudioManagerComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class AudioManager : SystemBase
{
    // normal bus state with no filter
    private const float DefaultBusLowPassGain = 1.0f;
    // normal resonance value (Q) for low-pass (1.0 = neutral - 10.0 max resonance)
    private const float DefaultBusLowPassResonance = 1.0f;
    // damage muffle strength for the sfx bus
    private const float DamageBusLowPassGain = 0.65f;
    // temporary resonance boost while damaged
    private const float DamageBusLowPassResonance = 2.0f;
    // how long the damage muffle stays active
    private const float DamageBusLowPassDuration = 2.0f;

    private static readonly AudioBus[] DamageLowPassBuses =
    {
        AudioBus.SFX,
        AudioBus.Music
    };

    public static AudioManager instance;
    public static List<Entity> sfxEntityList;
    public static Dictionary<string,Entity> sfxEntityDictionary;

    public float globalSFXVolume { get; private set; }
    public float globalBGMVolume { get; private set; }

    // countdown used to restore the sfx bus after damage
    private float _damageLowPassTimer;
    private float _damageLowPassDurationActive;
    private float _damageLowPassStartGain;
    private float _damageLowPassStartResonance;


    private bool OnStart(ref bool startBool, ulong objId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        instance = this;

        // reset the damage filter timer
        _damageLowPassTimer = 0.0f;
        _damageLowPassDurationActive = 0.0f;
        _damageLowPassStartGain = DefaultBusLowPassGain;
        _damageLowPassStartResonance = DefaultBusLowPassResonance;
        // make sure target buses start with no filter
        foreach (AudioBus bus in DamageLowPassBuses)
        {
            GrapeEngine.Scripting.Services.Audio.ClearBusLowPassFilter(bus);
            GrapeEngine.Scripting.Services.Audio.ClearBusLowPassResonance(bus);
        }

        Entity audioManager = Entity.FromId(World!, objId);

        sfxEntityDictionary = [];
        sfxEntityList = audioManager.GetChildren();

        //store initial bgm volume
        ref AudioManagerComponent audComp = ref audioManager.GetComponent<AudioManagerComponent>();
        audComp.bgmStartVolume = audioManager.GetComponent<AudioSource>().Volume;

        foreach(Entity e in sfxEntityList)
        {
            if (!e.TryGetComponent<Name>(out _))
            {
                continue;
            }

            // Use assignment to avoid hard failures when scene edits accidentally duplicate names.
            sfxEntityDictionary[e.GetComponent<Name>().Value.ToString()] = e;

            // SFX entries should not auto-play on scene load.
            if (e.TryGetComponent<AudioSource>(out AudioSource aSource) &&  aSource.Bus == AudioBus.SFX)
            {
                e.GetComponent<AudioSource>().PlayOnStart = false;
            }
        }

        //End of Start
        return true;
    }
    private bool OnStart2(ref bool startBool, ref AudioSource audioSource, ref AudioSFXComponent audioSFX)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        audioSFX.startVolume = audioSource.Volume;
        return true;
    }
    protected override void OnUpdate()
    {

        foreach (var audioManager in World!.Query<AudioManagerComponent, AudioSource>()) //This shld only happen once
        {
        
            bool start_audioManager = audioManager.Component1.start;
            audioManager.Component1.start = OnStart(ref start_audioManager, audioManager.Entity.Id);


        }

        //So this foreach will iterate thru all other audiosources once as well
        foreach (var sfxObject in World!.Query<AudioSource, AudioSFXComponent>()) //SFX objs only
        {
            bool start_sfxObject = sfxObject.Component2.start;
            sfxObject.Component2.start = OnStart2(ref start_sfxObject, ref sfxObject.Component1, ref sfxObject.Component2);
        }

        // count down the damage low pass effect
        if (_damageLowPassTimer > 0.0f)
        {
            _damageLowPassTimer -= Time.DeltaTime;

            if (_damageLowPassDurationActive > 0.0f)
            {
                float progress = 1.0f - (_damageLowPassTimer / _damageLowPassDurationActive);
                progress = GMath.Clamp(progress, 0.0f, 1.0f);
                float gain = GMath.Lerp(_damageLowPassStartGain, DefaultBusLowPassGain, progress);
                float resonance = GMath.Lerp(_damageLowPassStartResonance, DefaultBusLowPassResonance, progress);

                foreach (AudioBus bus in DamageLowPassBuses)
                {
                    GrapeEngine.Scripting.Services.Audio.SetBusLowPassFilter(bus, gain);
                    GrapeEngine.Scripting.Services.Audio.SetBusLowPassResonance(bus, resonance);
                }
            }

            // restore the bus when the timer ends
            if (_damageLowPassTimer <= 0.0f)
            {
                _damageLowPassTimer = 0.0f;
                foreach (AudioBus bus in DamageLowPassBuses)
                {
                    GrapeEngine.Scripting.Services.Audio.SetBusLowPassFilter(bus, DefaultBusLowPassGain);
                    GrapeEngine.Scripting.Services.Audio.SetBusLowPassResonance(bus, DefaultBusLowPassResonance);
                }
            }
        }
    }

    // applies the temporary damage filter to target buses
    public void TriggerDamageLowPass(
        float gain = DamageBusLowPassGain,
        float duration = DamageBusLowPassDuration,
        float resonance = DamageBusLowPassResonance)
    {
        gain = GMath.Clamp(gain, 0.0f, 1.0f);
        resonance = GMath.Clamp(resonance, 1.0f, 10.0f);

        // set filters immediately
        foreach (AudioBus bus in DamageLowPassBuses)
        {
            GrapeEngine.Scripting.Services.Audio.SetBusLowPassFilter(bus, gain);
            GrapeEngine.Scripting.Services.Audio.SetBusLowPassResonance(bus, resonance);
        }

        _damageLowPassStartGain = gain;
        _damageLowPassStartResonance = resonance;
        _damageLowPassDurationActive = duration > 0.0f ? duration : 0.0f;

        // keep the longest active timer if this is called again quickly
        _damageLowPassTimer = GMath.Max(_damageLowPassTimer, duration);
    }


    public void PlaySFX(string sfxName)
    {
        if (sfxEntityDictionary == null || !sfxEntityDictionary.TryGetValue(sfxName, out Entity sfxEntity))
        {
            Log("missing sfx key " + sfxName, LogLevel.Warning);
            return;
        }

        if (!sfxEntity.IsAlive)
        {
            Log("sfx entity is not alive for key " + sfxName, LogLevel.Warning);
            return;
        }

        if (!sfxEntity.TryGetComponent<AudioSource>(out _))
        {
            Log("sfx entity missing AudioSource for key " + sfxName, LogLevel.Warning);
            return;
        }

        Entity chosenSfx = Entity.FromId(World!, sfxEntity.Id);
        if (!chosenSfx.IsAlive)
        {
            Log("resolved sfx entity is not alive for key " + sfxName, LogLevel.Warning);
            return;
        }

        ref AudioSource audsrc = ref chosenSfx.GetComponent<AudioSource>();

        // Re-arm the edge-trigger so repeated key presses remain stable.
        audsrc.PlayOnStart = false;
        audsrc.PlayOnStart = true;
    }
    public void StopSFX(string sfxName)
    {
        if (sfxEntityDictionary == null || !sfxEntityDictionary.TryGetValue(sfxName, out Entity sfxEntity))
        {
            Log("missing sfx key " + sfxName, LogLevel.Warning);
            return;
        }

        Entity chosenSfx = Entity.FromId(World!, sfxEntity.Id);
        ref AudioSource audsrc = ref chosenSfx.GetComponent<AudioSource>();
     
        audsrc.PlayOnStart = false;
    }

    public void UpdateBGMVolume(float percentage)
    {
        foreach(var bgm in World!.Query<AudioSource, AudioManagerComponent>())
        {
            bgm.Component1.Volume = bgm.Component2.bgmStartVolume * percentage;
        }
    }
    public void UpdateSFXVolume(float percentage)
    {
        foreach(var sfx in World!.Query<AudioSource, AudioSFXComponent>())
        {
            sfx.Component1.Volume = sfx.Component2.startVolume * percentage;
        }
    }
}
