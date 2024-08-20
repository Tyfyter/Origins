using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Tripod_Nip : ModItem, IJournalEntryItem, ICustomWikiStat {
		public string[] Categories => [
			"Misc",
			"LoreItem"
		];
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Whispers";
		public string EntryName => "Origins/" + typeof(Tripod_Nip_Entry).Name;
		
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.aggro -= 400;
			player.calmed = true;
		}
	}
}
public class Tripod_Nip_Entry : JournalEntry {
	public override string TextKey => "Tripod_Nip";
	public override ArmorShaderData TextShader => null;
}
