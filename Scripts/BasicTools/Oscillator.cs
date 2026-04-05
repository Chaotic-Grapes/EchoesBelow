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

namespace EchoesBelow.Scripts;

public class OscillatorData
{
    public ulong objID {  get; set; }
    public Vector3 startPos { get; set; }

    public OscillatorData(ulong objID, Vector3 startPos)
    {
        this.objID = objID;
        this.startPos = startPos;
    }
}
[Component] public record struct OscillatorComponent(bool start, float period, float timer, float height, bool awake);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class Oscillator : SystemBase
{
    float oscillateFac;
    //[SerializeField] float period = 1f;
    const float tau = GMath.Pi * 2f; //tau intitalized as 6.283

    public static Oscillator instance;
    public Dictionary<ulong, OscillatorData> oscillators {  get; private set; }

    private bool OnAwake(ref bool awakeBool)
    {
        if (awakeBool == true) return true;
        awakeBool = true;
        //Todo
       
        instance = this;

        oscillators = new Dictionary<ulong, OscillatorData>();

        //End of Start
        return true;
    }
    private bool OnStart(ref bool startBool, Vector3 startPos, ulong objID)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        OscillatorData osc = new OscillatorData(objID, startPos);
        oscillators.Add(objID, osc);

        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        foreach (var gameObject in World!.Query<OscillatorComponent>())
        {
            bool awake = gameObject.Component1.awake;
            gameObject.Component1.awake = OnAwake(ref awake);
        }

        foreach (var gameObject in World!.Query<OscillatorComponent, LocalTransform>())
        {
            //Log("StartPos1: " + startPos);
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Component2.Position, gameObject.Entity.Id);


            gameObject.Component1.timer += Time.DeltaTime;

            if (gameObject.Component1.period < GMath.Epsilon) return; //NaN protection Mathf.Epsilon is the smallest possible float in unity
            float cycles = gameObject.Component1.timer / gameObject.Component1.period; //determines the number of cycles passed
            float rawSineWave = GMath.Sin(cycles * tau); //creates my sine wave w the period indicated. Returns a value btwn -1 to 1
            oscillateFac = (rawSineWave + 1) / 2f; //converts the range (-1 to 1) to (0 to 2) then (0-1)

            //Log(oscillators[gameObject.Entity.Id].objID + "osc: " + oscillateFac);
            //Log("startPos: " + oscillators[result.Entity.Id].startPos);

            gameObject.Entity.GetComponent<LocalTransform>().Position
            = new Vector3(gameObject.Entity.GetComponent<LocalTransform>().Position.X,
                          oscillators[gameObject.Entity.Id].startPos.Y + (gameObject.Component1.height * oscillateFac),0);
           
        }
    }
}



