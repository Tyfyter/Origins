using Origins.Journal;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;

namespace Origins.Items.Other.Consumables.Broths {
	public class Hearty_Broth : BrothBase, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Hearty_Broth_Entry).Name;
		public class Hearty_Broth_Entry : JournalEntry {
			public override string TextKey => "Hearty_Broth";
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new Color(140, 0, 0),
				new Color(165, 61, 0),
				new Color(184, 129, 0)
			];
		}
		public override void PreUpdateMinion(Projectile minion) {
			minion.GetGlobalProjectile<MinionGlobalProjectile>().tempBonusUpdates += minion.MaxUpdates * 0.1f;
			if (minion.TryGetGlobalProjectile(out ArtifactMinionGlobalProjectile artifact)) artifact.maxHealthModifier += 0.2f;
		}
	}
}
