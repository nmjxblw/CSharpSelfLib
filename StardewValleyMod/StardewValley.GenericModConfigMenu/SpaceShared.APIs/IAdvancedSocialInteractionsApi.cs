using System;

namespace SpaceShared.APIs;

public interface IAdvancedSocialInteractionsApi
{
	event EventHandler<Action<string, Action>> AdvancedInteractionStarted;
}
