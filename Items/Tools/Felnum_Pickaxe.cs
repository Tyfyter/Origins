using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Tools {
	public class Felnum_Pickaxe : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Pickaxe");
			Tooltip.SetDefault("Able to mine Hellstone");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.DeathbringerPickaxe);
			Item.damage = 13;
			Item.DamageType = DamageClass.Melee;
            Item.pick = 75;
			Item.width = 34;
			Item.height = 32;
            Item.useTime = 13;
			Item.useAnimation = 22;
			Item.knockBack = 4f;
			Item.value = 3600;
			Item.UseSound = SoundID.Item1;
		}
        public override float UseTimeMultiplier(Player player) {
            return 1f / ((player.pickSpeed-1)*0.75f+1);
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage = damage.MultiplyBonuses(1.5f);
		}
	}
}
