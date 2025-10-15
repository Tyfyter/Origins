using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;

namespace Origins.Items.Weapons.Ranged {
	public class Blotopus : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Gun
		];
		public override void SetStaticDefaults() {
			Origins.FlatDamageMultiplier[Type] = 4f / 8f;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.damage = 4;
			Item.width = 64;
			Item.height = 22;
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.shoot = ModContent.ProjectileType<Blotopus_P>();
			Item.useAmmo = AmmoID.Bullet;
			Item.knockBack = 0;
			Item.shootSpeed = 7f;
			Item.value = Item.sellPrice(gold: 1);
			Item.UseSound = SoundID.Item2;
			Item.autoReuse = true;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
			velocity = velocity.RotatedByRandom(0.1f);
		}
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return !Main.rand.NextBool(5);
		}
	}
	public class Blotopus_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ranged/Blotopus_P";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.alpha = 0;
		}
		public override void AI() {
			Projectile.rotation -= MathHelper.PiOver2;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Bleeding, Main.rand.Next(120, 241));
		}
	}
}
