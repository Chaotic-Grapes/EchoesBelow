using EchoesBelow.Scripts.MarineSnowSystem;
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

    public static ulong[] slotObjIds;
    public static Dictionary<string, Slot> slotInstances;

    static bool isPressed_Q;
    static bool isPressed_X;

    public bool isEnabled_xInput = true;

    static int iterator;

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

        //Initialize our lists!
        ms01_List = new List<ulong>();
        ms02_List = new List<ulong>();

        slotObjIds = new ulong[7];
        slotInstances = new Dictionary<string, Slot>();

        iterator = slotObjIds.Length - 1;

        //Finds every child of a slot, and aligns it to the parent slot in ui space
        //Also stores the unique references to instances of lists
        foreach (var slot in World!.Query<SlotComponent>())
        {
            Entity slotEntity = Entity.FromId(World!, slot.Entity.Id);
            string slotName = slotEntity.GetComponent<Name>().Value.ToString();

            Slot slotInstance = new Slot(false, 0); // initialized with a false isStoringItem bool, 0 refers to an empty slot
            //slotObjIds.Add(slot.Entity.Id);
            slotInstances.Add(slotName, slotInstance);

            //Arranging in the correct order for nonstackables and the total overarching list
            if (slotName == "Slot07") //contains only MS02s
            {
                slotObjIds[6] = slot.Entity.Id;
            }
            else if(slotName == "Slot06") //contains only MS01s
            {
                slotObjIds[5] = slot.Entity.Id;
            }
            else if (slotName == "Slot05")
            {
                slotObjIds[4] = slot.Entity.Id;
            }
            else if (slotName == "Slot04")
            {
                slotObjIds[3] = slot.Entity.Id;
            }
            else if (slotName == "Slot03")
            {
                slotObjIds[2] = slot.Entity.Id;
            }
            else if (slotName == "Slot02")
            {
                slotObjIds[1] = slot.Entity.Id;
            }
            else if (slotName == "Slot01")
            {
                slotObjIds[0] = slot.Entity.Id;
            }
            else { }

            slotInstance.ms_ImageRefList = Entity.FromId(World!, slot.Entity.Id).GetChildren();

            //foreach(Entity MS_imageRef in slotInstance.ms_ImageRefList)
            //{
            //    //Currently the engine applies GUI position transformations to children after scene load
            //    //So the current way caters to this
            //    //If that somehow breaks, go back to the old way on the right
            //    MS_imageRef.GetComponent<GUIElement>().Position.X = slotEntity.GetComponent<GUIElement>().Position.X;
            //    MS_imageRef.GetComponent<GUIElement>().Position.Y = slotEntity.GetComponent<GUIElement>().Position.Y;
            //}

        }

        //Set the selection
        ref GUIImage guiImage = ref Entity.FromId(World!, slotObjIds[iterator]).GetComponent<GUIImage>();
        guiImage.Color = new Color(100, 100, 100, 255);
        Log("Set to Black!");

        return true;
    }
    protected override void OnUpdate()
    {
        //This is gonna be overhauled
        //check for input
        isPressed_Q = Input.IsKeyPressed(KeyCode.Q);
        if(isEnabled_xInput) isPressed_X = Input.IsKeyPressed(KeyCode.X);

        foreach(var gameObject in World!.Query<InventoryControllerComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity.Id);
            //Todo


            //Iterator
            if (isPressed_Q)
            {
                Iterator(ref iterator);
            }

            if (isPressed_X && slotInstances[Entity.FromId(World!, slotObjIds[iterator]).GetComponent<Name>().Value.ToString()].isStoringItem)
            {
                if(iterator == 6) RemoveFromInventory(2);
                else if (iterator == 5) RemoveFromInventory(1);
                else RemoveFromInventory(slotInstances[Entity.FromId(World!,slotObjIds[iterator]).GetComponent<Name>().Value.ToString()].storedMsId);
            }

            //else if (Input.IsKeyPressed(KeyCode.Keypad1)) { iterator = 0; InstantIterate(); }
            //else if (Input.IsKeyPressed(KeyCode.Keypad2)) { iterator = 1; InstantIterate(); }
            //else if (Input.IsKeyPressed(KeyCode.Keypad3)) { iterator = 2; InstantIterate(); }
            //else if (Input.IsKeyPressed(KeyCode.Keypad4)) { iterator = 3; InstantIterate(); }
            //else if (Input.IsKeyPressed(KeyCode.Keypad5)) { iterator = 4; InstantIterate(); }
            //else if (Input.IsKeyPressed(KeyCode.Keypad6)) { iterator = 5; InstantIterate(); }
            //else if (Input.IsKeyPressed(KeyCode.Keypad7)) { iterator = 6; InstantIterate(); }


            //For Checking
            if (Input.IsKeyPressed(KeyCode.J))
            {
                Log("J is Pressed!==========");
                //Entity e = Entity.FromId(World!, u);
                foreach (KeyValuePair<string, Slot> stringSlot in slotInstances)
                {
                    Log($"{stringSlot.Key} contains {stringSlot.Value.ms_ImageRefList.Count} refs, isStoringItem = {stringSlot.Value.isStoringItem}");
                }

                Log("%%%%%%%%%%%%%%%%%%%%%%%%%%");
                foreach (ulong id in slotObjIds)
                {
                    Log($"slotObjIds contains a {Entity.FromId(World!, id).GetComponent<Name>().Value.ToString()}");
                }
                Log("End======================");
            }
        }
        


    }
    private void Iterator(ref int iterator)
    {
        iterator--;

        if (iterator < 0)
        {
            iterator = slotObjIds.Length - 1;
        }

        for (int i = slotObjIds.Length - 1; i >= 0; i--)
        {
            ref GUIImage guiImage = ref Entity.FromId(World!, slotObjIds[i]).GetComponent<GUIImage>();
            //if selected, gray out / higlight the slot
            if (i == iterator)
            {
                guiImage.Color = new Color(100, 100, 100, 255);
            }
            //else restore the color back to white
            else
            {
                guiImage.Color = new Color(255, 255, 255, 255);
            }
        }

        UpdateSelection();
        //Log($"Current Selected: {iterator}");
        bool isSlotEmpty = !slotInstances[Entity.FromId(World!, slotObjIds[iterator]).GetComponent<Name>().Value.ToString()].isStoringItem;
        
        //if (isSlotEmpty)
        //{
        //    Log($"There's nothing in Slot0{iterator+1}");
        //}
        //else
        //{
        //    Log($"There's msid: {slotInstances[Entity.FromId(World!, slotObjIds[iterator]).GetComponent<Name>().Value.ToString()].storedMsId} in Slot0{iterator + 1}");
        //}
    }
   
    private void UpdateSelection()
    {
        int msIdInSlot = slotInstances[Entity.FromId(World!, slotObjIds[iterator]).GetComponent<Name>().Value.ToString()].storedMsId;
        foreach (var gameObject in World!.Query<MatchSignifierComponent>())
        {
            if(gameObject.Component1.signifierID == 787878)
            {
                //Do the following when it finds the DisplaySelector Obj
                Entity displaySelectorEntity = Entity.FromId(World!, gameObject.Entity.Id);
                //Initialize and declare an ordered list of displaySelector's children
                ulong[] displaySelector_msImageArray_ordered = new ulong[7];
                foreach (Entity ms_Image in displaySelectorEntity.GetChildren())
                {
                    switch (ms_Image.GetComponent<Name>().Value.ToString())
                    {
                        case "MS01_ui":
                            displaySelector_msImageArray_ordered[0] = ms_Image.Id;
                            break;
                        case "MS02_ui":
                            displaySelector_msImageArray_ordered[1] = ms_Image.Id;
                            break;
                        case "MS03_ui":
                            displaySelector_msImageArray_ordered[2] = ms_Image.Id;
                            break;
                        case "MS04_ui":
                            displaySelector_msImageArray_ordered[3] = ms_Image.Id;
                            break;
                        case "MS05_ui":
                            displaySelector_msImageArray_ordered[4] = ms_Image.Id;
                            break;
                        case "MS06_ui":
                            displaySelector_msImageArray_ordered[5] = ms_Image.Id;
                            break;
                        case "MS07_ui":
                            displaySelector_msImageArray_ordered[6] = ms_Image.Id;
                            break;
                    }
                }
                
                //Iterate thru the ordered list, and switch GUIImages on and off as required
                for (int i = 0; i < displaySelector_msImageArray_ordered.Length; i++)
                {
                    ref GUIElement guiElement = ref Entity.FromId(World!, displaySelector_msImageArray_ordered[i]).GetComponent<GUIElement>();
                    if (msIdInSlot-1 == i)
                    {
                        //Turn on the image if the slot contains the item!
                        guiElement.Visible = true;
                    }
                    else
                    {
                        //turn off all other slots
                        guiElement.Visible = false;
                    }
                }
               
            }
        }
    }
    public void AddToInventory(int msID, ulong otherId)
    {
        switch (msID)
        {
            case 1:
            foreach(var gameObject in World!.Query<MatchSignifierComponent>())
            {
                foreach(var gameObject2 in World!.Query<InventoryControllerComponent>())
                {
                    if(gameObject.Component1.signifierID == gameObject2.Component1.ms01_signifier && ms01_Count != 10) 
                    {
                            ms01_Count = GMath.Clamp(++ms01_Count, 0, 10);
                            MS_Manager.instance.SendToPool(otherId);
                            slotInstances["Slot06"].isStoringItem = true;
                            slotInstances["Slot06"].storedMsId = 1;
                            gameObject.Entity.GetComponent<GUIText>().TextId = Strings.Intern($"{ms01_Count}");
                            UpdateSelection();
                        }
                }
            }
            break;
            case 2:
            foreach (var gameObject in World!.Query<MatchSignifierComponent>())
            {
                foreach (var gameObject2 in World!.Query<InventoryControllerComponent>())
                {
                    if (gameObject.Component1.signifierID == gameObject2.Component1.ms02_signifier && ms02_Count != 10)
                    {
                            ms02_Count = GMath.Clamp(++ms02_Count, 0, 10);
                            MS_Manager.instance.SendToPool(otherId);
                            slotInstances["Slot07"].isStoringItem = true;
                            slotInstances["Slot07"].storedMsId = 2;
                            gameObject.Entity.GetComponent<GUIText>().TextId = Strings.Intern($"{ms02_Count}");
                            UpdateSelection();
                        }
                }
            }
            break;
            case 3:
                //Find the slotInstance that contains the ms_imageRefList
                AddItemToNonStackableSlot("MS03_ui", otherId, 3);
                UpdateSelection();
                break;
            case 4:
                AddItemToNonStackableSlot("MS04_ui", otherId, 4);
                UpdateSelection();
                break;
            case 5:
                AddItemToNonStackableSlot("MS05_ui", otherId, 5);
                UpdateSelection();
                break;
            case 6:
                AddItemToNonStackableSlot("MS06_ui", otherId, 6);
                UpdateSelection();
                break;
            case 7:
                AddItemToNonStackableSlot("MS07_ui", otherId, 7);
                UpdateSelection();
                break;
        }
        

    }

    public void AddItemToNonStackableSlot(string msIdCheck, ulong otherId, int msId)
    {
        FindAvailableSlot(out Entity slotEntity);
        foreach (Entity image in slotInstances[Entity.FromId(World!, slotEntity.Id).GetComponent<Name>().Value.ToString()].ms_ImageRefList)
        {
            if (Entity.FromId(World!, image.Id).GetComponent<Name>().Value.ToString() == msIdCheck)
            {
                if (!FindAvailableSlot(out Entity e)) return;
                MS_Manager.instance.SendToPool(otherId);
                image.GetComponent<GUIElement>().Visible = true;
                slotInstances[Entity.FromId(World!, slotEntity.Id).GetComponent<Name>().Value.ToString()].isStoringItem = true;
                slotInstances[Entity.FromId(World!, slotEntity.Id).GetComponent<Name>().Value.ToString()].storedMsId = msId;
            }
        }
    }

    public void RemoveFromInventory(int msID)
    {
        //if (msID == 0) return;
        
        switch (msID)
        {
            case 1:
                foreach (var gameObject in World!.Query<MatchSignifierComponent>())
                {
                    foreach (var gameObject2 in World!.Query<InventoryControllerComponent>())
                    {
                        if (gameObject.Component1.signifierID == gameObject2.Component1.ms01_signifier && ms01_Count > 0)
                        {
                            ms01_Count = GMath.Clamp(--ms01_Count, 0, 10);
                            //MS_Manager.instance.SendToPool(otherId);
                            if(ms01_Count <= 0)
                            {
                                slotInstances["Slot06"].isStoringItem = false;
                                slotInstances["Slot06"].storedMsId = 0;
                            }
                            
                            gameObject.Entity.GetComponent<GUIText>().TextId = Strings.Intern($"{ms01_Count}");
                            UpdateSelection();
                        }
                    }
                }
                break;
            case 2:
                foreach (var gameObject in World!.Query<MatchSignifierComponent>())
                {
                    foreach (var gameObject2 in World!.Query<InventoryControllerComponent>())
                    {
                        if (gameObject.Component1.signifierID == gameObject2.Component1.ms02_signifier && ms02_Count > 0)
                        {
                            ms02_Count = GMath.Clamp(--ms02_Count, 0, 10);
                            //MS_Manager.instance.SendToPool(otherId);
                            if (ms02_Count <= 0)
                            {
                                slotInstances["Slot07"].isStoringItem = false;
                                slotInstances["Slot07"].storedMsId = 0;
                            }
                            gameObject.Entity.GetComponent<GUIText>().TextId = Strings.Intern($"{ms02_Count}");
                            UpdateSelection();
                        }
                    }
                }
                break;
            default:
                Entity slotEntity = Entity.FromId(World!, slotObjIds[iterator]);
                string slotName = slotEntity.GetComponent<Name>().Value.ToString();
                foreach (Entity image in slotInstances[slotName].ms_ImageRefList)
                {
                    image.GetComponent<GUIElement>().Visible = false;
                    slotInstances[slotName].isStoringItem = false;
                    slotInstances[slotName].storedMsId = 0;
                }
                UpdateSelection();
                break;
        }
        MS_Manager.instance.TakeFromPool(msID, new Vector3(Player.instance.currentPos.X, Player.instance.currentPos.Y - 0.6f, 0), 15f);
        
    }
    public bool FindAvailableSlot(out Entity slotEntity)
    {
        //Iterate thru the nonstackable era in sequence
        for (int i=4; i>=0; i--)
        {
            //Entity slotEntity = Entity.FromId(World!,nonStackableSlots[i]);
            if (!slotInstances[Entity.FromId(World!, slotObjIds[i]).GetComponent<Name>().Value.ToString()].isStoringItem)
            {
                slotEntity = Entity.FromId(World!, slotObjIds[i]);
                return true;
            }
        }
        //Default case
        slotEntity = Entity.FromId(World!, slotObjIds[0]);
        return false;
    }
}

[Component] public record struct SlotComponent(bool start);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class Slot
{
    public List<Entity> ms_ImageRefList { get; set; }
    public bool isStoringItem { get; set; }
    public int storedMsId { get; set; }
    public Slot(bool isStoringItem, int storedMsId)
    {
        ms_ImageRefList = new List<Entity>();
        this.isStoringItem = isStoringItem;
        this.storedMsId = storedMsId;
    }
}

public enum MS
{
    MS01, MS02, MS03, MS04, MS05, MS06, MS07
}

