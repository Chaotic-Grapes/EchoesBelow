using EchoesBelow.Scripts.Audio;
using GrapeEngine.Math;
using GrapeEngine.Scripting.Components;
using GrapeEngine.Scripting.Core;
using GrapeEngine.Scripting.Services;
using GrapeEngine.Scripting.Systems;
using GrapeEngine.Scripting.Systems.Attributes;
using Scripts;
using Scripts.Menu;
using System;
using System.Collections.Generic;

namespace EchoesBelow.Scripts;

[Component] public record struct TutorialControllerComponent(
    bool start, 
    bool awake
    );
[RequireForUpdate<TutorialControllerComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class TutorialController : SystemBase
{
    public static TutorialController instance { get; private set; }

    public static List<Entity> tutorialpageList { get; private set; }
    public static Dictionary<string, TutorialPanel> tutorialPageDict { get; private set; }


    //Tutorial centric
    public bool isTutoring = false;

    public static bool gateSpongeTicked;
    public static bool gateSpongeComplete;

    public static bool barrelSpongeTicked;
    public static bool barrelSpongeComplete;

    public static bool spongeButtonTicked;
    public static bool spongeButtonComplete;

    public static bool lexicateTicked;
    public static bool lexicateComplete;

    public static bool coralBuilder01Ticked;
    public static bool coralBuilder02Ticked;
    public static bool coralBuilder03Ticked;
    public static bool coralBuilder04Ticked;
    public static bool coralBuilder05Ticked;
    public static bool coralBuilder06Ticked;

    public static bool coralBuilderConfirmComplete;
    public static bool coralBuilderNavigateComplete;
    public static bool coralBuilderEntryComplete;

    public static bool marineSnow01Ticked;
    public static bool marineSnow02Ticked;
    public static bool marineSnow03Ticked;
    public static bool marineSnowComplete;

    public static bool spiritTicked;
    public static bool spiritComplete;

    //Names of specific tutorials
    const string gateSponge = "GateSponge_Tutorial";
    const string barrelSponge = "BarrelSponge_Tutorial";
    const string spongeButton = "SpongeButton_Tutorial";
    const string lexicate = "Lexicate_Tutorial";
    const string spiritOfTheOcean = "SpiritOfTheOcean_Tutorial";

    const string coralBuilder01 = "CoralBuilder_Tutorial01";
    const string coralBuilder02 = "CoralBuilder_Tutorial02";
    const string coralBuilder03 = "CoralBuilder_Tutorial03";
    const string coralBuilder04 = "CoralBuilder_Tutorial04";
    const string coralBuilder05 = "CoralBuilder_Tutorial05";
    const string coralBuilder06 = "CoralBuilder_Tutorial06";

    const string marineSnow01 = "MarineSnow_Tutorial01";
    const string marineSnow02 = "MarineSnow_Tutorial02";
    const string marineSnow03 = "MarineSnow_Tutorial03";



    private bool OnAwake(ref bool awakeBool, ulong objId) //Onawake must only play once at the beginning per script.
    {
        if (awakeBool == true) return true;
        awakeBool = true;

        instance = this;

        gateSpongeTicked = false;
        barrelSpongeTicked = false;
        spongeButtonTicked = false;
        lexicateTicked = false;

        coralBuilder01Ticked = false;
        coralBuilder02Ticked = false;
        coralBuilder03Ticked = false;
        coralBuilder04Ticked = false;
        coralBuilder05Ticked = false;
        coralBuilder06Ticked = false;

        marineSnow01Ticked = false;
        marineSnow02Ticked = false;
        marineSnow03Ticked = false;

        spiritTicked = false;

        gateSpongeComplete = false;
        barrelSpongeComplete = false;
        spongeButtonComplete = false;
        lexicateComplete = false;

        coralBuilderConfirmComplete = false;
        coralBuilderEntryComplete = false;
        coralBuilderNavigateComplete = false;

        marineSnowComplete = false;
        spiritComplete = false;

        isTutoring = false;

        tutorialPageDict = new Dictionary<string, TutorialPanel>();

        tutorialpageList = new List<Entity>();

        foreach (Entity child in Entity.FromId(World!, objId).GetChildren())
        {
            //disable on awake
            tutorialpageList.Add(child);
            ref GUIElement gui = ref child.GetComponent<GUIElement>();
            gui.Visible = false;
        }

        return true;
    }
    private bool OnStart(ref bool startBool)
    {
        if (startBool == true) return true;
        startBool = true;
        //Todo

        foreach (Entity panelEntity in tutorialpageList)
        {
            string name = panelEntity.GetComponent<Name>().Value.ToString();
            TutorialPanel tutPanel = new TutorialPanel(panelEntity.Id, name, panelEntity);

            tutorialPageDict.Add(name, tutPanel);

        }

        //End of Start
        return true;
    }
    protected override void OnUpdate()
    {
        foreach (var gameObject in World!.Query<TutorialControllerComponent>())
        {
            bool awake = gameObject.Component1.awake;
            gameObject.Component1.awake = OnAwake(ref awake, gameObject.Entity.Id);
        }

        foreach (var gameObject in World!.Query<TutorialControllerComponent>())
        {
            bool start = gameObject.Component1.start;
            gameObject.Component1.start = OnStart(ref start);

            bool isRMBPressed = Input.IsMousePressed(1) || Input.IsGamepadButtonPressed(0,GamepadButton.A);

            //GateSponge===============================================================================================
            if (gateSpongeTicked && isRMBPressed && !gateSpongeComplete)
            {
                DisplayTutorial(gateSponge, false);
                gateSpongeComplete = true;
                Time.TimeScale = 1f;
                PauseMenuController.instance.isPausable = true;
            }

            //BarrelSponge=============================================================================================
            if (barrelSpongeTicked && isRMBPressed && !barrelSpongeComplete)
            {
                DisplayTutorial(barrelSponge, false);
                barrelSpongeComplete = true;
                Time.TimeScale = 1f;
                PauseMenuController.instance.isPausable = true;
            }

            //spongeButton=============================================================================================
            if (spongeButtonTicked && isRMBPressed && !spongeButtonComplete)
            {
                DisplayTutorial(spongeButton, false);
                spongeButtonComplete = true;
                Time.TimeScale = 1f;
                PauseMenuController.instance.isPausable = true;
            }

            //Lexicate=================================================================================================
            if (lexicateTicked && isRMBPressed && !lexicateComplete)
            {
                DisplayTutorial(lexicate, false);
                lexicateComplete = true;
                Time.TimeScale = 1f;
                PauseMenuController.instance.isPausable = true;
            }

            //MarineSnow===============================================================================================
            if (marineSnow03Ticked && isRMBPressed && !marineSnowComplete)
            {
                DisplayTutorial(marineSnow03, false);
                marineSnowComplete = true;
                Time.TimeScale = 1f;
                PauseMenuController.instance.isPausable = true;
            }
            else if (marineSnow02Ticked && isRMBPressed && !marineSnowComplete)
            {
                DisplayTutorial(marineSnow02, false);
                DisplayTutorial(marineSnow03, true);
                marineSnow03Ticked = true;
            }
            else if (marineSnow01Ticked && isRMBPressed && !marineSnowComplete)
            {
                DisplayTutorial(marineSnow01, false);
                DisplayTutorial(marineSnow02, true);
                marineSnow02Ticked = true;
            }

            //CraftAnemone================================================================================================
            if (coralBuilder06Ticked && isRMBPressed && !coralBuilderConfirmComplete)
            {
                DisplayTutorial(coralBuilder06, false);
                coralBuilderConfirmComplete = true;
                Time.TimeScale = 1f;
                PauseMenuController.instance.isPausable = true;
            }
            else if (coralBuilder05Ticked && isRMBPressed && !coralBuilderConfirmComplete)
            {
                DisplayTutorial(coralBuilder05, false);
                DisplayTutorial(coralBuilder06, true);
                coralBuilder06Ticked = true; 
            }

            //CraftAnemone================================================================================================
            if (coralBuilder04Ticked && isRMBPressed && !coralBuilderNavigateComplete)
            {
                DisplayTutorial(coralBuilder04, false);
                coralBuilderNavigateComplete = true;
                Time.TimeScale = 1f;
                PauseMenuController.instance.isPausable = true;
            }
            else if (coralBuilder03Ticked && isRMBPressed && !coralBuilderNavigateComplete)
            {
                DisplayTutorial(coralBuilder03, false);
                DisplayTutorial(coralBuilder04, true);
                coralBuilder04Ticked = true;
            }
            else if (coralBuilder02Ticked && isRMBPressed && !coralBuilderNavigateComplete)
            {
                DisplayTutorial(coralBuilder02, false);
                DisplayTutorial(coralBuilder03, true);
                coralBuilder03Ticked = true;
            }

            //CraftAnemone=================================================================================================
            if (coralBuilder01Ticked && isRMBPressed && !coralBuilderEntryComplete)
            {
                DisplayTutorial(coralBuilder01, false);
                coralBuilderEntryComplete = true;
                Time.TimeScale = 1f;
                PauseMenuController.instance.isPausable = true;
            }

            //Spirit Of the OCean==========================================================================================
            if (spiritTicked && isRMBPressed && !spiritComplete)
            {
                DisplayTutorial(spiritOfTheOcean , false);
                spiritComplete = true;
                Time.TimeScale = 1f;
                PauseMenuController.instance.isPausable = true;
            }

        }

        
    }
    //Tut specific
    public void EnableGateSponge()
    {
        foreach(var tutorial in World!.Query<TutorialBoolContainer01Component>())
        {
            if (tutorial.Component1.gateSpongeTut && !gateSpongeTicked)
            {
                DisplayTutorial(gateSponge, true);
                Log("Marinesnowticked: " + marineSnow01Ticked);
                gateSpongeTicked = true;

                tutorial.Component1.gateSpongeTut = false;
            }
        }
    }
    public void EnableBarrelSponge()
    {
        foreach (var tutorial in World!.Query<TutorialBoolContainer01Component>())
        {
            if (tutorial.Component1.barrelSpongeTut && !barrelSpongeTicked)
            {
                DisplayTutorial(barrelSponge, true);
                barrelSpongeTicked = true;

                tutorial.Component1.barrelSpongeTut = false;
            }
        }
    }
    public void EnableSpongeButton()
    {
        foreach (var tutorial in World!.Query<TutorialBoolContainer01Component>())
        {
            if (tutorial.Component1.spongeButtonTut && !spongeButtonTicked)
            {
                DisplayTutorial(spongeButton, true);
                spongeButtonTicked = true;

                tutorial.Component1.spongeButtonTut = false;
            }
        }
    }
    public void EnableLexicate()
    {
        foreach (var tutorial in World!.Query<TutorialBoolContainer01Component>())
        {
            if (tutorial.Component1.lexicateTut && !lexicateTicked)
            {
                DisplayTutorial(lexicate, true);
                lexicateTicked = true;

                tutorial.Component1.lexicateTut = false;
            }
        }
    }
    public void EnableCoralBuilderEntry()
    {
        foreach (var tutorial in World!.Query<TutorialBoolContainer02Component>())
        {
            if (tutorial.Component1.coralBuilderTut && !coralBuilder01Ticked)
            {
                DisplayTutorial(coralBuilder01, true);
                coralBuilder01Ticked = true;

            }
        }
    }

    public void EnableCoralBuilderNavigate()
    {
        foreach (var tutorial in World!.Query<TutorialBoolContainer02Component>())
        {
            if (tutorial.Component1.coralBuilderTut && !coralBuilder02Ticked)
            {
                DisplayTutorial(coralBuilder02, true);
                coralBuilder02Ticked = true;
            }
        }
    }

    public void EnableCoralBuilderConfirm()
    {
        foreach (var tutorial in World!.Query<TutorialBoolContainer02Component>())
        {
            if (tutorial.Component1.coralBuilderTut && !coralBuilder05Ticked)
            {
                DisplayTutorial(coralBuilder05, true);
                coralBuilder05Ticked = true;

                tutorial.Component1.coralBuilderTut = false;
            }
        }
    }

    public void EnableMarineSnow()
    {
        foreach (var tutorial in World!.Query<TutorialBoolContainer02Component>())
        {
            if (tutorial.Component1.marineSnowTut && !marineSnow01Ticked)
            {
                DisplayTutorial(marineSnow01, true);
                marineSnow01Ticked = true;

                tutorial.Component1.marineSnowTut = false;
            }
        }
    }
    public void EnableSpiritOfTheOcean()
    {
        foreach (var tutorial in World!.Query<TutorialBoolContainer02Component>())
        {
            if (tutorial.Component1.spiritOfTheOceanTut && !spiritTicked)
            {
                DisplayTutorial(spiritOfTheOcean, true);
                spiritTicked = true;

                tutorial.Component1.spiritOfTheOceanTut = false;
            }
        }
    }
    //Functions
    public void DisplayTutorial(string tutorialName, bool enable)
    {
        AudioManager.instance.PlaySFX("UI003");

        TutorialPanel selectedTutorial = tutorialPageDict[tutorialName];
        ref GUIElement guiElement = ref selectedTutorial.Entity.GetComponent<GUIElement>();

        //Always turn visible for all
        if (enable)
        {
            guiElement.Visible = true;

            PauseMenuController.instance.isPausable = false;
            Time.TimeScale = 0;

            foreach (var obj in World!.Query<MatchSignifierComponent>())
            {
                if (obj.Component1.signifierID == 666666 || obj.Component1.signifierID == 777777)
                {
                    ref GUIText guiText = ref obj.Entity.GetComponent<GUIText>();
                    guiText.Color = new Color(0f, 0f, 0f, 0f);
                }
            }
        }
        //when closing, this is where the two types deviate
        else
        {
            guiElement.Visible = false;
            foreach (var obj in World!.Query<MatchSignifierComponent>())
            {
                if (obj.Component1.signifierID == 666666 || obj.Component1.signifierID == 777777)
                {
                    ref GUIText guiText = ref obj.Entity.GetComponent<GUIText>();
                    guiText.Color = new Color(0f, 0f, 0f, 1f);
                }
            }
        }

        
    }

}

[Component]public record struct TutorialBoolContainer01Component(bool gateSpongeTut, bool barrelSpongeTut, bool spongeButtonTut, bool lexicateTut);
[RequireForUpdate<TutorialControllerComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]

public class TutorialBoolContainer01 : SystemBase
{

}
[Component] public record struct TutorialBoolContainer02Component(bool marineSnowTut, bool coralBuilderTut, bool spiritOfTheOceanTut);
[RequireForUpdate<TutorialControllerComponent>]
[System(SystemGroup.Update, SystemRunMode.PlayOnly)]
public class TutorialBoolContainer02 : SystemBase
{

}