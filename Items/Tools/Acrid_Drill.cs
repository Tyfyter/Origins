using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Acrid_Drill : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Drill");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.TitaniumDrill);
			item.damage = 28;
            item.pick = 195;
			item.width = 34;
			item.height = 32;
			item.knockBack*=2f;
            item.shootSpeed = 56f;
            item.shoot = ModContent.ProjectileType<Acrid_Drill_P>();
			item.value = 3600;
			item.rare = ItemRarityID.LightRed;
		}
        public override float UseTimeMultiplier(Player player) {
            return player.wet?1.5f:1;
        }
	}
    public class Acrid_Drill_P : ModProjectile {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Drill");
			Main.projFrames[projectile.type] = 2;
		}
		public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.TitaniumDrill);
		}
        public override void AI() {
            if(Main.player[projectile.owner].wet)++projectile.frameCounter;
			if (++projectile.frameCounter > 4) {
				projectile.frameCounter = 0;
				if (++projectile.frame > 1) {
					projectile.frame = 0;
				}
			}
        }
    }
}
