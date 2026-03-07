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
    public static AudioManager instance;
    public static List<Entity> sfxEntityList;
    public static Dictionary<string,Entity> sfxEntityDictionary;
    
    private bool OnStart(ref bool startBool, ulong objId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        instance = this;

        Entity audioManager = Entity.FromId(World!, objId);

        sfxEntityDictionary = [];
        sfxEntityList = audioManager.GetChildren();

        foreach(Entity e in sfxEntityList)
        {
            sfxEntityDictionary.Add(e.GetComponent<Name>().Value.ToString(), e);
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
        foreach (var audioManager in World!.Query<AudioManagerComponent, AudioSource, GUIElement>()) //This shld only happen once
        {
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
                audioManager.Component1.globalSFXVolume = GMath.Clamp(audioManager.Component1.globalSFXVolume - audioManager.Component1.volumeStep,0,100);
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
