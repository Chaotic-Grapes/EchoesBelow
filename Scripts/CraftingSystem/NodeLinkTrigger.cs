using EchoesBelow.Scripts;
using EchoesBelow.Scripts.Audio;
using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System.Collections.Generic;
using System.Runtime.InteropServices.Swift;

namespace Scripts.CraftingSystem;

[Component] public record struct NodeLinkTriggerComponent(bool isActiveTrigger);
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class NodeLinkTrigger : TriggerSystemBase
{
    protected override void OnTriggerStay(Entity self, TriggerEvent evt)
    {
        //Log($"self: {Entity.FromId(World!, self.Id).GetComponent<Name>().Value.ToString()} / other: {Entity.FromId(World!, evt.OtherEntityId).GetComponent<Name>().Value.ToString()}");
        //Filter out all non NodeLink Trigger, SELF = NodeLinkTrigger, EVT = CraftMove particle
        if (Entity.FromId(World!, self.Id).HasComponent<NodeLinkTriggerComponent>() 
            && Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().isActiveTrigger
            && Entity.FromId(World!, evt.OtherEntityId).HasComponent<NodeLinkTriggerComponent>()
            && !Entity.FromId(World!, evt.OtherEntityId).GetComponent<NodeLinkTriggerComponent>().isActiveTrigger)
        {
            //Enable the E key
            //CraftAnemone.isEnabled_EInput = true;

            Entity otherEntity = Entity.FromId(World!, evt.OtherEntityId);


            //Very important, asign this for everyone
            //NodeLink.currentNodeLinkObj = self;
            //NodeLinkData.currentActivePort = self.Id;
           
            DrawLink(self, otherEntity);
        }

    }
    protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    {
        if (Entity.FromId(World!, self.Id).HasComponent<NodeLinkTriggerComponent>()
            && Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().isActiveTrigger
            && Entity.FromId(World!, evt.OtherEntityId).HasComponent<NodeLinkTriggerComponent>()
            && !Entity.FromId(World!, evt.OtherEntityId).GetComponent<NodeLinkTriggerComponent>().isActiveTrigger)
        {
            //Enable the E key
            //CraftAnemone.isEnabled_EInput = false;

            Entity otherEntity = Entity.FromId(World!, evt.OtherEntityId);

            foreach (Entity port in self.GetChildren())
            {
                if (port.TryGetComponent<CraftPortComponent>(out CraftPortComponent cPort))
                {
                    ResetLink(port);
                }
            }
            
        }
    }
    private void DrawLink(Entity self, Entity otherEntity)
    {
        //Retrieve positions
        Vector2 selfPos = new Vector2(self.GetComponent<LocalTransform>().Position.X, self.GetComponent<LocalTransform>().Position.Y);
        Vector2 otherPos = new Vector2(otherEntity.GetComponent<LocalTransform>().Position.X, otherEntity.GetComponent<LocalTransform>().Position.Y);

        //Determine Other Obj Pos orientation relative to active self
        Vector2 localUp = new Vector2(0, 1);
        Vector2 localRight = new Vector2(1, 0);
        Vector2 originToOther = otherPos - selfPos;

        Vector2 originToOtherNormalized = (-0.0001f <= originToOther.X && originToOther.X <= 0.0001f && -0.0001f <= originToOther.Y && originToOther.Y <= 0.0001f) ? Vector2.Zero : originToOther.Normalized;

        float vertDot = GMath.Dot(localUp, originToOtherNormalized);
        float horizDot = GMath.Dot(localRight, originToOtherNormalized);

        //Select Child Port
        const float limit = 0.8f;
        Entity selectedPort = self;

        NodeLinkData nodeLinkData = NodeLink.instances[otherEntity.Id];
     
        if (vertDot < -limit)
        {
            foreach(Entity port in self.GetChildren())
            {
                if (port.TryGetComponent<CraftPortComponent>(out CraftPortComponent cPort)
                    && cPort.NSEW_1234 == 1)
                {
                    if (nodeLinkData.port_N_isFilled)
                    {
                        ResetLink(port);
                        break;
                    }
                    selectedPort = port;
                    NodeLinkData.currentActivePort = port.Id;
                    break;
                }
            }
        }
        else if(vertDot > limit)
        {
            foreach (Entity port in self.GetChildren())
            {
                if (port.TryGetComponent<CraftPortComponent>(out CraftPortComponent cPort)
                    && cPort.NSEW_1234 == 2)
                {
                    if (nodeLinkData.port_S_isFilled)
                    {
                        ResetLink(port);
                        break;
                    }
                    selectedPort = port;
                    NodeLinkData.currentActivePort = port.Id;
                    break;
                }
            }
        }
        if (horizDot < -limit)
        {
            foreach (Entity port in self.GetChildren())
            {
                if (port.TryGetComponent<CraftPortComponent>(out CraftPortComponent cPort)
                    && cPort.NSEW_1234 == 3)
                {
                    if (nodeLinkData.port_E_isFilled)
                    {
                        ResetLink(port);
                        break;
                    }
                    selectedPort = port;
                    NodeLinkData.currentActivePort = port.Id;
                    break;
                }
            }
        }
        else if (horizDot > limit)
        {
            foreach (Entity port in self.GetChildren())
            {
                if (port.TryGetComponent<CraftPortComponent>(out CraftPortComponent cPort)
                    && cPort.NSEW_1234 == 4)
                {
                    if (nodeLinkData.port_W_isFilled)
                    {
                        ResetLink(port);
                        break;
                    }
                    selectedPort = port;
                    NodeLinkData.currentActivePort = port.Id;
                    break;
                }
            }
        }
        else
        {
            foreach (Entity port in self.GetChildren())
            {
                if (port.TryGetComponent<CraftPortComponent>(out CraftPortComponent cPort))
                {
                    ResetLink(port);
                    //CraftAnemone.isEnabled_EInput = false;
                }
            }
        }

        //Retrieve Shapeline, and map the position of the MS snow into parentObj's local space
        if (!selectedPort.HasComponent<ShapeLine2D>()) return;
        ref ShapeLine2D lineRenderer = ref selectedPort.GetComponent<ShapeLine2D>();
    
        lineRenderer.A = new Vector2(0, 0);
        lineRenderer.B = new Vector2(otherPos.X - selfPos.X, otherPos.Y - selfPos.Y);

    }
    private void ResetLink(Entity port)
    {
        Vector2 nodeTriggerPos = new Vector2(port.GetComponent<LocalTransform>().Position.X, port.GetComponent<LocalTransform>().Position.Y);

        //Retrieve Shapeline
        ref ShapeLine2D lineRenderer = ref port.GetComponent<ShapeLine2D>();
        lineRenderer.A = new Vector2(0 - nodeTriggerPos.X, 0 - nodeTriggerPos.Y);
        lineRenderer.B = new Vector2(0 - nodeTriggerPos.X, 0 - nodeTriggerPos.Y);
    }
}
