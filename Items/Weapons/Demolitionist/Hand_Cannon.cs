using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.NPCs.MiscB.Shimmer_Construct;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Hand_Cannon : ModItem {
		public static AutoCastingAsset<Texture2D> UseTexture { get; private set; }
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Hand_Cannon_Portal>(30, 26, 10, 28, 28);
			Item.knockBack = 4;
			Item.noUseGraphic = true;
			Item.UseSound = null;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Aetherite_Bar>(12)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void HoldItem(Player player) {
			player.handon = Item.handOnSlot;
		}
		public override void UseItemFrame(Player player) {
			player.handon = Item.handOnSlot;
			player.SetCompositeArmFront(true, (player.itemAnimationMax - player.itemAnimation) switch {
				<= 0 => Player.CompositeArmStretchAmount.None,
				<= 1 => Player.CompositeArmStretchAmount.Quarter,
				<= 2 => Player.CompositeArmStretchAmount.ThreeQuarters,
				_ => Player.CompositeArmStretchAmount.Full
			}, player.itemRotation * player.gravDir - player.direction * MathHelper.PiOver2);
		}
	}
	public class Hand_Cannon_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Canister_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Canister_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 1;
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 900;
			Projectile.scale = 0.85f;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 6;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X == 0f) {
				Projectile.velocity.X = -oldVelocity.X;
			}
			if (Projectile.velocity.Y == 0f) {
				Projectile.velocity.Y = -oldVelocity.Y;
			}
			Projectile.timeLeft = 1;
			return true;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (OriginsModIntegrations.CheckAprilFools()) {
				float targetWeight = 4.5f;
				Vector2 targetDiff = default;
				NPC target = default;
				foreach (NPC potential in Main.ActiveNPCs) {
					if (potential.type == NPCID.Shimmerfly) {
						Vector2 currentDiff = potential.Center - Projectile.Center;
						float dist = currentDiff.Length();
						currentDiff /= dist;
						float weight = Vector2.Dot(Projectile.velocity, currentDiff) * (300f / (dist + 100));
						if (weight > targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, potential.position, potential.width, potential.height)) {
							targetWeight = weight;
							targetDiff = currentDiff;
							target = potential;
						}
					}
				}
				if (target is not null) {
					PolarVec2 velocity = (PolarVec2)Projectile.velocity;
					OriginExtensions.AngularSmoothing(
						ref velocity.Theta,
						(target.DirectionFrom(Projectile.Center)).ToRotation(),
						0.01f + velocity.R * 0.005f * Origins.HomingEffectivenessMultiplier[Projectile.type]
					);
					Projectile.velocity = (Vector2)velocity;
				}
			}
		}
	}
	public class Hand_Cannon_Portal : ModProjectile, ICanisterProjectile, ITriggerSCBackground {
		public override string Texture => "Terraria/Images/Extra_98";
		public AutoLoadingAsset<Texture2D> OuterTexture { get; } = "";
		public AutoLoadingAsset<Texture2D> InnerTexture { get; } = "";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			OriginsSets.Projectiles.ApplyLifetimeModifiers[Type] = false;
			ID = Type;
		}
		public override bool ShouldUpdatePosition() => true;
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 18;
			OriginsSets.Projectiles.ApplyLifetimeModifiers[Type] = false;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation();
			Player.CompositeArmStretchAmount oldStretch = player.compositeFrontArm.stretch;
			player.compositeFrontArm.stretch = Player.CompositeArmStretchAmount.Full;
			Projectile.position = player.GetCompositeArmPosition(false) + Projectile.velocity.SafeNormalize(default) * 12;
			player.compositeFrontArm.stretch = oldStretch;
			if (Projectile.timeLeft > 10) {
				if (Projectile.ai[0] <= 4) {
					if (++Projectile.ai[0] > 4) {
						SoundEngine.PlaySound(SoundID.Item60.WithPitchRange(0.15f, 0.3f).WithVolume(1f), Projectile.position);
						SoundEngine.PlaySound(SoundID.Item142, Projectile.position);
						SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithPitch(1f), Projectile.position);
						Projectile.SpawnProjectile(null,
							Projectile.position,
							Projectile.velocity,
							ModContent.ProjectileType<Hand_Cannon_P>(),
							Projectile.damage,
							Projectile.knockBack
						);
					}
				}
			} else {
				Projectile.ai[0] = Projectile.timeLeft - 5;
				if (Projectile.ai[0] <= 0) Projectile.active = false;
			}
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			if (SC_Phase_Three_Midlay.DrawnMaskSources.Add(Projectile)) {
				Texture2D circle = TextureAssets.Projectile[Type].Value;
				for (int i = 0; i < Projectile.ai[0]; i++) {
					SC_Phase_Three_Midlay.DrawDatas.Add(new(
						circle,
						Projectile.position - Main.screenPosition,
						null,
						Color.White
					) {
						rotation = projectile.rotation,
						origin = circle.Size() * 0.5f,
						scale = Vector2.One * Projectile.scale
					});
				}
			}
		}
	}
}
