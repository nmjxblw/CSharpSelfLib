using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Items;

internal class MovieSnackSubject : BaseSubject
{
	private readonly MovieConcession Target;

	public MovieSnackSubject(GameHelper gameHelper, MovieConcession item)
		: base(gameHelper)
	{
		this.Target = item;
		base.Initialize(item.DisplayName, item.getDescription(), I18n.Type_Other());
	}

	public override IEnumerable<ICustomField> GetData()
	{
		MovieConcession item = this.Target;
		IModInfo fromMod = base.GameHelper.TryGetModFromStringId(item.Id);
		if (fromMod != null)
		{
			yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(fromMod.Manifest.Name));
		}
		MovieInvitation? obj = ((IEnumerable<MovieInvitation>)Game1.player.team.movieInvitations).FirstOrDefault((Func<MovieInvitation, bool>)((MovieInvitation p) => p.farmer == Game1.player));
		NPC date = ((obj != null) ? obj.invitedNPC : null);
		if (date != null)
		{
			string taste = MovieTheater.GetConcessionTasteForCharacter((Character)(object)date, item);
			yield return new GenericField(I18n.Item_MovieSnackPreference(), I18n.ForMovieTasteLabel(taste, ((Character)date).displayName));
		}
		yield return new GenericField(I18n.InternalId(), I18n.Item_InternalId_Summary(item.Id, item.QualifiedItemId));
	}

	public override IEnumerable<IDebugField> GetDebugFields()
	{
		foreach (IDebugField item in base.GetDebugFieldsFrom(this.Target))
		{
			yield return item;
		}
	}

	public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		this.Target.drawInMenu(spriteBatch, position, 1f, 1f, 1f, (StackDrawType)0, Color.White, false);
		return true;
	}
}
