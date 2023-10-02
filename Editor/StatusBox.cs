using System;
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
        
        public void Close()
        {
            ShowElement = false;
            UpdateStatusBoxesState();
        }

        private void AddStatusBoxType(StatusBoxType statusBoxType)
        {
            switch (statusBoxType)
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
                    throw new ArgumentOutOfRangeException(nameof(statusBoxType));
            }
        }

        private void RemoveStatusBoxType()
        {
            switch (_currentType)
            {
                case StatusBoxType.Success:
                    _container.RemoveFromClassList(SuccessBoxElementClass);
                    break;
                case StatusBoxType.Info:
                    _container.RemoveFromClassList(InfoBoxElementClass);
                    break;
                case StatusBoxType.Warning: 
                    _container.RemoveFromClassList(WarningBoxElementClass);
                    break;
                case StatusBoxType.Error: 
                    _container.RemoveFromClassList(ErrorBoxElementClass);
                    break;
            }
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