using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Felnum_Hamaxe : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Hamaxe");
			Tooltip.SetDefault("'Its strikes sound like thunder'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MoltenHamaxe);
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.pick = 0;
			Item.hammer = 65;
			Item.axe = 22;
			Item.width = 42;
			Item.height = 38;
			Item.useTime = 13;
			Item.useAnimation = 25;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 40);
			Item.UseSound = SoundID.Item1;
			Item.rare = ItemRarityID.Green;
		}
		public override float UseTimeMultiplier(Player player) {
			return 1f / ((player.pickSpeed - 1) * 0.75f + 1);
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage = damage.Scale(1.5f);
		}
	}
}
