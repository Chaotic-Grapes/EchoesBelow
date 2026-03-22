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
        bool isKeyPressed_0 = Input.IsKeyDown(KeyCode.Keypad0);
        bool isKeyPressed_1 = Input.IsKeyDown(KeyCode.Keypad1);
        bool isKeyPressed_2 = Input.IsKeyDown(KeyCode.Keypad2);

        if (isKeyDown_R && isKeyPressed_0)
        {
            SceneManager sceneManager = SceneManager.Instance;
            //Loadscene use dalton's
            sceneManager.SetNextAudioTransition(2.0f, true);
            //var scene = SceneManager.Instance.LoadScene(TargetScenePath);
            //Like creating a new scene / allocate a new scene in the registry
            var sceneIndex = SceneManager.Instance.AddScene();
            var ss = SceneManager.Instance.LoadScene(sceneIndex, "Scenes/Newstartscene.scn");
            SceneManager.Instance.SetActive(sceneIndex);
        }

        if (isKeyDown_R && isKeyPressed_1)
        {
            SceneManager sceneManager = SceneManager.Instance;
            sceneManager.SetNextAudioTransition(2.0f, true);

            var sceneIndex = SceneManager.Instance.AddScene();
            var ss = SceneManager.Instance.LoadScene(sceneIndex, "Scenes/FeatureGym.scn");
            SceneManager.Instance.SetActive(sceneIndex);
        }

        if (isKeyDown_R && isKeyPressed_2)
        {
            SceneManager sceneManager = SceneManager.Instance;
            sceneManager.SetNextAudioTransition(2.0f, true);

            var sceneIndex = SceneManager.Instance.AddScene();
            var ss = SceneManager.Instance.LoadScene(sceneIndex, "Scenes/M5_L1.scn");
            SceneManager.Instance.SetActive(sceneIndex);
        }


        SceneManager sceneManager1 = SceneManager.Instance;
        //if (sceneManager1.GetScene(sceneManager1.GetActiveIndex())!.Name != "M5_L1.scn") return;

        if (Input.IsKeyPressed(KeyCode.N))
        {
            ref LocalTransform t = ref Player.instance.player.GetComponent<LocalTransform>();
            t.Position = new Vector3(-11.57f, 26f, 0f);
        }

        if (Input.IsKeyPressed(KeyCode.M))
        {
            ref LocalTransform t = ref Player.instance.player.GetComponent<LocalTransform>();
            t.Position = new Vector3(-8f, 49.12f, 0f);
        }
        if(Input.IsKeyPressed(KeyCode.K))
        {
            ref LocalTransform t = ref Player.instance.player.GetComponent<LocalTransform>();
            t.Position = new Vector3(9.95f, 89.68f, 0f);
        }
    }

    protected override void OnDestroy()
    {
        Log("System DevKeys destroyed");
    }
}
