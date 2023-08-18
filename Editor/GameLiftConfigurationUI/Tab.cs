// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Editor.GameLiftConfigurationUI
{
    public abstract class Tab
    {
        protected List<VisualElement> AllFoldoutElements = new();
        protected VisualElement Root;
        private VisualElement _currentWizard;
        private List<Button> _allButtons = new();
        private List<VisualElement> _allWizardElements = new();

        //This is called upon selecting a different profile using the UIs dropdown
        public abstract void OnAccountSelect();
        
        
        protected void SetupTab(string tabName, EventCallback<ClickEvent,Button> onTabButtonClicked)
        {
            _allButtons = Root.Query<Button>(null, tabName+"Button").ToList();
            foreach (var button in _allButtons)
            {
                button.RegisterCallback(onTabButtonClicked, button);
            }
        
            _allWizardElements = Root.Query<VisualElement>(null, tabName).ToList();
            AllFoldoutElements = Root.Query<VisualElement>(null, tabName+"Foldout").ToList();
        
            ResetWizard();
        }

        protected void ChangeWizard(VisualElement targetWizard)
        {
            if (_currentWizard != null)
            {
                _currentWizard.style.display = DisplayStyle.None;
            }

            _currentWizard = targetWizard;
            if (_currentWizard != null)
            {
                _currentWizard.style.display = DisplayStyle.Flex;
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

        protected void ToggleButtons(Button button, bool state)
        {
            button.SetEnabled(state);
        }
        
        protected VisualElement GetWizard(string wizardName)
        {
            return _allWizardElements.FirstOrDefault(element => element.name == wizardName);
        }
    
        protected VisualElement GetFoldout(string foldoutName)
        {
            return AllFoldoutElements.FirstOrDefault(element => element.name == foldoutName);
        }

        protected void EnableInfoBox(string infoBoxClass)
        {
            Root.Q<VisualElement>(null, infoBoxClass).style.display = DisplayStyle.Flex;
        }
        
        private void ResetWizard()
        {
            foreach (var wizardElement in _allWizardElements.Where(wizardElement => wizardElement != null))
            {
                wizardElement.style.display = DisplayStyle.None;
            }

            if (_allWizardElements.Count > 0)
            {
                ChangeWizard(_allWizardElements[0]);
            }
        }
    }
}