using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Origins.Buffs;
using Origins.Dev;
using Origins.Projectiles;
using PegasusLib;
using PegasusLib.Reflection;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Origins.Items.Weapons.Melee {
	public class Soul_Snatcher : ModItem, ICustomWikiStat {
		public override void Load() {
			IL_Player.ItemCheck_Inner += IL_Player_ItemCheck_Inner;
		}
		static void IL_Player_ItemCheck_Inner(ILContext il) {
			ILCursor c = new(il);
			try {
				ILLabel label = default;
				Func<Instruction, bool>[] predicates = [
					i => i.MatchLdarg0(),
					i => i.MatchLdfld<Player>(nameof(Player.mount)),
					i => i.MatchCallOrCallvirt<Mount>("get_" + nameof(Mount.Active)),
					i => i.MatchBrfalse(out label),

					i => i.MatchLdarg0(),
					i => i.MatchLdfld<Player>(nameof(Player.mount)),
					i => i.MatchCallOrCallvirt<Mount>("get_" + nameof(Mount.Type)),
					i => i.MatchLdcI4(MountID.Drill),
					i => i.MatchBneUn(out ILLabel _label) && _label.Target == label.Target
				];
				c.GotoNext(MoveType.AfterLabel, predicates);

				label = c.DefineLabel();
				c.EmitLdarg0();
				c.EmitDelegate((Player player) => {
					return player.HeldItem.ModItem is Soul_Snatcher;
				});
				c.EmitBrtrue(label);
				c.Index += predicates.Length;
				c.MarkLabel(label);
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(IL_Player_ItemCheck_Inner), e)) throw;
			}
		}
		public override void SetStaticDefaults() {
			ItemID.Sets.Spears[Type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 22;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 66;
			Item.height = 68;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4;
			Item.shoot = ModContent.ProjectileType<Soul_Snatcher_Spin>();
			Item.shootSpeed = 3.75f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.channel = true;
		}
		public override void HoldItem(Player player) {
			if (player.OriginPlayer().soulSnatcherActive) {
				if (player.HandPosition is not null) {
					Dust dust = Dust.NewDustDirect(
						player.HandPosition.Value - Vector2.One * 4,
						4,
						4,
						DustID.Shadowflame,
						0,
						0
					);
					dust.velocity *= 0.1f;
					dust.noGravity = true;
				}
			}
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.DemoniteBar, 9)
			.AddIngredient(ItemID.ShadowScale, 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.controlUseTile) {
				velocity = new((velocity.X > 0 ? 1 : -1) * velocity.Length(), 0);
			} else {
				type = ModContent.ProjectileType<Soul_Snatcher_P>();
			}
		}
		public override bool MeleePrefix() => true;
		public static void UpdateCharge(Player player, ref float soulSnatcherTime, ref bool soulSnatcherActive) {
			if (player.whoAmI != Main.myPlayer) return;
			bool wasEmpowered = soulSnatcherActive;
			if (soulSnatcherTime > 0) {
				if (soulSnatcherTime > 60 * 2) {
					if (!soulSnatcherActive || soulSnatcherTime > 60 * 3) soulSnatcherTime = 60 * 3;
					soulSnatcherActive = true;
				}
				soulSnatcherTime -= soulSnatcherActive ? 0.25f : 1f;
			} else if (soulSnatcherActive) {
				soulSnatcherTime = 0;
				if (player.ItemAnimationEndingOrEnded || player.HeldItem.ModItem is not Soul_Snatcher || player.channel) soulSnatcherActive = false;
			}
			if (Main.netMode != NetmodeID.SinglePlayer && wasEmpowered != soulSnatcherActive) {
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.soul_snatcher_activate);
				packet.Write((byte)player.whoAmI);
				packet.Write(soulSnatcherActive);
				packet.Send();
			}
		}
	}
	public class Soul_Snatcher_P : ModProjectile {
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 3600;
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.aiStyle = 0;
			Projectile.scale = 1f;
		}
		public float MovementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		#region empowered state
		bool empowered = false;
		public override void OnSpawn(IEntitySource source) {
			empowered = Main.player[Projectile.owner].OriginPlayer().soulSnatcherActive;
			Projectile.netUpdate = true;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(empowered);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			empowered = reader.ReadBoolean();
		}
		#endregion
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			player.heldProj = Projectile.whoAmI;
			Projectile.direction = player.direction;
			Projectile.spriteDirection = player.direction;
			player.itemTime = player.itemAnimation;
			Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, true);
			float oldFactor = MovementFactor;
			if (!player.frozen) {
				if (player.itemAnimation < player.itemAnimationMax / 2) {
					MovementFactor -= 2.1f;
				} else if (player.itemAnimation > player.itemAnimationMax / 2 + 1) {
					MovementFactor += 2.2f;
				}
				if (MovementFactor > 20) MovementFactor = 20;
			}
			Projectile.position += Projectile.velocity * MovementFactor * Projectile.scale;
			oldFactor -= MovementFactor;
			if (empowered) {
				for (int i = 0; i < 3; i++) {
					Dust dust = Dust.NewDustDirect(
						Projectile.position + Vector2.One * 4,
						Projectile.width - 8,
						Projectile.height - 8,
						DustID.Shadowflame,
						Projectile.velocity.X * 2 * oldFactor,
						Projectile.velocity.Y * 2 * oldFactor
					);
					dust.velocity *= 0.1f;
					dust.noGravity = true;
				}
			}
			if (player.ItemAnimationEndingOrEnded) {
				Projectile.Kill();
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
			if (Projectile.spriteDirection == 1) {
				Projectile.rotation -= MathHelper.PiOver2;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (empowered) {
				target.AddBuff(Shadefire_Debuff.ID, 120);
			} else {
				Main.player[Projectile.owner].OriginPlayer().soulSnatcherTime += 60;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects spriteEffects = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				new Vector2(69, 11).Apply(spriteEffects, TextureAssets.Projectile[Type].Size()),
				Projectile.scale,
				spriteEffects,
			0);
			return false;
		}
	}
	public class Soul_Snatcher_Spin : ModProjectile {
		public override string Texture => typeof(Soul_Snatcher_P).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 3600;
			//Projectile.width = 178;
			//Projectile.height = 178;
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 2;
			Projectile.scale = 1f;
		}
		#region empowered state
		bool empowered = false;
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(empowered);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			empowered = reader.ReadBoolean();
		}
		#endregion
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.timeLeft = 2;
			player.SetDummyItemTime(2);
			throwMode:
			if (Projectile.ai[0] != 0) {
				Projectile.hide = false;
				switch ((int)Projectile.ai[0]) {
					case 1: {
						Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * Projectile.spriteDirection;
						if (Projectile.spriteDirection == -1) Projectile.rotation += MathHelper.Pi;
						if (++Projectile.ai[1] > 15) {
							Projectile.velocity *= 0.93f;
							if (Projectile.velocity.IsWithin(Vector2.Zero, 3f)) {
								Projectile.ai[0] = 2;
								Projectile.ai[1] = 0;
							}
						}
						break;
					}
					case 2: {
						Vector2 diff = player.RotatedRelativePoint(player.MountedCenter, true) - Projectile.Center;
						float targetRot = diff.ToRotation() + MathHelper.PiOver4 * Projectile.spriteDirection;
						if (Projectile.spriteDirection == 1) targetRot += MathHelper.Pi;
						const int stationary_ticks = 15 * 3;
						if (++Projectile.ai[1] > stationary_ticks) {
							Projectile.rotation = targetRot;
							Projectile.ai[1] = stationary_ticks;
							Projectile.velocity *= 0.93f;
							float length = diff.Length();
							if (length < 32) {
								Projectile.Kill();
							} else {
								Projectile.velocity += diff * 0.8f / length;
							}
						} else {
							Projectile.velocity *= 0.95f;
							float speed = MathF.Pow((Projectile.ai[1] - 1) / stationary_ticks, 1.2f) * 0.1f;
							float oldRot = Projectile.rotation;
							GeometryUtils.AngularSmoothing(ref Projectile.rotation, targetRot, speed);
							Vector2 turningPoint = Projectile.Center + GeometryUtils.Vec2FromPolar(16, Projectile.rotation - MathHelper.PiOver4 * Projectile.spriteDirection) * Projectile.spriteDirection;
							Projectile.Center = Projectile.Center.RotatedBy(
								(float)GeometryUtils.AngleDif(Projectile.rotation, oldRot),
								turningPoint
							);
						}
						break;
					}
				}
				Dust dust = Dust.NewDustDirect(
					Projectile.position + Vector2.One * 4,
					Projectile.width - 8,
					Projectile.height - 8,
					DustID.Shadowflame,
					Projectile.velocity.X * 5,
					Projectile.velocity.Y * 5
				);
				dust.velocity *= 0.1f;
				dust.noGravity = true;
				return;
			} else if (empowered && player.OriginPlayer().realControlUseItem) {
				Projectile.ai[0] = 1;
				Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, true);
				Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length() * 2.5f;
				Projectile.netUpdate = true;
				player.ChangeDir((Projectile.velocity.X > 0).ToDirectionInt());
				goto throwMode;
			}
			if (!player.channel) {
				Projectile.Kill();
				return;
			}
			player.heldProj = Projectile.whoAmI;
			float baseSpeed = 0.095f;
			if (empowered) baseSpeed += 0.015f;
			Projectile.rotation += player.direction * baseSpeed * (28f / player.itemAnimationMax);
			Projectile.direction = player.direction;
			Projectile.spriteDirection = player.direction;
			Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, true);
			Vector2 headOffset = new Vector2((Projectile.spriteDirection > 0).ToDirectionInt(), -1).RotatedBy(Projectile.rotation);
			Projectile.Center += headOffset * 52;
			empowered = player.OriginPlayer().soulSnatcherActive;
			if (empowered) {
				Dust.NewDustDirect(
					Projectile.Center + headOffset * 4 - Vector2.One * 6,
					12,
					12,
					DustID.Shadowflame,
					headOffset.Y * 10 * Projectile.spriteDirection,
					headOffset.X * -10 * Projectile.spriteDirection
				).velocity *= 0.1f;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (empowered) {
				target.AddBuff(Shadefire_Debuff.ID, Projectile.ai[0] == 0 ? 60 : 120);
			} else {
				Main.player[Projectile.owner].OriginPlayer().soulSnatcherTime += 26;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.ai[0] != 0) return null;
			int steps = 3;
			Player player = Main.player[Projectile.owner];
			Vector2 mov = player.RotatedRelativePoint(player.MountedCenter, true) - Projectile.Center;
			mov /= steps;
			for (int i = 0; i < steps; i++) {
				Rectangle box = projHitbox;
				box.Offset((mov * i).ToPoint());
				if (box.Intersects(targetHitbox)) return true; 
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects spriteEffects = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				new Vector2(69, 11).Apply(spriteEffects, TextureAssets.Projectile[Type].Size()),
				//new Vector2(28, 52).Apply(spriteEffects, TextureAssets.Projectile[Type].Size()),
				Projectile.scale,
				spriteEffects,
			0);
			return false;
		}
	}
}
