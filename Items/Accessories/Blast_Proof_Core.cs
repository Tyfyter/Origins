using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Front)]
	public class Blast_Proof_Core : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Vitality
		];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(40, 40);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 8);
			Item.defense = 2;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer oP = player.OriginPlayer();
			oP.pacemaker = true;
			oP.blastProofCore = true;
			oP.explosiveSelfDamage -= float.Lerp(0.2f, 0.95f, Utils.GetLerpValue(150, 0, player.statLife));
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Blast_Resistant_Plate>()
			.AddIngredient<Pacemaker>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
