using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Catalytic_Converter : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Vitality
		];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(42, 44);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 8);
			Item.defense = 2;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer oP = player.OriginPlayer();
			oP.pacemaker = true;
			oP.blastProofCore = true;
			oP.explosiveSelfDamage -= float.Lerp(0.2f, 0.95f, Utils.GetLerpValue(150, 0, player.statLife));
			player.lifeRegen += 1;
			oP.bombCharminIt = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Blast_Proof_Core>()
			.AddIngredient<Bomb_Charm_of_Regeneration>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
