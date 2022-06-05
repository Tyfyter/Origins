using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Felnum_Hamaxe : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Hamaxe");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.MoltenHamaxe);
			Item.damage = 18;
			Item.melee = true;
            Item.pick = 0;
            Item.hammer = 65;
            Item.axe = 22;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 13;
			Item.useAnimation = 25;
			Item.knockBack = 4f;
			Item.value = 3600;
			Item.UseSound = SoundID.Item1;
		}
        public override float UseTimeMultiplier(Player player) {
            return 1f / ((player.pickSpeed-1)*0.75f+1);
        }
#pragma warning disable CS0672 // Member overrides incorrectly obsolete member
        public override void GetWeaponDamage(Player player, ref int damage) {
            if(!OriginPlayer.ItemChecking)damage+=(damage-18)/2;
        }
#pragma warning restore CS0672 // Member overrides incorrectly obsolete member
	}
}
