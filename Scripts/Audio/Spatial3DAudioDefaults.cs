using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;

namespace EchoesBelow.Scripts.Audio;

[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class Spatial3DAudioDefaults : SystemBase
{
    //  default set distances for spatial audio sources
    private const float DefaultMinDistance = 0.5f;
    private const float DefaultMaxDistance = 5.0f;

    // default spread, the lower the stronger the pan
    private const float DefaultSpread = 0.0f;

    // default level, the lower the quieter the sound
    private const float DefaultLevel = 1.0f;

    private bool _applied;

    protected override void OnUpdate()
    {
        if (_applied) return;

        // Only apply once the world actually contains at least one spatial source.
        foreach (var audio in World!.Query<AudioSource>())
        {
            if (!audio.Component1.Spatial3D) continue;

            Audio.SetDefault3DMinMaxDistance(DefaultMinDistance, DefaultMaxDistance);
            Audio.SetDefault3DSpread(DefaultSpread);
            Audio.SetDefault3DLevel(DefaultLevel);
            _applied = true;
            break;
        }
    }
}

