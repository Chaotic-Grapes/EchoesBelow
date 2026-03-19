using EchoesBelow.Scripts.Audio;
using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System;

namespace EchoesBelow.Scripts;

[Component] public record struct PlayerTriggerComponent();
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public sealed class PlayerTrigger : SystemBase
{
}

[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class PlayerTriggerHandler : TriggerSystemBase
{
    private const string endSceneName = "Scenes/EndScene.scn";
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        if (self.HasComponent<PlayerComponent>() ||
            other.HasComponent<PlayerComponent>())
        {
            if ((other.TryGetComponent<MatchSignifierComponent>(out MatchSignifierComponent mSign) && other.GetComponent<MatchSignifierComponent>().signifierID == 86118001) ||
                (self.TryGetComponent<MatchSignifierComponent>(out MatchSignifierComponent mSign2) && self.GetComponent<MatchSignifierComponent>().signifierID == 86118001))
            {
                Log("Endscene Entering. . . ");
                SceneManager sceneManager = SceneManager.Instance;
                //Loadscene use dalton's
                sceneManager.SetNextAudioTransition(2.0f, true);
                //var scene = SceneManager.Instance.LoadScene(TargetScenePath);
                //Like creating a new scene / allocate a new scene in the registry
                var sceneIndex = SceneManager.Instance.AddScene();
                var ss = SceneManager.Instance.LoadScene(sceneIndex, endSceneName);
                SceneManager.Instance.SetActive(sceneIndex);
            }
        }
        
        if (self.HasComponent<PlayerTriggerComponent>() ||
                other.HasComponent<PlayerTriggerComponent>())
        {
            //if I detect a marine snow obj, send it back to whence it came!
            if (Entity.FromId(World!, other.Id).TryGetComponent<MS_IDComponent>(out MS_IDComponent msM))
            {
                if (msM.collisionCooldown > 0) return; // if still cooling down, dont pick it up
                AudioManager.instance.PlaySFX("SFX003");
                //MS_Manager.instance.SendToPool(other.Id);
                InventoryController.instance.AddToInventory(other.GetComponent<MS_IDComponent>().msID, other.Id);
            }
        }
       
<<<<<<< Updated upstream

=======
        //if I detect a marine snow obj, send it back to whence it came!
        if (Entity.FromId(World!, other.Id).TryGetComponent<MS_IDComponent>(out MS_IDComponent msM))
        {
            if (msM.collisionCooldown > 0) return; // if still cooling down, dont pick it up
            //AudioManager.instance.PlaySFX("SFX003");

            if (!AudioManager.sfxEntityDictionary["SFX003"].GetComponent<AudioSource>().PlayOnStart)
            {
                AudioManager.instance.PlaySFX("SFX003");
            }
            else if (!AudioManager.sfxEntityDictionary["SFX003_alt01"].GetComponent<AudioSource>().PlayOnStart)
            {
                AudioManager.instance.PlaySFX("SFX003_alt01");
            }
            else if (!AudioManager.sfxEntityDictionary["SFX003_alt02"].GetComponent<AudioSource>().PlayOnStart)
            {
                AudioManager.instance.PlaySFX("SFX003_alt02");
            }

            //MS_Manager.instance.SendToPool(other.Id);
            InventoryController.instance.AddToInventory(other.GetComponent<MS_IDComponent>().msID, other.Id);
        }
>>>>>>> Stashed changes
    }
}