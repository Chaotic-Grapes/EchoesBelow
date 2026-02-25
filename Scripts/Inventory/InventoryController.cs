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
    public static ulong[] nonStackableSlots;

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
        nonStackableSlots = new ulong[5];

        //Finds every child of a slot, and aligns it to the parent slot in ui space
        //Also stores the unique references to instances of lists
        foreach(var slot in World!.Query<SlotComponent>())
        {
            Entity slotEntity = Entity.FromId(World!, slot.Entity.Id);
            string slotName = slotEntity.GetComponent<Name>().Value.ToString();

            Slot slotInstance = new Slot(false); // initialized with a false isStoringItem bool
            slotObjIds.Add(slot.Entity.Id);
            slotInstances.Add(slotName, slotInstance);

            if (slotName == "Slot06" || slotName == "Slot07") { }
            else if (slotName == "Slot05")
            {
                nonStackableSlots[4] = slot.Entity.Id;
            }
            else if (slotName == "Slot04")
            {
                nonStackableSlots[3] = slot.Entity.Id;
            }
            else if (slotName == "Slot03")
            {
                nonStackableSlots[2] = slot.Entity.Id;
            }
            else if (slotName == "Slot02")
            {
                nonStackableSlots[1] = slot.Entity.Id;
            }
            else if (slotName == "Slot01")
            {
                nonStackableSlots[0] = slot.Entity.Id;
            }
            else { }



            slotInstance.ms_ImageRefList = Entity.FromId(World!, slot.Entity.Id).GetChildren();
            foreach(Entity MS_imageRef in slotInstance.ms_ImageRefList)
            {
                MS_imageRef.GetComponent<GUIElement>().Position.X = slotEntity.GetComponent<GUIElement>().Position.X;
                MS_imageRef.GetComponent<GUIElement>().Position.Y = slotEntity.GetComponent<GUIElement>().Position.Y;
            }
            
        }
      
        return true;
    }
    protected override void OnUpdate()
    {
        //This is gonna be overhauled
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

        //For Checking
        if (Input.IsKeyPressed(KeyCode.J))
        {
            Log("J is Pressed!==========");
            foreach(ulong u in slotObjIds)
            {
                Entity e = Entity.FromId(World!, u);
                Log($"Obj ID: {u} of name: {e.GetComponent<Name>().Value.ToString()} >>");
                foreach(KeyValuePair<string,Slot> stringSlot in slotInstances)
                {
                    if(stringSlot.Key == e.GetComponent<Name>().Value.ToString())
                    {
                        Log($"Key Found!: {stringSlot.Key} contains slot instance with a msRefImageListCount of {stringSlot.Value.ms_ImageRefList.Count}");
                        Log("isStoringItem = " + stringSlot.Value.isStoringItem);
                    }
                    else { Log("Nothing Found in here"); }
                }
            }
            Log("**************************");
            for(int i = 0; i<5; i++)
            {
                Log($"index {i} of nonStackableSlots contains {Entity.FromId(World!,nonStackableSlots[i]).GetComponent<Name>().Value.ToString()}");
            }
            Log("End======================");
        }

    }
    public void AddToInventory(int msID)
    {
        switch (msID)
        {
            case 1:
            ms01_Count = GMath.Clamp(++ms01_Count,(ushort)0,(ushort)10);
                
            foreach(var gameObject in World!.Query<MatchSignifierComponent>())
            {
                foreach(var gameObject2 in World!.Query<InventoryControllerComponent>())
                {
                    if(gameObject.Component1.signifierID == gameObject2.Component1.ms01_signifier) 
                    {
                        gameObject.Entity.GetComponent<GUIText>().TextId = Strings.Intern($"{ms01_Count}");          
                    }
                }
            }
            break;
            case 2:
            ms02_Count = GMath.Clamp(++ms02_Count, (ushort)0, (ushort)10);
               
            foreach (var gameObject in World!.Query<MatchSignifierComponent>())
            {
                foreach (var gameObject2 in World!.Query<InventoryControllerComponent>())
                {
                    if (gameObject.Component1.signifierID == gameObject2.Component1.ms02_signifier)
                    {
                        gameObject.Entity.GetComponent<GUIText>().TextId = Strings.Intern($"{ms02_Count}");
                    }
                }
            }
            break;
            case 3:
                Log($"3 Available Slot: {Entity.FromId(World!,FindAvailableSlot().Id).GetComponent<Name>().Value.ToString()}");
                //Find the slotInstance that contains the ms_imageRefList
                AddItemToNonStackableSlot("MS03_ui");
                break;
            case 4:
                Log($"4 Available Slot: {Entity.FromId(World!, FindAvailableSlot().Id).GetComponent<Name>().Value.ToString()}");

                AddItemToNonStackableSlot("MS04_ui");
                break;
            case 5:
                Log($"5 Available Slot: {Entity.FromId(World!,FindAvailableSlot().Id).GetComponent<Name>().Value.ToString()}");
                AddItemToNonStackableSlot("MS05_ui");
                break;
            case 6:
                Log($"6 Available Slot: {Entity.FromId(World!,FindAvailableSlot().Id).GetComponent<Name>().Value.ToString()}");
                AddItemToNonStackableSlot("MS06_ui");
                break;
            case 7:
                Log($"7 Available Slot: {Entity.FromId(World!,FindAvailableSlot().Id).GetComponent<Name>().Value.ToString()}");
                AddItemToNonStackableSlot("MS07_ui");
                break;
        }
        

    }

    public void AddItemToNonStackableSlot(string msIdCheck)
    {
        foreach (Entity image in slotInstances[Entity.FromId(World!, FindAvailableSlot().Id).GetComponent<Name>().Value.ToString()].ms_ImageRefList)
        {
            if (Entity.FromId(World!, image.Id).GetComponent<Name>().Value.ToString() == msIdCheck)
            {
                image.GetComponent<GUIElement>().Visible = true;
                slotInstances[Entity.FromId(World!, FindAvailableSlot().Id).GetComponent<Name>().Value.ToString()].isStoringItem = true;
            }
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
    public Entity FindAvailableSlot()
    {
        //Iterate thru the nonstackable era in sequence
        for (int i=4; i>=0; i--)
        {
            Entity slotEntity = Entity.FromId(World!,nonStackableSlots[i]);
            if (!slotInstances[slotEntity.GetComponent<Name>().Value.ToString()].isStoringItem)
            {
                return slotEntity;
            }
        }
        //Default case
        return Entity.FromId(World!, nonStackableSlots[0]);
    }
}

[Component] public record struct SlotComponent(bool start);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class Slot
{
    public List<Entity> ms_ImageRefList { get; set; }
    public bool isStoringItem { get; set; }
    public Slot(bool isStoringItem)
    {
        ms_ImageRefList = new List<Entity>();
        this.isStoringItem = isStoringItem;
    }
}

public enum MSparticle
{
    MS01, MS02, MS03, MS04, MS05, MS06, MS07
}

