namespace ReleaseTheArrow.Core
{
    /// A visible marker so a bug report can say exactly which build it's from — bump this
    /// string every time a new APK is built and sent, so a build mismatch (an old install, a
    /// stale APK) is immediately obvious on screen instead of something to guess at afterward.
    public static class BuildInfo
    {
        public const string Version = "2026-09-11-r4";
    }
}
