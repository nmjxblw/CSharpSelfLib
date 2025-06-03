namespace SpaceShared;

internal static class DiagnosticMessages
{
	public const string CopiedFromGameCode = "The code was copied from the decompiled game, so we should avoid refactoring to simplify future updates.";

	public const string IsUsedViaReflection = "The class or member is used via reflection.";

	public const string IsPublicApi = "The class is part of a mod's API, so we can't make breaking changes.";

	public const string NamedForHarmony = "The parameter names must match the convention defined by Harmony so it can find them.";

	public const string DisposableOutlivesScope = "The disposable object can't be disposed since it survives past the end of this scope.";
}
