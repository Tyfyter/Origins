using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.Projectiles;
using PegasusLib;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Defective_Mortar_Shell : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Defective_Mortar_Shell_Entry).Name;
		public class Defective_Mortar_Shell_Entry : JournalEntry {
			public override string TextKey => "Defective_Mortar_Shell";
			public override JournalSortIndex SortIndex => new("The_Ashen", 2);
		}
		public string[] Categories => [
		];
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.damage = 38;
			Item.knockBack = 6;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.shoot = ModContent.ProjectileType<Minions.Ashen_Mortar>();
			Item.shootSpeed = 1;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.sentry = true;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<NE8>(), 9)
			.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse != 2) {
				Projectile.NewProjectile(source, Main.MouseWorld - new Vector2(20, 40), default, type, Item.damage, Item.knockBack, player.whoAmI);
				player.UpdateMaxTurrets();
			}
			return false;
		}
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Ashen_Mortar : ModProjectile {
		public override string Texture => typeof(Ashen_Mortar).GetDefaultTMLName() + "_Base";
		static readonly AutoLoadingAsset<Texture2D> headTexture = typeof(Ashen_Mortar).GetDefaultTMLName() + "_Barrel";
		Vector2 headCenterOffset => new Vector2(Projectile.width * 0.5f, 30);
		public override void SetStaticDefaults() {
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			OriginsSets.Projectiles.NoMultishot[Type] = true;
		}

		public override void SetDefaults() {
			//Projectile.CloneDefaults(ProjectileID.FrostHydra);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.sentry = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = Projectile.SentryLifeTime;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region General behavior
			Vector2 idlePosition = player.Bottom;

			// die if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				Projectile.Kill();
				return;
			}
			#endregion

			#region Find target
			// Starting search distance
			float distanceFromTarget = 2000f;
			Vector2 targetCenter = default;
			int target = -1;
			bool hasPriorityTarget = false;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				bool isCurrentTarget = npc.whoAmI == Projectile.ai[0];
				if ((isCurrentTarget || isPriorityTarget || !hasPriorityTarget) && npc.CanBeChasedBy()) {
					Vector2 offset = headCenterOffset;
					Vector2 targetPos = npc.Center + npc.velocity * 45;
					float between = Vector2.Distance(targetPos, Projectile.position + offset);
					between *= isCurrentTarget ? 0 : 1;
					bool closer = distanceFromTarget > between;
					if (closer || !foundTarget) {
						distanceFromTarget = between;
						targetCenter = targetPos;
						target = npc.whoAmI;
						foundTarget = true;
						hasPriorityTarget = isPriorityTarget;
					}
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);

			#endregion

			#region Movement
			bool resetShootingProgress = true;
			const float proj_speed = 10;
			if (foundTarget) {
				Vector2 diff = targetCenter - Projectile.Center;
				if (GeometryUtils.AngleToTarget(diff, proj_speed, 0.12f, true) is float angle && OriginExtensions.AngularSmoothing(ref Projectile.rotation, angle, Projectile.ai[0] == target ? 0.2f : 0.125f)) {
					Projectile.ai[0] = target;
					if (++Projectile.ai[1] >= 60) {
						Projectile.NewProjectile(
							Projectile.GetSource_FromAI(),
							Projectile.position + headCenterOffset,
							GeometryUtils.Vec2FromPolar(proj_speed, Projectile.rotation),
							ModContent.ProjectileType<Ashen_Mortar_P>(),
							Projectile.damage,
							Projectile.knockBack,
							Projectile.owner
						);
					} else {
						resetShootingProgress = false;
					}
				}
			}
			if (resetShootingProgress) {
				Projectile.ai[0] = -1;
				Projectile.ai[1] = 0;
			}
			Projectile.velocity.Y += 0.4f;
			#endregion
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X) {
				int dir = Math.Sign(oldVelocity.X);
				Vector2 collisionPos = (Projectile.Bottom + new Vector2(18 * dir, 0));
				if (Framing.GetTileSafely(collisionPos.ToTileCoordinates()).HasFullSolidTile() && !Framing.GetTileSafely((collisionPos - new Vector2(0, 12)).ToTileCoordinates()).HasFullSolidTile()) {
					Projectile.velocity.Y = -5;
				}
			}
			return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 24;
			fallThrough = false;
			return true;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D baseTexture = TextureAssets.Projectile[Type].Value;
			Vector2 basePosition = Projectile.position - Main.screenPosition + new Vector2(0, 2);
			Rectangle baseFrame = baseTexture.Frame(verticalFrames: Main.projFrames[Projectile.type], frameY: Projectile.frame);
			Main.EntitySpriteDraw(
				baseTexture,
				basePosition,
				baseFrame,
				lightColor,
				0,
				default,
				Projectile.scale,
				SpriteEffects.None
			);

			SpriteEffects barrelEffects = SpriteEffects.None;
			float rotation = Projectile.rotation + MathHelper.PiOver4;
			if (MathF.Cos(Projectile.rotation) > 0) {
				barrelEffects = SpriteEffects.FlipHorizontally;
			} else {
				rotation += MathHelper.PiOver2;
			}
			Main.EntitySpriteDraw(
				headTexture,
				basePosition + headCenterOffset,
				null,
				lightColor,
				rotation,
				new Vector2(21, 23).Apply(barrelEffects, headTexture.Value.Size()),
				Projectile.scale,
				barrelEffects
			);
			return false;
		}
	}
	public class Ashen_Mortar_P : ModProjectile {
		public override string Texture => typeof(Defective_Mortar_Shell).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			ProjectileID.Sets.SentryShot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CursedBullet);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.aiStyle = 0;
			Projectile.scale *= 0.8f;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
			Projectile.velocity.Y += 0.12f;
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.alpha < 200) {
				return lightColor * (1 - Projectile.alpha / 255);
			}
			return Color.Transparent;
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				new Vector2(29, 11),
				Projectile.scale,
				SpriteEffects.None
			);
			return false;
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, sound: SoundID.Item62);
		}
	}
}
