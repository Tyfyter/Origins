using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Stack_of_Shraps : ModItem, ICustomWikiStat, IJournalEntryItem {
		public string[] Categories => [
			"Combat",
			"ExplosiveBoostAcc"
		];
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Whispers";
		public string EntryName => "Origins/" + typeof(Stack_of_Shraps_Entry).Name;
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 26);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
			Item.expert = true;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.madHand = true;
			originPlayer.explosiveBlastRadius += 0.15f;
			originPlayer.explosiveThrowSpeed += 0.2f;
		}
	}
	public class Stack_of_Shraps_Entry : JournalEntry {
		public override string TextKey => nameof(Stack_of_Shraps);
		public override ArmorShaderData TextShader => null;
	}
}
