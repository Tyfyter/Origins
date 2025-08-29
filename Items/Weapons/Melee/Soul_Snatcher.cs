using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Origins.Buffs;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

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
					return OriginsSets.Items.ItemsThatCanChannelWithRightClick[player.HeldItem.type];
				});
				c.EmitBrtrue(label);
				c.Index += predicates.Length;
				c.MarkLabel(label);
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(IL_Player_ItemCheck_Inner), e)) throw;
			}
		}
		public class ItemDrawAnimation : DrawAnimation {
			public ItemDrawAnimation(int ticksperframe, int frameCount) {
				Frame = 0;
				FrameCounter = 0;
				FrameCount = frameCount;
				TicksPerFrame = ticksperframe;
			}
			public override void Update() {
				if (++FrameCounter >= TicksPerFrame) {
					FrameCounter = 0;
					if (++Frame >= FrameCount) Frame = 1;
				}
			}

			public override Rectangle GetFrame(Texture2D texture, int frameCounterOverride = -1) {
				int frame = Frame;
				if (frameCounterOverride != -1 || OriginPlayer.LocalOriginPlayer?.soulSnatcherActive != true) frame = 0;
				Rectangle result = texture.Frame(1, FrameCount, 0, frame);
				result.Height -= 2;
				return result;
			}
		}
		public override void SetStaticDefaults() {
			ItemID.Sets.Spears[Type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
			Main.RegisterItemAnimation(Item.type, new ItemDrawAnimation(5, 7));
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
			Item.autoReuse = true;
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
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			origin = new(32, 50);
			scale *= (0.5f / 0.3902439f);
			spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
			return false;
		}
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			Main.GetItemDrawFrame(Item.type, out Texture2D itemTexture, out Rectangle itemFrame);
			Vector2 drawOrigin = itemFrame.Size() / 2f;
			Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);
			itemFrame.Y = 0;
			spriteBatch.Draw(
				itemTexture,
				drawPosition,
				itemFrame,
				alphaColor,
				rotation,
				drawOrigin,
				scale,
				SpriteEffects.None,
			0);
			return false;
		}
	}
	public class Soul_Snatcher_P : ModProjectile {
		public static AutoLoadingAsset<Texture2D> empoweredTexture = typeof(Soul_Snatcher).GetDefaultTMLName() + "_Empowered";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
			ProjectileID.Sets.NoMeleeSpeedVelocityScaling[Type] = true;
			empoweredTexture.LoadAsset();
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
		public bool empowered = false;
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
					MovementFactor -= 1.8f;
				} else if (player.itemAnimation > player.itemAnimationMax / 2 + 1) {
					MovementFactor += 2.4f;
				}
				float speed = Projectile.velocity.Length();
				if (MovementFactor * speed > 24 * 3.75f) MovementFactor = 24 * 3.75f / speed;
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
				target.AddBuff(Shadefire_Debuff.ID, 180);
			} else {
				Main.player[Projectile.owner].OriginPlayer().soulSnatcherTime += 60;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = new(69, 11);
			Rectangle? frame = null;
			if (empowered) {
				texture = empoweredTexture;
				origin = new(69, 29);
				Rectangle itemFrame = Main.itemAnimations[ModContent.ItemType<Soul_Snatcher>()].GetFrame(texture);
				frame = texture.Frame(verticalFrames: 6, frameY: (itemFrame.Y / (itemFrame.Height + 2)) - 1);
			}
			SpriteEffects spriteEffects = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation,
				origin.Apply(spriteEffects, texture.Size()),
				Projectile.scale,
				spriteEffects,
			0);
			return false;
		}
	}
	public class Soul_Snatcher_Spin : ModProjectile {
		public static AutoLoadingAsset<Texture2D> empoweredTexture = typeof(Soul_Snatcher).GetDefaultTMLName() + "_ESpin";
		public static AutoLoadingAsset<Texture2D> launchTexture = typeof(Soul_Snatcher).GetDefaultTMLName() + "_Launch";
		public override string Texture => typeof(Soul_Snatcher_P).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
			empoweredTexture.LoadAsset();
			launchTexture.LoadAsset();
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
		public bool empowered = false;
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
				if (Projectile.soundDelay >= 0) {
					Projectile.soundDelay = -1;
					SoundEngine.PlaySound(SoundID.Item9.WithPitchRange(-1f, -0.2f), Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item60.WithPitchRange(-1f, -0.2f), Projectile.Center);
				}
				Vector2? handDir = null;
				Projectile.hide = false;
				switch ((int)Projectile.ai[0]) {
					case 1: {
						Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1; 
						handDir = Projectile.velocity;
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
						handDir = -diff;
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
				if (handDir.HasValue) {
					player.itemRotation = handDir.Value.ToRotation();
					if (handDir.Value.X < 0)
						player.itemRotation += MathHelper.Pi;

					player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
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
				if (Projectile.numUpdates == -1) launchAnimation.Update(Projectile);
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
			if (Projectile.soundDelay <= 0) {
				Projectile.soundDelay = Main.rand.RandomRound(MathHelper.TwoPi / (baseSpeed * (28f / player.itemAnimationMax)));
				SoundEngine.PlaySound(SoundID.Item71.WithPitch(1.3f), Projectile.Center);
			}
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
			if(Projectile.localAI[0] > 0) {
				Projectile.localAI[0] -= 1f / Projectile.MaxUpdates;
				if (Projectile.localAI[0] < 0) Projectile.localAI[0] = 0;
			}
		}
		public override bool? CanHitNPC(NPC target) => (Projectile.localAI[0] <= 0 || empowered) ? null : false;
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (Projectile.localAI[0] > 0 && Projectile.ai[0] == 0) {
				Player player = Main.player[Projectile.owner];
				modifiers.SourceDamage *= 1 - (Projectile.localAI[0] / (player.itemAnimationMax / 3)) * 0.5f;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Player player = Main.player[Projectile.owner];
			if (empowered) {
				target.AddBuff(Shadefire_Debuff.ID, Projectile.ai[0] == 0 ? 60 : 120);
			} else {
				player.OriginPlayer().soulSnatcherTime += 36;
			}
			Projectile.localAI[0] = player.itemAnimationMax / 3;
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
		LaunchAnimation launchAnimation = default;
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects spriteEffects = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = new(69, 11);
			Rectangle? frame = null;
			if (empowered) {
				if (Projectile.ai[0] == 0) {
					texture = empoweredTexture;
					origin = new(69, 30);
					frame = texture.Frame(verticalFrames: 3, frameY: (Main.itemAnimations[ModContent.ItemType<Soul_Snatcher>()].Frame - 1) % 3);
				} else {
					texture = Soul_Snatcher_P.empoweredTexture;
					origin = new(69, 29);
					frame = texture.Frame(verticalFrames: 6, frameY: Main.itemAnimations[ModContent.ItemType<Soul_Snatcher>()].Frame - 1);
				}
			}
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation,
				origin.Apply(spriteEffects, texture.Size()),
				Projectile.scale,
				spriteEffects,
			0);
			launchAnimation.Draw(Projectile);
			return false;
		}
		public struct LaunchAnimation {
			int frame;
			int frameCounter;
			Vector2 position;
			float rotation;
			public void Update(Projectile projectile) {
				switch ((int)projectile.ai[0]) {
					case 1:
					position = projectile.Center + new Vector2(-69 * projectile.spriteDirection, 69).RotatedBy(projectile.rotation);
					rotation = projectile.rotation - MathHelper.PiOver4;
					if (projectile.direction == -1) rotation += MathHelper.PiOver2;
					if (projectile.spriteDirection != projectile.direction) rotation -= MathHelper.PiOver2;
					else if (projectile.direction == -1) rotation += MathHelper.Pi;
					break;
					case 2:
					if (projectile.ai[1] < 15 * 3) {
						position = projectile.Center + new Vector2(-69, 69).RotatedBy(rotation + MathHelper.PiOver4);
					}
					break;
					default:
					frame = -1;
					return;
				}
				if (frame < 0) frame = 0;
				float lightScale;
				const float light_factor = 0.05f;
				switch (frame) {
					case 0 or 1 or 2 or 3 or 4:
					lightScale = light_factor * (frame + 1);
					break;
					default:
					lightScale = light_factor * (10 - frame);
					break;
				}
				Lighting.AddLight(position, lightScale * 0.6f, lightScale * 0.2f, lightScale);
			}
			public void Draw(Projectile projectile) {
				if (projectile.ai[0] == 0) return;
				Main.EntitySpriteDraw(
					launchTexture,
					position - Main.screenPosition,
					launchTexture.Value.Frame(verticalFrames: 11, frameY: frame),
					Color.White,
					rotation,
					new(164, 42),
					projectile.scale,
					SpriteEffects.None
				);
				// at 3 it begins dissipating almost exactly when the spear starts returning
				// at 4 the spear can come back through a little, but there's enough white left that it looks like there's still an active flame
				if (++frameCounter >= 3) {
					frameCounter = 0;
					frame++;
				}
			}
		}
	}
	public class Soul_Snatcher_Crit_Type : CritType<Soul_Snatcher>, IBrokenContent {
		public string BrokenReason => "Needs balancing";
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) {
			if (projectile?.ModProjectile is Soul_Snatcher_P stab) return stab.empowered;
			if (projectile?.ModProjectile is Soul_Snatcher_Spin spin) return spin.empowered;
			return false;
		}
		public override float CritMultiplier(Player player, Item item) => 1.2f;
	}
}
