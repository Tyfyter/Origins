using Origins.Buffs;
using Origins.Dev;
using Origins.Tiles.Other;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Felnum_Shock_Grenade : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ThrownExplosive",
			"IsGrenade",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 32;
			Item.shoot = ModContent.ProjectileType<Felnum_Shock_Grenade_P>();
			Item.shootSpeed *= 1.5f;
			Item.knockBack = 5f;
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(copper: 70);
			Item.rare = ItemRarityID.Green;
			Item.maxStack = 9999;
            Item.ArmorPenetration += 3;
        }
		public override void AddRecipes() {
			Recipe.Create(Type, 6)
			.AddIngredient(ItemID.Grenade, 6)
			.AddIngredient(ModContent.ItemType<Felnum_Ore_Item>())
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {

		}
	}
	public class Felnum_Shock_Grenade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Felnum_Shock_Grenade";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
			Projectile.penetrate = 1;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), Projectile.Center);
			int t = ModContent.ProjectileType<Felnum_Shock_Grenade_Shock>();
			for (int i = Main.rand.Next(2); i < 3; i++) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, t, (int)((Projectile.damage - 32) * 1.5f) + 16, 6, Projectile.owner);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Vector2 dest = Vector2.Lerp(target.Center, new Vector2(target.position.X + Main.rand.NextFloat(target.width), target.position.Y + Main.rand.NextFloat(target.height)), 0.5f);
			for (int i = 0; i < 16; i++) {
				Dust.NewDustPerfect(Vector2.Lerp(Projectile.Center, dest, i / 16f), 226, Main.rand.NextVector2Circular(1, 1), Scale: 0.5f);
			}
		}
	}
	public class Felnum_Shock_Grenade_Shock : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		protected override bool CloneNewInstances => true;
		Vector2 closest;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.DamageType = DamageClasses.ThrownExplosive;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 3;
			Projectile.width = Projectile.height = 0;
			Projectile.penetrate = 2;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 5;
		}
		public override void AI() {
			if (Projectile.penetrate == 1) {
				Projectile.penetrate = 2;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			closest = Projectile.position.Clamp(targetHitbox.TopLeft(), targetHitbox.BottomRight());
			return (Projectile.position - closest).Length() <= 96;
		}
		public override bool? CanHitNPC(NPC target) {
			return Projectile.penetrate > 1 ? base.CanHitNPC(target) : false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Static_Shock_Debuff.Inflict(target, 120);
			Projectile.damage -= (int)((Projectile.position - closest).Length() / 16f);
			if (!Main.rand.NextBool(5)) Projectile.timeLeft += hit.Crit ? 2 : 1;
			Vector2 dest = Vector2.Lerp(closest, new Vector2(target.position.X + Main.rand.NextFloat(target.width), target.position.Y + Main.rand.NextFloat(target.height)), 0.5f);
			Projectile.ai[0] = dest.X;
			Projectile.ai[1] = dest.Y;
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.ai[0] == 0 && Projectile.ai[1] == 0) return false;
			Vector2 dest = new(Projectile.ai[0], Projectile.ai[1]);
			Main.spriteBatch.DrawLightningArcBetween(
				Projectile.position - Main.screenPosition,
				dest - Main.screenPosition,
				Main.rand.NextFloat(-4, 4));
			for (int i = 0; i < 8; i++) {
				Dust.NewDustPerfect(Vector2.Lerp(Projectile.position, dest, i / 8f), 226, Main.rand.NextVector2Circular(1, 1), Scale: 0.5f);
			}
			return false;
		}
	}
}
