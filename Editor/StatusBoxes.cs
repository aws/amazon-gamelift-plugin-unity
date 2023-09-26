using System.Collections.Generic;

namespace AmazonGameLift.Editor
{
    public class StatusBoxes
    {
        private readonly List<StatusBox> _allStatusBoxes = new();
        private const string  InactiveStatusBoxElementName = "status-box__hidden";

        public void AddStatusBoxElements(IEnumerable<StatusBox> statusBoxes)
        {
            _allStatusBoxes.AddRange(statusBoxes);
        }

        public void UpdateStatusBoxesState()
        {
            foreach (var statusBox in _allStatusBoxes)
            {
                if (statusBox.ShowElement)
                {
                    statusBox.BoxElement.AddToClassList(InactiveStatusBoxElementName);
                }
                else
                {
                    statusBox.BoxElement.RemoveFromClassList(InactiveStatusBoxElementName);
                }
            }
        }
    }
}