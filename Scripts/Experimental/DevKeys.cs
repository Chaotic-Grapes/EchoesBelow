using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;

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
        Log("Start!");
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
        bool isKeyPressed_R = Input.IsKeyPressed(KeyCode.R);

        if (isKeyPressed_R)
        {
            Log("Endscene Entering. . . ");
            SceneManager sceneManager = SceneManager.Instance;
            //Loadscene use dalton's
            sceneManager.SetNextAudioTransition(2.0f, true);
            //var scene = SceneManager.Instance.LoadScene(TargetScenePath);
            //Like creating a new scene / allocate a new scene in the registry
            var sceneIndex = SceneManager.Instance.AddScene();
            var ss = SceneManager.Instance.LoadScene(sceneIndex, "Scenes/FeatureGym.scn");
            SceneManager.Instance.SetActive(sceneIndex);
        }
    }

    protected override void OnDestroy()
    {
        Log("System DevKeys destroyed");
    }
}
