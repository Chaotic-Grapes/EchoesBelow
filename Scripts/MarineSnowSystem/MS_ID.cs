using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;


namespace EchoesBelow.Scripts.MarineSnowSystem;
[Component] public record struct MS_IDComponent(int msID, bool start);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class MS_ID : SystemBase
{
    //This is just a container of useful fields for Marine Snow
 
}
