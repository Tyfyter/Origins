using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Defiled {
	public class Defiled_Chakram : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Krakram");
			Tooltip.SetDefault("Very pointy");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.ThornChakram);
			Item.damage = 25;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 18;
			Item.useAnimation = 18;
			//item.knockBack = 5;
            Item.shoot = ModContent.ProjectileType<Defiled_Chakram_P>();
            Item.shootSpeed = 9.75f;
			Item.value = 5000;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
        public override bool CanUseItem(Player player) {
            return player.ownedProjectileCounts[Item.shoot]<=0;
        }
    }
    public class Defiled_Chakram_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Defiled/Defiled_Chakram";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Krakram");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.ThornChakram);
            Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
            Projectile.localAI[0] = 1;
            //projectile.scale*=1.25f;
        }
        public override bool PreAI() {
            Projectile.aiStyle = 3;
            Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.localAI[0]*0.15f);
            Projectile.localAI[0] = (float)System.Math.Sin(Projectile.timeLeft);
            return true;
        }
        public override bool? CanHitNPC(NPC target) {
            Projectile.aiStyle = 0;
            return null;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
            width = 27;
            height = 27;
            return true;
        }
    }
}
