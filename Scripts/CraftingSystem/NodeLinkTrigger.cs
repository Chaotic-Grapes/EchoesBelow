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

namespace Scripts.CraftingSystem;

[Component] public record struct NodeLinkTriggerComponent(bool isActiveTrigger);
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class NodeLinkTrigger : TriggerSystemBase
{

    protected override void OnTriggerStay(Entity self, TriggerEvent evt)
    {
        //Filter out all non NodeLink Trigger, SELF = NodeLinkTrigger, EVT = CraftMove particle
        if (Entity.FromId(World!, self.Id).HasComponent<NodeLinkTriggerComponent>() 
            && Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().isActiveTrigger
            && Entity.FromId(World!, self.Id).HasComponent<NodeLinkTriggerComponent>()
            && !Entity.FromId(World!, self.Id).GetComponent<NodeLinkTriggerComponent>().isActiveTrigger)
        {
            //Enable the E key
            CraftAnemone.isEnabled_EInput = true;

            Entity nodeTriggerObj = Entity.FromId(World!, self.Id);
            Entity parentObj = Entity.FromId(World!, Entity.FromId(World!, self.Id).GetParent()!.Id);
            Entity playerMSobj = Entity.FromId(World!, evt.OtherEntityId);

            //Very important, asign this for everyone
            NodeLink.currentNodeLinkObj = parentObj;
            NodeLinkData nodeLinkData = NodeLink.instances[NodeLink.currentNodeLinkObj.Id];

            NodeLinkData.currentActiveTrigger = self.Id;

            //Check if ports are filled
            //If 'x' port is filled AND the corresponding 'x' trigger is queried here, return
            //if (nodeLinkData.port_N_isFilled && Entity.FromId(World!, self.Id).GetComponent<CraftPortComponent>().NSEW_1234 == 1) return;
            //if (nodeLinkData.port_S_isFilled && Entity.FromId(World!, self.Id).GetComponent<CraftPortComponent>().NSEW_1234 == 2) return;
            //if (nodeLinkData.port_E_isFilled && Entity.FromId(World!, self.Id).GetComponent<CraftPortComponent>().NSEW_1234 == 3) return;
            //if (nodeLinkData.port_W_isFilled && Entity.FromId(World!, self.Id).GetComponent<CraftPortComponent>().NSEW_1234 == 4) return;

            DrawLink(nodeTriggerObj, Entity.FromId(World!, NodeLink.currentNodeLinkObj.Id), playerMSobj);
        }

    }
    protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    {
        //if (Entity.FromId(World!, self.Id).HasComponent<CraftPortComponent>() && Entity.FromId(World!, evt.OtherEntityId).HasComponent<CraftMoveComponent>())
        //{
        //    //Disable the E key
        //    CraftAnemone.isEnabled_EInput = false;

        //    Entity parentObj = Entity.FromId(World!, Entity.FromId(World!, self.Id).GetComponent<CraftPortComponent>().parentObjId);
        //    Entity nodeTriggerObj = Entity.FromId(World!, self.Id);

        //    NodeLinkData nodeLinkData = NodeLink.instances[NodeLink.currentNodeLinkObj.Id];

        //    //Check if ports are filled
        //    //If 'x' port is filled AND the corresponding 'x' trigger is queried here, return
        //    if (nodeLinkData.port_N_isFilled && Entity.FromId(World!, self.Id).GetComponent<CraftPortComponent>().NSEW_1234 == 1) return;
        //    if (nodeLinkData.port_S_isFilled && Entity.FromId(World!, self.Id).GetComponent<CraftPortComponent>().NSEW_1234 == 2) return;
        //    if (nodeLinkData.port_E_isFilled && Entity.FromId(World!, self.Id).GetComponent<CraftPortComponent>().NSEW_1234 == 3) return;
        //    if (nodeLinkData.port_W_isFilled && Entity.FromId(World!, self.Id).GetComponent<CraftPortComponent>().NSEW_1234 == 4) return;

        //    ResetLink(nodeTriggerObj);
        //    Log("Exitted");
        //}
    }
    private void DrawLink(Entity nodeTriggerObj, Entity parentObj, Entity playerMSobj)
    {
        //Retrieve positions
        Vector2 nodeTriggerPos = new Vector2(nodeTriggerObj.GetComponent<LocalTransform>().Position.X, nodeTriggerObj.GetComponent<LocalTransform>().Position.Y);
        Vector2 parentObjPos = new Vector2(parentObj.GetComponent<LocalTransform>().Position.X, parentObj.GetComponent<LocalTransform>().Position.Y);
        Vector2 playerMSPos = new Vector2(playerMSobj.GetComponent<LocalTransform>().Position.X, playerMSobj.GetComponent<LocalTransform>().Position.Y);

        //Retrieve Shapeline, and map the position of the MS snow into parentObj's local space
        ref ShapeLine2D lineRenderer = ref nodeTriggerObj.GetComponent<ShapeLine2D>();
        lineRenderer.A = new Vector2(0 - nodeTriggerPos.X, 0 - nodeTriggerPos.Y);
        lineRenderer.B = new Vector2(playerMSPos.X - parentObjPos.X - nodeTriggerPos.X, playerMSPos.Y - parentObjPos.Y - nodeTriggerPos.Y);
    }
    private void ResetLink(Entity nodeTriggerObj)
    {
        Vector2 nodeTriggerPos = new Vector2(nodeTriggerObj.GetComponent<LocalTransform>().Position.X, nodeTriggerObj.GetComponent<LocalTransform>().Position.Y);

        //Retrieve Shapeline
        ref ShapeLine2D lineRenderer = ref nodeTriggerObj.GetComponent<ShapeLine2D>();
        lineRenderer.A = new Vector2(0 - nodeTriggerPos.X, 0 - nodeTriggerPos.Y);
        lineRenderer.B = new Vector2(0 - nodeTriggerPos.X, 0 - nodeTriggerPos.Y);
    }
}
