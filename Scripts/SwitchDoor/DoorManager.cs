using EchoesBelow.Scripts;
using EchoesBelow.Scripts.Audio;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Scripts.SwitchDoor;


[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class DoorManager : SystemBase
{
    static bool start = false;
    public static DoorManager instance;
    float timer = 0f;

    float camShakeIntensity;

    const float shakeDuration = 0.45f;
    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        Log("Door start!");
        instance = this;

        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        start = OnStart(ref start);

        if(timer> 0f)
        {
            timer -= Time.DeltaTime;

            CamFollow.instance.CamShake(true, camShakeIntensity);
            if (Input.IsGamepadConnected(0))
            {
                Input.SetGamepadVibration(0, 0.8f, 0.8f);
            }


            if (timer < 0f)
            {
                timer = 0f;
                CamFollow.instance.CamShake(false, 0f);
            }
        }
    }

    public void DeactivateDoor(ulong objID, float camShakeIntensity)
    {
        int audioRandomiser = GMath.Random(1, 4);

        switch (audioRandomiser)
        {
            case 1:
                AudioManager.instance.PlaySFX("SFX006_Track01");
                break;
            case 2:
                AudioManager.instance.PlaySFX("SFX006_Track02");
                break;
            case 3:
                AudioManager.instance.PlaySFX("SFX006_Track03");
                break;
            case 4:
                AudioManager.instance.PlaySFX("SFX006_Track04");
                break;
        }



        ref Active doorActive = ref Entity.FromId(World!, objID).GetComponent<Active>();
        doorActive.Enabled = false;

        this.camShakeIntensity = camShakeIntensity;

        timer = shakeDuration;
    }
}
