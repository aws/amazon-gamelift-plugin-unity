using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class StatusIndicator : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<StatusIndicator> { }

        private Label _label => this.Q<Label>();

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
            AddToClassList("separator");
            AddToClassList("separator--horizontal");
            AddToClassList("separator--centered");
        }

        public void Set(State state, string text)
        {
            Reset();
            _label.text = text;
            AddToClassList(_stateClassNames[state]);
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