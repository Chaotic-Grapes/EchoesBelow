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
