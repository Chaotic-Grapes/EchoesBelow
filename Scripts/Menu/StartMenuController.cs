using EchoesBelow.Scripts.Audio;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System;
using System.Collections.Generic;

namespace EchoesBelow.Scripts;

[Component] public record struct StartMenuControllerComponent(bool start, int startSignifier, int exitSignifier, bool isEndScene, float timer);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class StartMenuController : SystemBase
{
    //For startscene
    private const string SourceSceneName = "Newstartscene";
    private const string TargetScenePath = "Scenes/FGym2.scn";
    //for endscene
    private const string EndSceneName = "EndScene";
    private const string StartSceneName = "Scenes/NewStartScene.scn";

    // Requested navigation order: NewGame -> Continue -> Settings -> Exit.
    private readonly string[] normalButtonNames =
    {
        "NewGame_Button",
        "Continue_Button",
        "Settings_Button",
        "Exit_Button"
    };

    private readonly string[] lighterButtonNames =
    {
        "NewGame_Button_Lighter",
        "Continue_Button_Lighter",
        "Settings_Button_Lighter",
        "Exit_Button_Lighter"
    };

    private readonly Dictionary<string, Entity> buttonEntities = new();
    private int selectedIndex;

    private bool OnStart(ref bool startBool, ulong endSceneControllerId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        // Start with all buttons in normal state; do not preselect/highlight any.
        selectedIndex = -1;

        //reset endscene timer every time
        Entity.FromId(World!,endSceneControllerId).GetComponent<StartMenuControllerComponent>().timer = 1f;
        CacheMenuButtonEntities();
        ApplySelectionVisual();
        //End of Start
        return true;
    }

    private void CacheMenuButtonEntities()
    {
        buttonEntities.Clear();

        foreach (var ui in World!.Query<Name, GUIElement>())
        {
            string name = ui.Component1.Value.ToString();
            buttonEntities[name] = ui.Entity;
        }
    }

    private void SetVisible(string entityName, bool visible)
    {
        if (!buttonEntities.TryGetValue(entityName, out Entity entity) || !entity.IsAlive)
        {
            return;
        }

        if (!entity.TryGetComponent<GUIElement>(out _))
        {
            return;
        }

        entity.GetComponent<GUIElement>().Visible = visible;
    }

    private void ApplySelectionVisual()
    {
        for (int i = 0; i < normalButtonNames.Length; ++i)
        {
            bool selected = (selectedIndex >= 0) && (i == selectedIndex);
            SetVisible(normalButtonNames[i], !selected);
            SetVisible(lighterButtonNames[i], selected);
        }
    }

    private void MoveSelection(int delta)
    {
        int count = normalButtonNames.Length;
        if (count <= 0)
        {
            return;
        }

        if (selectedIndex < 0)
        {
            selectedIndex = 0;
        }
        else
        {
            selectedIndex = (selectedIndex + delta + count) % count;
        }

        ApplySelectionVisual();
    }

    private static void PlayMenuMoveSfx()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("SFX007");
        }
    }

    private void TriggerSelectedAction()
    {
        if (selectedIndex < 0)
        {
            return;
        }

        // New Game / Continue both transition to FGym2 with audio + visual fade.
        if (selectedIndex == 0 || selectedIndex == 1)
        {
            PlayMenuMoveSfx();
            SceneCrossFadeTransition.Request(TargetScenePath, 0.8f, true);
            return;
        }

        // Settings currently has no dedicated scene/action in this flow.
        if (selectedIndex == 2)
        {
            PlayMenuMoveSfx();
            return;
        }

        // Exit
        if (selectedIndex == 3)
        {
            PlayMenuMoveSfx();
            Application.Quit();
        }
    }

    protected override void OnUpdate()
    {
        SceneManager sceneManager = SceneManager.Instance;
        foreach(var controller in World!.Query<StartMenuControllerComponent>())
        {
            if (controller.Component1.isEndScene)
            {
                controller.Component1.timer -= Time.DeltaTime;
                if(Input.IsKeyDown(KeyCode.Space)) //controller.Component1.timer < 0 &&
                {
                    SceneCrossFadeTransition.Request(StartSceneName, 0.8f, true);
                }
            }
            else continue;
        }

        
        Scene? active = sceneManager.GetActive();
        if (active == null || !string.Equals(active.Name, SourceSceneName, StringComparison.Ordinal))
        {
            ////Log($"StartMenuController: Active scene is not the source scene: '{active?.Name ?? "null"}'; aborting switch.");
            return;
        }


        foreach (var controller in World!.Query<StartMenuControllerComponent>())
        {
            bool start = controller.Component1.start;
            controller.Component1.start = OnStart(ref start, controller.Entity.Id);
        }

        // Re-cache if entities changed due to scene edits/hot reload.
        if (buttonEntities.Count < 8)
        {
            CacheMenuButtonEntities();
            ApplySelectionVisual();
        }

        bool moveUp = Input.IsKeyPressed(KeyCode.W);
        bool moveDown = Input.IsKeyPressed(KeyCode.D);

        if (moveUp)
        {
            PlayMenuMoveSfx();
            MoveSelection(-1);
        }

        if (moveDown)
        {
            PlayMenuMoveSfx();
            MoveSelection(1);
        }

        if (Input.IsKeyPressed(KeyCode.Enter))
        {
            TriggerSelectedAction();
        }
    }
}
