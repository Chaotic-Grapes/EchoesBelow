using EchoesBelow.Scripts;
using EchoesBelow.Scripts.Audio;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Events;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using Scripts.Menu;
using System.Collections.Generic;

namespace Scripts.Menu;

//This one is Old and Deprecated
[Component] public record struct PauseMenuComponent(bool isPauseable, int resumeSiginifier, int exitSignifier, bool start);
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class PauseMenu : SystemBase
{
    protected override void OnUpdate()
    {
        bool isKeyPressed_P = Input.IsKeyPressed(KeyCode.P);
        bool isKeyPressed_Space = Input.IsKeyPressed(KeyCode.Space);
        
        isKeyPressed_vertical = Input.IsKeyPressed(KeyCode.W) || Input.IsKeyPressed(KeyCode.A) || Input.IsKeyPressed(KeyCode.S) || Input.IsKeyPressed(KeyCode.D);

        foreach (var pauseController in World!.Query<PauseMenuComponent>())
        {
            bool start = pauseController.Component1.start;
            pauseController.Component1.start = OnStart(ref start, pauseController.Entity.Id);

            if (!pauseController.Component1.isPauseable) return;
        }

        if (isKeyPressed_P && !isPaused)
        {
            Player.instance.isEnabled = false;
            AudioManager.instance.PlaySFX("UI002");
            
            Time.TimeScale = 0;
            isPaused = true;
            foreach (Entity menuElement in pauseMenuElementObjIds)
            {
                if (!Entity.FromId(World!, menuElement.Id).HasComponent<GUIElement>()) continue;
                Entity.FromId(World!, menuElement.Id).GetComponent<GUIElement>().Visible = true;
                Log("Launch Pause Menuz");
            }
            //Launch Pause Menu

            UpdateEssentialKeys();

        }
        else if (isKeyPressed_P && isPaused)
        {
            Player.instance.isEnabled = true;
            AudioManager.instance.PlaySFX("UI001");
           
            Time.TimeScale = 1;
            isPaused = false;
            foreach (Entity menuElement in pauseMenuElementObjIds)
            {
                Entity.FromId(World!, menuElement.Id).GetComponent<GUIElement>().Visible = false;
            Log("return to game w p Press");
            }
            //Close Pause Menu
        }

        if (isPaused)
        {
            HandlePauseVolumeHotkeys();
            UpdatePauseAudioSliders();

            if (isKeyPressed_vertical)
            {
                AudioManager.instance.PlaySFX("UI005_Track01");
                isFirstSelected = !isFirstSelected;
                UpdateEssentialKeys();
            }



            if (isFirstSelected)
            {
                if (isKeyPressed_Space)
                {
                    Player.instance.isEnabled = true;
                    Log("Resume");
                    AudioManager.instance.PlaySFX("UI005_Track01");

                    Time.TimeScale = 1;
                    isPaused = false;
                    foreach (Entity menuElement in pauseMenuElementObjIds)
                    {
                        Entity.FromId(World!, menuElement.Id).GetComponent<GUIElement>().Visible = false;
                        Log("Paused Game from resume Press");
                    }
                }
            }
            else
            {
                if (isKeyPressed_Space)
                {
                    Log("Exit");
                    Time.TimeScale = 1;
                    SceneCrossFadeTransition.Request(TargetScenePath, 2.0f, true);
                }
            }
        }
    }
    private void UpdateEssentialKeys()
    {
        resumeButton_Lighter.GetComponent<GUIElement>().Visible = isFirstSelected;
        exitButton_Lighter.GetComponent<GUIElement>().Visible = !isFirstSelected;
    }
    #endregion
    private void CacheAudioSliderEntities()
    {
        hasAudioSliderRefs = false;

        foreach (Entity child in pauseMenuElementObjIds)
        {
            Entity entity = Entity.FromId(World!, child.Id);
            if (!entity.TryGetComponent<Name>(out var name))
            {
                continue;
            }

            string value = name.Value.ToString();
            if (value == "SFX_Bubble")
            {
                sfxBubbleEntity = entity;
            }
            else if (value == "SFX_Slider")
            {
                sfxSliderEntity = entity;
            }
            else if (value == "BGM_Bubble")
            {
                bgmBubbleEntity = entity;
            }
            else if (value == "BGM_Slider")
            {
                bgmSliderEntity = entity;
            }
        }

        // Fallback by known signifiers in case names are changed or duplicated.
        foreach (var tagged in World!.Query<MatchSignifierComponent>())
        {
            int id = tagged.Component1.signifierID;
            if (id == 33342)
            {
                sfxBubbleEntity = tagged.Entity;
            }
            else if (id == 33343)
            {
                sfxSliderEntity = tagged.Entity;
            }
            else if (id == 44452)
            {
                bgmBubbleEntity = tagged.Entity;
            }
            else if (id == 44453)
            {
                bgmSliderEntity = tagged.Entity;
            }
        }

        hasAudioSliderRefs = sfxBubbleEntity is { IsAlive: true }
            && sfxSliderEntity is { IsAlive: true }
            && bgmBubbleEntity is { IsAlive: true }
            && bgmSliderEntity is { IsAlive: true };
    }

    private static bool IsPointInRect(double mouseX, double mouseY, GUIElement element)
    {
        return mouseX >= element.Position.X
            && mouseX <= element.Position.X + element.Size.X
            && mouseY >= element.Position.Y
            && mouseY <= element.Position.Y + element.Size.Y;
    }

    private static float Clamp01(float value)
    {
        if (value < 0.0f) return 0.0f;
        if (value > 1.0f) return 1.0f;
        return value;
    }

    private static float ComputeRangeT(Vector2 start, Vector2 end, double mouseX, double mouseY)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float lenSq = (dx * dx) + (dy * dy);
        if (lenSq <= 0.0001f)
        {
            return 0.0f;
        }

        float px = (float)mouseX - start.X;
        float py = (float)mouseY - start.Y;
        float dot = (px * dx) + (py * dy);
        return Clamp01(dot / lenSq);
    }

    private static float ComputeAxisPos(Vector2 start, Vector2 end, double mouseX, double mouseY)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float len = (float)System.Math.Sqrt((dx * dx) + (dy * dy));
        if (len <= 0.0001f)
        {
            return 0.0f;
        }

        float ux = dx / len;
        float uy = dy / len;
        float px = (float)mouseX - start.X;
        float py = (float)mouseY - start.Y;
        return (px * ux) + (py * uy);
    }

    private static bool IsPointNearSegment(Vector2 start, Vector2 end, double mouseX, double mouseY, float radius)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float lenSq = (dx * dx) + (dy * dy);
        if (lenSq <= 0.0001f)
        {
            return false;
        }

        float px = (float)mouseX - start.X;
        float py = (float)mouseY - start.Y;
        float t = Clamp01(((px * dx) + (py * dy)) / lenSq);
        float cx = start.X + dx * t;
        float cy = start.Y + dy * t;
        float ox = (float)mouseX - cx;
        float oy = (float)mouseY - cy;
        return (ox * ox) + (oy * oy) <= radius * radius;
    }

    private static void ApplyBubbleFromT(Entity bubbleEntity, Vector2 start, Vector2 end, float t)
    {
        ref GUIElement bubbleElement = ref bubbleEntity.GetComponent<GUIElement>();
        bubbleElement.Position = new Vector2(
            start.X + (end.X - start.X) * t,
            start.Y + (end.Y - start.Y) * t
        );
    }

    private static void UpdateDebugText(Entity entity, string label, float percent)
    {
        if (!entity.TryGetComponent<GUIText>(out _))
        {
            return;
        }

        ref GUIText bubbleText = ref entity.GetComponent<GUIText>();
        int rounded = (int)(percent + 0.5f);
        if (rounded < 0) rounded = 0;
        if (rounded > 100) rounded = 100;
        bubbleText.TextId = Strings.Intern($"{label} {rounded}");
    }

    private static bool IsEntityHeld(Entity entity)
    {
        if (!entity.TryGetComponent<GUIInput>(out _))
        {
            return false;
        }

        ref GUIInput input = ref entity.GetComponent<GUIInput>();
        return input.Pressed || input.Dragging;
    }

    private void UpdatePauseAudioSliders()
    {
        if (!hasAudioSliderRefs)
        {
            return;
        }

        bool mousePressed = Input.IsMousePressed(MouseButton.Left);
        double mouseX = Input.MouseX;
        double mouseY = Input.MouseY;

        ref GUIElement sfxBubble = ref sfxBubbleEntity!.GetComponent<GUIElement>();
        ref GUIElement sfxSlider = ref sfxSliderEntity!.GetComponent<GUIElement>();
        ref GUIElement bgmBubble = ref bgmBubbleEntity!.GetComponent<GUIElement>();
        ref GUIElement bgmSlider = ref bgmSliderEntity!.GetComponent<GUIElement>();

        // Primary path: true hold/drag from GUIInput state.
        bool sfxHeld = IsEntityHeld(sfxBubbleEntity!) || IsEntityHeld(sfxSliderEntity!);
        bool bgmHeld = IsEntityHeld(bgmBubbleEntity!) || IsEntityHeld(bgmSliderEntity!);

        // Clicking track/empty area should set slider value immediately, then dragging should continue.
        bool sfxClicked = mousePressed && (
            IsPointInRect(mouseX, mouseY, sfxBubble)
            || IsPointInRect(mouseX, mouseY, sfxSlider)
            || IsPointNearSegment(SfxRangeStart, SfxRangeEnd, mouseX, mouseY, SliderTrackHitRadius)
        );
        bool bgmClicked = mousePressed && (
            IsPointInRect(mouseX, mouseY, bgmBubble)
            || IsPointInRect(mouseX, mouseY, bgmSlider)
            || IsPointNearSegment(BgmRangeStart, BgmRangeEnd, mouseX, mouseY, SliderTrackHitRadius)
        );

        if (sfxClicked)
        {
            isDraggingSfx = true;
            sfxLastAxisPos = ComputeAxisPos(SfxRangeStart, SfxRangeEnd, mouseX, mouseY);
        }
        else if (!sfxHeld)
        {
            isDraggingSfx = false;
        }

        if (bgmClicked)
        {
            isDraggingBgm = true;
            bgmLastAxisPos = ComputeAxisPos(BgmRangeStart, BgmRangeEnd, mouseX, mouseY);
        }
        else if (!bgmHeld)
        {
            isDraggingBgm = false;
        }

        float sfxVolume = cachedSfxVolume;
        float bgmVolume = cachedBgmVolume;
        bool hasAudioManager = false;

        foreach (var audioManager in World!.Query<AudioManagerComponent>())
        {
            hasAudioManager = true;
            sfxVolume = audioManager.Component1.globalSFXVolume;
            bgmVolume = audioManager.Component1.globalBGMVolume;

            if (sfxClicked)
            {
                sfxVolume = ComputeRangeT(SfxRangeStart, SfxRangeEnd, mouseX, mouseY) * 100.0f;
                audioManager.Component1.globalSFXVolume = sfxVolume;
            }
            else if (isDraggingSfx && sfxHeld)
            {
                float axisLen = (float)System.Math.Sqrt(
                    (SfxRangeEnd.X - SfxRangeStart.X) * (SfxRangeEnd.X - SfxRangeStart.X)
                    + (SfxRangeEnd.Y - SfxRangeStart.Y) * (SfxRangeEnd.Y - SfxRangeStart.Y)
                );
                if (axisLen > 0.0001f)
                {
                    float axisNow = ComputeAxisPos(SfxRangeStart, SfxRangeEnd, mouseX, mouseY);
                    float deltaPercent = ((axisNow - sfxLastAxisPos) / axisLen) * 100.0f;
                    sfxLastAxisPos = axisNow;
                    sfxVolume = GMath.Clamp(sfxVolume + deltaPercent, 0.0f, 100.0f);
                    audioManager.Component1.globalSFXVolume = sfxVolume;
                }
            }

            if (bgmClicked)
            {
                bgmVolume = ComputeRangeT(BgmRangeStart, BgmRangeEnd, mouseX, mouseY) * 100.0f;
                audioManager.Component1.globalBGMVolume = bgmVolume;
            }
            else if (isDraggingBgm && bgmHeld)
            {
                float axisLen = (float)System.Math.Sqrt(
                    (BgmRangeEnd.X - BgmRangeStart.X) * (BgmRangeEnd.X - BgmRangeStart.X)
                    + (BgmRangeEnd.Y - BgmRangeStart.Y) * (BgmRangeEnd.Y - BgmRangeStart.Y)
                );
                if (axisLen > 0.0001f)
                {
                    float axisNow = ComputeAxisPos(BgmRangeStart, BgmRangeEnd, mouseX, mouseY);
                    float deltaPercent = ((axisNow - bgmLastAxisPos) / axisLen) * 100.0f;
                    bgmLastAxisPos = axisNow;
                    bgmVolume = GMath.Clamp(bgmVolume + deltaPercent, 0.0f, 100.0f);
                    audioManager.Component1.globalBGMVolume = bgmVolume;
                }
            }

            break;
        }

        if (!hasAudioManager)
        {
            if (sfxClicked)
            {
                sfxVolume = ComputeRangeT(SfxRangeStart, SfxRangeEnd, mouseX, mouseY) * 100.0f;
            }
            else if (isDraggingSfx && sfxHeld)
            {
                float axisLen = (float)System.Math.Sqrt(
                    (SfxRangeEnd.X - SfxRangeStart.X) * (SfxRangeEnd.X - SfxRangeStart.X)
                    + (SfxRangeEnd.Y - SfxRangeStart.Y) * (SfxRangeEnd.Y - SfxRangeStart.Y)
                );
                if (axisLen > 0.0001f)
                {
                    float axisNow = ComputeAxisPos(SfxRangeStart, SfxRangeEnd, mouseX, mouseY);
                    float deltaPercent = ((axisNow - sfxLastAxisPos) / axisLen) * 100.0f;
                    sfxLastAxisPos = axisNow;
                    sfxVolume = GMath.Clamp(sfxVolume + deltaPercent, 0.0f, 100.0f);
                }
            }

            if (bgmClicked)
            {
                bgmVolume = ComputeRangeT(BgmRangeStart, BgmRangeEnd, mouseX, mouseY) * 100.0f;
            }
            else if (isDraggingBgm && bgmHeld)
            {
                float axisLen = (float)System.Math.Sqrt(
                    (BgmRangeEnd.X - BgmRangeStart.X) * (BgmRangeEnd.X - BgmRangeStart.X)
                    + (BgmRangeEnd.Y - BgmRangeStart.Y) * (BgmRangeEnd.Y - BgmRangeStart.Y)
                );
                if (axisLen > 0.0001f)
                {
                    float axisNow = ComputeAxisPos(BgmRangeStart, BgmRangeEnd, mouseX, mouseY);
                    float deltaPercent = ((axisNow - bgmLastAxisPos) / axisLen) * 100.0f;
                    bgmLastAxisPos = axisNow;
                    bgmVolume = GMath.Clamp(bgmVolume + deltaPercent, 0.0f, 100.0f);
                }
            }
        }

        cachedSfxVolume = sfxVolume;
        cachedBgmVolume = bgmVolume;

        ApplyBubbleFromT(sfxBubbleEntity!, SfxRangeStart, SfxRangeEnd, Clamp01(sfxVolume / 100.0f));
        ApplyBubbleFromT(bgmBubbleEntity!, BgmRangeStart, BgmRangeEnd, Clamp01(bgmVolume / 100.0f));
        UpdateDebugText(sfxSliderEntity!, "SFX", sfxVolume);
        UpdateDebugText(bgmSliderEntity!, "BGM", bgmVolume);
    }

    private void HandlePauseVolumeHotkeys()
    {
        bool decSfx = Input.IsKeyDown(KeyCode.I);
        bool incSfx = Input.IsKeyDown(KeyCode.O);
        bool decBgm = Input.IsKeyDown(KeyCode.Y);
        bool incBgm = Input.IsKeyDown(KeyCode.U);

        if (!decSfx && !incSfx && !decBgm && !incBgm)
        {
            return;
        }

        foreach (var audioManager in World!.Query<AudioManagerComponent>())
        {
            float step = audioManager.Component1.volumeStep;
            if (step <= 0.0f)
            {
                step = 1.0f;
            }
            float smoothStep = step * (Time.UnscaledDeltaTime / VolumeHotkeyRepeatInterval);

            if (decSfx)
            {
                audioManager.Component1.globalSFXVolume = GMath.Clamp(audioManager.Component1.globalSFXVolume - smoothStep, 0.0f, 100.0f);
            }
            if (incSfx)
            {
                audioManager.Component1.globalSFXVolume = GMath.Clamp(audioManager.Component1.globalSFXVolume + smoothStep, 0.0f, 100.0f);
            }
            if (decBgm)
            {
                audioManager.Component1.globalBGMVolume = GMath.Clamp(audioManager.Component1.globalBGMVolume - smoothStep, 0.0f, 100.0f);
            }
            if (incBgm)
            {
                audioManager.Component1.globalBGMVolume = GMath.Clamp(audioManager.Component1.globalBGMVolume + smoothStep, 0.0f, 100.0f);
            }

            break;
        }
    }
    //Pause Menu is off by default

}
