using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;
using EngineAudio = GrapeEngine.Scripting.Services.Audio;
using EngineAudioBus = GrapeEngine.Scripting.Services.AudioBus;

namespace EchoesBelow.Scripts.Audio;

[Component]
public record struct AudioManagerComponent(
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

    private const float DamageBusLowPassGain = 0.200f;
    // how long the damage muffle stays active
    private const float DamageBusLowPassDuration = 4.5f;


    public static AudioManager instance;
    public static List<Entity> sfxEntityList;
    // lookup from sfx name to entity
    public static Dictionary<string, Entity> sfxEntityDictionary;

    private float _damageLowPassTimer;
    private float _damageLowPassDurationActive;
    private float _damageLowPassStartGain;
    private EngineAudioBus _damageLowPassBus;
    private bool _debugBusLowPassForced;

    // runs once when the audio manager entity starts

    private bool OnStart(ref bool startBool, ulong objId)
    {
        // skip if start already ran
        if (startBool == true) return true;
        // mark this component as started
        startBool = true;
        //Todo

        // cache the system instance
        instance = this;

        // reset the damage filter timer
        _damageLowPassTimer = 0.0f;
        _damageLowPassDurationActive = 0.0f;
        _damageLowPassStartGain = DefaultBusLowPassGain;
        _damageLowPassBus = EngineAudioBus.SFX;
        _debugBusLowPassForced = false;
        // make sure the selected bus starts with no filter
        EngineAudio.ClearBusLowPassFilter(_damageLowPassBus);


        Entity audioManager = Entity.FromId(World!, objId);

        sfxEntityDictionary = [];
        sfxEntityList = audioManager.GetChildren();

        foreach (Entity e in sfxEntityList)
        {
            sfxEntityDictionary.Add(e.GetComponent<Name>().Value.ToString(), e);
        }

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

        if (Input.IsKeyPressed(KeyCode.P))
        {
            _debugBusLowPassForced = !_debugBusLowPassForced;
            float debugGain = _debugBusLowPassForced ? 0.05f : DefaultBusLowPassGain;
            _damageLowPassTimer = 0.0f;
            _damageLowPassDurationActive = 0.0f;
            EngineAudio.SetBusLowPassFilter(_damageLowPassBus, debugGain);
            Log(_debugBusLowPassForced ? "debug bus lpf on" : "debug bus lpf off");
        }

        // count down the damage low pass effect
        if (_damageLowPassTimer > 0.0f)
        {
            _damageLowPassTimer -= Time.DeltaTime;

            // lerp from the triggered gain back to normal over the active duration
            if (_damageLowPassDurationActive > 0.0f)
            {
                float progress = 1.0f - (_damageLowPassTimer / _damageLowPassDurationActive);
                progress = GMath.Clamp(progress, 0.0f, 1.0f);
                float gain = GMath.Lerp(_damageLowPassStartGain, DefaultBusLowPassGain, progress);
                EngineAudio.SetBusLowPassFilter(_damageLowPassBus, gain);
            }

            // restore the bus when the timer ends
            if (_damageLowPassTimer <= 0.0f)
            {
                _damageLowPassTimer = 0.0f;
                EngineAudio.SetBusLowPassFilter(_damageLowPassBus, DefaultBusLowPassGain);
            }
        }

        foreach (var audioManager in World!.Query<AudioManagerComponent, AudioSource, GUIElement>()) //This shld only happen once
        {
            _damageLowPassBus = (EngineAudioBus)audioManager.Component2.Bus;

            bool start_audioManager = audioManager.Component1.start;
            audioManager.Component1.start = OnStart(ref start_audioManager, audioManager.Entity.Id);

            //So this foreach will iterate thru all other audiosources once as well
            foreach (var sfxObject in World!.Query<AudioSource, AudioSFXComponent>()) //SFX objs only
            {
                bool start_sfxObject = sfxObject.Component2.start;
                sfxObject.Component2.start = OnStart2(ref start_sfxObject, ref sfxObject.Component1, ref sfxObject.Component2);

                sfxObject.Component1.Volume = sfxObject.Component2.startVolume * (audioManager.Component1.globalSFXVolume / 100); //Divided by a 100 to act as a percentage
            }

            //Debug Volume Slider
            if (Input.IsKeyPressed(KeyCode.I)) // -- sfx volume
            {
                audioManager.Component1.globalSFXVolume = GMath.Clamp(audioManager.Component1.globalSFXVolume - audioManager.Component1.volumeStep, 0, 100);
                Log("-1 volume / percentage: " + audioManager.Component1.globalSFXVolume / 100);
            }
            else if (Input.IsKeyPressed(KeyCode.O)) // ++ sfx volume
            {
                audioManager.Component1.globalSFXVolume = GMath.Clamp(audioManager.Component1.globalSFXVolume + audioManager.Component1.volumeStep, 0, 100);
                Log("+1 Volume / percentage: " + audioManager.Component1.globalSFXVolume / 100);
            }

            //Debug Volume Slider (ALWAYS ON)
            audioManager.Component3.Size.Y = 200 * (audioManager.Component1.globalSFXVolume / 100);
        }



    }

    // applies the temporary damage filter to the sfx bus
    public void TriggerDamageLowPass(float gain = DamageBusLowPassGain, float duration = DamageBusLowPassDuration)
    {
        gain = GMath.Clamp(gain, 0.0f, 1.0f);

        // set the selected bus filter immediately
        EngineAudio.SetBusLowPassFilter(_damageLowPassBus, gain);

        _damageLowPassStartGain = gain;
        _damageLowPassDurationActive = duration > 0.0f ? duration : 0.0f;

        // keep the longest active timer if this is called again quickly
        _damageLowPassTimer = GMath.Max(_damageLowPassTimer, duration);
    }


    public void PlaySFX(string sfxName)
    {
        Entity chosenSfx = Entity.FromId(World!, sfxEntityDictionary[sfxName].Id);
        ref AudioSource audsrc = ref chosenSfx.GetComponent<AudioSource>();

        audsrc.PlayOnStart = true;
    }
    public void StopSFX(string sfxName)
    {
        Entity chosenSfx = Entity.FromId(World!, sfxEntityDictionary[sfxName].Id);
        ref AudioSource audsrc = ref chosenSfx.GetComponent<AudioSource>();

        audsrc.PlayOnStart = false;
    }
}
