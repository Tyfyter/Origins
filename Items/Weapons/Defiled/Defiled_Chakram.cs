using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Defiled {
	public class Defiled_Chakram : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Defiled_Chakram");
			Tooltip.SetDefault("Very pointy");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.ThornChakram);
			item.damage = 25;
			item.width = 34;
			item.height = 34;
			item.useTime = 18;
			item.useAnimation = 18;
			//item.knockBack = 5;
            item.shoot = ModContent.ProjectileType<Defiled_Chakram_P>();
            item.shootSpeed = 9.75f;
			item.value = 5000;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
		}
        public override bool CanUseItem(Player player) {
            return player.ownedProjectileCounts[item.shoot]<=0;
        }
    }
    public class Defiled_Chakram_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Defiled/Defiled_Chakram";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Defiled_Chakram");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.ThornChakram);
            projectile.penetrate = -1;
			projectile.width = 34;
			projectile.height = 34;
            projectile.localAI[0] = 1;
            //projectile.scale*=1.25f;
        }
        public override bool PreAI() {
            projectile.aiStyle = 3;
            projectile.velocity = projectile.velocity.RotatedBy(projectile.localAI[0]*0.15f);
            projectile.localAI[0] = (float)System.Math.Sin(projectile.timeLeft);
            return true;
        }
        public override bool? CanHitNPC(NPC target) {
            projectile.aiStyle = 0;
            return null;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
            width = 27;
            height = 27;
            return true;
        }
    }
}
