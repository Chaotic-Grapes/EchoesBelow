using GrapeEngine.Scripting.Events;
using System.Collections.Generic;


namespace EchoesBelow.Scripts;

public class AnimState
{
    public string name { get; set; }
    public int row         { get; set; }
    public int frameOffset { get; set; }
    public int frameLength { get; set; }
    public float fps       { get; set; }
    //This is a constructor
    public AnimState(string name, int row, int frameOffset, int frameLength, float fps)
    {
        this.name = name;
        this.row = row;
        this.frameOffset = frameOffset;
        this.frameLength = frameLength;
        this.fps = fps;
    }
}
public enum playerAnimPreset
{
    Idle, Dash
}
