using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Origins.Dev;
using PegasusLib;
using System.Linq;
using System;
using Terraria.Enums;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Origins.Buffs;
using SteelSeries.GameSense;
using Origins.NPCs.Defiled;
using Origins.NPCs;
using System.Collections.Generic;
using Origins.CrossMod;

namespace Origins.Items.Weapons.Melee {
	public class Tyrfing : ModItem, PegasusLib.ICustomDrawItem {
		public override void SetStaticDefaults() {
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Electrified_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.damage = 62;
			Item.DamageType = DamageClass.Melee;
			Item.width = 78;
			Item.height = 78;
			Item.useTime = 17;
			Item.useAnimation = 17;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 4;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.shoot = ModContent.ProjectileType<Tyrfing_P>();
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Lime;
			Item.UseSound = SoundID.Item1;
			Item.channel = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Excalibur)
			.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			if (player.altFunctionUse != 2) {
				if (player.itemAnimation < player.itemAnimationMax * 0.333) {
					OriginExtensions.FixedUseItemHitbox(Item, player, ref hitbox, ref noHitbox);
					hitbox.Height += 16;
				} else if (player.itemAnimation >= player.itemAnimationMax * 0.666) {
					hitbox.Y += 16;
					hitbox.Height -= 16;
					hitbox.Inflate(-24, 0);
					hitbox.X -= player.direction * 16;
				}
			} else {
				hitbox = default;
				noHitbox = true;
			}
		}
		public override bool CanShoot(Player player) => player.altFunctionUse == 2;
		public override float UseSpeedMultiplier(Player player) => player.altFunctionUse == 2 ? 0.8f : 1;
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Electrified_Debuff.ID, 180);
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			if (drawInfo.drawPlayer.altFunctionUse == 2) return;
			Rectangle bounds = itemTexture.Bounds;
			drawInfo.DrawDataCache.Add(new(
				itemTexture,
				(drawInfo.drawPlayer.HandPosition ?? drawInfo.drawPlayer.GetFrontHandPosition(drawInfo.drawPlayer.compositeFrontArm.stretch, drawInfo.drawPlayer.compositeFrontArm.rotation)) - Main.screenPosition,
				bounds,
				lightColor,
				drawInfo.drawPlayer.itemRotation + MathHelper.ToRadians(7.31f) * drawInfo.drawPlayer.direction,
				new Vector2(7, 79).Apply(drawInfo.itemEffect, bounds.Size()),
				drawInfo.drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect
			));
		}
	}
	public class Tyrfing_P : ModProjectile {
		public const int trail_length = 20;
		public override string Texture => typeof(Tyrfing).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		protected const int HitboxSteps = 4;
		protected static float Startup => 0.25f;
		protected static float End => 0.25f;
		protected float MinAngle => (-2.5f) - Projectile.ai[0] * 0.25f;
		protected float MaxAngle => 2.5f + Projectile.ai[0] * 0.25f;
		protected Rectangle lastHitHitbox;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 3;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.noEnchantmentVisuals = true;
			//DrawHeldProjInFrontOfHeldItemAndArms = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				Projectile.scale *= itemUse.Item.scale;
				itemUse.Player.ApplyMeleeScale(ref Projectile.scale);
				Projectile.ai[1] = -itemUse.Player.direction;
			}
		}
		protected float SwingFactor {
			get => Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (!player.active || player.dead) {
				Projectile.Kill();
				return;
			}
			float updateOffset = (Projectile.MaxUpdates - (Projectile.numUpdates + 1)) / (float)(Projectile.MaxUpdates + 1);
			if (player.channel) {
				updateOffset = 0;
				Projectile.timeLeft = player.itemTimeMax * Projectile.MaxUpdates;
				if (Projectile.owner == Main.myPlayer) {
					Vector2 newVel = (Main.MouseWorld - Projectile.Center).SafeNormalize(default);
					if (Projectile.velocity != newVel) {
						Projectile.velocity = newVel;
						Projectile.netUpdate = true;
					}
				}
				player.SetDummyItemTime(player.itemTimeMax - 1);
				Projectile.ai[0] += 2f / Projectile.timeLeft;
				if (Projectile.ai[0] >= 1) {
					Projectile.ai[0] = 1;
					Projectile.timeLeft += Projectile.timeLeft / 2;
				}
			}
			if (player.itemTime <= 2) {
				Projectile.localAI[2] = 1;
			}
			SwingFactor = ((player.itemTime - updateOffset) / (float)player.itemTimeMax) * (1 + Startup + End) - End;
			if (SwingFactor > 0) SwingFactor = MathHelper.Lerp(MathF.Pow(SwingFactor, 2f), MathF.Pow(SwingFactor, 0.5f), SwingFactor * SwingFactor);
			if (Projectile.localAI[2] == 1) {
				player.SetDummyItemTime(2);
				SwingFactor = 0;
			}
			Projectile.rotation = MathHelper.Lerp(
				MaxAngle,
				MinAngle,
				MathHelper.Clamp(SwingFactor, 0, 1)
			) * Projectile.ai[1] * player.gravDir;

			float realRotation = Projectile.rotation * player.gravDir + Projectile.velocity.ToRotation() * player.gravDir;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			Projectile.Center = player.GetCompositeArmPosition(false);
			player.itemLocation = Projectile.Center + GeometryUtils.Vec2FromPolar(26, realRotation + 0.3f * player.direction);
			player.itemRotation = player.compositeFrontArm.rotation;
			player.direction = Math.Sign(Projectile.velocity.X);
			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
			}
			EmitEnchantmentVisuals();
		}
		public virtual void EmitEnchantmentVisuals() {
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width * 0.95f;
			float velocityMult = 0;
			float rotMult = 0.15f;
			if (Projectile.localAI[2] == 0) {
				if (Main.player[Projectile.owner].channel) {
					velocityMult = 2;
				} else {
					velocityMult = 8;
					rotMult = 0.05f;
				}
			}
			Player player = Main.player[Projectile.owner];
			int swingingDustFactor = (!player.channel && Projectile.localAI[2] != 1).ToInt();
			for (int j = 0; j <= HitboxSteps; j++) {
				Projectile.EmitEnchantmentVisualsAt(Projectile.position + vel * j, Projectile.width, Projectile.height);
				if (j > 1 && Main.rand.NextFloat(2 * Projectile.MaxUpdates) < swingingDustFactor + Projectile.ai[0]) {
					Dust dust = Dust.NewDustDirect(
						Projectile.position + vel * j,
						Projectile.width, Projectile.height,
						DustID.PortalBoltTrail,
						newColor: new(0, 225, 255, 64)
					);
					dust.velocity = dust.velocity * 0.25f + Projectile.velocity.RotatedBy(Projectile.rotation * rotMult) * velocityMult;
					dust.position += dust.velocity * 2;
					dust.noGravity = true;
				}
			}
		}
		public override void CutTiles() {
			if (Main.player[Projectile.owner].channel) return;
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + (Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width * HitboxSteps);
			Utils.PlotTileLine(Projectile.Center, end, Projectile.width, DelegateMethods.CutTiles);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Main.player[Projectile.owner].channel) return false;
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width;
			Vector2 additionalOffset = vel.SafeNormalize(default) * 12;
			for (int j = 0; j <= HitboxSteps; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j + additionalOffset;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					lastHitHitbox = hitbox;
					return true;
				}
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			int debuffID = Electrified_Debuff.ID;
			int hasBuff = target.HasBuff(debuffID).ToInt();
			float range_per_arc = 8 + hasBuff * 2;
			const float max_chain_count = 3;
			float baseRange = range_per_arc * max_chain_count * (Projectile.ai[0] * 0.7f + hasBuff * 0.3f);
			target.AddBuff(debuffID, 180);

			bool ShouldInflict(Rectangle fromHitbox, Rectangle hitbox, bool fromDefiled, bool defiled, float range) {
				Vector2 pointA = hitbox.Center().Clamp(fromHitbox);
				Vector2 pointB = fromHitbox.Center().Clamp(hitbox);
				if (pointA.IsWithin(pointB, 16 * Math.Min(range, range_per_arc) * ((fromDefiled || defiled) ? 2 : 1))) {
					DoArcVisual(pointA, pointB);
					if (Main.netMode != NetmodeID.SinglePlayer) {
						ModPacket packet = Mod.GetPacket();
						packet.Write(Origins.NetMessageType.tyrfing_zap);
						packet.WriteVector2(pointA);
						packet.WriteVector2(pointB);
						packet.Send();
					}
					return true;
				}
				return false;
			}
			Player player = Main.player[Projectile.owner];
			float centerX = player.Center.X;
			float luck = player.luck;
			List<(NPC npc, float range)> fromNPC = [(target, baseRange)];
			HashSet<NPC> arcedTo = [target];
			HashSet<NPC> arcedFrom = [];
			for (int i = 0; i < fromNPC.Count; i++) {
				(NPC from, float range) = fromNPC[i];
				if (!arcedFrom.Add(from)) continue;
				Rectangle entityHitbox = from.Hitbox;
				bool entityDefiled = from.ModNPC is IDefiledEnemy;
				foreach (NPC other in Main.ActiveNPCs) {
					if (arcedTo.Contains(other) || other.buffImmune[debuffID] || (other.dontTakeDamage && !other.ShowNameOnHover)) continue;
					if ((other.type == NPCID.TargetDummy || !other.friendly) && ShouldInflict(entityHitbox, other.Hitbox, entityDefiled, other.ModNPC is IDefiledEnemy, range)) {
						other.AddBuff(debuffID, 120);
						arcedTo.Add(other);
						fromNPC.Add((other, range - range_per_arc));
						float damageMult = (range - range_per_arc) / (range_per_arc * max_chain_count);
						float entityCenterX = other.Center.X;
						other.SimpleStrikeNPC(
							damageMult > 0 ? Main.rand.RandomRound(damageMult * hit.SourceDamage) : 1,
							entityCenterX == centerX ? Projectile.direction : (entityCenterX > centerX).ToDirectionInt(),
							false,
							damageMult * Projectile.knockBack,
							Projectile.DamageType,
							true,
							luck
						);
					}
				}
			}
		}
		static uint lastSoundFrame = 0;
		public static void DoArcVisual(Vector2 pointA, Vector2 pointB) {
			if (!Main.dedServ && Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), pointA, pointB)) {
				Dust.NewDustPerfect(pointA, ModContent.DustType<Tyrfing_Arc_Dust>(), pointB);
				if (lastSoundFrame < Origins.gameFrameCount) {
					lastSoundFrame = Origins.gameFrameCount + 5;
					SoundEngine.PlaySound(Main.rand.Next(Origins.Sounds.LightningSounds), (pointA + pointB) * 0.5f);
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects spriteEffects = Projectile.ai[1] < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
			float rotation = Projectile.rotation + Projectile.velocity.ToRotation() + ((MathHelper.PiOver4 + MathHelper.ToRadians(7.31f)) * Projectile.ai[1]);
			Vector2 origin = new Vector2(7, 79).Apply(spriteEffects, TextureAssets.Projectile[Type].Size());
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				rotation,
				origin,// origin point in the sprite, 'round which the whole sword rotates
				Projectile.scale,
				spriteEffects,
				0
			);
			return false;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 1 + Projectile.ai[0] * 0.5f;
			modifiers.Knockback *= 1 + Projectile.ai[0] * 0.5f;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.SourceDamage *= 1 + Projectile.ai[0] * 0.5f;
			modifiers.Knockback *= 1 + Projectile.ai[0] * 0.5f;
		}
	}
	public class Tyrfing_Arc_Dust : ModDust {
		public override string Texture => "Terraria/Images/Item_1";
		public override void OnSpawn(Dust dust) {
			dust.alpha = 0;
		}
		public override bool Update(Dust dust) {
			dust.alpha++;
			if (dust.alpha > 7) dust.active = false;
			return false;
		}
		public override bool MidUpdate(Dust dust) {
			return false;
		}
		public override bool PreDraw(Dust dust) {
			Main.spriteBatch.DrawLightningArcBetween(
				dust.position - Main.screenPosition,
				dust.velocity - Main.screenPosition,
				Main.rand.NextFloat(-4, 4),
				0.1f,
				(0.225f, new Color(35, 156, 169, 0) * 0.5f),
				(0.150f, new Color(80, 204, 219, 0) * 0.5f),
				(0.075f, new Color(80, 251, 255, 0) * 0.5f),
				(0.025f, new Color(200, 255, 255, 0) * 0.5f)
			);
			return false;
		}
	}
}
