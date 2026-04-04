using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;

namespace EchoesBelow.Scripts;


[Component] public record struct LoadSceneComponent(int level);
[RequireForUpdate<LoadSceneComponent>]
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class LoadScene : TriggerSystemBase
{
    private const string endSceneName = "Scenes/EndScene.scn";
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity other = Entity.FromId(World!, evt.OtherEntityId);
        if (other.HasComponent<PlayerComponent>())
        {
            if (self.TryGetComponent<MatchSignifierComponent>(out MatchSignifierComponent mSign) && mSign.signifierID == 86118001)
            {
                //Log($"self: {self.GetComponent<Name>().Value.ToString()} / other: {other.GetComponent<Name>().Value.ToString()}");
                foreach(var sceneLoader in World!.Query<LoadSceneComponent>())
                {   
                    SceneManager sceneManager = SceneManager.Instance;
                    switch (sceneLoader.Component1.level)
                    {
                        case 1:
                            sceneManager.SetNextAudioTransition(2.0f, true);
                            SceneCrossFadeTransition.Request("Scenes/M6_L1_C1.scn", 1.5f, true);
                            break;
                        case 2:
                            sceneManager.SetNextAudioTransition(2.0f, true);
                            SceneCrossFadeTransition.Request("Scenes/M6_L1_C2.scn", 1.5f, true);
                            break;
                        case 3:
                            sceneManager.SetNextAudioTransition(2.0f, true);
                            SceneCrossFadeTransition.Request("Scenes/M6_L1_C3.scn", 1.5f, true);
                            break;
                        case 4:
                            sceneManager.SetNextAudioTransition(2.0f, true);
                            SceneCrossFadeTransition.Request("Scenes/M6_L2_C1.scn", 1.5f, true);
                            break;
                        case 5:
                            sceneManager.SetNextAudioTransition(2.0f, true);
                            SceneCrossFadeTransition.Request("Scenes/M6_L2_C2.scn", 1.5f, true);
                            break;
                        case 6:
                            sceneManager.SetNextAudioTransition(2.0f, true);
                            SceneCrossFadeTransition.Request("Scenes/M6_L2_C3.scn", 1.5f, true);
                            break;
                        case 7:
                            sceneManager.SetNextAudioTransition(2.0f, true);
                            SceneCrossFadeTransition.Request(endSceneName, 1.5f, true);
                            break;
                        default:
                            break;

                    }
                    //

                }
            }
        }
    }
}
