using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.NPCs;
using Origins.Projectiles;
using Origins.Tiles.Brine;
using PegasusLib;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Brine_Convergence_Incantation : ModItem, ICustomDrawItem {
		private Asset<Texture2D> _smolTexture;
		public Texture2D SmolTexture => (_smolTexture ??= this.GetSmallTexture())?.Value;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrystalVileShard);
			Item.damage = 35;
			Item.DamageType = DamageClasses.Incantation;
			Item.noMelee = true;
			Item.knockBack = 3;
			Item.shoot = ModContent.ProjectileType<Brine_Convergence_Incantation_Spawn_P>();
			Item.noUseGraphic = true;
			Item.shootSpeed = 8f;
			Item.UseSound = null;
			Item.mana = 20;
			Item.useTime = 38;
			Item.useAnimation = 38;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
			Item.holdStyle = ItemHoldStyleID.HoldLamp;
			Item.UseSound = SoundID.Item8;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SpellTome)
			.AddIngredient<Brineglow_Item>(5)
			.AddIngredient<Eitrite_Bar>(8)
			.AddTile(TileID.Bookcases)
			.Register();
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item117.WithPitchRange(0.0f, 0.2f), player.itemLocation);
			return null;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false;
		}
		public override void UseItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public override void HoldItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public bool BackHand => true;
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Incantations.DrawInHand(
				SmolTexture,
				ref drawInfo,
				lightColor
			);
		}
	}
	public class Brine_Convergence_Incantation_Spawn_P : ModProjectile {
		public override string Texture => typeof(Brine_Convergence_Incantation).GetDefaultTMLName();
		const int lifespan = 1800;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.friendly = false;
			Projectile.timeLeft = lifespan;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.aiStyle = 0;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (owner.itemAnimation < owner.itemAnimationMax * 0.5f) {
				if (Projectile.ai[0] == 0 && Projectile.owner == Main.myPlayer) {
					Projectile.ai[0] = 1;
					Vector2 direction = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitY);
					Projectile.velocity = direction * Projectile.velocity.Length();
					float targetDist = 5 * 16;
					targetDist *= targetDist;
					Vector2 targetPos = Main.MouseWorld;
					bool foundTarget = owner.DoHoming((target) => {
						Vector2 currentDiff = Main.MouseWorld.Clamp(target.Hitbox) - Main.MouseWorld;
						float dist = currentDiff.LengthSquared();
						if (dist < targetDist) {
							targetDist = dist;
							targetPos = target.Center;
							return true;
						}
						return false;
					});
					int type = ModContent.ProjectileType<Brine_Convergence_Incantation_P>();
					int count = Main.rand.Next(4, 7);
					for (int i = 0; i < count; i++) {
						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(),
							Projectile.position,
							Projectile.velocity.RotatedBy((i - (count - 1) * 0.5f) * (count * 0.03f)),
							type,
							Projectile.damage,
							Projectile.knockBack,
							ai1: targetPos.X,
							ai2: targetPos.Y
						);
					}
				}
				Projectile.position = owner.MountedCenter;
				owner.heldProj = Projectile.whoAmI;
				owner.direction = Math.Sign(Projectile.velocity.X);
				owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - MathHelper.PiOver2);
			} else {
				Projectile.position = owner.MountedCenter;
				owner.heldProj = Projectile.whoAmI;
				owner.SetCompositeArmFront(true,
					Player.CompositeArmStretchAmount.Quarter,
					(-1.6f - Math.Clamp((owner.velocity.Y - owner.GetModPlayer<OriginPlayer>().AverageOldVelocity().Y) / 64, -0.1f, 0.1f)) * owner.direction
				);
				Vector2 center = owner.GetFrontHandPosition(owner.compositeFrontArm.stretch, owner.compositeFrontArm.rotation);
				float radius = 8;
				Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius) * Main.rand.NextFloat(0.9f, 1f);
				Dust dust = Dust.NewDustPerfect(
					center - offset,
					DustID.Electric,
					offset * 0.125f,
					Scale: 0.65f
				);
				dust.velocity += owner.velocity;
				dust.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.AcidDye);
				dust.noGravity = false;
				dust.noLight = true;
			}
			if (owner.ItemAnimationEndingOrEnded) Projectile.Kill();
		}
	}
	public class Brine_Convergence_Incantation_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 5;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 1;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.alpha = 0;
			Projectile.appliesImmunityTimeOnSingleHits = true;
		}
		public override void AI() {
			ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(ItemID.AcidDye);
			for (int i = 0; i < 2; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, 1, 1, DustID.Electric);
				dust.position = Projectile.position - Projectile.velocity * (i * 0.5f);
				dust.position.X += Projectile.width / 2;
				dust.position.Y += Projectile.height / 2;
				dust.scale = Main.rand.NextFloat(0.65f, 0.65f);
				dust.velocity = dust.velocity * 0.2f + Projectile.velocity * 0.1f;
				dust.shader = shader;
				dust.noGravity = false;
				dust.noLight = true;
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (++Projectile.ai[0] < 120) {
				float targetDist = 5 * 16;
				targetDist *= targetDist;
				Vector2 oldTargetPos = new(Projectile.ai[1], Projectile.ai[2]);
				Vector2 targetPos = oldTargetPos;
				bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
					Vector2 currentDiff = Main.MouseWorld.Clamp(target.Hitbox) - oldTargetPos;
					float dist = currentDiff.LengthSquared();
					if (dist < targetDist) {
						targetDist = dist;
						targetPos = target.Center;
						return true;
					}
					return false;
				});
				if (foundTarget) {
					(Projectile.ai[1], Projectile.ai[2]) = targetPos;
				}
				PolarVec2 velocity = (PolarVec2)Projectile.velocity;
				float target = MathF.Atan2(Projectile.ai[2] - Projectile.Center.Y, Projectile.ai[1] - Projectile.Center.X);
				float diff = GeometryUtils.AngleDif(velocity.Theta, target, out int dir);

				float rate = 0.002f + velocity.R * 0.001f * Origins.HomingEffectivenessMultiplier[Projectile.type] + Projectile.ai[0] * 0.0001f;
				diff = Math.Abs(diff);
				float aRate = Math.Abs(rate);
				if (diff < aRate) {
					velocity.Theta = target;
				} else {
					velocity.Theta += rate * dir;
				}
				Projectile.velocity = (Vector2)velocity;
			}
			if (++Projectile.frameCounter >= 6) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
			const int HalfSpriteWidth = 28 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox towards the end"
			if (Projectile.spriteDirection == 1) {
				DrawOriginOffsetX = -(HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = (int)-DrawOriginOffsetX * 2;
				DrawOriginOffsetY = 0;
			} else {
				DrawOriginOffsetX = (HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = 0;
				DrawOriginOffsetY = 0;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			target.AddBuff(ModContent.BuffType<Brine_Incantation_Buff>(), 240);
			target.AddBuff(Toxic_Shock_Debuff.ID, 120);
		}
		public override Color? GetAlpha(Color lightColor) {
			int val = 255 - Projectile.alpha;
			return new Color(val, val, val, val);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 6;
			height = 6;
			return true;
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 32, false, SoundID.Item10, 0, 5, 0);
		}
	}
	public class Brine_Incantation_Buff : ModBuff, ICustomWikiStat {
		public bool CanExportStats => false;
		public override string Texture => "Terraria/Images/Buff_160";
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().brineIncantationDebuff = true;
		}
	}
}
