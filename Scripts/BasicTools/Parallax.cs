using EchoesBelow.Scripts.Audio;
using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;

namespace EchoesBelow.Scripts.BasicTools;

[Component] public record struct ParallaxComponent(bool start, int parallaxFacIn100s, int target_signifierID);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class Parallax : SystemBase
{

    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo



        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        foreach(var gameObject in World!.Query<ParallaxComponent, LocalTransform>())
        {
            //This is how we find matching signifiers============================================================

            ulong targetObjId = MS_Manager.instance.emptyId; //default objId, just borrowing MS_Manager's empty ID

            foreach (var result in World!.Query<MatchSignifierComponent>())
            {
                if (result.Component1.signifierID == gameObject.Component1.target_signifierID)
                {
                    targetObjId = result.Entity.Id;
                }
            }

            //===================================================================================================
            Entity entity = Entity.FromId(World!, gameObject.Entity.Id);
            ref LocalTransform transform = ref gameObject.Component2;

            Entity targetEntity = Entity.FromId(World!, targetObjId);
            ref LocalTransform targetTransform = ref targetEntity.GetComponent<LocalTransform>();

            transform.Position = new Vector3(targetTransform.Position.X * ((float)gameObject.Component1.parallaxFacIn100s/100f)
                , targetTransform.Position.Y * ((float)gameObject.Component1.parallaxFacIn100s/100f)
                , 0f);
        }
    }
}
