using EchoesBelow.Scripts;
using EchoesBelow.Scripts.Audio;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using Scripts.Menu;
using System.Collections.Generic;

namespace Scripts.Menu;

//This one is Old and Deprecated
[Component] public record struct PauseMenuComponent(bool isPauseable, int resumeSiginifier, int exitSignifier, bool start);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class PauseMenu : SystemBase
{
    protected override void OnUpdate()
    {
        //nothin ere
    }
}