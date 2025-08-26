using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Buffs;
using System.Collections.Generic;
using Origins.World.BiomeData;
using Origins.Gores.NPCs;
using Origins.Projectiles;
using Terraria.Audio;
using Origins.NPCs;

namespace Origins.Items.Weapons.Magic {
    public class Dew_Justice : ModItem, ICustomWikiStat, ITornSource {
		public static float TornSeverity => 0.3f;
		float ITornSource.Severity => TornSeverity;
		public string[] Categories => [
			"SpellBook"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Torn_Debuff.ID, Slow_Debuff.ID];
		}
		public override void SetDefaults() {
			const int use_time = 9;
			Item.DefaultToMagicWeapon(ModContent.ProjectileType<Dew_Justice_P>(), use_time * 2, 12);
			Item.useTime = use_time;
			Item.reuseDelay = use_time;
			Item.DamageType = DamageClass.Magic;
			Item.damage = 41;
			Item.knockBack = 8;
			Item.width = 18;
			Item.height = 20;
			Item.mana = 15;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = Origins.Sounds.DefiledIdle.WithPitchRange(-0.6f, -0.4f);
		}
		const int halfSize = 8;
		public override bool CanUseItem(Player player) {
			return !new Rectangle((int)(Main.MouseWorld.X - halfSize), (int)(Main.MouseWorld.Y - halfSize), halfSize * 2, halfSize * 2).OverlapsAnyTiles();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Rectangle hitbox = new(0, 0, halfSize * 2, halfSize * 2);
			for (int n = 0; n < 2; n++) {
				bool foundPosition = false;
				int tries = 100;
				while (!foundPosition) {
					position = Main.MouseWorld;
					velocity = velocity.RotatedByRandom(MathHelper.TwoPi);
					for (int i = 0; i < 32; i++) {
						position -= velocity * 0.5f;
						hitbox.X = (int)(position.X - halfSize);
						hitbox.Y = (int)(position.Y - halfSize);
						if (hitbox.OverlapsAnyTiles()) {
							if (i > 4) foundPosition = true;
							break;
						}
					}
					if (--tries <= 0) foundPosition = true;
				}
				Projectile.NewProjectile(
					source,
					position,
					velocity,
					type,
					damage,
					knockback,
					player.whoAmI
				);
				SoundEngine.PlaySound(Main.rand.NextBool() ? SoundID.Item111 : SoundID.Item112, position);
			}
			return false;
		}
	}
	public class Dew_Justice_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 7;
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
			Projectile.timeLeft = 60;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.extraUpdates = 0;
			Projectile.penetrate = 3;
			Projectile.hide = true;
			Projectile.alpha = 0;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override bool ShouldUpdatePosition() => Projectile.ai[0] > 0;
		public override void AI() {
			if (!Projectile.tileCollide && !CollisionExtensions.OverlapsAnyTiles(Projectile.Hitbox)) Projectile.tileCollide = true;
			if (++Projectile.frameCounter > (ShouldUpdatePosition() ? 3 : 2)) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) {
					Projectile.frame = 4;
				}
				if (Projectile.frame > 3) Projectile.ai[0] = 1;
			}
		}
		Entity collisionEntity = null;
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Slow_Debuff.ID, 240);
			OriginGlobalNPC.InflictTorn(target, 75, targetSeverity: Dew_Justice.TornSeverity);
			collisionEntity = target;
			Projectile.Kill();
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override void OnKill(int timeLeft) {
			Gore gore = Gore.NewGoreDirect(
				Projectile.GetSource_Death(),
				Projectile.Center,
				Projectile.velocity,
				Main.rand.Next(R_Effect_Blood1.GoreIDs),
				1f
			);
			if (timeLeft > 1) {
				R_Effect_Blood1.Splatter(gore, Projectile.velocity, collisionEntity);
			}
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 64, false, fireDustAmount: 0, smokeGoreAmount: 0, smokeDustAmount: 0);
		}
		public override Color? GetAlpha(Color lightColor) => Riven_Hive.GetGlowAlpha(lightColor);
	}
}
