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
                SceneCrossFadeTransition.Request(endSceneName, 2.0f, true);
            }
        }
        
        if (self.HasComponent<PlayerTriggerComponent>() ||
                other.HasComponent<PlayerTriggerComponent>())
        {
            //if I detect a marine snow obj, send it back to whence it came!
            if (Entity.FromId(World!, other.Id).TryGetComponent<MS_IDComponent>(out MS_IDComponent msM))
            {
                if (msM.collisionCooldown > 0) return; // if still cooling down, dont pick it up
                                                       //AudioManager.instance.PlaySFX("SFX003");

                int audioRandomiser = GMath.Random(1, 10);

                switch (audioRandomiser)
                {
                    case 1:
                        AudioManager.instance.PlaySFX("SFX003_Track01");
                        break;
                    case 2:
                        AudioManager.instance.PlaySFX("SFX003_Track02");
                        break;
                    case 3:
                        AudioManager.instance.PlaySFX("SFX003_Track03");
                        break;
                    case 4:
                        AudioManager.instance.PlaySFX("SFX003_Track04");
                        break;
                    case 5:
                        AudioManager.instance.PlaySFX("SFX003_Track05");
                        break;
                    case 6:
                        AudioManager.instance.PlaySFX("SFX003_Track06");
                        break;
                    case 7:
                        AudioManager.instance.PlaySFX("SFX003_Track07");
                        break;
                    case 8:
                        AudioManager.instance.PlaySFX("SFX003_Track08");
                        break;
                    case 9:
                        AudioManager.instance.PlaySFX("SFX003_Track09");
                        break;
                    case 10:
                        AudioManager.instance.PlaySFX("SFX003_Track10");
                        break;
                }


                //MS_Manager.instance.SendToPool(other.Id);
                InventoryController.instance.AddToInventory(other.GetComponent<MS_IDComponent>().msID, other.Id);
            }
        }
    }
}
