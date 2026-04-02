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

[Component] public record struct PauseMenuControllerComponent(bool start, bool awake);
[RequireForUpdate<PauseMenuControllerComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class PauseMenuController : SystemBase
{
    public static PauseMenuController instance {  get; private set; }

    public static List<Entity> panelEntities { get; private set; }
    public static Dictionary<string, MenuPanel> buttons { get; private set; }

    //Pause Menu Fields
    public bool isPaused {  get; set; }
    public bool isPausable { get; set; }
    public bool isPauseButtonPressed { get; set; }


    //Current Button Fields
    public static MenuPanel currentButton {  get; private set; }
    //Available button names
    const string resumeButton = "Resume_Button";
    const string exitButton = "Exit_Button";
    const string sfxButton = "SFX_Button";
    const string bgmButton = "BGM_Button";

    //Slider Fields
    //Vector2 bgmSliderMin = new Vector2(618f, 207f);
    //Vector2 bgmSliderMax = new Vector2(916f, 151f);
    //Vector2 sfxSliderMin = new Vector2(594, 366f);
    //Vector2 sfxSliderMax = new Vector2(896f, 415f);



    private const string TargetScenePath = "Scenes/Newstartscene.scn";

    private bool OnAwake(ref bool awakeBool, ulong objId) //Onawake must only play once at the beginning per script.
    {
        if (awakeBool == true) return true;
        awakeBool = true;

        instance = this;

        isPausable = true;

        buttons = new Dictionary<string, MenuPanel>();

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
    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
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
            }
        }

        currentButton = buttons[resumeButton];

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
            gameObject.Component1.start = OnStart(ref start);

            //Do the rest

            //AudioManager.instance.UpdateBGMVolume(1);


            //For future code to interact with
            if (!isPausable) return;

            isPauseButtonPressed = Input.IsKeyPressed(KeyCode.P) || Input.IsMousePressed(2) || Input.IsGamepadButtonPressed(0,GamepadButton.Start);

            if (!isPaused && isPauseButtonPressed) Pause(true);
            else if (isPaused && isPauseButtonPressed) Pause(false);

            if (!isPaused) return;

            ref GUIInput currentButton_guiInput = ref currentButton.Entity.GetComponent<GUIInput>();

            //Locate the button
            if(!currentButton_guiInput.Dragging)
            UpdateCurrentButton();



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

    private static void UpdateCurrentButton()
    {
        foreach (MenuPanel button in buttons.Values)
        {
            ref GUIInput gui = ref button.Entity.GetComponent<GUIInput>();
            if (gui.Hovered) currentButton = button;
        }
    }

    private void Pause(bool isPausing)
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

        SceneManager sceneManager = SceneManager.Instance;
        sceneManager.SetNextAudioTransition(0.8f, true);

        ulong sceneIndex = sceneManager.AddScene();
        bool loaded = sceneManager.LoadScene(sceneIndex, TargetScenePath);

        if (loaded)
        {
            sceneManager.SetActive(sceneIndex);
            return;
        }

        // Fallback to existing transition request path.
        SceneCrossFadeTransition.Request(TargetScenePath, 0.8f, true);
        //SceneCrossFadeTransition.Request(StartSceneName, 0.8f, true);
    }
    private void SFXButtonFunc()
    {
        GUISlider currentButton_slider = currentButton.Entity.GetComponent<GUISlider>();
        AudioManager.instance.UpdateSFXVolume(currentButton_slider.Value);
     
    }
    private void BGMButtonFunc()
    {
        GUISlider currentButton_slider = currentButton.Entity.GetComponent<GUISlider>();
        AudioManager.instance.UpdateBGMVolume(currentButton_slider.Value);

    }
}
