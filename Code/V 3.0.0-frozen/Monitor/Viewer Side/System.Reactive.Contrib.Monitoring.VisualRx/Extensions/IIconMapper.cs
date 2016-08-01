using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace System.Reactive.Contrib.Monitoring.Contracts
{
    /// <summary>
    /// Map image to marble diagram name
    /// </summary>
    public interface IIconMapper
    {
        Uri GetImageUri(string marbleDiagramName);
    }
}