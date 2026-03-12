using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using System.Collections.Generic;


namespace EchoesBelow.Scripts;

[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class PlayerAnimManager : SystemBase
{
    public static PlayerAnimManager instance;
    public string currentState;
    protected override void OnCreate()
    {
        instance = this;
    }

    public void SetAnimState(AnimState animState)
    {
        foreach(var animator in  World!.Query<PlayerComponent, SpriteSheetAnimation2D>())
        {
            currentState = animState.name;
            animator.Component2.Row = animState.row;
            animator.Component2.FrameOffset = animState.frameOffset;
            animator.Component2.FrameLength = animState.frameLength;
            animator.Component2.FramesPerSecond = animState.fps;
        }
    }
}
