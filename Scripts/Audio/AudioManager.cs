using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;

namespace EchoesBelow.Scripts.Audio;

[Component] public record struct AudioManagerComponent(
    bool start, 
    //From 1 to 100
    float globalSFXVolume,
    //From 1 to 100
    float globalBGMVolume, 

    float volumeStep

);
[RequireForUpdate<AudioManagerComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class AudioManager : SystemBase
{
    // normal bus state with no filter
    private const float DefaultBusLowPassGain = 1.0f;
    // damage muffle strength for the sfx bus
    private const float DamageBusLowPassGain = 0.43f;
    // how long the damage muffle stays active
    private const float DamageBusLowPassDuration = 4.0f;

    private static readonly AudioBus[] DamageLowPassBuses =
    {
        AudioBus.SFX,
        AudioBus.Music
    };

    public static AudioManager instance;
    public static List<Entity> sfxEntityList;
    public static Dictionary<string,Entity> sfxEntityDictionary;
    private static Dictionary<ulong, float> baseAudioVolumeByEntityId;

    // countdown used to restore the sfx bus after damage
    private float _damageLowPassTimer;
    private float _damageLowPassDurationActive;
    private float _damageLowPassStartGain;


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
        // make sure target buses start with no filter
        foreach (AudioBus bus in DamageLowPassBuses)
        {
            GrapeEngine.Scripting.Services.Audio.ClearBusLowPassFilter(bus);
        }

        Entity audioManager = Entity.FromId(World!, objId);

        sfxEntityDictionary = [];
        sfxEntityList = audioManager.GetChildren();
        baseAudioVolumeByEntityId = [];

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

        foreach (var audio in World!.Query<AudioSource>())
        {
            baseAudioVolumeByEntityId[audio.Entity.Id] = audio.Component1.Volume;
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

        // count down the damage low pass effect
        if (_damageLowPassTimer > 0.0f)
        {
            _damageLowPassTimer -= Time.DeltaTime;

            if (_damageLowPassDurationActive > 0.0f)
            {
                float progress = 1.0f - (_damageLowPassTimer / _damageLowPassDurationActive);
                progress = GMath.Clamp(progress, 0.0f, 1.0f);
                float gain = GMath.Lerp(_damageLowPassStartGain, DefaultBusLowPassGain, progress);

                foreach (AudioBus bus in DamageLowPassBuses)
                {
                    GrapeEngine.Scripting.Services.Audio.SetBusLowPassFilter(bus, gain);
                }
            }

            // restore the bus when the timer ends
            if (_damageLowPassTimer <= 0.0f)
            {
                _damageLowPassTimer = 0.0f;
                foreach (AudioBus bus in DamageLowPassBuses)
                {
                    GrapeEngine.Scripting.Services.Audio.SetBusLowPassFilter(bus, DefaultBusLowPassGain);
                }
            }
        }

        foreach (var audioManager in World!.Query<AudioManagerComponent, AudioSource, GUIElement>()) //This shld only happen once
        {
            bool start_audioManager = audioManager.Component1.start;
            audioManager.Component1.start = OnStart(ref start_audioManager, audioManager.Entity.Id);

            foreach (var audio in World!.Query<AudioSource>())
            {
                if (!baseAudioVolumeByEntityId.TryGetValue(audio.Entity.Id, out float baseVolume))
                {
                    baseVolume = audio.Component1.Volume;
                    baseAudioVolumeByEntityId[audio.Entity.Id] = baseVolume;
                }

                bool isSfx = audio.Entity.TryGetComponent<AudioSFXComponent>(out _);
                float sliderPercent = isSfx
                    ? (audioManager.Component1.globalSFXVolume / 100.0f)
                    : (audioManager.Component1.globalBGMVolume / 100.0f);

                audio.Component1.Volume = baseVolume * sliderPercent;
            }

            //So this foreach will iterate thru all other audiosources once as well
            foreach (var sfxObject in World!.Query<AudioSource, AudioSFXComponent>()) //SFX objs only
            {
                bool start_sfxObject = sfxObject.Component2.start;
                sfxObject.Component2.start = OnStart2(ref start_sfxObject, ref sfxObject.Component1, ref sfxObject.Component2);
            }

            //Debug Volume Slider (ALWAYS ON)
            audioManager.Component3.Size.Y = 200 * (audioManager.Component1.globalSFXVolume / 100);
        }



    }

    // applies the temporary damage filter to target buses
    public void TriggerDamageLowPass(float gain = DamageBusLowPassGain, float duration = DamageBusLowPassDuration)
    {
        gain = GMath.Clamp(gain, 0.0f, 1.0f);

        // set filters immediately
        foreach (AudioBus bus in DamageLowPassBuses)
        {
            GrapeEngine.Scripting.Services.Audio.SetBusLowPassFilter(bus, gain);
        }

        _damageLowPassStartGain = gain;
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
}
