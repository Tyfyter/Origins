using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PegasusLib;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.GameContent.Liquid;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Collections.Generic;

namespace Origins.Items.Weapons.Magic {
	public class Shimmerstar_Staff : ModItem, ICustomDrawItem {
		public override string Texture => typeof(Bled_Out_Staff).GetDefaultTMLName();
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
			Item.UseSound = SoundID.Item67;
			Item.autoReuse = true;
			Item.useLimitPerAnimation = MainFireCount;
			Item.ChangePlayerDirectionOnShoot = false;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override float UseAnimationMultiplier(Player player) => player.altFunctionUse == 2 ? 0.35f : 1;
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			if (player.altFunctionUse == 2) mult /= MainFireCount / 2;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse != 2) {
				int turnDir = (velocity.RotatedBy(-player.fullRotation).X > 0).ToDirectionInt();
				if (player.ItemUsesThisAnimation == 1) player.ChangeDir(turnDir);

				int arcIndex = player.direction == 1 ? player.ItemUsesThisAnimation : ((MainFireCount + 1) - player.ItemUsesThisAnimation);
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, ai0: (arcIndex - 0.5f) / (float)MainFireCount, ai1: player.itemAnimation);

				if (player.ItemUsesThisAnimation == Item.useLimitPerAnimation) player.ChangeDir(turnDir);
			} else {
				if (player.ItemUsesThisAnimation == 1) {
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
			if (player.altFunctionUse != 2) {
				float offset;
				float timePerSwing = player.itemAnimationMax * 0.5f;
				const float stop = 0.5f;
				if (player.itemAnimation > timePerSwing) {
					offset = (player.itemAnimation - timePerSwing) * -2 / timePerSwing + 1;
					offset = float.Clamp(offset * (1 + stop), -1, 1);
				} else {
					offset = (timePerSwing - player.itemAnimation) * -2 / timePerSwing + 1;
					offset = float.Clamp(offset * (1 + stop), -1, 1);
				}
				player.SetCompositeArmFront(
					true,
					Player.CompositeArmStretchAmount.Full,
					(MathHelper.Pi * 1.0625f - offset) * player.direction
				);
			} else {
				float offset;
				float timePerSwing = player.itemAnimationMax;
				ref int itemComboAnimationTime = ref player.OriginPlayer().itemComboAnimationTime;
				if (itemComboAnimationTime > 0) {
					offset = (timePerSwing - player.itemAnimation) * -2 / timePerSwing + 1;
					itemComboAnimationTime = player.ItemAnimationEndingOrEnded ? 0 : 4; 
				} else {
					offset = (player.itemAnimation - timePerSwing) * -2 / timePerSwing - 1;
					itemComboAnimationTime = player.ItemAnimationEndingOrEnded ? 4 : 0;
				}
				offset = (1 - MathF.Pow(1 - Math.Abs(offset), 2)) * Math.Sign(offset);
				offset *= 1.8f;
				offset = float.Clamp(offset, -1.65f, 1.65f);
				player.SetCompositeArmFront(
					true,
					Player.CompositeArmStretchAmount.Full,
					player.itemRotation - (MathHelper.PiOver2 + offset) * player.direction
				);
			}
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			float offset;
			float timePerSwing = drawInfo.drawPlayer.itemAnimationMax * 0.5f;
			if (drawInfo.drawPlayer.itemAnimation > timePerSwing) {
				offset = (drawInfo.drawPlayer.itemAnimation - timePerSwing) * -2 / timePerSwing + 1;
			} else {
				offset = (timePerSwing - drawInfo.drawPlayer.itemAnimation) * -2 / timePerSwing + 1;
			}
			Vector2 origin = new(13, 13);
			float rotOffset = MathHelper.PiOver2 * 0.875f;
			if (drawInfo.drawPlayer.altFunctionUse == 2) {
				origin = new(9, 9);
				rotOffset = MathHelper.PiOver4;
			}
			DrawData data = new(
				itemTexture,
				drawInfo.drawPlayer.GetFrontHandPosition(drawInfo.drawPlayer.compositeFrontArm.stretch, drawInfo.drawPlayer.compositeFrontArm.rotation) - Main.screenPosition,
				itemTexture.Bounds,
				lightColor,
				drawInfo.drawPlayer.compositeFrontArm.rotation + MathHelper.Pi - rotOffset * drawInfo.drawPlayer.direction,
				origin.Apply(drawInfo.itemEffect ^ SpriteEffects.FlipVertically, itemTexture.Size()),
				1f,
				drawInfo.itemEffect
			);
			drawInfo.DrawDataCache.Add(data);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Aetherite_Bar>(), 8)
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
				Vector2 targetPos = player.Top - ((Projectile.ai[0]) * MathHelper.Pi).ToRotationVector2() * 32;
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
			MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
			miscShaderData.UseSaturation(-2.8f);
			miscShaderData.UseOpacity(4f);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			Color StripColors(float progressOnStrip) {
				if (float.IsNaN(progressOnStrip)) return Color.Transparent;
				Vector2 pos = Projectile.oldPos[(int)(progressOnStrip * (Projectile.oldPos.Length - 1))];
				return new(LiquidRenderer.GetShimmerBaseColor(pos.X / 16, pos.Y / 16));
			}
			float StripWidth(float progressOnStrip) {
				float num = 1f;
				float lerpValue = Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
				num *= 1f - (1f - lerpValue) * (1f - lerpValue);
				return MathHelper.Lerp(0f, 32f * Utils.GetLerpValue(0f, 32f, Projectile.position.Distance(Projectile.oldPos[12]), clamped: true), num);
			}

			Vector2 vector73 = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Color color = new(255, 255, 255, 0);
			Vector2 origin = new Vector2(texture.Width, texture.Height) * 0.5f;
			float rotation = 0;
			float scale = Projectile.scale * Utils.GetLerpValue(32f, 0f, Projectile.position.Distance(Projectile.oldPos[12]), clamped: true);
			scale = Math.Min(scale * scale * 1.4f, 0.5f);
			Vector2 spinningpoint6 = new Vector2(2f * scale + (float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi) * 0.4f, 0f).RotatedBy(rotation + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi);
			for (float f = 0f; f < 1f; f += 1f / 6f) {
				Vector2 pos = vector73 + spinningpoint6.RotatedBy(f * MathHelper.TwoPi);
				Main.EntitySpriteDraw(texture, pos, null, new(LiquidRenderer.GetShimmerBaseColor((pos.X + Main.screenPosition.X) / 16, (pos.Y + Main.screenPosition.Y) / 16) * new Vector4(Vector3.One, 0.5f)), rotation, origin, scale, SpriteEffects.None);
			}
			return false;
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
