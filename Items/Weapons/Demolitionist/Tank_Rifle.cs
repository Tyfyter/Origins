﻿using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using PegasusLib;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Tank_Rifle : ModItem, ICustomDrawItem, ICustomWikiStat {
		public string[] Categories => [
			"Launcher"
		];
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 5));
		}
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Tank_Rifle_P>(200, 90, 8, 156, 38);
			Item.UseSound = Origins.Sounds.HeavyCannon;
			Item.knockBack = 18f;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Lime;
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			float itemRotation = drawPlayer.itemRotation;

			Vector2 pos = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

			int frame = (int)(float.Pow(1 - drawPlayer.itemAnimation / (float)drawPlayer.itemAnimationMax, 1.1f) * 5 * 3 + 1);
			if (frame >= 5) frame = 0;

			Texture2D texture = TextureAssets.Item[Type].Value;
			Rectangle sourceRect = texture.Frame(verticalFrames: 5, frameY: frame);
			drawInfo.DrawDataCache.Add(new DrawData(
				texture,
				pos,
				sourceRect,
				Item.GetAlpha(lightColor),
				itemRotation,
				new Vector2(31).Apply(drawInfo.itemEffect, sourceRect.Size()),
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect,
			0));
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int selfDamage = (int)player.OriginPlayer().GetSelfDamageModifier().ApplyTo(Main.DamageVar(30));
			if (selfDamage > 0) player.Hurt(
				PlayerDeathReason.ByPlayerItem(player.whoAmI, Item),
				selfDamage,
				0,
				cooldownCounter: -2,
				dodgeable: false,
				knockback: 0,
				scalingArmorPenetration: 0.5f
			);
			player.velocity -= velocity * 2;
			return true;
		}
	}
	public class Tank_Rifle_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Canister_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Canister_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 900;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = ProjectileID.Sets.TrailCacheLength[Type] * 10 + 64;
			ID = Type;
		}
		public override void SetDefaults() {
			OriginsSets.Projectiles.HomingEffectivenessMultiplier[Type] = 0.0125f;
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 99;
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
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.TryGetGlobalProjectile(out ExplosiveGlobalProjectile global)) global.modifierBlastRadius *= 2;
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
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
		public override void OnKill(int timeLeft) {
			if (NetmodeActive.Server) return;
			if (Projectile.owner != Main.myPlayer) {
				if (!Projectile.hide) {
					Projectile.hide = true;
					try {
						Projectile.active = true;
						Projectile.timeLeft = timeLeft;
						Projectile.Update(Projectile.whoAmI);
					} finally {
						Projectile.active = false;
						Projectile.timeLeft = 0;
					}
				}
				return;
			}
			Vector2[] oldPos = [..Projectile.oldPos];
			float[] oldRot = [..Projectile.oldRot];
			for (int i = 0; i < oldPos.Length; i++) {
				if (oldPos[i] == default) {
					Array.Resize(ref oldPos, i);
					Array.Resize(ref oldRot, i);
					break;
				}
				oldPos[i] += Projectile.Size * 0.5f;
				oldRot[i] += MathHelper.PiOver2;
			}
			Dust.NewDustPerfect(
				Main.LocalPlayer.Center,
				ModContent.DustType<Vertex_Trail_Dust>(),
				Vector2.Zero
			).customData = new Vertex_Trail_Dust.TrailData(oldPos, oldRot, StripColors(Projectile.GetGlobalProjectile<CanisterGlobalProjectile>().CanisterData.InnerColor), StripWidth, 14);
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			Vector2 origin = OuterTexture.Value.Size() * 0.5f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				InnerTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.InnerColor,
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				OuterTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);

			MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
			Vector2[] oldPos = [.. projectile.oldPos];
			float[] oldRot = [.. projectile.oldRot];
			for (int i = 0; i < oldPos.Length; i++) {
				if (oldPos[i] == default) {
					Array.Resize(ref oldPos, i);
					Array.Resize(ref oldRot, i);
					break;
				}
				oldRot[i] += MathHelper.PiOver2;
			}
			miscShaderData.UseSaturation(-2.8f);
			miscShaderData.UseOpacity(4f);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(oldPos, oldRot, StripColors(canisterData.InnerColor), StripWidth, -Main.screenPosition + projectile.Size / 2f);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}
		static VertexStrip.StripColorFunction StripColors(Color color) => progressOnStrip => {
			if (float.IsNaN(progressOnStrip)) return Color.Transparent;
			float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
			return color * (1f - lerpValue * lerpValue);
		};
		static float StripWidth(float progressOnStrip) {
			float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
			return MathHelper.Lerp(0, 8, 1f - lerpValue * lerpValue);
		}
		private static readonly VertexStrip _vertexStrip = new();
	}
}
