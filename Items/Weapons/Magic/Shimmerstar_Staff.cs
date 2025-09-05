using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Journal;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Shimmerstar_Staff : ModItem, ICustomDrawItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Shimmerstar_Staff_Entry).Name;
		public class Shimmerstar_Staff_Entry : JournalEntry {
			public override string TextKey => "Shimmerstar_Staff";
			public override JournalSortIndex SortIndex => new("Arabel", 1);
		}
		public static AutoLoadingAsset<Texture2D> swipeTexture = typeof(Shimmerstar_Staff).GetDefaultTMLName() + "_Swipe";
		public static int MainFireCount => 4;
		public override void SetStaticDefaults() {
			Item.staff[Item.type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 18;
			Item.noMelee = true;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 6;
			Item.useAnimation = 42;
			Item.shoot = ModContent.ProjectileType<Shimmerstar_Staff_P>();
			Item.shootSpeed = 12f;
			Item.mana = 12;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = null;
			Item.autoReuse = true;
			Item.useLimitPerAnimation = MainFireCount;
			Item.ChangePlayerDirectionOnShoot = false;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override float UseAnimationMultiplier(Player player) => player.altFunctionUse == 2 ? 0.35f : 1;
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			if (player.altFunctionUse == 2) mult /= MainFireCount / 2;
		}
		static (Vector2 origin, float rotation) GetFrameCompensation(int frame) {
			switch (frame) {
				default: return (new(49, 69), new Vector2(-42, -26).ToRotation());
				case 1: return (new(49, 69), new Vector2(-42, -26).ToRotation());
				case 2: return (new(49, 67), new Vector2(-35, -36).ToRotation());
				case 3: return (new(51, 59), new Vector2(-15, -47).ToRotation());
				case 4: return (new(60, 56), new Vector2(0, -1).ToRotation());
				case 5: return (new(69, 59), new Vector2(37, -35).ToRotation());
				case 6: return (new(71, 63), new Vector2(45, -23).ToRotation());
				case 7: return (new(73, 71), new Vector2(51, -7).ToRotation());
				case 8: return (new(73, 71), new Vector2(51, -7).ToRotation());
				case 9: return (new(69, 63), new Vector2(37, -35).ToRotation());
				case 10: return (new(67, 59), new Vector2(29, -43).ToRotation());
				case 11: return (new(60, 57), new Vector2(5, -51).ToRotation());
				case 12: return (new(55, 59), new Vector2(-23, -43).ToRotation());
				case 13: return (new(53, 63), new Vector2(-35, -37).ToRotation());
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse != 2) {
				int turnDir = (velocity.RotatedBy(-player.fullRotation).X > 0).ToDirectionInt();
				if (player.ItemUsesThisAnimation == 1) player.ChangeDir(turnDir);
				SoundEngine.PlaySound(SoundID.Item35.WithPitchRange(0.15f, 0.4f).WithVolume(0.5f), player.Center);
				SoundEngine.PlaySound(SoundID.Item43.WithPitch(1f), player.Center);
				int arcIndex = player.direction == 1 ? player.ItemUsesThisAnimation : ((MainFireCount + 1) - player.ItemUsesThisAnimation);
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, ai0: (arcIndex - 0.5f) / (float)MainFireCount, ai1: player.itemAnimation);

				if (player.ItemUsesThisAnimation == Item.useLimitPerAnimation) player.ChangeDir(turnDir);
			} else {
				if (player.ItemUsesThisAnimation == 1) {
					SoundEngine.PlaySound(SoundID.Item35.WithPitchRange(0.15f, 0.4f).WithVolume(0.5f), player.Center);
					SoundEngine.PlaySound(SoundID.Item43.WithPitch(1f), player.Center);
					player.ChangeDir((velocity.RotatedBy(-player.fullRotation).X > 0).ToDirectionInt());
					position = player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation);
					bool secondSwing = player.OriginPlayer().itemComboAnimationTime > 0;
					Vector2 staffOffset = (player.compositeFrontArm.rotation - (MathHelper.PiOver4 * (secondSwing.ToInt() + 1)) * player.direction).ToRotationVector2() * 32;
					if (player.direction != -1) staffOffset = -staffOffset;
					Projectile.NewProjectile(
						source,
						position + staffOffset,
						velocity.RotatedBy(secondSwing.ToDirectionInt() * MathHelper.PiOver2 * 1.25f),
						ModContent.ProjectileType<Shimmerstar_Staff_P2>(),
						damage,
						knockback,
						ai0: -3
					);
				}
			}
			return false;
		}
		public override void UseItemFrame(Player player) {
			float[] handles = [0, 0.2017f, 0.12f, 0.85f, 1f, 1f, 1f, 1f];
			if (player.altFunctionUse != 2) {
				float progress = (player.itemAnimationMax - player.itemAnimation) * 2 / (float)player.itemAnimationMax;
				if (progress >= 1) {
					progress -= 1;
					for (int i = 0; i < handles.Length; i++) handles[i] = 1 - handles[i];
				}
				float offset = (float.Clamp(OriginExtensions.Bezier(float.Lerp, progress, handles), 0, 1) - 0.5f) * 2.5f;
				player.SetCompositeArmFront(
					true,
					Player.CompositeArmStretchAmount.Full,
					(MathHelper.Pi * 1.0625f - offset) * player.direction + player.direction * 0.25f
				);
			} else {
				ref int itemComboAnimationTime = ref player.OriginPlayer().itemComboAnimationTime;
				if (itemComboAnimationTime > 0) {
					for (int i = 0; i < handles.Length; i++) handles[i] = 1 - handles[i];
					itemComboAnimationTime = player.ItemAnimationEndingOrEnded ? 0 : 4; 
				} else {
					itemComboAnimationTime = player.ItemAnimationEndingOrEnded ? 4 : 0;
				}
				float offset = (float.Clamp(OriginExtensions.Bezier(float.Lerp, 1 - player.itemAnimation / (float)player.itemAnimationMax, handles), 0, 1) - 0.5f) * 3.5f;
				player.SetCompositeArmFront(
					true,
					Player.CompositeArmStretchAmount.Full,
					player.itemRotation - (MathHelper.PiOver2 + offset) * player.direction
				);
			}
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Vector2 origin = new(10, 10);
			float rotOffset = MathHelper.PiOver2 * 0.875f;
			Rectangle frame;
			float progress = 1 - drawInfo.drawPlayer.itemAnimation / (float)drawInfo.drawPlayer.itemAnimationMax;
			itemTexture = swipeTexture;
			if (drawInfo.drawPlayer.altFunctionUse == 2) {
				int frameNum = (int)(10 * progress);
				frame = itemTexture.Frame(verticalFrames: 14, frameY: frameNum);
				(origin, float rotCompensation) = GetFrameCompensation(frameNum);
				if (drawInfo.drawPlayer.OriginPlayer().itemComboAnimationTime <= 0) {
					drawInfo.itemEffect ^= SpriteEffects.FlipHorizontally;
					rotCompensation = MathHelper.Pi - rotCompensation;
				}
				rotOffset = rotCompensation + MathHelper.PiOver2;
			} else {
				int frameNum = (int)(14 * progress);
				frame = itemTexture.Frame(verticalFrames: 14, frameY: frameNum);
				(origin, float rotCompensation) = GetFrameCompensation(frameNum);
				drawInfo.itemEffect ^= SpriteEffects.FlipHorizontally;
				rotCompensation = MathHelper.Pi - rotCompensation;
				rotOffset = rotCompensation + MathHelper.PiOver2;
			}
			DrawData data = new(
				itemTexture,
				drawInfo.drawPlayer.GetFrontHandPosition(drawInfo.drawPlayer.compositeFrontArm.stretch, drawInfo.drawPlayer.compositeFrontArm.rotation) - Main.screenPosition,
				frame,
				lightColor,
				drawInfo.drawPlayer.compositeFrontArm.rotation + MathHelper.Pi - rotOffset * drawInfo.drawPlayer.direction,
				origin.Apply(drawInfo.itemEffect, itemTexture.Size()),
				1f,
				drawInfo.itemEffect
			);
			drawInfo.DrawDataCache.Add(data);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Aetherite_Bar>(), 12)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Shimmerstar_Staff_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowRodBullet;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 3;
			ProjectileID.Sets.TrailCacheLength[Type] = 30;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RainbowRodBullet);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.extraUpdates = 0;
			Projectile.aiStyle = -1;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 600;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
			Projectile.tileCollide = false;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[2] = Projectile.velocity.Length();
		}
		public override void AI() {
			Projectile.hide = false;
			if (Projectile.ai[1] > 0) {
				Projectile.hide = true;
				Player player = Main.player[Projectile.owner];
				Vector2 targetPos = player.Top - (Projectile.ai[0] * MathHelper.Pi).ToRotationVector2() * 32;
				Projectile.velocity = (targetPos - Projectile.Center).WithMaxLength(8) + player.velocity;
				if (--Projectile.ai[1] <= 0) {
					Projectile.ai[0] = -1;
					if (Projectile.owner == Main.myPlayer) {
						float bestAngle = 0.5f;
						Vector2 aimOrigin = player.Top;
						Vector2 aimVector = aimOrigin.DirectionTo(Main.MouseWorld);
						bool foundTarget = player.DoHoming((target) => {
							float angle = Vector2.Dot(aimOrigin.DirectionTo(Main.MouseWorld.Clamp(target.Hitbox)), aimVector);
							if (angle > bestAngle) {
								Projectile.ai[0] = target.whoAmI;
								bestAngle = angle;
								return true;
							}
							return false;
						});
						if (foundTarget) {
							aimVector = Projectile.DirectionTo(Main.npc[(int)Projectile.ai[0]].Center);
						}
						Projectile.velocity = aimVector * Projectile.ai[2];
						Projectile.netUpdate = true;
					}
				}
			} else {
				if (Projectile.ai[0] != -1) {
					NPC target = Main.npc[(int)Projectile.ai[0]];
					if (target.CanBeChasedBy(Projectile)) {
						Projectile.velocity = Projectile.DirectionTo(target.Center) * Projectile.ai[2];
					} else {
						Projectile.ai[0] = -1;
					}
				}
				if (!Projectile.tileCollide && !Projectile.Hitbox.OverlapsAnyTiles()) {
					Projectile.tileCollide = true;
				}
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			DrawShimmerstar(Projectile);
			return false;
		}
		public static void DrawShimmerstar(Projectile projectile) {
			float opacity = projectile.Opacity;
			if (projectile.oldPos.Length > 0 && projectile.oldPos[^1] != projectile.position) {
				MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
				miscShaderData.UseSaturation(-2.8f);
				miscShaderData.UseOpacity(4f);
				miscShaderData.Apply();
				_vertexStrip.PrepareStripWithProceduralPadding(projectile.oldPos, projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + projectile.Size / 2f);
				_vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				Color StripColors(float progressOnStrip) {
					if (float.IsNaN(progressOnStrip)) return Color.Transparent;
					Vector2 pos = projectile.oldPos[(int)(progressOnStrip * (projectile.oldPos.Length - 1))];
					return new Color(LiquidRenderer.GetShimmerBaseColor(pos.X / 16, pos.Y / 16)) * opacity;
				}
				float StripWidth(float progressOnStrip) {
					float num = 1f;
					float lerpValue = Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
					num *= 1f - (1f - lerpValue) * (1f - lerpValue);
					return MathHelper.Lerp(0f, 32f * Utils.GetLerpValue(0f, 32f, projectile.position.Distance(projectile.oldPos[12]), clamped: true), num);
				}
			}

			Vector2 center = projectile.Center + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
			Texture2D texture = TextureAssets.Projectile[projectile.type].Value;
			Color color = new(255, 255, 255, 0);
			Vector2 origin = new Vector2(texture.Width, texture.Height) * 0.5f;
			float rotation = 0;
			float scale = projectile.scale * Utils.GetLerpValue(32f, 0f, projectile.position.Distance(projectile.oldPos.GetIfInRange(12, projectile.position)), clamped: true);
			scale = Math.Min(scale * scale * 1.4f, 0.5f);
			Vector2 spinningpoint6 = new Vector2(2f * scale + (float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi) * 0.4f, 0f).RotatedBy(rotation + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi);
			for (float f = 0f; f < 1f; f += 1f / 6f) {
				Vector2 pos = center + spinningpoint6.RotatedBy(f * MathHelper.TwoPi);
				Main.EntitySpriteDraw(texture, pos, null, new Color(LiquidRenderer.GetShimmerBaseColor((pos.X + Main.screenPosition.X) / 16, (pos.Y + Main.screenPosition.Y) / 16) * new Vector4(Vector3.One, 0.5f)) * opacity, rotation, origin, scale, SpriteEffects.None);
			}
		}
	}
	public class Shimmerstar_Staff_P2 : Shimmerstar_Staff_P {
		public override bool ShouldUpdatePosition() => Projectile.ai[0] != -3;
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			Player player = Main.player[Projectile.owner];
			if (Projectile.ai[0] == -3) {
				if (player.ItemAnimationEndingOrEnded) {
					Projectile.ai[0] = -2;
					Projectile.penetrate = 1;
				} else {
					Vector2 position = player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation);
					bool firstSwing = player.OriginPlayer().itemComboAnimationTime > 0;
					Vector2 staffDir = (player.compositeFrontArm.rotation - MathHelper.PiOver2 * player.direction).ToRotationVector2();
					if (player.direction != -1) staffDir = -staffDir;
					Projectile.velocity = staffDir.RotatedBy(firstSwing.ToDirectionInt() * player.direction * MathHelper.PiOver2 * 0.5f) * Projectile.ai[2];
					Projectile.Center = position + staffDir * 40;
					Projectile.penetrate = -1;
					return;
				}
			}
			if (Projectile.ai[0] == -2) {
				Projectile.ai[0] = -1;
				if (Projectile.owner == Main.myPlayer) {
					float bestAngle = 0.5f;
					Vector2 aimOrigin = Projectile.Center;
					Vector2 aimVector = aimOrigin.DirectionTo(Main.MouseWorld);
					bool foundTarget = player.DoHoming((target) => {
						float angle = Vector2.Dot(aimOrigin.DirectionTo(Main.MouseWorld.Clamp(target.Hitbox)), aimVector);
						if (angle > bestAngle) {
							Projectile.ai[0] = target.whoAmI;
							bestAngle = angle;
							return true;
						}
						return false;
					});
					if (foundTarget) {
						aimVector = Main.npc[(int)Projectile.ai[0]].Center - Projectile.Center;
					}
					Projectile.ai[1] = aimVector.ToRotation();
					Projectile.netUpdate = true;
				}
			} else {
				if (Projectile.ai[0] != -1) {
					NPC target = Main.npc[(int)Projectile.ai[0]];
					if (target.CanBeChasedBy(Projectile)) {
						Projectile.ai[1] = (Main.npc[(int)Projectile.ai[0]].Center - Projectile.Center).ToRotation();
					} else {
						Projectile.ai[0] = -1;
					}
				}
				PolarVec2 movement = (PolarVec2)Projectile.velocity;
				float growingTurnSpeed = Math.Min(Projectile.localAI[0] / 20, 1f);
				growingTurnSpeed *= growingTurnSpeed;
				GeometryUtils.AngularSmoothing(ref movement.Theta, Projectile.ai[1], 0.1f + growingTurnSpeed * 0.1f);
				Projectile.velocity = (Vector2)movement;
			}
			if (++Projectile.localAI[0] >= 15 && !Projectile.tileCollide && !Projectile.Hitbox.OverlapsAnyTiles()) {
				Projectile.tileCollide = true;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.Knockback *= 2;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.Knockback *= 2;
		}
	}
}
