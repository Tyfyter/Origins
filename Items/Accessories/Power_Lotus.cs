using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Other.Dyes;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Power_Lotus : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Misc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.shoot = ModContent.ProjectileType<Power_Lotus_Seeker_Fairy>();
			Item.knockBack = 8;
			Item.rare = ItemRarityID.LightPurple;
			Item.master = true;
			Item.hasVanityEffects = true;
			Item.value = Item.sellPrice(gold: 12);
			Item.maxStack = 1;
			Item.dye = 0;
		}
		public override void UpdateAccessory(Player player, bool isHidden) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.fairyLotus = true;
			player.treasureMagnet = true;
			originPlayer.goldenLotus = true;
			originPlayer.goldenLotusItem = Item;
			player.kbGlove = true;
			player.autoReuseGlove = true;
			player.meleeScaleGlove = true;
			player.GetAttackSpeed(DamageClass.Melee) += 0.12f;
			if (originPlayer.amebicVialCooldown > 0) {
				originPlayer.amebicVialVisible = false;
				return;
			}
			originPlayer.amebicVialVisible = !isHidden;
			const float maxDist = 71 * 71;
			Vector2 target = default;
			float bestWeight = 0;
			Projectile projectile;
			Vector2 currentPos;
			Vector2 diff;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				projectile = Main.projectile[i];
				if (projectile.active && (projectile.hostile || (Main.player[projectile.owner].hostile && Main.player[projectile.owner].team != player.team)) && Amebic_Vial.CanBeDeflected[projectile.type]) {
					currentPos = projectile.Hitbox.ClosestPointInRect(player.MountedCenter);
					diff = player.Hitbox.ClosestPointInRect(projectile.Center) - currentPos;
					float dist = diff.LengthSquared();
					if (dist > maxDist) continue;
					float currentWeight = Vector2.Dot(projectile.velocity, diff.SafeNormalize(default)) * dist;
					if (currentWeight > bestWeight) {
						bestWeight = currentWeight;
						target = currentPos;
					}
				}
			}
			NPC npc;
			for (int i = 0; i < Main.maxNPCs; i++) {
				npc = Main.npc[i];
				if (npc.active && npc.damage > 0 && !npc.friendly) {
					currentPos = npc.Hitbox.ClosestPointInRect(player.MountedCenter);
					diff = player.Hitbox.ClosestPointInRect(npc.Center) - currentPos;
					float dist = diff.LengthSquared();
					if (dist > maxDist) continue;
					float currentWeight = Vector2.Dot(npc.velocity, diff.SafeNormalize(default)) * dist;
					if (currentWeight > bestWeight) {
						bestWeight = currentWeight;
						target = currentPos;
					}
				}
			}
			if (bestWeight > 0) {
				Projectile.NewProjectile(
					player.GetSource_Accessory(Item),
					target.Clamp(player.Hitbox),
					(target - player.MountedCenter).SafeNormalize(default) * 3.8f,
					Power_Lotus_Deflect_Fairy.ID,
					1,
					player.GetWeaponKnockback(Item),
					player.whoAmI
				);
				originPlayer.amebicVialCooldown = 100;
			}
		}
		public override void UpdateVanity(Player player) {
			player.GetModPlayer<OriginPlayer>().amebicVialVisible = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Golden_Lotus>()
			.AddIngredient<Handy_Helper>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.SubstituteKeybind(Keybindings.GoldenLotus);
		}
	}
	public class Power_Lotus_Seeker_Fairy : Golden_Lotus_Fairy, IShadedProjectile {
		public override string Texture => typeof(Golden_Lotus_Fairy).GetDefaultTMLName();
		public int Shader => Lotusy_Dye.ShaderID;
		public override void DisplayRange() {
			if (Projectile.owner != Main.myPlayer) return;
			for (float angle = 0; angle < MathHelper.TwoPi; angle += 1 / Projectile.localAI[0]) {
				int dustType = DustID.CoralTorch;
				switch ((int)(angle * Projectile.localAI[0] / MathHelper.TwoPi) % 2) {
					case 1:
					dustType = DustID.BlueTorch;
					break;
				}
				Dust dust = Dust.NewDustPerfect(
					Projectile.position + GeometryUtils.Vec2FromPolar(Projectile.localAI[0] * range_per_frame, angle + Projectile.frameCounter),
					dustType,
					Vector2.Zero
				);
				dust.noGravity = true;
				//dust.noLight = true;
			}
		}
		public override void AddFairyLight() {
			const int HalfSpriteWidth = 24 / 2;
			const int HalfSpriteHeight = 24 / 2;

			const float shaderGradientCompensation = HalfSpriteWidth + HalfSpriteHeight * 10;
			Lighting.AddLight(Projectile.Center,
				0f,
				(MathF.Sin((Main.GlobalTimeWrappedHourly + shaderGradientCompensation) * 2) + 1) * 0.5f,
				(MathF.Sin(Main.GlobalTimeWrappedHourly + shaderGradientCompensation) + 3) * 0.25f
			);
		}
		public override Color GetItemColor(Item item, Vector2 position, Vector2 velocity) {
			float shaderGradientCompensation = position.X / 100 + position.Y / 10;
			return Color.Lerp(Color.White, new Color(
				0f,
				(MathF.Sin((Main.GlobalTimeWrappedHourly + shaderGradientCompensation) * 2) + 1) * 0.5f,
				(MathF.Sin(Main.GlobalTimeWrappedHourly + shaderGradientCompensation) + 3) * 0.25f
			), 0.5f);
		}
	}
	public class Power_Lotus_Deflect_Fairy : ModProjectile, IShadedProjectile {
		public int Shader => Lotusy_Dye.ShaderID;
		public override string Texture => typeof(Golden_Lotus_Fairy).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 90;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.aiStyle = 0;
			Projectile.tileCollide = false;
		}
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			Vector2 direction = Projectile.Center - ownerMountedCenter;
			float distance = direction.Length();
			if (distance < 16 && Projectile.ai[0] > 30) {
				Projectile.Kill();
				return;
			}
			direction = distance == 0 ? Vector2.Zero : (direction / distance);

			if (movementFactor == 0f) {
				movementFactor = 5.1f;
				Projectile.netUpdate = true;
			}
			Projectile.timeLeft = 2;
			Projectile.ai[0]++;
			float factor = (Projectile.ai[0] / 30 - 1);
			MathUtils.LinearSmoothing(ref Projectile.velocity, direction * -Math.Min(factor * 8 + 4, distance), Math.Clamp(factor, 0.05f, 1));

			Projectile.spriteDirection = Projectile.direction;
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other.hostile && Amebic_Vial.CanBeDeflected[other.type] && Projectile.Colliding(Projectile.Hitbox, other.Hitbox)) {
					other.velocity = Vector2.Lerp(other.velocity, direction * 3.5f, 0.6f);
				}
			}
			if (++Projectile.frameCounter >= 6) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
			const int HalfSpriteWidth = 24 / 2;
			const int HalfSpriteHeight = 24 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			const float shaderGradientCompensation = HalfSpriteWidth + HalfSpriteHeight * 10;
			Lighting.AddLight(Projectile.Center,
				0f,
				(MathF.Sin((Main.GlobalTimeWrappedHourly + shaderGradientCompensation) * 2) + 1) * 0.5f,
				(MathF.Sin(Main.GlobalTimeWrappedHourly + shaderGradientCompensation) + 3) * 0.25f
			);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (target.knockBackResist > 0) {
				target.oldVelocity = target.velocity;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.knockBackResist > 0) {
				target.velocity = Vector2.Lerp(target.oldVelocity, Projectile.velocity * hit.Knockback / 3.2f, (float)Math.Sqrt(target.knockBackResist));
			}
		}
		public override Color? GetAlpha(Color lightColor) => Color.White * 0.8f;
	}
}
