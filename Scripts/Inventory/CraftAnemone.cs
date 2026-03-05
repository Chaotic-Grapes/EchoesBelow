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


namespace EchoesBelow.Scripts.BasicTools;

public class CraftAnemoneData
{
    public ulong objId { get; set; }
    public string name { get; set; }
    public bool isCaptured { get; set; }
    public bool isOpened { get; set; }
    public bool isEnteredAnemone {  get; set; }
    public bool isExitingAnemone { get; set; }
    public Vector3 startNodePos { get; set; }
    public Entity startNode { get; set; }

 


    // I can have multiple unique fields in here, these cant be set from the outset
    // But colliders can query and send info to the corresponding CMachineData container
    // Accessing thru ulong ids
    public CraftAnemoneData(ulong objId, string name)
    {
        this.objId = objId;
        this.name = name;
    }
}
[Component] public record struct CraftAnemoneComponent(bool start, float lerpFacInMiliseconds);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class CraftAnemone : SystemBase
{
    public static Dictionary<ulong, CraftAnemoneData> instances;
    private const float cameraOffsetY = 2.20f;
    private const float FOVoffset = 141;
    private const float FOVoriginal = 128.5f;
    private const float cameraOriginalY = 0f;
    private const float marginAllowance = 0.25f;

    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo
        instances = new Dictionary<ulong, CraftAnemoneData>();

        foreach (var gameObject in World!.Query<CraftAnemoneComponent>())
        {
            // Process component
            //creates a new craftAnemoneData container per CraftAnemone detected
            CraftAnemoneData craftAnemone = new CraftAnemoneData(gameObject.Entity.Id, Entity.FromId(World!, gameObject.Entity.Id).GetComponent<Name>().Value.ToString());
            instances.Add(gameObject.Entity.Id, craftAnemone);

            //Assign to that specific anemone
            CraftAnemoneData craftAnemoneInstance = instances[gameObject.Entity.Id];
            craftAnemoneInstance.isCaptured = false;
            craftAnemoneInstance.isOpened = false;

            //craftAnemone.startNodePos = Entity.FromId(World!, gameObject.Entity.Id);

            //Assign the appropriate start pos
            foreach (Entity child in Entity.FromId(World!, gameObject.Entity.Id).GetChildren())
            {
                if (child.TryGetComponent<MatchSignifierComponent>(out MatchSignifierComponent mSignifier) && mSignifier.signifierID == 466)
                {
                    craftAnemone.startNodePos = child.GetComponent<LocalTransform>().Position;
                    craftAnemoneInstance.startNode = child;
                }
            }


        }
     
        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        //Use this
        foreach (var gameObject in World!.Query<CraftAnemoneComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start);

            float lerpFac = gameObject.Component1.lerpFacInMiliseconds / 1000f;

            ////Do everyth else
            if (instances[gameObject.Entity.Id].isCaptured)
            {
                TractorBeam(CraftAnemoneHandler.capturedEntity, gameObject.Entity.Id, lerpFac);

                float yBloom = 1.55f;
                float yWilt = -0.86f; // might be unused but good to know!
                if (!instances[gameObject.Entity.Id].isOpened) //if it hasnt been opened, open the craftanemone start node!
                    Bloom(instances[gameObject.Entity.Id], yBloom,lerpFac * 0.8f);
                //if you want it to wilt, plug in the yWilt value!
            }

            if (instances[gameObject.Entity.Id].isEnteredAnemone && !instances[gameObject.Entity.Id].isExitingAnemone)
            {
                TransitionCamera(instances[gameObject.Entity.Id], lerpFac*1.6f);
            }
            if (instances[gameObject.Entity.Id].isExitingAnemone && !instances[gameObject.Entity.Id].isEnteredAnemone)
            {
                ResetCamera(instances[gameObject.Entity.Id], lerpFac * 1.6f);
            }

        }
    }
    public void Bloom(CraftAnemoneData cr, float yOffset ,float lerpFac)
    {
        Entity startNodeEntity = Entity.FromId(World!, cr.startNode.Id);

        startNodeEntity.GetComponent<Active>().Enabled = true;
        startNodeEntity.GetFirstChild()!.GetComponent<Active>().Enabled = true;

        ref LocalTransform startNodeTransform = ref startNodeEntity.GetComponent<LocalTransform>();

        //Hardcoded transform values

        startNodeTransform.Position = new Vector3(startNodeTransform.Position.X,
                                                  GMath.Lerp(startNodeTransform.Position.Y, yOffset, lerpFac), 
                                                  startNodeTransform.Position.Z);

        if (startNodeTransform.Position.Y >= yOffset - marginAllowance)
        {
            cr.isOpened = true;
        }
    }
    public void TransitionCamera(CraftAnemoneData cr, float lerpFac)
    {
        foreach (var camera in World!.Query<Camera3D>())
        {

            ref LocalTransform cameraTransform = ref Entity.FromId(World!, camera.Entity.Id).GetComponent<LocalTransform>();

            //Lerp transform to 2.2 on positive y
            //And change FOV to 141
            cameraTransform.Position = new Vector3(cameraTransform.Position.X, GMath.Lerp(cameraTransform.Position.Y, cameraOffsetY, lerpFac), cameraTransform.Position.Z);
            Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV = GMath.Lerp(Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV, FOVoffset, lerpFac);

            Log($"camPos: {cameraTransform.Position.Y} bool check with camOffSetY {cameraOffsetY}: {cameraTransform.Position.Y == cameraOffsetY}");
            if (cameraTransform.Position.Y >= cameraOffsetY - marginAllowance && Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV >= FOVoffset - marginAllowance)
            {   //When Done entering
                cr.isEnteredAnemone = false;
                cr.isExitingAnemone = false;
                Log("Done Entering!");
            }

        }
    }
    public void ResetCamera(CraftAnemoneData cr, float lerpFac)
    {
        foreach (var camera in World!.Query<Camera3D>())
        {

            ref LocalTransform cameraTransform = ref Entity.FromId(World!, camera.Entity.Id).GetComponent<LocalTransform>();

            //Lerp transform to 2.2 on positive y
            //And change FOV to 141
            cameraTransform.Position = new Vector3(cameraTransform.Position.X, GMath.Lerp(cameraTransform.Position.Y, cameraOriginalY, lerpFac), cameraTransform.Position.Z);
            Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV = GMath.Lerp(Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV, FOVoriginal, lerpFac);

            if (cameraTransform.Position.Y <= cameraOriginalY + marginAllowance && Entity.FromId(World!, camera.Entity.Id).GetComponent<Camera3D>().FOV <= FOVoriginal + marginAllowance)
            {   //When done exiting
                cr.isEnteredAnemone = false;
                cr.isExitingAnemone = false;
                Log("Done Exiting!");
            }


        }
    }
    public void TractorBeam(Entity e, ulong objId, float lerpFac)
    {
        Entity capturedEntity = Entity.FromId(World!, e.Id);
        Entity anemone = Entity.FromId(World!, objId);

        ref LocalTransform capturedEntityTransform = ref capturedEntity.GetComponent<LocalTransform>();
        ref LocalTransform transform = ref anemone.GetComponent<LocalTransform>();

        //Zero out angular and linearVelocities
        ref LinearVelocity2D capturedEntityLinearVelocity = ref capturedEntity.GetComponent<LinearVelocity2D>();
        ref AngularVelocity2D capturedEntityAngularVelocity = ref capturedEntity.GetComponent<AngularVelocity2D>();
        capturedEntityAngularVelocity.Value = 0;
        capturedEntityLinearVelocity.Value = Vector2.Zero;

        //Zero out rotation, and reset!
        capturedEntityTransform.Rotation = Quaternion.Identity;

        //Interpolate towards anemone!
        capturedEntityTransform.Position = new Vector3(GMath.Lerp(capturedEntityTransform.Position.X, transform.Position.X, lerpFac),
                                                       GMath.Lerp(capturedEntityTransform.Position.Y, transform.Position.Y, lerpFac), 0);

        //If position is within the agreed allowance, stop the tractor beam
        float playerXpos = capturedEntity.GetComponent<LocalTransform>().Position.X;
        float Xboundary = transform.Position.X;

        float playerYpos = capturedEntity.GetComponent<LocalTransform>().Position.Y;
        float yBoundary = transform.Position.Y;

        if (Xboundary - 0.125f < playerXpos && playerXpos < Xboundary + 0.125f &&
            yBoundary - 0.125f < playerYpos && playerYpos < yBoundary + 0.125f)
        {
            instances[objId].isCaptured = false;
        }
    }
}

[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class CraftAnemoneHandler : TriggerSystemBase
{
    ////this passes information to CraftAnemone class
    public static Entity capturedEntity;
    protected override void OnTriggerEnter(Entity self, TriggerEvent evt)
    {
        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        if (Entity.FromId(World!, self.Id).HasComponent<CraftAnemoneComponent>()) { }
        else return;

        if (other.HasComponent<PlayerComponent>()) LaunchCrafting(self, other);

    }
    protected override void OnTriggerExit(Entity self, TriggerExitEvent evt)
    {
        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        if (Entity.FromId(World!, self.Id).HasComponent<CraftAnemoneComponent>()) { }
        else return;


        if (other.HasComponent<PlayerTriggerComponent>())
        {
            CraftAnemone.instances[self.Id].isExitingAnemone = true;
            CraftAnemone.instances[self.Id].isEnteredAnemone = false;
        }

    }

    private void LaunchCrafting(Entity self, Entity other)
    {
        //This is everything that happens when a player is captured by the anemone
        capturedEntity = other;

        CraftAnemone.instances[self.Id].isCaptured = true;
        //CraftAnemone.instances[self.Id].isOpened = true;
        CraftAnemone.instances[self.Id].isEnteredAnemone = true;
        CraftAnemone.instances[self.Id].isExitingAnemone = false;

        //if (!CraftAnemone.instances[self.Id].isOpened)
        //Entity.FromId(World!, CraftAnemone.instances[self.Id].startNode.Id).GetComponent<Active>().Enabled = true;
        //Entity.FromId(World!, CraftAnemone.instances[self.Id].startNode.Id).GetFirstChild()!.GetComponent<Active>().Enabled = true;
    }

}
