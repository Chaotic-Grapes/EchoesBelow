using EchoesBelow.Scripts;
using EchoesBelow.Scripts.Audio;
using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;

namespace Scripts.CraftingSystem;

[Component] public record struct CraftPortComponent(int NSEW_1234, ulong parentObjId, ulong parentAnemone);
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class CraftPort
{
    //A simple identifier for CMachineTriggers, North is 1, South is 2, East is 3 and West is 4

}
