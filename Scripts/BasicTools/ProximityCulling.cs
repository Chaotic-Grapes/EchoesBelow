using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using System;
using EchoesBelow.Scripts;

namespace Scripts.BasicTools;

[Component] public record struct ProximityCullingComponent(bool isCulling, int cullDist);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class ProximityCulling : SystemBase
{
    public static Vector3 playerPos;
    private Vector3 currentPos;
    protected override void OnUpdate()
    {
        foreach(var gameObject in World!.Query<ProximityCullingComponent, Active, LocalTransform>())
        {
            playerPos = Player.instance.currentPos;
            currentPos = gameObject.Component3.Position;

            float displacement = (playerPos - currentPos).Magnitude;

            if(displacement > gameObject.Component1.cullDist)
            {
                ref Active active = ref Entity.FromId(World!, gameObject.Entity.Id).GetFirstChild()!.GetComponent<Active>();
                active.Enabled = false;
                gameObject.Component1.isCulling = true;
            }
            else
            {
                ref Active active = ref Entity.FromId(World!, gameObject.Entity.Id).GetFirstChild()!.GetComponent<Active>();
                active.Enabled = true;
                gameObject.Component1.isCulling = false;
            }
        }
    }

    protected override void OnDestroy()
    {
        Log("System ProximityCulling destroyed");
    }
}
