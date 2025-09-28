using Origins.Dev;
using Origins.Journal;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Eccentric_Stone : ModItem, IJournalEntrySource, ICustomWikiStat, IItemObtainabilityProvider {
		public string[] Categories => [
			"Combat",
			"LoreItem",
			"SummonBoostAcc"
		];
		public string EntryName => "Origins/" + typeof(Eccentric_Stone_Entry).Name;
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 30);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.maxMinions += 1;
			Lighting.AddLight(player.MountedCenter - new Vector2(0, 6), 0.2f, 0.05f, 0.175f);
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (npc.active && npc.type == NPCID.DesertLamiaDark && npc.DistanceSQ(player.MountedCenter) < 160 * 160) {
					int prefix = Item.prefix;
					Item.SetDefaults(ModContent.ItemType<Spirit_Shard>());
					Item.Prefix(prefix);
					break;
				}
			}
		}
		public IEnumerable<int> ProvideItemObtainability() {
			yield return ModContent.ItemType<Spirit_Shard>();
		}
	}
	public class Eccentric_Stone_Entry : JournalEntry {
		public override string TextKey => "Eccentric_Stone";
	}
}
