using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Rebreather : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Misc"
		];
		public static int FaceSlot { get; private set; }
		public override void SetStaticDefaults() {
			FaceSlot = Item.faceSlot;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 22);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().rebreather = true;
		}
	}
}
