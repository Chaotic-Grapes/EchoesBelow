using GrapeEngine.Math;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;


namespace Scripts;

public delegate void Action();
public class MenuPanel
{
    public ulong objID { get; set; }
    public string name { get; set; }
    public Entity Entity { get; set; }

    //For directional key input (Gamepad and Mouse)
    public Entity up {  get; set; }
    public Entity down { get; set; }
    public Entity left { get; set; }
    public Entity right { get; set; }

    //unused
    public Vector2 startPos {  get; set; }
    public Vector2 retreatPos {  get; set; } // must be keyed in remotely?

    //Delegate for storing function
    public Action Action {  get; set; }
    public MenuPanel(ulong objID, string name ,Entity Entity)
    {
        this.objID = objID;
        this.name = name;
        this.Entity = Entity;
    }
}

public class TutorialPanel
{
    public ulong objID { get; set; }
    public string name { get; set; }
    public Entity Entity { get; set; }
    public TutorialPanel NextPanel { get; set; }
    //Delegate for storing function
    public Action Action { get; set; }
    public TutorialPanel(ulong objID, string name, Entity Entity)
    {
        this.objID = objID;
        this.name = name;
        this.Entity = Entity;
    }
}

