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
            item.CloneDefaults(ItemID.MoltenHamaxe);
			item.damage = 18;
			item.melee = true;
            item.pick = 0;
            item.hammer = 65;
            item.axe = 22;
			item.width = 34;
			item.height = 34;
			item.useTime = 13;
			item.useAnimation = 25;
			item.knockBack = 4f;
			item.value = 3600;
			item.UseSound = SoundID.Item1;
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
