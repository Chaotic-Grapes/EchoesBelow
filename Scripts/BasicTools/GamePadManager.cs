using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;

namespace Scripts.BasicTools;

/// <summary>
/// System that processes entities with specific components.
/// This is a pure ECS system: it queries entities and updates their components.
/// </summary>
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class GamePadManager : SystemBase
{
    public static bool isGamePadConnected { get; private set; }
    public static bool isGamePadJustConnected { get; private set; }
    protected override void OnUpdate()
    {
        isGamePadConnected = Input.IsGamepadConnected(0);


        float axis1 = Input.GetGamepadAxis(0, GamepadAxis.LeftY);
        float axis2 = Input.GetGamepadAxis(0, GamepadAxis.RightX);

        if (isGamePadJustConnected)
        {
            Log("Just Connected GamePad");
        }

        if (isGamePadConnected)
        {
            //Log("Connected Gamepad!");
            //Log($"axis1: {axis1} / axis2: {axis2}");
        }

        //if (Input.IsGamepadButtonDown(0, GamepadButton.A))
        //{
        //    Log("A");
        //}

        //if (Input.IsGamepadButtonPressed(0, GamepadButton.B))
        //{
        //    Log("B");
        //}
        //if (Input.IsGamepadButtonDown(0, GamepadButton.A))
        //{
        //    Log("X");
        //}
        //if (Input.IsGamepadButtonDown(0, GamepadButton.Y))
        //{
        //    Log("Y");
        //}
        //if (Input.IsGamepadButtonDown(0, GamepadButton.DPadDown))
        //{
        //    Log("DPAD down");
        //}
        //if (Input.IsGamepadButtonDown(0, GamepadButton.RightBumper))
        //{
        //    Log("Right Bumper");
        //}
        //if (Input.IsGamepadButtonDown(0, GamepadButton.RightThumb))
        //{
        //    Log("Right Thumb");
        //}

    }
}
