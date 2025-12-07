using Origins.Buffs;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ammo {
	public class Valkyrum_Bullet : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CursedBullet);
			Item.damage = 12;
			Item.shoot = ModContent.ProjectileType<Valkyrum_Bullet_P>();
			Item.shootSpeed = 5f;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(copper: 7);
			Item.rare = ItemRarityID.Lime;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 70)
			.AddIngredient(ModContent.ItemType<Valkyrum_Bar>())
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Valkyrum_Bullet_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Ammo/Generic_Bullet";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.light = 0.5f;
			Projectile.alpha = 255;
			Projectile.scale = 1.2f;
			Projectile.timeLeft = 600;
			Projectile.extraUpdates = 3;
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
			Projectile.BulletShimmer();
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.alpha < 200) {
				return new Color(0.2f, 0.9f, 1f, 0.75f) * Projectile.Opacity;
			}
			return Color.Transparent;
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			float luck = Main.player[Projectile.owner].luck;
			target.AddBuff(Electrified_Debuff.ID, Main.rand.Next(180, 241));
			foreach (Entity other in Static_Shock_Debuff.GetValidChainTargets(target, 10)) {
				if (other is NPC npc) {
					npc.SimpleStrikeNPC(damageDone, (other.Center.X > target.Center.X).ToDirectionInt(), hit.Crit, hit.Knockback * 0.5f, hit.DamageType, true, luck);
					npc.AddBuff(Electrified_Debuff.ID, Main.rand.Next(180, 241));
				}
			}
		}
	}
}
