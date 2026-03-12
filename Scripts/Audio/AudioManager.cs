using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;
//using GrapeEngine.Scripting.Services.Audio;


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
    private const float DamageBusLowPassGain = 0.45f;
    // how long the damage muffle stays active
    private const float DamageBusLowPassDuration = 0.35f;

    // global access to this system instance
    public static AudioManager instance;
    // list of sfx child entities under the audio manager
    public static List<Entity> sfxEntityList;
    // lookup from sfx name to entity
    public static Dictionary<string,Entity> sfxEntityDictionary;
    // countdown used to restore the sfx bus after damage
    private float _damageLowPassTimer;
    
    // runs once when the audio manager entity starts
    private bool OnStart(ref bool startBool, ulong objId)
    {
        // skip if start already ran
        if (startBool == true) return true;
        // mark this component as started
        startBool = true;

        // cache the system instance
        instance = this;
        // reset the damage filter timer
        _damageLowPassTimer = 0.0f;
        // make sure the sfx bus starts with no filter
        GrapeEngine.Scripting.Services.Audio.ClearBusLowPassFilter(AudioBus.SFX);//=================================================================

        // get the audio manager entity
        Entity audioManager = Entity.FromId(World!, objId);

        // create the sfx lookup containers
        sfxEntityDictionary = [];
        sfxEntityList = audioManager.GetChildren();

        // register every child sfx by its name
        foreach(Entity e in sfxEntityList)
        {
            sfxEntityDictionary.Add(e.GetComponent<Name>().Value.ToString(), e);
        }

        // finish start
        return true;
    }

    // runs once for each sfx object to cache its original volume
    private bool OnStart2(ref bool startBool, ref AudioSource audioSource, ref AudioSFXComponent audioSFX)
    {
        // skip if start already ran
        if (startBool == true) return true;
        // mark this component as started
        startBool = true;

        // store the original source volume for later scaling
        audioSFX.startVolume = audioSource.Volume;
        return true;
    }

    // runs every frame while the game is playing
    protected override void OnUpdate()
    {
        // count down the damage low pass effect
        if (_damageLowPassTimer > 0.0f)
        {
            _damageLowPassTimer -= Time.DeltaTime;

            // restore the bus when the timer ends
            if (_damageLowPassTimer <= 0.0f)
            {
                _damageLowPassTimer = 0.0f;
                GrapeEngine.Scripting.Services.Audio.SetBusLowPassFilter(AudioBus.SFX, DefaultBusLowPassGain);//=============================================================
            }
        }

        // this query should only match the main audio manager object
        foreach (var audioManager in World!.Query<AudioManagerComponent, AudioSource, GUIElement>()) //This shld only happen once
        {
            // run the manager start logic once
            bool start_audioManager = audioManager.Component1.start;
            audioManager.Component1.start = OnStart(ref start_audioManager, audioManager.Entity.Id);

            // update every sfx source volume using the manager percentage
            foreach (var sfxObject in World!.Query<AudioSource, AudioSFXComponent>()) //SFX objs only
            {
                // run the per source start logic once
                bool start_sfxObject = sfxObject.Component2.start;
                sfxObject.Component2.start = OnStart2(ref start_sfxObject, ref sfxObject.Component1, ref sfxObject.Component2);

                // scale each source from its saved starting volume
                sfxObject.Component1.Volume = sfxObject.Component2.startVolume * (audioManager.Component1.globalSFXVolume / 100); //Divided by a 100 to act as a percentage
            }

            // lower the debug sfx volume with keyboard input
            if (Input.IsKeyPressed(KeyCode.I)) // -- sfx volume
            {
                audioManager.Component1.globalSFXVolume = GMath.Clamp(audioManager.Component1.globalSFXVolume - audioManager.Component1.volumeStep,0,100);
                Log("- volume / percentage: " + audioManager.Component1.globalSFXVolume / 100);
            }

            // raise the debug sfx volume with keyboard input
            else if (Input.IsKeyPressed(KeyCode.O)) // ++ sfx volume
            {
                audioManager.Component1.globalSFXVolume = GMath.Clamp(audioManager.Component1.globalSFXVolume + audioManager.Component1.volumeStep, 0, 100);
                Log("+ Volume / percentage: " + audioManager.Component1.globalSFXVolume / 100);
            }

            // update the ui meter to match the current sfx volume
            audioManager.Component3.Size.Y = 200 * (audioManager.Component1.globalSFXVolume / 100);
        }



    }

    // applies the temporary damage filter to the sfx bus
    public void TriggerDamageLowPass(float gain = DamageBusLowPassGain, float duration = DamageBusLowPassDuration)
    {
        // set the bus filter immediately
        GrapeEngine.Scripting.Services.Audio.SetBusLowPassFilter(AudioBus.SFX, gain);//===================================================================

        // keep the longest active timer if this is called again quickly
        _damageLowPassTimer = GMath.Max(_damageLowPassTimer, duration);
    }

    // turns on an sfx source by name
    public void PlaySFX(string sfxName)
    {
        // find the requested sfx entity
        Entity chosenSfx = Entity.FromId(World!, sfxEntityDictionary[sfxName].Id);

        // enable play on start so the source begins playing
        ref AudioSource audsrc = ref chosenSfx.GetComponent<AudioSource>();
        
        audsrc.PlayOnStart = true;
    }

    // turns off an sfx source by name
    public void StopSFX(string sfxName)
    {
        // find the requested sfx entity
        Entity chosenSfx = Entity.FromId(World!, sfxEntityDictionary[sfxName].Id);

        // disable play on start so the source stops being requested
        ref AudioSource audsrc = ref chosenSfx.GetComponent<AudioSource>();
     
        audsrc.PlayOnStart = false;
    }
}
