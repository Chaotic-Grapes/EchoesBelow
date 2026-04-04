using EchoesBelow.Scripts;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;

using GrapeEngine.Math;
using GrapeEngine.Scripting.Events;
using System;

namespace Scripts.Experimental;

/// <summary>
/// System that processes entities with specific components.
/// This is a pure ECS system: it queries entities and updates their components.
/// </summary>
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class DevKeys : SystemBase
{
    protected override void OnCreate()
    {
    }

    protected override void OnUpdate()
    {
        // TODO: Query entities and update components
        // Example:
        // var query = Query<Transform>();
        // foreach (var (entity, transform) in query)
        // {
        //     // Process component
        // }
        bool isKeyDown_R = Input.IsKeyDown(KeyCode.R);
        bool isKeyPressed_9 = Input.IsKeyDown(KeyCode.Keypad9);

        bool isKeyPressed_0 = Input.IsKeyDown(KeyCode.Keypad0);
        bool isKeyPressed_1 = Input.IsKeyDown(KeyCode.Keypad1);
        bool isKeyPressed_2 = Input.IsKeyDown(KeyCode.Keypad2);
        bool isKeyPressed_3 = Input.IsKeyDown(KeyCode.Keypad3);
        bool isKeyPressed_4 = Input.IsKeyDown(KeyCode.Keypad4);
        bool isKeyPressed_5 = Input.IsKeyDown(KeyCode.Keypad5);
        bool isKeyPressed_6 = Input.IsKeyDown(KeyCode.Keypad6);

        if (isKeyDown_R && isKeyPressed_0)
        {
            SceneManager sceneManager = SceneManager.Instance;
            //Loadscene use dalton's
            sceneManager.SetNextAudioTransition(2.0f, true);
            //var scene = SceneManager.Instance.LoadScene(TargetScenePath);
            //Like creating a new scene / allocate a new scene in the registry
            SceneCrossFadeTransition.Request("Scenes/StartScene.scn", 0.7f, true);
        }

        if (isKeyDown_R && isKeyPressed_9)
        {
            SceneManager sceneManager = SceneManager.Instance;
            sceneManager.SetNextAudioTransition(2.0f, true);

            var sceneIndex = SceneManager.Instance.AddScene();
            var ss = SceneManager.Instance.LoadScene(sceneIndex, "Scenes/FeatureGym.scn");
            SceneManager.Instance.SetActive(sceneIndex);
        }

        if (isKeyDown_R && isKeyPressed_1)
        {
            SceneManager sceneManager = SceneManager.Instance;
            sceneManager.SetNextAudioTransition(2.0f, true);

            SceneCrossFadeTransition.Request("Scenes/M5_L1.scn", 0.7f, true);
        }

        if (isKeyDown_R && isKeyPressed_2)
        {
            SceneManager sceneManager = SceneManager.Instance;
            sceneManager.SetNextAudioTransition(2.0f, true);

            SceneCrossFadeTransition.Request("Scenes/M5_L1.scn", 0.7f, true);
        }

        if (isKeyDown_R && isKeyPressed_3)
        {
            SceneManager sceneManager = SceneManager.Instance;
            sceneManager.SetNextAudioTransition(2.0f, true);

            SceneCrossFadeTransition.Request("Scenes/M5_L1.scn", 0.7f, true);
        }

        if (isKeyDown_R && isKeyPressed_4)
        {
            SceneManager sceneManager = SceneManager.Instance;
            sceneManager.SetNextAudioTransition(2.0f, true);

            SceneCrossFadeTransition.Request("Scenes/M5_L1.scn", 0.7f, true);
        }

        if (isKeyDown_R && isKeyPressed_5)
        {
            SceneManager sceneManager = SceneManager.Instance;
            sceneManager.SetNextAudioTransition(2.0f, true);

            SceneCrossFadeTransition.Request("Scenes/M5_L1.scn", 0.7f, true);
        }

        if (isKeyDown_R && isKeyPressed_6)
        {
            SceneManager sceneManager = SceneManager.Instance;
            sceneManager.SetNextAudioTransition(2.0f, true);

            SceneCrossFadeTransition.Request("Scenes/M5_L1.scn", 0.7f, true);
        }

        SceneManager sceneManager1 = SceneManager.Instance;
        //if (sceneManager1.GetScene(sceneManager1.GetActiveIndex())!.Name != "M5_L1.scn") return;

        if (isKeyDown_R && Input.IsKeyPressed(KeyCode.N))
        {
            ref LocalTransform t = ref Player.instance.player.GetComponent<LocalTransform>();
            t.Position = new Vector3(-11.57f, 26f, 0f);
        }

        if (isKeyDown_R && Input.IsKeyPressed(KeyCode.M))
        {
            ref LocalTransform t = ref Player.instance.player.GetComponent<LocalTransform>();
            t.Position = new Vector3(-8f, 49.12f, 0f);
        }
        if(isKeyDown_R && Input.IsKeyPressed(KeyCode.K))
        {
            ref LocalTransform t = ref Player.instance.player.GetComponent<LocalTransform>();
            t.Position = new Vector3(9.95f, 89.68f, 0f);
        }
    }
}
