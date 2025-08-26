using Origins.Dev;
using Origins.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Hellfire_Grenade : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            "ThrownExplosive",
			"IsGrenade",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 48;
			Item.shoot = ModContent.ProjectileType<Hellfire_Grenade_P>();
			Item.shootSpeed *= 1.25f;
			Item.value *= 9;
			Item.rare = ItemRarityID.Orange;
			Item.glowMask = glowmask;
        }
		public override void AddRecipes() {
			Recipe.Create(Type, 7)
			.AddIngredient(ItemID.Grenade, 7)
			.AddIngredient(ItemID.Hellstone)
			.Register();
		}
	}
	public class Hellfire_Grenade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Hellfire_Grenade";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.penetrate = 1;
			Projectile.timeLeft = 135;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool PreKill(int timeLeft) {
            Projectile.type = ProjectileID.Grenade;
            return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			Projectile.NewProjectile(
				Projectile.GetSource_Death(),
				Projectile.Center,
				default,
				ModContent.ProjectileType<Hellfire_Grenade_Fire>(),
				Projectile.damage / 3,
				0,
				Projectile.owner
			);
		}
	}
    public class Hellfire_Grenade_Fire : ExplosionProjectile {
        public override DamageClass DamageType => DamageClasses.ThrownExplosive;
        public override int Size => 80;
        public override SoundStyle? Sound => null;
        public override int FireDustAmount => 2;
        public override int SmokeDustAmount => 1;
        public override int SmokeGoreAmount => 0;
        public override int SelfDamageCooldownCounter => ImmunityCooldownID.WrongBugNet;
        public override void SetDefaults() {
            base.SetDefaults();
            Projectile.timeLeft = 60;
            Projectile.usesLocalNPCImmunity = false;
            Projectile.usesIDStaticNPCImmunity = false;
            Projectile.idStaticNPCHitCooldown = 6;
        }
        public override void AI() {
            base.AI();
            Projectile.ai[0] = 0;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(BuffID.OnFire3, 180);
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
            modifiers.Knockback *= 0;
            modifiers.FinalDamage *= 0.3f;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info) {
            target.AddBuff(BuffID.OnFire3, 100);
        }
    }
}
