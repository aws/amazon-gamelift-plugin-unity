// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AmazonGameLift.Editor
{
    internal abstract class Setting
    {
        public string Title { get; }

        public string PrimaryActionMessage { get; protected set; }

        public string PrimaryActionDisabledTooltip { get; }

        public string Tooltip { get; }

        public virtual bool IsConfigured { get; private set; }

        public virtual bool IsPrimaryActionEnabled { get; protected set; } = true;

        protected Setting(string title, string primaryActionMessage, string tooltip, string primaryActionDisabledTooltip = null)
        {
            Title = title;
            PrimaryActionMessage = primaryActionMessage;
            Tooltip = tooltip;
            PrimaryActionDisabledTooltip = primaryActionDisabledTooltip;
        }

        /// <summary>
        /// Run any action needed by clicking the primary setting button.
        /// </summary>
        internal abstract void RunPrimaryAction();

        internal virtual void Refresh()
        {
            IsConfigured = RefreshIsConfigured();
        }

        protected abstract bool RefreshIsConfigured();
    }
}
