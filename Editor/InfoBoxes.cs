using System.Collections.Generic;

namespace AmazonGameLift.Editor
{
    public class InfoBoxes
    {
        private readonly List<InfoBox> _allInfoBoxes = new();
        private const string  InactiveInfoBoxElementName = "info-box__hidden";

        public void AddInfoBoxElements(IEnumerable<InfoBox> infoBoxes)
        {
            _allInfoBoxes.AddRange(infoBoxes);
        }

        public void UpdateInfoBoxesState()
        {
            foreach (var infoBox in _allInfoBoxes)
            {
                if (infoBox.ShowElement)
                {
                    infoBox.BoxElement.AddToClassList(InactiveInfoBoxElementName);
                }
                else
                {
                    infoBox.BoxElement.RemoveFromClassList(InactiveInfoBoxElementName);
                }
            }
        }
    }
}