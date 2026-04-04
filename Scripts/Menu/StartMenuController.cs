using EchoesBelow.Scripts.Audio;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using Scripts;
using Scripts.Menu;
using System;
using System.Collections.Generic;

namespace EchoesBelow.Scripts;

[Component] public record struct StartMenuControllerComponent(bool start, bool awake);
[RequireForUpdate<StartMenuControllerComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class StartMenuController : SystemBase
{
    public static StartMenuController instance { get; private set; }

    public static List<Entity> panelEntities { get; private set; }
    public static Dictionary<string, MenuPanel> buttons { get; private set; }
    public static List<MenuPanel> buttonList { get; private set; }

    //Start Menu Fields
    static int iterator = 0;
    private const string TargetScenePath = "Scenes/FeatureGym.scn";
    static bool isNewGamePressedOnce = false;

    //hailmary fix
    public float timer = 0;


    //Current Button Fields
    public static MenuPanel currentButton { get; private set; }
    //Available button names
    const string newGameButton = "NewGame_Button";
    const string exitButton = "Exit_Button";
    const string settingsButton = "Settings_Button";
    const string continueButton = "Continue_Button";

    private bool OnAwake(ref bool awakeBool, ulong objId) //Onawake must only play once at the beginning per script.
    {
        if (awakeBool == true) return true;
        awakeBool = true;

        timer = 0f;

        isNewGamePressedOnce = false;

        instance = this;

        buttons = new Dictionary<string, MenuPanel>();

        buttonList = new List<MenuPanel>();

        panelEntities = new List<Entity>();

        foreach (Entity child in Entity.FromId(World!, objId).GetChildren())
        {
            panelEntities.Add(child);
            ref GUIElement gui = ref child.GetComponent<GUIElement>();
        }

        return true;
    }
    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        Log("PanelEntities count: " + panelEntities.Count);
        foreach (Entity panelEntity in panelEntities)
        {
            if (panelEntity.HasComponent<GUIInput>())
            {
                //Initialize my menu panels
                string name = panelEntity.GetComponent<Name>().Value.ToString();
                MenuPanel menuP = new MenuPanel(panelEntity.Id, name, panelEntity);

                switch (name)
                {
                    case newGameButton:
                        menuP.Action = NewGameButtonFunc;
                        break;
                    case continueButton:
                        menuP.Action = ContinueButtonFunc;
                        break;
                    case exitButton:
                        menuP.Action = ExitButtonFunc;
                        break;
                    case settingsButton:
                        menuP.Action = SettingsButtonFunc;
                        break;
                }
                buttons.Add(name, menuP);
                buttonList.Add(menuP);
            }
        }

        //Assign updownleftright Entities
        foreach (MenuPanel menuP in buttons.Values)
        {
            string name = menuP.Entity.GetComponent<Name>().Value.ToString();
            switch (name)
            {
                case newGameButton:
                    menuP.up = buttons[newGameButton].Entity;
                    menuP.down = buttons[newGameButton].Entity;
                    menuP.left = buttons[exitButton].Entity;
                    menuP.right = buttons[continueButton].Entity;
                    break;
                case continueButton:
                    menuP.up = buttons[continueButton].Entity;
                    menuP.down = buttons[continueButton].Entity;
                    menuP.left = buttons[newGameButton].Entity;
                    menuP.right = buttons[exitButton].Entity;
                    break;
                case exitButton:
                    menuP.up = buttons[settingsButton].Entity;
                    menuP.down = buttons[settingsButton].Entity;
                    menuP.left = buttons[continueButton].Entity;
                    menuP.right = buttons[newGameButton].Entity;
                    break;
                case settingsButton:
                    menuP.up = buttons[exitButton].Entity;
                    menuP.down = buttons[exitButton].Entity;
                    menuP.left = buttons[continueButton].Entity;
                    menuP.right = buttons[newGameButton].Entity;
                    break;
            }
        }

        currentButton = buttons[newGameButton];

        if (Input.IsGamepadConnected(0))
        {
            currentButton = buttons[newGameButton];

            ref GUIStateStyle guistateStyle = ref currentButton.Entity.GetComponent<GUIStateStyle>();
            guistateStyle.NormalColor = new Color(3f, 3f, 3f, 1f);
        }
        //Update for gamepad connecting
        if (Input.IsGamepadConnected(0))
        {
            foreach (MenuPanel menuP in buttons.Values)
            {
                ref GUIStateStyle stateStyle = ref menuP.Entity.GetComponent<GUIStateStyle>();
                stateStyle.HoverColor = new Color(1f, 1f, 1f, 1f);
                stateStyle.PressedColor = new Color(1f, 1f, 1f, 1f);
            }
        }

        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        foreach (var gameObject in World!.Query<StartMenuControllerComponent>())
        {
            bool awake = gameObject.Component1.awake;
            gameObject.Component1.awake = OnAwake(ref awake, gameObject.Entity.Id);
        }

        foreach (var gameObject in World!.Query<StartMenuControllerComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start);

            if (PauseMenuController.instance.isPaused) return;
  
            //Do the rest
            ref GUIInput currentButton_guiInput = ref currentButton.Entity.GetComponent<GUIInput>();

            if (Input.IsGamepadConnected(0))
            {
                UpdateCurrentButtonForGamePad();
            }


            //Update for gamepad connecting
            UpdateButtonDefaults();


            if(timer > 0f)
            {
                timer -= Time.DeltaTime;
                if (timer <= 0f) timer = 0f;
            }

            if (timer > 0f) return;

            if (Input.IsGamepadButtonPressed(0,GamepadButton.A) && !currentButton.Entity.HasComponent<GUISlider>())
            {
                AudioManager.instance.PlaySFX("UI005_Track01");
                currentButton.Action();

            }
            else if (Input.IsGamepadButtonPressed(0, GamepadButton.A) && timer <= 0f)
            {
                AudioManager.instance.PlaySFX("UI005_Track01");
                currentButton.Action();
            }

            //=========================================================================
            if (Input.IsGamepadConnected(0)) return;
           
            //Locate the button
            if (!currentButton_guiInput.Dragging && !Input.IsGamepadConnected(0))
                UpdateCurrentButtonForMouse();

            if (currentButton_guiInput.Hovered)
            {
                //Log("Hovering over " + currentButton.name);
            }
            if (currentButton_guiInput.Clicked && !currentButton.Entity.HasComponent<GUISlider>())
            {
                AudioManager.instance.PlaySFX("UI005_Track01");
                currentButton.Action();

            }
            if (currentButton_guiInput.Entered)
            {
                AudioManager.instance.PlaySFX("UI005_Track01");
            }
            if (currentButton_guiInput.Dragging && currentButton.Entity.HasComponent<GUISlider>())
            {
                currentButton.Action();
            }

        }
    }
    private static void UpdateButtonDefaults()
    {
        if (Input.IsGamepadJustConnected(0))
        {
            foreach (MenuPanel menuP in buttons.Values)
            {
                ref GUIStateStyle stateStyle = ref menuP.Entity.GetComponent<GUIStateStyle>();
                stateStyle.HoverColor = new Color(1f, 1f, 1f, 1f);
                stateStyle.PressedColor = new Color(1f, 1f, 1f, 1f);
            }

            ref GUIStateStyle stateStyle2 = ref currentButton.Entity.GetComponent<GUIStateStyle>();
            stateStyle2.NormalColor = new Color(3f, 3f, 3f, 1f);
        }

        if (Input.IsGamepadJustDisconnected(0))
        {
            foreach (MenuPanel menuP in buttons.Values)
            {
                ref GUIStateStyle stateStyle = ref menuP.Entity.GetComponent<GUIStateStyle>();
                stateStyle.HoverColor = new Color(3f, 3f, 3f, 1f);
                stateStyle.PressedColor = new Color(5f, 4f, 5f, 1f);
                stateStyle.NormalColor = new Color(1f,1f,1f,1f);
            }
        }
    }
    private static void UpdateCurrentButtonForGamePad()
    {
        if (Input.IsGamepadButtonPressed(0, GamepadButton.DPadUp))
        {
            currentButton = buttons[currentButton.up.GetComponent<Name>().Value.ToString()];
            UpdateGamePadSelection();
        }
        if (Input.IsGamepadButtonPressed(0, GamepadButton.DPadDown))
        {
            currentButton = buttons[currentButton.down.GetComponent<Name>().Value.ToString()];
            UpdateGamePadSelection();
        }
        if (Input.IsGamepadButtonPressed(0, GamepadButton.DPadLeft))
        {
            currentButton = buttons[currentButton.left.GetComponent<Name>().Value.ToString()];
            UpdateGamePadSelection();
        }
        if (Input.IsGamepadButtonPressed(0, GamepadButton.DPadRight))
        {
            currentButton = buttons[currentButton.right.GetComponent<Name>().Value.ToString()];
            UpdateGamePadSelection();
        }
    }
    private static void UpdateGamePadSelection()
    {
        AudioManager.instance.PlaySFX("UI005_Track01");
        foreach (MenuPanel menuP in buttons.Values)
        {
            ref GUIStateStyle stateStyle = ref menuP.Entity.GetComponent<GUIStateStyle>();

            if (menuP.Entity.TryGetComponent<GUISlider>(out _))
            {
                ref GUISlider slider = ref menuP.Entity.GetComponent<GUISlider>();
                if (menuP.name == currentButton.name)
                {
                    slider.KnobColor = new Color(3f, 3f, 3f, 1f);
                }
                else
                {
                    slider.KnobColor = new Color(1f, 1f, 1f, 1f);
                }
            }
            else
            {
                if (menuP.name == currentButton.name)
                {
                    stateStyle.NormalColor = new Color(3f, 3f, 3f, 1f);
                }
                else
                {
                    stateStyle.NormalColor = new Color(1f, 1f, 1f, 1f);
                }
            }

        }
    }
    private void UpdateCurrentButtonForMouse()
    {
        foreach (MenuPanel button in buttons.Values)
        {
            ref GUIInput gui = ref button.Entity.GetComponent<GUIInput>();
            {
                if (gui.Hovered) currentButton = button;
            }
        }
    }
    private void NewGameButtonFunc()
    {
        if (!isNewGamePressedOnce)
        {
            SceneCrossFadeTransition.Request(TargetScenePath, 1.5f, true);
            isNewGamePressedOnce = true;
        }

    } 
    private void ContinueButtonFunc()
    {
        Log("Continue!");
    }
    private void SettingsButtonFunc()
    {
        //turn off a bunch of buttons and turn on sliders
        Log("Settings!");
        timer = 0.5f;
        //if (PauseMenuController.instance.isPaused) return;
        PauseMenuController.instance.Pause(true);
    }
    private void ExitButtonFunc()
    {
        Application.Quit();
    }
}