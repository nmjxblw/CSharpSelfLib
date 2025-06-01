namespace MonoGame.Framework.Utilities.Deflate;

internal enum BlockState
{
	NeedMore,
	BlockDone,
	FinishStarted,
	FinishDone
}
