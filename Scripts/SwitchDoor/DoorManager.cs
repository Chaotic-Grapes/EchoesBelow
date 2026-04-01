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
    static bool start;
    public static DoorManager instance;
    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        instance = this;

        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        start = OnStart(ref start);
    }

    public void DeactivateDoor(ulong objID)
    {
        AudioManager.instance.PlaySFX("SFX006");
        ref Active doorActive = ref Entity.FromId(World!, objID).GetComponent<Active>();
        doorActive.Enabled = false;
    }
}
