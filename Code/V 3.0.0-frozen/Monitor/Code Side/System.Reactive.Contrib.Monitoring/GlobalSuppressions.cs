// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the
// Error List, point to "Suppress Message(s)", and click
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Contrib", Scope = "namespace", Target = "System.Reactive.Contrib.Monitoring")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Contrib")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Scope = "member", Target = "System.Reactive.Contrib.Monitoring.MonitorOperator`1.#.ctor(System.String,System.Func`3<!0,System.Reactive.Contrib.Monitoring.Contracts.MarbleBase,System.String>,System.String,System.Double,System.String[])")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "System.Reactive.Contrib.Monitoring.MonitorOperator`1.#WriteLog(System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "System.Reactive.Contrib.Monitoring.MonitorOperator`1.#Publish(System.Reactive.Contrib.Monitoring.Contracts.MarbleBase)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Scope = "member", Target = "System.Reactive.Contrib.Monitoring.ExtensionMethods.#Monitor`1(System.IObservable`1<!!0>,System.String,System.Func`3<!!0,System.Reactive.Contrib.Monitoring.Contracts.MarbleBase,System.String>,System.String,System.Double,System.String[])")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Scope = "type", Target = "System.Reactive.Contrib.Monitoring.MarbleQueueServiceAdapter")]