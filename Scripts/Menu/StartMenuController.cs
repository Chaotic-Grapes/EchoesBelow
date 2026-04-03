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
    private const float SceneTransitionDuration = 1.5f;
    //For startscene
    private const string SourceSceneName = "Newstartscene";
    private const string TargetScenePath = "Scenes/FeatureGym.scn";
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
    private int enterLockFrames;

    private bool OnStart(ref bool startBool, ulong endSceneControllerId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        selectedIndex = 0;
        enterLockFrames = 0;

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
    private void SetImageAlpha(string entityName, float alpha)
    {
        if (!buttonEntities.TryGetValue(entityName, out Entity entity) || !entity.IsAlive)
        {
            return;
        }

        if (!entity.TryGetComponent<GUIImage>(out _))
        {
            return;
        }

        ref GUIImage image = ref entity.GetComponent<GUIImage>();
        Color color = image.Color;
        color.A = alpha;
        image.Color = color;
    }

    private void ApplySelectionVisual()
    {
        for (int i = 0; i < normalButtonNames.Length; ++i)
        {
            bool selected = (selectedIndex >= 0) && (i == selectedIndex);

            // Keep both entities visible and crossfade via alpha.
            SetVisible(normalButtonNames[i], true);
            SetVisible(lighterButtonNames[i], true);
            SetImageAlpha(normalButtonNames[i], selected ? 0.0f : 1.0f);
            SetImageAlpha(lighterButtonNames[i], selected ? 1.0f : 0.0f);
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
            //AudioManager.instance.PlaySFX("SFX007");
        }
    }

    private void TransitionToGameScene()
    {
        Log("Transitioning");
        SceneCrossFadeTransition.Request(TargetScenePath, SceneTransitionDuration, true);
    }

    private void TriggerSelectedAction()
    {
        if (selectedIndex < 0)
        {
            return;
        }

        // New Game / Continue both transition to FGym2 with audio + visual fade.
        if (selectedIndex == 0)
        {
            TransitionToGameScene();
            return;
        }

        if(selectedIndex == 1)
        {
            return;
        }

        // Settings currently has no dedicated scene/action in this flow.
        if (selectedIndex == 2)
        {
            return;
        }

        // Exit
        if (selectedIndex == 3)
        {
            Application.Quit();
        }
    }

    protected override void OnUpdate()
    {
        //SceneManager sceneManager = SceneManager.Instance;
        //foreach(var controller in World!.Query<StartMenuControllerComponent>())
        //{
        //    if (controller.Component1.isEndScene)
        //    {
        //        controller.Component1.timer -= Time.DeltaTime;
        //        bool confirmPressedEndScene = Input.IsKeyPressed(KeyCode.Space);
        //        if (confirmPressedEndScene) //controller.Component1.timer < 0 &&
        //        { 
        //            Log("StartMenu end-scene confirm key pressed (G)");
        //            //SceneCrossFadeTransition.Request(TargetScenePath, 0.8f, true);
        //        }
        //    }
        //    else continue;
        //}
        
        //Scene? active = sceneManager.GetActive();
        //if (active == null || !string.Equals(active.Name, SourceSceneName, StringComparison.Ordinal))
        //{
        //    ////Log($"StartMenuController: Active scene is not the source scene: '{active?.Name ?? "null"}'; aborting switch.");
        //    return;
        //}


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
        //ignore
        bool moveLeft = Input.IsKeyPressed(KeyCode.A);
        bool moveRight = Input.IsKeyPressed(KeyCode.D);
        bool movedSelection = false;
        bool confirmPressed = Input.IsKeyPressed(KeyCode.Space);

        if (moveLeft)
        {
            AudioManager.instance.PlaySFX("UI005_Track01");
            MoveSelection(-1);
            movedSelection = true;
        }

        if (moveRight)
        {
            AudioManager.instance.PlaySFX("UI005_Track02");
            MoveSelection(1);
            movedSelection = true;
        }


        if (movedSelection)
        {
            // Block confirm briefly after navigation so actions cannot trigger from stale input.
            enterLockFrames = 2;
        }
        else if (enterLockFrames > 0)
        {
            enterLockFrames -= 1;
        }

        // Guard against accidental action confirmation on movement frames.
        if (!movedSelection && enterLockFrames == 0 && confirmPressed)
        {
            Log("StartMenu confirm key pressed (G)");
            Log("StartMenu confirm fired; selectedIndex=" + selectedIndex);
            TriggerSelectedAction();
        }
    }
}
