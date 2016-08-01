#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Monitoring.Contracts.Internals;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace System.Reactive.Contrib.Monitoring
{
    /// <summary>
    /// Candidate data before it is constructed into a marble
    /// </summary>
    public struct MarbleCandidate
    {
        public MarbleCandidate(string name, MarbleKind kind, string[] keywords)
        {
            _name = name;
            _kind = kind;
            _keywords = keywords;
        }

        private string _name;
        public string Name { get { return _name; } }
        private MarbleKind _kind;
        public MarbleKind Kind { get { return _kind; } }
        private string[] _keywords;
        public string[] Keywords { get { return _keywords; } }
    }
}