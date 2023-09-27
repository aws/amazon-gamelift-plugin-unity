﻿using System;
using Editor.Resources.EditorWindow;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class StatusBox : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<StatusBox> { }

        private readonly VisualElement _container;
        private Label _statusTextLabel;
        private bool ShowElement { get; set; }
        
        private const string  InactiveStatusBoxClassName = "hidden";
        private const string  SuccessBoxElementClass = "status-box--success";
        private const string  InfoBoxElementClass = "status-box--info";
        private const string  WarningBoxElementClass = "status-box--warning";
        private const string  ErrorBoxElementClass = "status-box--error";

        public StatusBox(StatusBoxType type)
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/StatusBox");
            asset.CloneTree(this);

            LocalizeText();

            _container = this.Q("StatusBox").Q("StatusBox");
            _statusTextLabel = _container.Q<Label>("StatusBoxLabel");
            switch (type)
            {
                case StatusBoxType.Success:
                    _container.AddToClassList(SuccessBoxElementClass);
                    break;
                case StatusBoxType.Info:
                    _container.AddToClassList(InfoBoxElementClass);
                    break;
                case StatusBoxType.Warning: 
                    _container.AddToClassList(WarningBoxElementClass);
                    break;
                case StatusBoxType.Error: 
                    _container.AddToClassList(ErrorBoxElementClass);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
            
            this.Q<Button>("StatusBoxCloseButton").RegisterCallback<ClickEvent>(_ =>
            {
                ShowElement = false;
                UpdateStatusBoxesState();
            });

            UpdateStatusBoxesState();
        }

        public StatusBox()
        {
        }

        public void AddExternalButton(string externalButtonLink, string externalButtonText)
        {
            var externalButton = this.Q<Button>("StatusBoxExternalButton");
            
            if (!string.IsNullOrWhiteSpace(externalButtonText))
            {
                externalButton.text = externalButtonText;
                externalButton.RegisterCallback<ClickEvent>(_ => Application.OpenURL(externalButtonLink));
            }
        }

        public void Show(string statusBoxText)
        {
            _statusTextLabel.text = statusBoxText;
            ShowElement = true;
            UpdateStatusBoxesState();
        }
        
        public void Close()
        {
            ShowElement = false;
            UpdateStatusBoxesState();
        }
        
        private void UpdateStatusBoxesState()
        {
            if (ShowElement)
            {
                _container.RemoveFromClassList(InactiveStatusBoxClassName);
            }
            else
            {
                _container.AddToClassList(InactiveStatusBoxClassName);
            }
        }

        public enum StatusBoxType
        {
            Success,
            Info,
            Warning,
            Error
        }
        
        private void LocalizeText()
        {
            var l = new ElementLocalizer(this);
            // l.SetElementText("DropdownLabel", Strings.ProfileSelectorDropdownLabel);
            // l.SetElementText("BucketNameLabel", Strings.ProfileSelectorBucketNameLabel);
            // l.SetElementText("RegionLabel", Strings.ProfileSelectorRegionLabel);
            // l.SetElementText("BootstrapStatusLabel", Strings.ProfileSelectorStatusLabel);
        }
    }
}