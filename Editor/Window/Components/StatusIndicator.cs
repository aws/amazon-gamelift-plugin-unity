using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class StatusIndicator : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<StatusIndicator> { }

        private Label _label => this.Q<Label>();
        private VisualElement _imageContainer => this.Q<VisualElement>("image-container");

        private readonly Dictionary<State, string> _stateClassNames = new()
        {
            { State.Inactive, "status-indicator--inactive" },
            { State.InProgress, "status-indicator--in-progress" },
            { State.Success, "status-indicator--success" },
            { State.Failed, "status-indicator--failed" },
        };

        public StatusIndicator()
        {
            var uxml = Resources.Load<VisualTreeAsset>("EditorWindow/Components/StatusIndicator");
            uxml.CloneTree(this);
            AddToClassList("status-indicator");
            AddToClassList("separator");
            AddToClassList("separator--horizontal");
        }

        private void AddSpin()
        {
            schedule.Execute(() => { _imageContainer.AddToClassList("status-indicator--spin"); }).StartingIn(10);
        }

        public void Set(State state, string text)
        {
            Reset();
            _label.text = text;
            AddToClassList(_stateClassNames[state]);
            if (state == State.InProgress)
            {
                _imageContainer.RegisterCallback<TransitionEndEvent>(evt =>
                {
                    _imageContainer.RemoveFromClassList("status-indicator--spin");
                    if (state == State.InProgress)
                    {
                        AddSpin();
                    }
                });
                AddSpin();
            }
        }

        private void Reset()
        {
            foreach (var className in _stateClassNames.Values)
            {
                RemoveFromClassList(className);
            }
        }
    }

    public enum State
    {
        Inactive,
        InProgress,
        Success,
        Failed
    }
}