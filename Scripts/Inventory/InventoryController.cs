using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;
using System.Runtime;

namespace EchoesBelow.Scripts;

[Component] public record struct InventoryControllerComponent(bool start, int ms01_signifier, int ms02_signifier);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class InventoryController : SystemBase
{
    //I think i only need one instance
    public static InventoryController instance;
    public static int ms01_Count;
    public static int ms02_Count;

    public static List<ulong> ms01_List;
    public static List<ulong> ms02_List;

    public static List<ulong> slotObjIds;
    public static Dictionary<string, Slot> slotInstances;

    static bool isPressed_Q;
    static bool leftSlotIsSelected;
    protected override void OnCreate()
    {
        instance = this;
    }
    private bool OnStart(ref bool startBool, ulong objId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        //Initialize our values!
        ms01_Count = 0;
        ms02_Count = 0;

        leftSlotIsSelected = true;

        //Initialize our lists!
        ms01_List = new List<ulong>();
        ms02_List = new List<ulong>();

        slotObjIds = new List<ulong>();
        slotInstances = new Dictionary<string, Slot>();

        Log("0=================");
        //Finds every child of a slot, and aligns it to the parent slot in ui space
        //Also stores the unique references to instances of lists
        foreach(var slot in World!.Query<SlotComponent>())
        {
            Log("1");
            Entity slotEntity = Entity.FromId(World!, slot.Entity.Id);

            Slot slotInstance = new Slot();
            slotObjIds.Add(slot.Entity.Id);
            slotInstances.Add(slotEntity.GetComponent<Name>().Value.ToString(), slotInstance);
            Log("2");
            slotInstance.ms_ImageRefList = Entity.FromId(World!, slot.Entity.Id).GetChildren();
            foreach(Entity MS_imageRef in slotInstance.ms_ImageRefList)
            {
                MS_imageRef.GetComponent<GUIElement>().Position.X = slotEntity.GetComponent<GUIElement>().Position.X;
                MS_imageRef.GetComponent<GUIElement>().Position.Y = slotEntity.GetComponent<GUIElement>().Position.Y;

            }
            Log("3");
        }
        Log("4+++++++++++++++++");
      
        return true;
    }
    protected override void OnUpdate()
    {
        //check for input
        isPressed_Q = Input.IsKeyPressed(KeyCode.Q);

        foreach(var gameObject in World!.Query<InventoryControllerComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity.Id);
            //Todo
        }
        //MS01 slot is always first
        if (isPressed_Q)
        {
            leftSlotIsSelected = false;
            foreach (var gameObject in World!.Query<GUIElement, MatchSignifierComponent>())
            {
                //Toggle!
                if(gameObject.Entity.GetComponent<MatchSignifierComponent>().signifierID == 1234 ||
                   gameObject.Entity.GetComponent<MatchSignifierComponent>().signifierID == 2345)
                {
                gameObject.Entity.GetComponent<GUIElement>().Visible = !gameObject.Entity.GetComponent<GUIElement>().Visible;
                }
                
            }
        }

        if (Input.IsKeyPressed(KeyCode.J))
        {
            Log("J is Pressed!");
            foreach(var gameObject in World!.Query<MatchSignifierComponent>())
            {
                if(gameObject.Component1.signifierID == 2121)
                {
                    
                }
            }
        }

    }
    public void IncrementInStackSlot(int msID)
    {
        switch (msID)
        {
            
            case 1:
                ms01_Count = GMath.Clamp(++ms01_Count,(ushort)0,(ushort)10);
                
                foreach(var ui in World!.Query<MatchSignifierComponent>())
                {
                    foreach(var inventory in World!.Query<InventoryControllerComponent>())
                    {
                        if(ui.Component1.signifierID == inventory.Component1.ms01_signifier) 
                        {
                            ui.Entity.GetComponent<GUIText>().TextId = Strings.Intern($"{ms01_Count}");          
                        }
                    }
                }
                break;
            case 2:
                ms02_Count = GMath.Clamp(++ms02_Count, (ushort)0, (ushort)10);
               
                foreach (var ui in World!.Query<MatchSignifierComponent>())
                {
                    foreach (var inventory in World!.Query<InventoryControllerComponent>())
                    {
                        if (ui.Component1.signifierID == inventory.Component1.ms02_signifier)
                        {
                            ui.Entity.GetComponent<GUIText>().TextId = Strings.Intern($"{ms02_Count}");
                        }
                    }
                }
                break;
        }
        

    }
    public void DecrementInStackSlot(int msID)
    {
        switch (msID)
        {
            case 1:
                ms01_Count = GMath.Clamp(--ms01_Count, 0, 10);

                foreach (var ui in World!.Query<MatchSignifierComponent>())
                {
                    foreach (var inventory in World!.Query<InventoryControllerComponent>())
                    {
                        if (ui.Component1.signifierID == inventory.Component1.ms01_signifier)
                        {
                            ui.Entity.GetComponent<GUIText>().TextId = Strings.Intern($"{ms01_Count}");
                        }
                    }
                }
                break;
            case 2:
                ms02_Count = GMath.Clamp(--ms02_Count, 0, 10);

                foreach (var ui in World!.Query<MatchSignifierComponent>())
                {
                    foreach (var inventory in World!.Query<InventoryControllerComponent>())
                    {
                        if (ui.Component1.signifierID == inventory.Component1.ms02_signifier)
                        {
                            ui.Entity.GetComponent<GUIText>().TextId = Strings.Intern($"{ms02_Count}");
                        }
                    }
                }
                break;
        }
        
    }
}

[Component] public record struct SlotComponent(bool start);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class Slot
{
    public List<Entity> ms_ImageRefList { get; set; }
    public Slot()
    {
        ms_ImageRefList = new List<Entity>();
    }
}

