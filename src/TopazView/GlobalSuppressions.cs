// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Design",
    "CA1003: Use generic event handler instances",
    Justification = "Simple event delegate with no unused argument is better.")]

[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Validation comes with a cost. Performance is more important.")]
[assembly: SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>", Scope = "type", Target = "~T:Tenray.TopazView.ViewFlags")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Dependency Injection is used.")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>", Scope = "member", Target = "~P:Tenray.TopazView.IPage.data")]
[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>", Scope = "member", Target = "~P:Tenray.TopazView.Page.data")]
