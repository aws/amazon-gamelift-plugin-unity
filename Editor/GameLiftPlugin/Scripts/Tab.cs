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
    
    protected VisualElement GetFoldout(string foldoutName)
    {
        return AllFoldoutElements.FirstOrDefault(element => element.name == foldoutName);
    }


    protected void EnableInfoBox(string infoBoxClass)
    {
        Root.Q<VisualElement>(null, infoBoxClass).style.display = DisplayStyle.Flex;
    }

    public enum InfoType
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }
}