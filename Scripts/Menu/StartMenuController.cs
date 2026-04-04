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

        //Update for gamepad connecting
        UpdateButtonDefaults();

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

            //Do the rest
            ref GUIInput currentButton_guiInput = ref currentButton.Entity.GetComponent<GUIInput>();

            //Locate the button
            if (!currentButton_guiInput.Dragging && !Input.IsGamepadConnected(0))
                UpdateCurrentButton();

            //float xInput = Input.GetGamepadAxis(0, GamepadAxis.LeftX);
            //float yInput = Input.GetGamepadAxis(0, GamepadAxis.LeftY);

            //if(GMath.Abs(xInput) > 0.9f || GMath.Abs(yInput) > 0.9f)
            if (Input.IsGamepadButtonPressed(0, GamepadButton.DPadDown) || Input.IsGamepadButtonPressed(0, GamepadButton.DPadUp)
            || Input.IsGamepadButtonPressed(0, GamepadButton.DPadLeft) || Input.IsGamepadButtonPressed(0, GamepadButton.DPadRight))
            {
                UpdateCurrentButton();
            }

            //if(Input.IsGamepadButtonPressed(0,GamepadAxis.LeftX))


            //Update for gamepad connecting
            UpdateButtonDefaults();

            if (Input.IsGamepadConnected(0)) return;

            if (currentButton_guiInput.Hovered)
            {
                //Log("Hovering over " + currentButton.name);
            }
            if ((currentButton_guiInput.Clicked || Input.IsGamepadButtonPressed(0, GamepadButton.A)) && !currentButton.Entity.HasComponent<GUISlider>())
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
        if (Input.IsGamepadConnected(0))
        {
            foreach (MenuPanel menuP in buttons.Values)
            {
                ref GUIStateStyle stateStyle = ref menuP.Entity.GetComponent<GUIStateStyle>();
                stateStyle.HoverColor = new Color(1f, 1f, 1f, 1f);
                stateStyle.PressedColor = new Color(1f, 1f, 1f, 1f);
            }
        }

        if (Input.IsGamepadJustConnected(0))
        {
            foreach (MenuPanel menuP in buttons.Values)
            {
                ref GUIStateStyle stateStyle = ref menuP.Entity.GetComponent<GUIStateStyle>();
                stateStyle.HoverColor = new Color(1f, 1f, 1f, 1f);
                stateStyle.PressedColor = new Color(1f, 1f, 1f, 1f);
            }
        }

        if (Input.IsGamepadJustDisconnected(0))
        {
            foreach (MenuPanel menuP in buttons.Values)
            {
                ref GUIStateStyle stateStyle = ref menuP.Entity.GetComponent<GUIStateStyle>();
                stateStyle.HoverColor = new Color(3f, 3f, 3f, 1f);
                stateStyle.PressedColor = new Color(5f, 4f, 5f, 1f);
            }
        }
    }
    private static void UpdateCurrentButton()
    {
        if (Input.IsGamepadConnected(0))
        {
            ++iterator;
            if (iterator > buttonList.Count - 1) iterator = 0;

            currentButton = buttonList[iterator];
        }

        foreach (MenuPanel button in buttons.Values)
        {
            ref GUIInput gui = ref button.Entity.GetComponent<GUIInput>();

            ref GUIStateStyle guistateStyle = ref button.Entity.GetComponent<GUIStateStyle>();

            if (Input.IsGamepadConnected(0) && currentButton == button)
            {
                guistateStyle.NormalColor = new Color(3f, 3f, 3f, 1f);
            }
            else if (Input.IsGamepadConnected(0) && currentButton != button)
            {
                guistateStyle.NormalColor = new Color(1f, 1f, 1f, 1f);
            }
            else
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
    }
    private void ExitButtonFunc()
    {
        Application.Quit();
    }
}