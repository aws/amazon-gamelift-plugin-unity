// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using AmazonGameLift.Editor;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow
{
    public class ElementLocalizer
    {
        private readonly VisualElement _root;
        private readonly TextProvider _textProvider = TextProviderFactory.Create();

        public ElementLocalizer(VisualElement root)
        {
            _root = root;
        }

        public void SetElementText(string elementName, string textKey)
        {
            var element = _root.Q<TextElement>(elementName);
            if (element != null)
            {
                element.text = _textProvider.Get(textKey);
            }
            else
            {
                var foldout = _root.Q<Foldout>(elementName);
                if (foldout != null)
                {
                    foldout.text = _textProvider.Get(textKey);
                }
            }
        }
    }
}