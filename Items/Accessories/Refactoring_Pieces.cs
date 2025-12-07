using Origins.Dev;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Refactoring_Pieces : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"ManaShielding"
		];
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 6));
			Item.ResearchUnlockCount = 1;
			glowmask = Origins.AddGlowMask(this);
		}
		static short glowmask;
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 32);
			Item.rare = ItemRarityID.Blue;
			Item.expert = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.glowMask = glowmask;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.refactoringPieces = true;
			originPlayer.manaShielding += 0.1f;
			player.endurance += (1 - player.endurance) * 0.1f;
		}
	}
}
