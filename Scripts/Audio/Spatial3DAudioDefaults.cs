using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;

namespace EchoesBelow.Scripts.Audio;

[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class Spatial3DAudioDefaults : SystemBase
{
    //  default set distances for spatial audio sources
    private const float DefaultMinDistance = 0.5f;
    private const float DefaultMaxDistance = 3.0f;

    // default spread, the lower the stronger the pan
    private const float DefaultSpread = 0.1f;

    // default level, the lower the quieter the sound
    private const float DefaultLevel = 1.0f;

    private bool _applied;
    private bool _reportedMismatch;

    protected override void OnCreate()
    {
        _applied = false;
        _reportedMismatch = false;
    }

    protected override void OnUpdate()
    {
        if (_applied)
        {
            return;
        }

        // Apply once per scene; retry until native readback confirms it stuck.
        Audio.SetDefault3DMinMaxDistance(DefaultMinDistance, DefaultMaxDistance);
        Audio.SetDefault3DSpread(DefaultSpread);
        Audio.SetDefault3DLevel(DefaultLevel);

        // Runtime verification: ensure script values are actually reaching native audio state.
        float readMin = Audio.GetDefault3DMinDistance();
        float readMax = Audio.GetDefault3DMaxDistance();
        float minDiff = readMin - DefaultMinDistance;
        if (minDiff < 0.0f) minDiff = -minDiff;
        float maxDiff = readMax - DefaultMaxDistance;
        if (maxDiff < 0.0f) maxDiff = -maxDiff;

        if (minDiff <= 0.001f && maxDiff <= 0.001f)
        {
            _applied = true;
            return;
        }

        if (!_reportedMismatch)
        {
            _reportedMismatch = true;
            Log($"Spatial3D defaults mismatch. Requested min/max=({DefaultMinDistance}, {DefaultMaxDistance}) but engine has ({readMin}, {readMax})", LogLevel.Warning);
        }
    }
}
