using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Tank_Rifle : ModItem, ICustomDrawItem {
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 5));
		}
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Tank_Rifle_P>(120, 80, 8, 156, 38);
			Item.UseSound = Origins.Sounds.HeavyCannon.WithPitch(-0.5f);
			Item.knockBack = 18f;
			Item.value = Item.sellPrice(gold: 1, silver: 80);
			Item.rare = ItemRarityID.Blue;
		}
		static int GetFrame(Player player) {
			if (player.itemAnimationMax == player.itemAnimation) return 0;
			int frame = (int)(float.Pow(1 - player.itemAnimation / (float)player.itemAnimationMax, 0.8f) * 5 * 3 + 1);
			if (frame >= 5) frame = 0;
			return frame;
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (GetFrame(player) == 1) {
				Lighting.AddLight(
					player.itemLocation + player.itemRotation.ToRotationVector2() * player.direction * 120 + Vector2.UnitY * player.HeightOffsetVisual,
					Color.Orange.ToVector3()
				);
			}
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			float itemRotation = drawPlayer.itemRotation;

			Vector2 pos = new((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y + drawInfo.mountOffSet));

			int frame = GetFrame(drawPlayer);

			Rectangle sourceRect = itemTexture.Frame(verticalFrames: 5, frameY: frame);
			DrawData data = new(
				itemTexture,
				pos,
				sourceRect,
				Item.GetAlpha(lightColor),
				itemRotation,
				new Vector2(31).Apply(drawInfo.itemEffect, sourceRect.Size()),
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect
			);
			drawInfo.DrawDataCache.Add(data);
			data.texture = TextureAssets.GlowMask[Item.glowMask].Value;
			data.color = Color.White;
			drawInfo.DrawDataCache.Add(data);
			frame = (drawPlayer.itemAnimationMax - drawPlayer.itemAnimation) - 1;
			if (frame < 4) {
				Texture2D texture = TextureAssets.Projectile[ProjectileID.DD2ExplosiveTrapT1Explosion].Value;
				sourceRect = texture.Frame(verticalFrames: 4, frameY: frame / 2 + 1);
				drawInfo.DrawDataCache.Add(new(
					texture,
					pos + new Vector2(86, -13).Apply(drawInfo.itemEffect, Vector2.Zero).RotatedBy(itemRotation),
					sourceRect,
					Color.White,
					itemRotation + MathHelper.PiOver2 * drawPlayer.direction,
					new Vector2(42, 72).Apply(drawInfo.itemEffect, sourceRect.Size()),
					drawPlayer.GetAdjustedItemScale(Item),
					drawInfo.itemEffect
				));
			}
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position += 12 * velocity.Normalized(out _).MatrixMult(Vector2.UnitY * player.direction + Vector2.UnitX * 4, Vector2.UnitX * -player.direction + Vector2.UnitY * 4);
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
			player.velocity = (player.velocity - velocity * 2).WithMaxLength(Math.Max(player.velocity.Length(), Item.shootSpeed * 3));
			return true;
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item38.WithPitch(-1.5f), player.itemLocation);
			SoundEngine.PlaySound(SoundID.Item88.WithPitch(-1f), player.itemLocation);
			return null;
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
			if (Projectile.soundDelay == 0) {
				Projectile.soundDelay = -1;
				Dust.NewDustPerfect(
					Projectile.Center + Projectile.velocity.Normalized(out _) * 120,
					ModContent.DustType<Rocket_Launch>(),
					Projectile.velocity,
					newColor: Color.DimGray
				);
			}
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
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.ScalingArmorPenetration += 0.75f;
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
	public class Debug_Tank_Rifle_Prefix : Explosive_Prefix, IBlastRadiusPrefix, ISelfDamagePrefix {
		public override bool CanRoll(Item item) => item?.ModItem is Tank_Rifle;
#if DEBUG
		public override float RollChance(Item item) => 1;
#else
		public override float RollChance(Item item) => 0;
#endif
		public StatModifier BlastRadius() => new(1, 1.5f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			useTimeMult *= 0.1f;
			shootSpeedMult /= 8f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
		public StatModifier SelfDamage() => default;
	}
}
