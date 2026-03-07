using EchoesBelow.Scripts.MarineSnowSystem;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;


namespace EchoesBelow.Scripts.Audio;

[Component] public record struct AudioSFXComponent(float startVolume, bool start);
[RequireForUpdate<AudioSFXComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class AudioSFX : SystemBase
{
    //This is just a data container
}
