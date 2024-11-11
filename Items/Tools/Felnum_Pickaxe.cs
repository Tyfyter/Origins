using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Felnum_Pickaxe : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Tool"
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.DeathbringerPickaxe);
			Item.damage = 13;
			Item.DamageType = DamageClass.Melee;
			Item.pick = 75;
			Item.width = 36;
			Item.height = 24;
			Item.useTime = 13;
			Item.useAnimation = 22;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 40);
			Item.UseSound = SoundID.Item1;
			Item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes() {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Felnum_Bar>(), 20)
            .AddTile(TileID.Anvils)
            .Register();
        }
        public override float UseTimeMultiplier(Player player) {
			return (player.pickSpeed - 1) * 0.75f + 1;
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage = damage.Scale(1.5f);
		}
	}
}
