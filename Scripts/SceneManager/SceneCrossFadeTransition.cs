using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;

namespace EchoesBelow.Scripts;

[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class SceneCrossFadeTransition : SystemBase
{
    private enum TransitionState
    {
        Idle,
        FadingOut,
        FadingIn
    }

    private const string FallbackOverlayName = "SceneFadeOverlay";

    private static bool s_hasRequest;
    private static string s_targetScenePath = string.Empty;
    private static float s_duration = 0.6f;
    private static bool s_allowAudioCrossfade = true;
    private static int s_overlaySignifierId;

    private static bool s_pendingFadeIn;
    private static float s_pendingFadeInDuration = 0.6f;
    private static int s_pendingFadeInOverlaySignifierId;

    private TransitionState _state = TransitionState.Idle;
    private float _timer;
    private float _activeDuration = 0.6f;
    private int _activeOverlaySignifierId;

    // Call from any gameplay/menu script to trigger a visual scene transition.
    public static void Request(string targetScenePath, float duration = 0.6f, bool allowAudioCrossfade = true, int overlaySignifierId = 0)
    {
        if (string.IsNullOrWhiteSpace(targetScenePath))
        {
            return;
        }

        s_hasRequest = true;
        s_targetScenePath = targetScenePath;
        s_duration = duration <= 0.01f ? 0.01f : duration;
        s_allowAudioCrossfade = allowAudioCrossfade;
        s_overlaySignifierId = overlaySignifierId;
    }

    protected override void OnUpdate()
    {
        if (s_pendingFadeIn && _state == TransitionState.Idle)
        {
            _state = TransitionState.FadingIn;
            _timer = 0.0f;
            _activeDuration = s_pendingFadeInDuration;
            _activeOverlaySignifierId = s_pendingFadeInOverlaySignifierId;
            s_pendingFadeIn = false;
        }

        if (_state == TransitionState.Idle && s_hasRequest)
        {
            _state = TransitionState.FadingOut;
            _timer = 0.0f;
            _activeDuration = s_duration;
            _activeOverlaySignifierId = s_overlaySignifierId;
            s_hasRequest = false;
        }

        if (_state == TransitionState.Idle)
        {
            return;
        }

        Entity overlay = FindOverlayEntity(_activeOverlaySignifierId);
        if (!overlay.IsAlive || !overlay.TryGetComponent<GUIElement>(out _) || !overlay.TryGetComponent<GUIPanel>(out _))
        {
            // If no overlay exists, perform a direct scene switch and keep audio fade support.
            if (_state == TransitionState.FadingOut)
            {
                PerformSceneSwap();
            }
            _state = TransitionState.Idle;
            return;
        }

        ref GUIElement overlayElement = ref overlay.GetComponent<GUIElement>();
        ref GUIPanel overlayPanel = ref overlay.GetComponent<GUIPanel>();

        overlayElement.Visible = true;
        _timer += Time.DeltaTime;

        float t = _activeDuration > 0.0001f ? _timer / _activeDuration : 1.0f;
        t = GMath.Clamp(t, 0.0f, 1.0f);

        if (_state == TransitionState.FadingOut)
        {
            SetPanelAlpha(ref overlayPanel, t);
            if (t >= 1.0f)
            {
                PerformSceneSwap();
                _state = TransitionState.Idle;
            }
        }
        else if (_state == TransitionState.FadingIn)
        {
            SetPanelAlpha(ref overlayPanel, 1.0f - t);
            if (t >= 1.0f)
            {
                overlayElement.Visible = false;
                _state = TransitionState.Idle;
            }
        }
    }

    private static void SetPanelAlpha(ref GUIPanel panel, float alpha)
    {
        panel.Color = new Color(panel.Color.R, panel.Color.G, panel.Color.B, GMath.Clamp(alpha, 0.0f, 1.0f));
    }

    private Entity FindOverlayEntity(int signifierId)
    {
        Entity byName = default!;

        foreach (var candidate in World!.Query<GUIElement, GUIPanel>())
        {
            if (signifierId != 0)
            {
                if (candidate.Entity.TryGetComponent<MatchSignifierComponent>(out var match) && match.signifierID == signifierId)
                {
                    return candidate.Entity;
                }
            }

            if (candidate.Entity.TryGetComponent<Name>(out var name) && name.Value.ToString() == FallbackOverlayName)
            {
                byName = candidate.Entity;
            }
        }

        return byName;
    }

    private void PerformSceneSwap()
    {
        if (string.IsNullOrWhiteSpace(s_targetScenePath))
        {
            return;
        }

        SceneManager sceneManager = SceneManager.Instance;
        sceneManager.SetNextAudioTransition(_activeDuration, s_allowAudioCrossfade);

        ulong sceneIndex = sceneManager.AddScene();
        if (sceneManager.LoadScene(sceneIndex, s_targetScenePath))
        {
            sceneManager.SetActive(sceneIndex);
            s_pendingFadeIn = true;
            s_pendingFadeInDuration = _activeDuration;
            s_pendingFadeInOverlaySignifierId = _activeOverlaySignifierId;
        }
    }
}
