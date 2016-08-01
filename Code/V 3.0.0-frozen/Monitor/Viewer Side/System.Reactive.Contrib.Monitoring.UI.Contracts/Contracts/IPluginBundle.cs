#region Using

using System;
using System.Windows.Media;
using System.ComponentModel.Composition;
using System.Collections.Generic;

#endregion // Using

// TODO: custom configuration command

namespace System.Reactive.Contrib.Monitoring.UI.Contracts
{
    /// <summary>
    /// Plug-in bundle contract
    /// </summary>
    [InheritedExport]
    public interface IPluginBundle
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the plug-in product version.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Gets the publisher.
        /// </summary>
        string PublisherName { get; }

        /// <summary>
        /// Gets the publisher email.
        /// </summary>
        /// <value>
        /// The publisher email.
        /// </value>
        string PublisherEmail { get; }

        /// <summary>
        /// Gets the plug-in support URL.
        /// </summary>
        string SupportUrl { get; }

        /// <summary>
        /// Gets the plug-in product URL.
        /// </summary>
        string ProductUrl { get; }

        /// <summary>
        /// Gets the plug-in's icon.
        /// </summary>
        ImageSource Icon { get; }

        /// <summary>
        /// Gets a value indicating whether this plug--in is having a custom configuration.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has custom configuration; otherwise, <c>false</c>.
        /// </value>
        bool HasCustomConfiguration { get; }

        /// <summary>
        /// Edits the configuration (also responsible to handle the configuration persistence).
        /// </summary>
        void EditConfiguration();

        /// <summary>
        /// Gets the interception plug-ins.
        /// use this plug-in for non-UI interception
        /// </summary>
        IItemInterceptionPlugin InterceptionPlugin { get; }

        /// <summary>
        /// Gets the diagram image mapping plug-ins.
        /// used to replace the diagram's image.
        /// </summary>
        ILineHeaderImagePlugin LineHeaderImagePlugin { get; }

        /// <summary>
        /// Gets the marble diagram template selector plug-in.
        /// use this option in order to replace or manipulate the marble-diagram template. 
        /// </summary>
        IMarblePanelPlugin MarblePanelPlugin { get; }

        /// <summary>
        /// Gets the marble template plug-in.
        /// use this option in order to replace a marble template. 
        /// </summary>
        IMarbleItemPlugin MarbleItemPlugin { get; }

        /// <summary>
        /// Gets the load resources plug-in.
        /// use this plug-in in order to load additional resources.  
        /// </summary>
        ILoadResourcesPlugin LoadResourcesPlugin { get; }

        /// <summary>
        /// Gets the tab template plug-in.
        /// use this plug-in in order of adding a new tabs.
        /// </summary>
        ITabPlugin TabPlugin { get; }

    }
}