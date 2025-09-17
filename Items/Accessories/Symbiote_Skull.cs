using Origins.Buffs;
using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Symbiote_Skull : ModItem, ICustomWikiStat, IJournalEntrySource, ITornSource {
		public static float TornSeverity => 0.1f;
		float ITornSource.Severity => TornSeverity;
		public string[] Categories => [
			"Combat",
			"Torn",
			"TornSource"
		];
        static short glowmask;
		public string EntryName => "Origins/" + typeof(Symbiote_Skull_Entry).Name;
		public class Symbiote_Skull_Entry : JournalEntry {
			public override string TextKey => "Symbiote_Skull";
			public override JournalSortIndex SortIndex => new("Riven", 4);
		}
		public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
        }
        public override void SetDefaults() {
			Item.DefaultToAccessory(22, 28);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
            Item.glowMask = glowmask;
        }
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().symbioteSkull = true;
		}
	}
}
