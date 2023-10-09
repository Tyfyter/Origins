using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Starfuze : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodenArrow);
			Item.maxStack = 999;
			/*Item.damage = 20;
            Item.shoot = ModContent.ProjectileType<Alkahest_Arrow_P>();
			Item.shootSpeed = 4f;
            Item.knockBack = 3f;*/
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Cyan;
			Item.glowMask = glowmask;
		}
	}
	/*public class Alkahest_Arrow_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Ammo/Alkahest_Arrow_P";
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.penetrate = 1;
            Projectile.width = 14;
            Projectile.height = 32;
        }
        public override void Kill(int timeLeft) {
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            OriginGlobalNPC.InflictTorn(target, 300, 180, 0.33f);
        }
    }*/
}
