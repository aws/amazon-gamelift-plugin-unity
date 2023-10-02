using System;
using System.Collections.Generic;
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
        
        private const string  HiddenClassName = "hidden";
        private const string  SuccessBoxElementClass = "status-box--success";
        private const string  InfoBoxElementClass = "status-box--info";
        private const string  WarningBoxElementClass = "status-box--warning";
        private const string  ErrorBoxElementClass = "status-box--error";

        private StatusBoxType _currentType;

        private readonly IReadOnlyDictionary<StatusBoxType, string> _statusBoxClasses =
            new Dictionary<StatusBoxType, string>()
            {
                { StatusBoxType.Success, SuccessBoxElementClass },
                { StatusBoxType.Info, InfoBoxElementClass },
                { StatusBoxType.Warning, WarningBoxElementClass },
                { StatusBoxType.Error, ErrorBoxElementClass }
            };

        public StatusBox()
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/StatusBox");
            asset.CloneTree(this);

            _container = this.Q("StatusBox");
            _statusTextLabel = _container.Q<Label>("StatusBoxLabel");

            this.Q<Button>("StatusBoxCloseButton").RegisterCallback<ClickEvent>(_ =>
            {
                Close();
            });

            UpdateStatusBoxesState();
        }

        public void AddExternalButton(string externalButtonLink, string externalButtonText)
        {
            var externalButton = this.Q<Button>("StatusBoxExternalButton");
            
            if (!string.IsNullOrWhiteSpace(externalButtonText))
            {
                externalButton.text = externalButtonText;
                externalButton.RegisterCallback<ClickEvent>(_ => Application.OpenURL(externalButtonLink));
                externalButton.RemoveFromClassList(HiddenClassName);
            }
        }
        
        private void RemoveExternalButton()
        {
            var externalButton = this.Q<Button>("StatusBoxExternalButton");
            externalButton.AddToClassList(HiddenClassName);
        }

        public void Show(StatusBoxType statusBoxType)
        {
            RemoveExternalButton();
            RemoveStatusBoxType();
            AddStatusBoxType(statusBoxType);
            ShowElement = true;
            UpdateStatusBoxesState();
        }

        public void SetText(ElementLocalizer localizer, string textKey)
        {
            localizer.SetElementText(_statusTextLabel.name, textKey);
        }
        
        public void SetText(string text)
        {
            _statusTextLabel.text = text;
        }

        private void Close()
        {
            ShowElement = false;
            UpdateStatusBoxesState();
        }

        private void AddStatusBoxType(StatusBoxType statusBoxType)
        {
            _container.AddToClassList(_statusBoxClasses[statusBoxType]);
            _currentType = statusBoxType;
        }

        private void RemoveStatusBoxType()
        {
            _container.RemoveFromClassList(_statusBoxClasses[_currentType]);
        }
        
        private void UpdateStatusBoxesState()
        {
            if (ShowElement)
            {
                _container.RemoveFromClassList(HiddenClassName);
            }
            else
            {
                _container.AddToClassList(HiddenClassName);
            }
        }

        public enum StatusBoxType
        {
            Success,
            Info,
            Warning,
            Error
        }
    }
}