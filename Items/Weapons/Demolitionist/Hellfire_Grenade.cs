using Origins.Graphics;
using Origins.Items.Weapons.Magic;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Hellfire_Grenade : ModItem {
		static short glowmask;
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
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
	public class Hellfire_Grenade_P : ModProjectile, IBroken {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Hellfire_Grenade";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			Hand_Grenade_Launcher.AltUseCount[Type] = 5;
			Hand_Grenade_Launcher.AltUseTimeMultiplier[Type] = 0.1f;
			Hand_Grenade_Launcher.AltAnimationMultiplier[Type] = 0.6f;
			Hand_Grenade_Launcher.AltFireAction[Type] = (player, source, position, velocity, type, damage, knockback) => {
				Projectile.NewProjectile(source, position, velocity * 0.5f, ModContent.ProjectileType<Hellfire_Grenade_Flamethrower>(), damage / 4, knockback / 3, player.whoAmI);
			};
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
		public class Hellfire_Grenade_Flamethrower : ModProjectile {
			public override string Texture => typeof(Blast_Furnace_P).GetDefaultTMLName();
			public static float Lifetime => 40f;
			public static float MinSize => 6f;
			public static float MaxSize => 60f;
			private readonly float[] sizes = new float[21];
			public override void SetStaticDefaults() {
				ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
				ProjectileID.Sets.TrailCacheLength[Projectile.type] = sizes.Length;
				OriginsSets.Projectiles.FireProjectiles[Type] = true;
			}
			float Size => Math.Max(float.Lerp(MinSize, MaxSize, float.Pow(Utils.GetLerpValue(0f, Lifetime, Projectile.ai[0]), Projectile.ai[0] < 0 ? 1 : 0.8f)), 0);
			public override void SetDefaults() {
				Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
				Projectile.width = Projectile.height = 6;
				Projectile.penetrate = 4;
				Projectile.friendly = true;
				Projectile.alpha = 255;
				Projectile.extraUpdates = 3;
				Projectile.usesLocalNPCImmunity = true;
				Projectile.localNPCHitCooldown = -1;
				for (int i = 0; i < Projectile.oldPos.Length; i++)
					Projectile.oldRot[i] = Main.rand.NextFloatDirection();
			}
			public override void AI() {
				Projectile.localAI[0] += 1f;
				for (int i = sizes.Length - 1; i > 0; i--) {
					sizes[i] = sizes[i - 1];
				}
				sizes[0] = Size;
				float brightnessMult = 1;
				if (Projectile.ai[2] != 0) brightnessMult = Utils.GetLerpValue(0f, Lifetime, Projectile.ai[0]);
				Max(ref brightnessMult, 0);
				Lighting.AddLight(Projectile.Center, 0.85f * brightnessMult, 0.4f * brightnessMult, 0f);
				if (Main.rand.Next(1000) < sizes[0] * sizes[0]) {
					Vector2 halfSize = new(sizes[0] * 0.5f);
					Dust.NewDustDirect(Projectile.Center - halfSize, (int)halfSize.X, (int)halfSize.Y, DustID.Torch).velocity += Projectile.velocity * 0.5f;
				}
				//Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FrostStaff);
				Projectile.ai[0]++;
				Projectile.scale = Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize / 96f, MaxSize / 96f);
				Projectile.alpha = (int)(200 * (1 - (Projectile.localAI[0] / Lifetime)));
				Projectile.rotation += 0.3f * Projectile.direction;
				if (Projectile.ai[0] > Lifetime) {
					SoundEngine.PlaySound(SoundID.Item34);
					Projectile.Kill();
				}
			}
			public override void ModifyDamageHitbox(ref Rectangle hitbox) {
				int scale = (int)(Size / 2) - hitbox.Width;
				hitbox.Inflate(scale, scale);
			}
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
				target.AddBuff(BuffID.OnFire, 240);
			}
			public override bool PreDraw(ref Color lightColor) {
				float progress = (Projectile.ai[0] / Lifetime);
				Flamethrower_Drawer.Draw(Projectile, float.Pow(1 - progress, 2f), TextureAssets.Projectile[Type].Value, Color.Black, sizes);
				return false;
			}
			public override bool OnTileCollide(Vector2 oldVelocity) {
				Projectile.ai[2] = 2;
				Projectile.velocity = default;
				return false;
			}
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
		public override void SetStaticDefaults() {
			OriginsSets.Projectiles.FireProjectiles[Type] = true;
		}
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
