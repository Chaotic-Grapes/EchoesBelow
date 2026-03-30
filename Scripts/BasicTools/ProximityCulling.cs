using EchoesBelow.Scripts;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;

namespace Scripts.BasicTools;

[Component] public record struct ProximityCullingComponent
    (bool isCulling, 
    int cullDist, 
    bool start
    );
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class ProximityCulling : SystemBase
{
    public static Vector3 playerPos;
    private Vector3 currentPos;
    //private bool OnStart(ref bool startBool, Entity e)
    //{
    //    if (startBool == true) return true;
    //    startBool = true;
    //    //Todo

    //    ref ProximityCullingComponent prox = ref e.GetComponent<ProximityCullingComponent>();
    //    prox.cullDist = 9;

    //    //End of Start
    //    return true;
    //}
    protected override void OnUpdate()
    {

        foreach (var gameObject in World!.Query<ProximityCullingComponent, Active, LocalTransform>())
        {
            Entity e = Entity.FromId(World!, gameObject.Entity.Id);

            //bool start = gameObject.Component1.start;
            //gameObject.Component1.start = OnStart(ref start, e);

            playerPos = Player.instance.currentPos;
            currentPos = gameObject.Component3.Position;

            float displacement = (playerPos - currentPos).Magnitude;

            if(displacement > gameObject.Component1.cullDist)
            {
                ref Active active = ref e.GetComponent<Active>();
                active.Enabled = false;
                gameObject.Component1.isCulling = true;
            }
            else
            {
                ref Active active = ref e.GetComponent<Active>();
                active.Enabled = true;
                gameObject.Component1.isCulling = false;
            }
        }
    }

}
