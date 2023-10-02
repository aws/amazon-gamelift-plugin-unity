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

        private ElementLocalizer _elementLocalizer;
        private readonly Button _externalButton;

        public StatusBox()
        {
            var asset = Resources.Load<VisualTreeAsset>("EditorWindow/Components/StatusBox");
            asset.CloneTree(this);

            _container = this.Q("StatusBox");
            _statusTextLabel = _container.Q<Label>("StatusBoxLabel");
            _externalButton = this.Q<Button>("StatusBoxExternalButton");
            _elementLocalizer = new ElementLocalizer(_container);
            this.Q<Button>("StatusBoxCloseButton").RegisterCallback<ClickEvent>(_ =>
            {
                Close();
            });

            UpdateStatusBoxesState();
        }

        private void AddExternalButton(string externalButtonLink, string externalButtonText)
        {
            if (!string.IsNullOrWhiteSpace(externalButtonText))
            {
                _externalButton.text = externalButtonText;
                _externalButton.RegisterCallback<ClickEvent>(_ => Application.OpenURL(externalButtonLink));
                _externalButton.RemoveFromClassList(HiddenClassName);
            }
        }
        
        public void Show(StatusBoxType statusBoxType, string text, string externalButtonLink = "", string externalButtonText = "")
        {
            _container.RemoveFromClassList(_statusBoxClasses[_currentType]);
            
            _container.AddToClassList(_statusBoxClasses[statusBoxType]);
            _currentType = statusBoxType;
            
            _elementLocalizer.SetElementText(_statusTextLabel.name, text);
            
            if (externalButtonLink != "")
            {
                AddExternalButton(externalButtonLink, externalButtonText);
            }
            else
            {
                _externalButton.AddToClassList(HiddenClassName);
            }

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