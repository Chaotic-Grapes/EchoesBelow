using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using EchoesBelow.Scripts.MarineSnowSystem;
using System;
using EchoesBelow.Scripts.Audio;
using Scripts.CraftingSystem;

namespace EchoesBelow.Scripts;

[Component] public record struct PlayerComponent(
    //[Pseudo-SerializeField]
    float driftSpeed,
    float periodicForceIntervalinMS,
    float moveSpeed,
    float angularVelocity,
    float dashSpeed,
    bool start, // required for start function
    float vomitSpeed
);
[RequireForUpdate<PlayerComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class Player : SystemBase
{
    public static Player instance;
    public Entity player {  get; set; }
    
    //Make anim states here
    AnimState moveState = new AnimState("moveState",0, 0, 20, 24f);
    AnimState idleState = new AnimState("idleState",1, 5, 65, 30f);
    AnimState dashState = new AnimState("dashState",5, 6, 26, 30f);
    AnimState dmgFlashState = new AnimState("dmgFlashState", 12, 11, 1, 24f);

    public static Vector2 playerDir;
    public static Compass abs_InputDirection = Compass.N;
    
    public float vomitSpeed;
    public Vector3 currentPos;
    const float lerpFac = 1;
    const float maxSpeed = 8;
    float timer_forRotation = 0;
    float timer_forPeriodicForce = 0;
    float dashCoolDownTimer;
    bool isCoolingDown;
    public bool cueIsHitVisual;
    public float hitVisualCoolDown = 0;

    static bool isKeyDown_W = false;
    static bool isKeyDown_A = false;
    static bool isKeyDown_S = false;
    static bool isKeyDown_D = false;
    static bool isKeyPressed_Space = false;
    public bool isDashing = false;

    public bool isEnabled;

    protected override void OnCreate()
    {
        instance = this;
        //Log("System Player initialized");
    }
    private bool OnStart(ref bool startBool, ulong objId)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        isEnabled = true;

        player = Entity.FromId(World!, objId);

        dashCoolDownTimer = 0;
        isCoolingDown = false;
        cueIsHitVisual = false;
        hitVisualCoolDown = 0;

        PlayerAnimManager.instance.SetAnimState(idleState);


        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {

        if (isEnabled)
        {
            isKeyDown_W = Input.IsKeyDown(KeyCode.W);
            isKeyDown_A = Input.IsKeyDown(KeyCode.A); 
            isKeyDown_S = Input.IsKeyDown(KeyCode.S);
            isKeyDown_D = Input.IsKeyDown(KeyCode.D);
            isKeyPressed_Space = Input.IsKeyPressed(KeyCode.Space);
        }

        foreach (var gameObject in World!.Query<PlayerComponent, LinearVelocity2D, AngularVelocity2D, LocalTransform>())
        {
            //A Pseudo Start function, called once per obj at runtime
            //This allows onStart to work
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start, gameObject.Entity.Id);

            //Variables
            vomitSpeed = gameObject.Component1.vomitSpeed;

            ref LocalTransform transform = ref gameObject.Component4;
            ref LinearVelocity2D lv = ref gameObject.Component2;
            ref AngularVelocity2D av = ref gameObject.Component3;
            Vector2 moveDir = Vector2.Zero;
            Vector2 moveDirNormalized = Vector2.Zero;
            float moveSpeed = gameObject.Component1.moveSpeed * 0.01f; //this allows floating point decimal values
            float driftSpeed = gameObject.Component1.driftSpeed * 0.01f; //this allows floating point decimal values
            float dashSpeed = gameObject.Component1.dashSpeed * 0.01f; //this allows floating point decimal values
            float angularVelocity = gameObject.Component1.angularVelocity * 0.01f; //100 == 1
            float periodicForceInterval = gameObject.Component1.periodicForceIntervalinMS/1000;

            moveDir = ProcessInput(moveDir, lerpFac);

            //Anim
            if (cueIsHitVisual)
            {
                PlayerAnimManager.instance.SetAnimState(dmgFlashState);
                //Highlight the color!
                ref SpriteRenderer2D spr = ref gameObject.Entity.GetComponent<SpriteRenderer2D>();
                spr.Color = new Color(1.5f, 1.5f, 1.5f, 1f);

                hitVisualCoolDown -= Time.DeltaTime;
                if (hitVisualCoolDown < 0) 
                {
                    spr.Color = new Color(1f, 1f, 1f, 1f);
                    cueIsHitVisual = false; 
                }
            }
            else if (isDashing)
            {
              
                PlayerAnimManager.instance.SetAnimState(dashState);

                //When dashing ends
                if(Entity.FromId(World!, gameObject.Entity.Id).GetComponent<AnimationState2D>().CurrentFrame == 25)
                {
                    //Log($"currentState : {PlayerAnimManager.instance.currentState}");
                    isDashing = false;
                    foreach (var ps in World!.Query<MatchSignifierComponent, ParticleEmitter>())
                    {
                        if (ps.Component1.signifierID == 2)
                        {
                            ps.Component2.EmissionRate = 1.5f;
                        }
                    }
                }
            }
            else if (!isDashing && (isKeyDown_A || isKeyDown_D || isKeyDown_W || isKeyDown_S))
            {
                PlayerAnimManager.instance.SetAnimState(moveState);
            } // can add an else if in between for spacebar soon
            else
            {
                PlayerAnimManager.instance.SetAnimState(idleState);
                timer_forPeriodicForce = 0;
            }


            //NaN protection for normalization
            if (-0.0001f <= moveDir.X && moveDir.X <= 0.0001f && -0.0001f <= moveDir.Y && moveDir.Y <= 0.0001f) moveDirNormalized = Vector2.Zero;
            else moveDirNormalized = moveDir.Normalized;

            //Handling Rotation! Aligning Grain to moveDir=================================================
            //Convert from ZYX Quaternion to angle in radians
            //Find the local "Up" Vector of the Player. Think of this as gameObject.transform.up in Unity
            float playerAngle = Quat2EulerAxisZ(transform.Rotation);
            playerDir = new Vector2(GMath.Cos(playerAngle + (90 * GMath.Deg2Rad)), GMath.Cos(playerAngle));

            //Find change in angle required using dot product between moveDirNormalized and playerDir
            //NaN protection when player is facing up or at rest
            if (-0.0001f < playerDir.X && playerDir.X < 0.0001f && 0.9999f < playerDir.Y && playerDir.Y < 1.0001f) playerDir = new Vector2(0, 1);

            //============================================================================================
            RotationPolarityHandler(transform);
            float flipFactor = GMath.Clamp(HeadingDifference(playerAngle * GMath.Rad2Deg, (float)abs_InputDirection) * GMath.Rad2Deg, -1,1);
            
            //Dot product operation to determine theta as presented by angleBetween in radians!
            float angleBetween_rad = GMath.Acos(GMath.Dot(playerDir, moveDirNormalized) / (playerDir.Magnitude * moveDirNormalized.Magnitude));
            angleBetween_rad = (float.IsNaN(angleBetween_rad)) ? 0 : angleBetween_rad;

            //Find change in time required to complete a rotation. This formula requires radians
            //Must always be positive so we use Magnitude thru Abs
            float rotDuration = GMath.Abs(angleBetween_rad / angularVelocity);
            
            //start Rotation process
            bool isRotating = false;
            if (angleBetween_rad != 0) 
            { 
                isRotating = true; 
            } 
            if (isRotating)
            {
                timer_forRotation += Time.DeltaTime;
                if(flipFactor >= 0) av.Value = GMath.Lerp(av.Value, angularVelocity, lerpFac);
                else                av.Value = GMath.Lerp(av.Value, -angularVelocity, lerpFac);
            }
            if (timer_forRotation > rotDuration)
            {
                isRotating = false;
                timer_forRotation = 0;
                av.Value = 0;
            }

            if(isKeyPressed_Space && !isCoolingDown)
            {
                //Zero out animstate2D
                Entity.FromId(World!, gameObject.Entity.Id).GetComponent<AnimationState2D>().CurrentFrame = 0;


                AddInstantaneousForce(ref lv, playerDir, dashSpeed);
                isCoolingDown = true;
                dashCoolDownTimer = 1.25f;

                //for accessing the particle system
                foreach (var ps in World!.Query<MatchSignifierComponent, ParticleEmitter>())
                {
                    if (ps.Component1.signifierID == 2)
                    {
                        ps.Component2.EmissionRate = 25;
                    }
                }

            }
            else if (isKeyDown_W || isKeyDown_S
            || isKeyDown_A || isKeyDown_D)
            {
                AddDriftForce(ref lv, playerDir, driftSpeed, maxSpeed);
                AddPeriodicalForce(ref lv, periodicForceInterval, ref timer_forPeriodicForce, playerDir, moveSpeed);
            }

            if (isCoolingDown)
            {
                dashCoolDownTimer -= Time.DeltaTime;
                if(dashCoolDownTimer < 0)
                {
                    isCoolingDown = false;
                }
            }


            //Finally, cap the overall speed thru capping the linear velocities
            SpeedLimit(ref lv, maxSpeed);

            //update Position
            currentPos = transform.Position;


            GrapeEngine.Scripting.Services.Audio.SetListener(
              transform.Position,
             (transform.Position - currentPos) / (Time.DeltaTime > 0.0f ? Time.DeltaTime : 0.0001f),
             new Vector3(playerDir.X, playerDir.Y, 0.0f), 
             new Vector3(0.0f, 0.0f, 1.0f));



        }
    }
    private void SpeedLimit(ref LinearVelocity2D lv,float maxSpeed)
    {
        lv.Value.X = GMath.Clamp(lv.Value.X, -maxSpeed, maxSpeed);
        lv.Value.Y = GMath.Clamp(lv.Value.Y, -maxSpeed, maxSpeed);

    }
    private void AddInstantaneousForce(ref LinearVelocity2D lv, Vector2 playerDir, float dashSpeed)
    {
        //AudioManager.instance.PlaySFX("SFX005");
        if (!AudioManager.sfxEntityDictionary["SFX005"].GetComponent<AudioSource>().PlayOnStart)
        {
            AudioManager.instance.PlaySFX("SFX005");
        }
        else if (!AudioManager.sfxEntityDictionary["SFX010"].GetComponent<AudioSource>().PlayOnStart)
        {
            AudioManager.instance.PlaySFX("SFX010");
        }
        else if (!AudioManager.sfxEntityDictionary["SFX011"].GetComponent<AudioSource>().PlayOnStart)
        {
            AudioManager.instance.PlaySFX("SFX011");
        }

        //lv.Value.X += playerDir.X * moveSpeed * 2 * GMath.Clamp(lv.Value.X, 1, 10);
        //lv.Value.Y += playerDir.Y * moveSpeed * 2 *  GMath.Clamp(lv.Value.X, 1, 10);
        isDashing = true;
        lv.Value = playerDir.Normalized * dashSpeed;
    }
    private static void AddDriftForce(ref LinearVelocity2D lv, Vector2 playerDir, float moveSpeed, float maxSpeed)
    {
        lv.Value.X += playerDir.X * moveSpeed * Time.DeltaTime;
        lv.Value.Y += playerDir.Y * moveSpeed * Time.DeltaTime;
        //Clamping these values to a maxSpeed
        lv.Value.X = GMath.Clamp(lv.Value.X, -maxSpeed, maxSpeed);
        lv.Value.Y = GMath.Clamp(lv.Value.Y, -maxSpeed, maxSpeed);
       
    }
    private void AddPeriodicalForce(ref LinearVelocity2D lv, float periodicForceInterval, ref float timer_forPeriodicForce, Vector2 playerDir, float moveSpeed)
    {
        //The periodical force is applied 
        timer_forPeriodicForce += Time.DeltaTime;
        if(timer_forPeriodicForce > periodicForceInterval)
        {
            timer_forPeriodicForce = 0;
            lv.Value.X += playerDir.X * moveSpeed * GMath.Clamp(lv.Value.X, 1, 10) * Time.DeltaTime;
            lv.Value.Y += playerDir.Y * moveSpeed * GMath.Clamp(lv.Value.X, 1, 10) * Time.DeltaTime;

            int audioRandomiser = GMath.Random(1, 10);

            switch (audioRandomiser)
            {
                case 1:
                    AudioManager.instance.PlaySFX("SFX001_Track01");
                    break;
                case 2:
                    AudioManager.instance.PlaySFX("SFX001_Track02");
                    break;
                case 3:
                    AudioManager.instance.PlaySFX("SFX001_Track03");
                    break;
                case 4:
                    AudioManager.instance.PlaySFX("SFX001_Track04");
                    break;
                case 5:
                    AudioManager.instance.PlaySFX("SFX001_Track05");
                    break;
                case 6:
                    AudioManager.instance.PlaySFX("SFX001_Track06");
                    break;
                case 7:
                    AudioManager.instance.PlaySFX("SFX001_Track07");
                    break;
                case 8:
                    AudioManager.instance.PlaySFX("SFX001_Track08");
                    break;
                case 9:
                    AudioManager.instance.PlaySFX("SFX001_Track09");
                    break;
                case 10:
                    AudioManager.instance.PlaySFX("SFX001_Track10");
                    break;
            }
        }
    }
    private void RotationPolarityHandler(LocalTransform transform)
    {
        //Find InputAbsDirection direction
        //For a quirk in the angles, Keycodes D and A (left and right) are swapped!
        if (isKeyDown_W && isKeyDown_D) abs_InputDirection = Compass.NW;
        else if (isKeyDown_S && isKeyDown_D) abs_InputDirection = Compass.SW;
        else if (isKeyDown_S && isKeyDown_A) abs_InputDirection = Compass.SE;
        else if (isKeyDown_W && isKeyDown_A) abs_InputDirection = Compass.NE;
        else if (isKeyDown_W) abs_InputDirection = Compass.N;
        else if (isKeyDown_D) abs_InputDirection = Compass.W;
        else if (isKeyDown_S) abs_InputDirection = Compass.S;
        else if (isKeyDown_A) abs_InputDirection = Compass.E;
    }

    private Vector2 ProcessInput(Vector2 moveDir, float lerpFac)
    {
        if (isKeyDown_W) moveDir.Y = GMath.Lerp(moveDir.Y, 1, lerpFac);
        if (isKeyDown_S) moveDir.Y = GMath.Lerp(moveDir.Y, -1, lerpFac);
        if (isKeyDown_A) moveDir.X = GMath.Lerp(moveDir.X, -1, lerpFac);
        if (isKeyDown_D) moveDir.X = GMath.Lerp(moveDir.X, 1, lerpFac);
        //Always decelerrating moveDir but at a slower rate
        moveDir.X = GMath.Lerp(moveDir.X, 0, lerpFac / 2);
        moveDir.Y = GMath.Lerp(moveDir.Y, 0, lerpFac / 2);
        return moveDir;
    }

    private float Quat2EulerAxisZ(Quaternion quat)
    {
        //To find out how
        //Search up Conversion of ZYX Quaternion to Euler Angle (z-yaw)
        float x = quat.X;
        float y = quat.Y;
        float z = quat.Z;
        float w = quat.W;

        float a = 2 * (w * z + x * y);
        float b = 1 - (2 * ((y * y) + (z * z)));
        float outAngle = GMath.Atan2(a, b);
        return outAngle;
    }
    private float HeadingDifference(float heading1, float heading2)
    {
        float diff = (heading2 - heading1 + 180) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }
    protected override void OnDestroy()
    {
        //Log("System Player destroyed");
    }
    public void ResetInputs()
    {
        isKeyDown_W = false;
        isKeyDown_A = false;
        isKeyDown_S = false;
        isKeyDown_D = false;
        isKeyPressed_Space = false;
    }
    public enum Compass
    {
        NE = 45,
        E  = 90,
        SE = 135,
        S  = 180,
        SW = 225,
        W  = 270,
        NW = 315,
        N  = 0
    }
}

//[Component] public record struct PlayerCollisionHandler();
[System(SystemGroup.PostPhysics, SystemRunMode.PlayOnly)]
public class PlayerCollisionHandler : CollisionSystemBase
{
    //int i = 0;
    protected override void OnCollisionEnter(Entity self, CollisionEvent evt)
    {
        //Log($"self: {self.GetComponent<Name>().ToString()} / other {Entity.FromId(World!,evt.OtherEntityId).GetComponent<Name>().Value.ToString()} . . . Id: {evt.OtherEntityId}");

        //Log($"Entering self: {self.GetComponent<Name>().ToString()} into TryGetComponent<PlayerComponent> Check . . . . . . . . . . . . . . . . . . . . . .");
        if (Entity.FromId(World!, self.Id).TryGetComponent<PlayerComponent>(out PlayerComponent plc))
        {
            //Log("Collision: I have a Player Component!");
        }
        else
        {
            return;
        }
        
        PlayerCollisionEvents(self, evt);
    }
    protected override void OnCollisionStay(Entity self, CollisionEvent evt)
    {
        //Log($"self: {Entity.FromId(World!, self.Id).GetComponent<Name>().Value.ToString()} / other: {evt.OtherEntityId} {Entity.FromId(World!, evt.OtherEntityId).GetComponent<Name>().Value.ToString()}");
    }
    private void PlayerCollisionEvents(Entity self, CollisionEvent evt)
    {
        //Log("PlayerCollisionEvents Start >>>");
        Entity other = Entity.FromId(World!, evt.OtherEntityId);

        //Find Tag Mask, Then take damage and open doors 
        if (Entity.FromId(World!, other.Id).TryGetComponent<TagMask>(out TagMask tg))
        {

            if (tg.Mask == 32)
            {
                //Take damage
                AudioManager.instance.PlaySFX("SFX004");
                ProcessDeath.instance.TakeHit(evt.OtherEntityId, self.Id);
                Log("Take Damage!");

                Player.instance.cueIsHitVisual = true;
                Player.instance.hitVisualCoolDown = 0.125f;
            }
            
            if (tg.Mask == 4 && Player.instance.isDashing && GMath.Abs(Player.instance.player.GetComponent<LinearVelocity2D>().Value.Magnitude) > 0.1f)
            {
                AudioManager.instance.PlaySFX("SFX006");
                //door detected
                other.GetComponent<Active>().Enabled = false;
                Log("Door Detected!");
            }
        }

        //Marine Snow Trigger detection is handled by the Squidward!
        if (Entity.FromId(World!, other.Id).TryGetComponent<MS_IDComponent>(out MS_IDComponent msM) && !Entity.FromId(World!, other.Id).HasComponent<CraftMoveComponent>())
        {
            if (msM.collisionCooldown > 0) return; // if still cooling down, dont pick it up
            // AudioManager.instance.PlaySFX("SFX003");

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
    }
}


