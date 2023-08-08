using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Tab
{
    public List<Button> AllButtons = new();
    public List<VisualElement> AllWizardElements = new();
    public List<VisualElement> AllFoldoutElements = new();
    public List<Label> AllExternalLinks = new();
    public int TabNumber = 0;
    public VisualElement TabContainer;
    public VisualElement Root;
    public VisualElement CurrentWizard;

    protected void SetupTab(string tabName, EventCallback<ClickEvent,Button> onTabButtonClicked)
    {
        AllButtons = Root.Query<Button>(null, tabName+"Button").ToList();
        foreach (var button in AllButtons)
        {
            button.RegisterCallback(onTabButtonClicked, button);
        }
        
        AllWizardElements = Root.Query<VisualElement>(null, tabName).ToList();
        AllFoldoutElements = Root.Query<VisualElement>(null, tabName+"Foldout").ToList();
        
        ResetWizard();
    }

    


    protected void ChangeWizard(VisualElement targetWizard)
    {
        if (CurrentWizard != null)
        {
            CurrentWizard.style.display = DisplayStyle.None;
        }

        CurrentWizard = targetWizard;
        if (CurrentWizard != null)
        {
            CurrentWizard.style.display = DisplayStyle.Flex;
        }
    }

    protected void ChangeFoldout(VisualElement currentFoldout, VisualElement targetFoldout)
    {
        if (currentFoldout != null)
        {
            currentFoldout.style.display = DisplayStyle.None;
        }
        
        if (targetFoldout != null)
        {
            targetFoldout.style.display = DisplayStyle.Flex;
        }
    }

    protected void ResetWizard()
    {
        foreach (var wizardElement in AllWizardElements.Where(wizardElement => wizardElement != null))
        {
            wizardElement.style.display = DisplayStyle.None;
        }

        if (AllWizardElements.Count > 0)
        {
            ChangeWizard(AllWizardElements[0]);
        }
        
    }

    protected void ToggleButtons(Button button, bool state)
    {
        button.SetEnabled(state);
    }


    protected VisualElement GetWizard(string wizardName)
    {
        return AllWizardElements.FirstOrDefault(element => element.name == wizardName);
    }

    public VisualElement BuildInfoBox(InfoType infoType, string message, Button actionButton)
    {
        Color infoColour = Color.blue;
        Color warningColour = Color.yellow;
        Color errorColour = Color.red;
        Color usedColor = default;
        int iconType = 0;

        switch (infoType)
        {
            case InfoType.Info:
                usedColor = infoColour;
                iconType = (int)InfoType.Info;
                break;
            case InfoType.Warning:
                usedColor = warningColour;
                iconType = (int)InfoType.Warning;
                break;
            case InfoType.Error:
                usedColor = errorColour;
                iconType = (int)InfoType.Error;
                break;
        }
        

        var usedColorFaded = usedColor;
        usedColorFaded.a = 0.133f;
        var icon = new VisualElement();
        
        
        var infoBox = new GroupBox
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                backgroundColor = usedColorFaded,
                borderRightColor = infoColour,
                borderLeftColor = infoColour,
                borderBottomColor = infoColour,
                borderTopColor = infoColour
            }
        };
        infoBox.Add(icon);
        
        var textBox = new Label
        {
            text = message,
            style =
            {
                fontSize = 9,
                alignContent = Align.Center
            }
        };
        infoBox.Add(textBox);

        infoBox.Add(actionButton);
        
        var closeButton = new Button
        {
            text = "X",
            style =
            {
                backgroundColor = Color.clear,
                borderRightColor = Color.clear,
                borderLeftColor = Color.clear,
                borderBottomColor = Color.clear,
                borderTopColor = Color.clear
            }
        };
        closeButton.RegisterCallback<MouseUpEvent>(t =>
        {
            infoBox.style.display = DisplayStyle.None;
        });
        infoBox.Add(closeButton);
        return infoBox;
    }

    public enum InfoType
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }
}