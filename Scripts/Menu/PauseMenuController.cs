using EchoesBelow.Scripts;
using EchoesBelow.Scripts.Audio;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using Scripts.CraftingSystem;
using Scripts.Menu;
using System.Collections.Generic;

namespace Scripts.Menu;

[Component] public record struct PauseMenuControllerComponent(bool start, bool awake, bool isInStartScene);
[RequireForUpdate<PauseMenuControllerComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class PauseMenuController : SystemBase
{
    public static PauseMenuController instance {  get; private set; }

    public static List<Entity> panelEntities { get; private set; }
    public static Dictionary<string, MenuPanel> buttons { get; private set; }
    public static List<MenuPanel> buttonList { get; private set; }

    //Pause Menu Fields
    public bool isPaused {  get; set; }
    public bool isPausable { get; set; }
    public bool isPauseButtonPressed { get; set; }

    static int iterator = 0;

    //Current Button Fields
    public static MenuPanel currentButton {  get; private set; }
    public static bool isUsingSlider = false;

    //Available button names
    const string resumeButton = "Resume_Button";
    const string exitButton = "Exit_Button";
    const string sfxButton = "SFX_Button";
    const string bgmButton = "BGM_Button";


    private const string TargetScenePath = "Scenes/StartScene.scn";

    private bool OnAwake(ref bool awakeBool, ulong objId) //Onawake must only play once at the beginning per script.
    {
        if (awakeBool == true) return true;
        awakeBool = true;

        instance = this;

        isPausable = true;

        buttons = new Dictionary<string, MenuPanel>();

        buttonList = new List<MenuPanel>();

        panelEntities = new List<Entity>();

        foreach(Entity child in Entity.FromId(World!, objId).GetChildren())
        {
            foreach(Entity grandChild in child.GetChildren())
            {
                panelEntities.Add(grandChild);
                ref GUIElement gui = ref grandChild.GetComponent<GUIElement>();
                gui.Visible = false;
            }
        }

        return true;
    }
    private bool OnStart(ref bool startBool, bool isInStartScene)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        isUsingSlider = false;

        Log("PanelEntities count: " + panelEntities.Count);
        foreach(Entity panelEntity in panelEntities)
        {
            if (panelEntity.HasComponent<GUIInput>())
            {
                //Initialize my menu panels
                string name = panelEntity.GetComponent<Name>().Value.ToString();
                MenuPanel menuP = new MenuPanel(panelEntity.Id, name, panelEntity);

                switch (name)
                {
                    case resumeButton:
                        menuP.Action = ResumeButtonFunc;
                        
                        break;
                    case exitButton:
                        menuP.Action = ExitButtonFunc;
                        break;
                    case bgmButton:
                        menuP.Action = BGMButtonFunc;
                        break;
                    case sfxButton:
                        menuP.Action = SFXButtonFunc;
                        break;
                }
                buttons.Add(name, menuP);
                buttonList.Add(menuP);
            }
        }


        if (!isInStartScene)
        {
            //Assign updownleftright Entities
            foreach (MenuPanel menuP in buttons.Values)
            {
                string name = menuP.Entity.GetComponent<Name>().Value.ToString();
                switch (name)
                {
                    case resumeButton:
                        menuP.up = buttons[sfxButton].Entity;
                        menuP.down = buttons[bgmButton].Entity;
                        menuP.left = buttons[exitButton].Entity;
                        menuP.right = buttons[exitButton].Entity;
                        break;
                    case exitButton:
                        menuP.up = buttons[sfxButton].Entity;
                        menuP.down = buttons[exitButton].Entity;
                        menuP.left = buttons[resumeButton].Entity;
                        menuP.right = buttons[resumeButton].Entity;
                        break;
                    case bgmButton:
                        menuP.up = buttons[resumeButton].Entity;
                        menuP.down = buttons[sfxButton].Entity;
                        menuP.left = buttons[bgmButton].Entity;
                        menuP.right = buttons[exitButton].Entity;
                        break;
                    case sfxButton:
                        menuP.up = buttons[bgmButton].Entity;
                        menuP.down = buttons[resumeButton].Entity;
                        menuP.left = buttons[exitButton].Entity;
                        menuP.right = buttons[exitButton].Entity;
                        break;
                }
            }
            currentButton = buttons[resumeButton];

            if (Input.IsGamepadConnected(0))
            {
                currentButton = buttons[resumeButton];

                ref GUIStateStyle guistateStyle = ref currentButton.Entity.GetComponent<GUIStateStyle>();
                guistateStyle.NormalColor = new Color(3f, 3f, 3f, 1f);
            }
        }
        else
        {
            //Assign updownleftright Entities
            foreach (MenuPanel menuP in buttons.Values)
            {
                string name = menuP.Entity.GetComponent<Name>().Value.ToString();
                switch (name)
                {
                    case exitButton:
                        menuP.up = buttons[exitButton].Entity;
                        menuP.down = buttons[exitButton].Entity;
                        menuP.left = buttons[bgmButton].Entity;
                        menuP.right = buttons[bgmButton].Entity;
                        break;
                    case bgmButton:
                        menuP.up = buttons[sfxButton].Entity;
                        menuP.down = buttons[sfxButton].Entity;
                        menuP.left = buttons[exitButton].Entity;
                        menuP.right = buttons[exitButton].Entity;
                        break;
                    case sfxButton:
                        menuP.up = buttons[bgmButton].Entity;
                        menuP.down = buttons[bgmButton].Entity;
                        menuP.left = buttons[exitButton].Entity;
                        menuP.right = buttons[exitButton].Entity;
                        break;
                }
            }
            currentButton = buttons[exitButton];

            if (Input.IsGamepadConnected(0))
            {
                currentButton = buttons[exitButton];

                ref GUIStateStyle guistateStyle = ref currentButton.Entity.GetComponent<GUIStateStyle>();
                guistateStyle.NormalColor = new Color(3f, 3f, 3f, 1f);
            }
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

        foreach (var gameObject in World!.Query<PauseMenuControllerComponent>())
        {
            bool awake = gameObject.Component1.awake;
            gameObject.Component1.awake = OnAwake(ref awake, gameObject.Entity.Id);
        }

        foreach (var gameObject in World!.Query<PauseMenuControllerComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Component1.isInStartScene);

            //Do the rest

            //AudioManager.instance.UpdateBGMVolume(1);


            //For future code to interact with
            if (!isPausable) return;

            isPauseButtonPressed = (Input.IsKeyPressed(KeyCode.P) || Input.IsMousePressed(2) || Input.IsGamepadButtonPressed(0, GamepadButton.Start))
                                    && !gameObject.Component1.isInStartScene;

            if (!isPaused && isPauseButtonPressed) Pause(true);
            else if (isPaused && isPauseButtonPressed) Pause(false);

            if (!isPaused) return;

            ref GUIInput currentButton_guiInput = ref currentButton.Entity.GetComponent<GUIInput>();

            if (Input.IsGamepadConnected(0) && !isUsingSlider)
            {
                UpdateCurrentButtonForGamePad();
            }

            //Update for gamepad connecting
            UpdateButtonDefaults();

            if (Input.IsGamepadButtonPressed(0, GamepadButton.A) && !currentButton.Entity.HasComponent<GUISlider>())
            {
                isUsingSlider = false;
                AudioManager.instance.PlaySFX("UI005_Track01");
                currentButton.Action();
                Log("ACtion!");

            }
            else if (Input.IsGamepadButtonDown(0, GamepadButton.A) && currentButton.Entity.HasComponent<GUISlider>())
            {
                isUsingSlider = true;

                currentButton.Action();
            }
            else
            {
                isUsingSlider = false;
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
                stateStyle.NormalColor = new Color(1f, 1f, 1f, 1f);
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
        //Log("Current button" + currentButton.name);
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
                if(menuP.name == currentButton.name)
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
                if(menuP.name == currentButton.name)
                {
                    stateStyle.NormalColor = new Color(3f, 3f, 3f, 1f);
                    stateStyle.HoverColor = new Color(3f, 3f, 3f, 1f);
                    stateStyle.PressedColor = new Color(3f, 3f, 3f, 1f);
                }
                else
                {
                    stateStyle.NormalColor = new Color(1f,1f,1f,1f);
                    stateStyle.HoverColor = new Color(1f, 1f, 1f, 1f);
                    stateStyle.PressedColor = new Color(1f, 1f, 1f, 1f);
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

    public void Pause(bool isPausing)
    {
        if (isPausing)
        {
            //Pause the Game
            Player.instance.isEnabled = false;
            AudioManager.instance.PlaySFX("UI002");

            Time.TimeScale = 0;
            isPaused = true;
            //Launch Pause Menu
            foreach (Entity e in panelEntities)
            {
                ref GUIElement gui = ref e.GetComponent<GUIElement>();
                gui.Visible = true;
            }
        }
        else
        {
            //UnPause the Game
            Player.instance.isEnabled = true;
            AudioManager.instance.PlaySFX("UI001");

            Time.TimeScale = 1;
            isPaused = false;
            //Launch Pause Menu
            foreach (Entity e in panelEntities)
            {
                ref GUIElement gui = ref e.GetComponent<GUIElement>();
                gui.Visible = false;
            }
        }
    }
    //Menupanel Functions
    private void ResumeButtonFunc()
    {
        Pause(false);
    }
    private void ExitButtonFunc()
    {
        Pause(false);

        foreach(var pauseMenu in World!.Query<PauseMenuControllerComponent>())
        {
            if (!pauseMenu.Component1.isInStartScene)
            {
                SceneManager sceneManager = SceneManager.Instance;
                sceneManager.SetNextAudioTransition(0.8f, true);

                // Fallback to existing transition request path.
                SceneCrossFadeTransition.Request(TargetScenePath, 1.5f, true);
                //SceneCrossFadeTransition.Request(StartSceneName, 0.8f, true);
            }
            else
            {
                Log("UNPAUSED");
                StartMenuController.instance.timer = 0.5f;
            }
        }

    }
    private void SFXButtonFunc()
    {
        ref GUISlider currentButton_slider = ref currentButton.Entity.GetComponent<GUISlider>();
 
        if (Input.IsGamepadButtonPressed(0, GamepadButton.DPadLeft)) 
        { 
            AudioManager.instance.PlaySFX("UI005_Track01");
            currentButton_slider.Value -= 0.1f; 
        }
        if (Input.IsGamepadButtonPressed(0, GamepadButton.DPadRight))
        {
            AudioManager.instance.PlaySFX("UI005_Track01");
            currentButton_slider.Value += 0.1f;
        }
        AudioManager.instance.UpdateSFXVolume(currentButton_slider.Value);
     
    }
    private void BGMButtonFunc()
    {
        ref GUISlider currentButton_slider = ref currentButton.Entity.GetComponent<GUISlider>();

        if (Input.IsGamepadButtonPressed(0, GamepadButton.DPadLeft))
        {
            AudioManager.instance.PlaySFX("UI005_Track01");
            currentButton_slider.Value -= 0.1f;
        }
        if (Input.IsGamepadButtonPressed(0, GamepadButton.DPadRight))
        {
            AudioManager.instance.PlaySFX("UI005_Track01");
            currentButton_slider.Value += 0.1f;
        }

        AudioManager.instance.UpdateBGMVolume(currentButton_slider.Value);

    }
}
