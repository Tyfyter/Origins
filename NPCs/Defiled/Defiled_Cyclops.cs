using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Armor.Defiled;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Melee;
using Origins.Journal;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Defiled_Cyclops : Glowing_Mod_NPC, IDefiledEnemy, IWikiNPC, IJournalEntrySource {
		public Rectangle DrawRect => new(0, 7, 46, 58);
		public int AnimationFrames => 32;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.03f;
		public const float speedMult = 1.5f;
		public string EntryName => "Origins/" + typeof(Defiled_Cyclops_Entry).Name;
		public class Defiled_Cyclops_Entry : JournalEntry {
			public override string TextKey => "Defiled_Cyclops";
			public override JournalSortIndex SortIndex => new("The_Defiled", 12);
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 7;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 110;
			NPC.defense = 8;
			NPC.damage = 26;
			NPC.width = 28;
			NPC.height = 40;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.value = 90;
			NPC.knockBackResist = 0.5f;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type,
				ModContent.GetInstance<Underground_Defiled_Wastelands_Biome>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 50;
		public int MaxManaDrain => 10;
		public float Mana { get; set; }
		public void Regenerate(out int lifeRegen) {
			int factor = 48 / ((NPC.life / 10) + 1);
			lifeRegen = factor;
			Mana -= factor / 120f;// 1 mana for every 1 health regenerated
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.DesertCave) return 0;
			return Defiled_Wastelands.SpawnRates.LandEnemyRate(spawnInfo, false) * Defiled_Wastelands.SpawnRates.Cyclops;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Latchkey>(), 8, 2, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bone_Latcher>(), 38));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Lousy_Liver>(), 87));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Greaves>(), 525));
		}
		public override bool? CanFallThroughPlatforms() => NPC.directionY == 1 && NPC.target >= 0 && NPC.targetRect.Bottom > NPC.position.Y + NPC.height + NPC.velocity.Y;
		public override void AI() {
			if (Main.rand.NextBool(800)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle, NPC.Center);
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					Vector2 diff = NPC.GetTargetData().Center - NPC.Center;
					float dist = diff.Length();
					if (dist > 192 && ++NPC.localAI[0] > 180) {
						NPC.localAI[0] = 0;
						NPC.localAI[3] = Projectile.NewProjectileDirect(
							NPC.GetSource_FromAI(),
							NPC.Center,
							diff * 4 / dist,
							ModContent.ProjectileType<Defiled_Cyclops_Bone_Latcher>(),
							10,
							4,
							ai2: NPC.whoAmI
						).identity;
					}
				}
			}
			if (NPC.localAI[3] != -1) {
				Projectile arm = Main.projectile.FirstOrDefault(x => x.identity == NPC.localAI[3]);
				if (arm is null || !arm.active || arm.type != ModContent.ProjectileType<Defiled_Cyclops_Bone_Latcher>()) {
					NPC.localAI[3] = -1;
				}
			}
			if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X /= speedMult;
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 5) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 58) % 406, 46, 56);
				NPC.frameCounter = 0;
			}
		}
		public void SpawnWisp(NPC npc)
		{
			if (Main.masterMode || (Main.expertMode && Main.rand.NextBool()))
			{
				NPC.NewNPC(npc.GetSource_Death(), (int)npc.position.X, (int)npc.position.Y, ModContent.NPCType<Defiled_Wisp>());
			}
		}
		public override void PostAI() {
			if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X *= speedMult;
			/*if(!attacking) {
				npc.Hitbox = new Rectangle((int)npc.position.X+(npc.oldDirection == 1 ? 70 : 52), (int)npc.position.Y, 56, npc.height);
			}*/
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;)
				Origins.instance.SpawnGoreByName(NPC.GetSource_OnHit(NPC), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_OnHit(NPC), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(NPC.localAI[3]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.localAI[3] = reader.ReadSingle();
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			base.PostDraw(spriteBatch, screenPos, drawColor);
			if (NPC.localAI[3] != -1) return;
			int segments = 20;
			List<Vector2> controlPoints = new(segments);
			float rangeMultiplier = 0.8f;
			float num = 1f / Defiled_Cyclops_Bone_Latcher.UseTime;
			float num2 = 0.5f;
			float num3 = 1f + num2;
			float num4 = MathHelper.Pi * 10f * (1f - num * num3) * (-NPC.spriteDirection) / segments;
			float num5 = num * num3;
			float num6 = 0f;
			if (num5 > 1f) {
				num6 = (num5 - 1f) / num2;
				num5 = MathHelper.Lerp(1f, 0f, num6);
			}
			float num7 = (Defiled_Cyclops_Bone_Latcher.UseTime * 2) * num;
			float num8 = 4 * num7 * num5 * rangeMultiplier / segments;
			float num9 = 1f;
			Vector2 playerArmPosition = NPC.Center + ArmStartOffset(NPC);
			Vector2 vector = playerArmPosition;
			float num10 = -MathHelper.PiOver2;
			Vector2 vector2 = vector;
			float num11 = MathHelper.PiOver2 + MathHelper.PiOver2 * NPC.spriteDirection;
			Vector2 vector3 = vector;
			float num12 = MathHelper.PiOver2;
			controlPoints.Add(playerArmPosition);
			float rot = new Vector2(NPC.spriteDirection, 0).ToRotation();
			for (int i = 0; i < segments; i++) {
				float num13 = i / (float)segments;
				float num14 = num4 * num13 * num9;
				Vector2 vector4 = vector + num10.ToRotationVector2() * num8;
				Vector2 vector5 = vector3 + num12.ToRotationVector2() * (num8 * 2f);
				Vector2 vector6 = vector2 + num11.ToRotationVector2() * (num8 * 2f);
				float num15 = 1f - num5;
				float num16 = 1f - num15 * num15;
				Vector2 value = Vector2.Lerp(vector5, vector4, num16 * 0.9f + 0.1f);
				Vector2 vector7 = Vector2.Lerp(vector6, value, num16 * 0.7f + 0.3f);
				Vector2 spinningpoint = playerArmPosition + (vector7 - playerArmPosition) * new Vector2(1f, num3);
				float num17 = num6;
				num17 *= num17;
				Vector2 item = spinningpoint.RotatedBy(rot + 4.712389f * num17 * NPC.spriteDirection, playerArmPosition);
				controlPoints.Add(item);
				num10 += num14;
				num12 += num14;
				num11 += num14;
				vector = vector4;
				vector3 = vector5;
				vector2 = vector6;
			}


			SpriteEffects flip = NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Defiled_Cyclops_Bone_Latcher>()].Value;

			Vector2 pos = controlPoints[1];
			for (int i = 1; i < controlPoints.Count; i++) {
				// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
				// You can change them if they don't!
				Rectangle frame = new Rectangle(0, 0, 48, 28);
				Vector2 origin = new Vector2(24, 14);
				Vector2 scale = new Vector2(0.85f);

				if (i == controlPoints.Count - 1) {
					frame.Y = 112;
				} else if (i > 10) {
					frame.Y = 84;
				} else if (i > 5) {
					frame.Y = 56;
				} else if (i > 0) {
					frame.Y = 28;
				}

				Vector2 element = controlPoints[i];
				Vector2 diff;
				if (i == controlPoints.Count - 1) {
					diff = element - controlPoints[i - 1];
				} else {
					diff = controlPoints[i + 1] - element;
				}

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, drawColor, rotation, origin, scale, flip, 0);

				pos += diff;
			}
		}
		public static Vector2 ArmStartOffset(NPC npc) {
			Vector2 offset = Vector2.Zero;
			switch (npc.frame.Y / npc.frame.Height) {
				case 0:
				offset = new(14, -5);
				break;
				case 1:
				offset = new(13, -8);
				break;
				case 2:
				case 3:
				offset = new(13, -10);
				break;
				case 4:
				offset = new(14, -5);
				break;
				case 5:
				offset = new(14, -7);
				break;
				case 6:
				offset = new(13, -10);
				break;
			}
			offset.X *= -npc.direction;
			return offset;
		}
	}
	public class Defiled_Cyclops_Bone_Latcher : ModProjectile, ITangelaHaver {
		public static int UseTime => 45;
		public override string Texture => typeof(Bone_Latcher_P).GetDefaultTMLName();
		public NPC Owner => Main.npc[(int)Projectile.ai[2]];
		public override void SetStaticDefaults() {
			Amebic_Vial.CanBeDeflected[Type] = false;
			AssimilationLoader.AddProjectileAssimilation<Defiled_Assimilation>(Type, 0.04f);
		}
		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.aiStyle = -1;
			Projectile.ownerHitCheck = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.extraUpdates = 1;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.WhipSettings.Segments = 20;
			Projectile.WhipSettings.RangeMultiplier = 0.8f * Projectile.scale;
		}
		public override void AI() {
			NPC owner = Owner;
			if (!owner.active) Projectile.active = false;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi / 2f;
			Projectile.ai[0] += 1f;
			float timeToFlyOut = UseTime * Projectile.MaxUpdates;
			Vector2 handPosition = owner.Center + Defiled_Cyclops.ArmStartOffset(owner);
			Projectile.Center = handPosition + Projectile.velocity * (Projectile.ai[0] - 1f);
			Projectile.spriteDirection = Vector2.Dot(Projectile.velocity, Vector2.UnitX) >= 0f ? 1 : -1;
			if (Projectile.ai[0] >= timeToFlyOut) {
				Projectile.Kill();
				return;
			}
			if (Projectile.ai[0] >= (int)(timeToFlyOut / 2f) && Projectile.soundDelay >= 0) {
				Projectile.soundDelay = -1;
				Projectile.WhipPointsForCollision.Clear();
				FillWhipControlPoints(Projectile, handPosition, Projectile.WhipPointsForCollision);
				Vector2 vector = Projectile.WhipPointsForCollision[^1];
				SoundEngine.PlaySound(SoundID.Item153, vector);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Projectile.WhipPointsForCollision.Clear();
			NPC owner = Owner;
			Vector2 handPosition = owner.Center + Defiled_Cyclops.ArmStartOffset(owner);
			FillWhipControlPoints(Projectile, handPosition, Projectile.WhipPointsForCollision);
			for (int m = 0; m < Projectile.WhipPointsForCollision.Count; m++) {
				Point point = Projectile.WhipPointsForCollision[m].ToPoint();
				projHitbox.Location = new Point(point.X - projHitbox.Width / 2, point.Y - projHitbox.Height / 2);
				if (projHitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			float range = 160 * Projectile.scale;
			if (target.DistanceSQ(Owner.Center) > range * range) {
				modifiers.HitDirectionOverride = -modifiers.HitDirection;
				modifiers.Knockback *= 1.5f;
			}
		}
		private void DrawLine(List<Vector2> list) {
			if (TangelaVisual.DrawOver) return;
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new Vector2(frame.Width / 2, 0);

			Vector2 pos = list[0];
			float progress = 0;
			for (int i = 0; i < list.Count - 1; i++) {
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				float length = diff.Length();
				Vector2 scale = new Vector2(2, length / frame.Height);

				TangelaVisual.DrawTangela(this, texture, pos - Main.screenPosition, frame, rotation, origin, scale, SpriteEffects.None, extraOffset: new(0, progress));

				pos += diff;
				progress += length;
			}
		}
		public int? TangelaSeed { get => (int)Projectile.ai[2]; set { } }
		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = new List<Vector2>();
			NPC owner = Owner;
			Vector2 handPosition = owner.Center + Defiled_Cyclops.ArmStartOffset(owner);
			FillWhipControlPoints(Projectile, handPosition, list);
			DrawLine(list);

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.instance.LoadProjectile(Type);
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 pos = list[1];

			for (int i = 1; i < list.Count; i++) {
				// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
				// You can change them if they don't!
				Rectangle frame = new Rectangle(0, 0, 48, 28);
				Vector2 origin = new Vector2(24, 14);
				Vector2 scale = new Vector2(0.85f) * Projectile.scale;

				if (i == list.Count - 1) {
					frame.Y = 112;
				} else if (i > 10) {
					frame.Y = 84;
				} else if (i > 5) {
					frame.Y = 56;
				} else if (i > 0) {
					frame.Y = 28;
				}

				Vector2 element = list[i];
				Vector2 diff;
				if (i == list.Count - 1) {
					diff = element - list[i - 1];
				} else {
					diff = list[i + 1] - element;
				}

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
				Color color = Lighting.GetColor(element.ToTileCoordinates());
				NPCLoader.DrawEffects(owner, ref color);
				color = owner.GetNPCColorTintedByBuffs(color);

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			return false;
		}
		public static void FillWhipControlPoints(Projectile proj, Vector2 playerArmPosition, List<Vector2> controlPoints) {
			int timeToFlyOut = UseTime * proj.MaxUpdates;
			int segments = proj.WhipSettings.Segments;
			float rangeMultiplier = proj.WhipSettings.RangeMultiplier;
			float num = proj.ai[0] / timeToFlyOut;
			float num2 = 0.5f;
			float num3 = 1f + num2;
			float num4 = MathHelper.Pi * 10f * (1f - num * num3) * (-proj.spriteDirection) / segments;
			float num5 = num * num3;
			float num6 = 0f;
			if (num5 > 1f) {
				num6 = (num5 - 1f) / num2;
				num5 = MathHelper.Lerp(1f, 0f, num6);
			}
			float num7 = (UseTime * 2) * num;
			float num8 = proj.velocity.Length() * num7 * num5 * rangeMultiplier / segments;
			float num9 = 1f;
			Vector2 vector = playerArmPosition;
			float num10 = -MathHelper.PiOver2;
			Vector2 vector2 = vector;
			float num11 = MathHelper.PiOver2 + MathHelper.PiOver2 * proj.spriteDirection;
			Vector2 vector3 = vector;
			float num12 = MathHelper.PiOver2;
			controlPoints.Add(playerArmPosition);
			for (int i = 0; i < segments; i++) {
				float num13 = i / (float)segments;
				float num14 = num4 * num13 * num9;
				Vector2 vector4 = vector + num10.ToRotationVector2() * num8;
				Vector2 vector5 = vector3 + num12.ToRotationVector2() * (num8 * 2f);
				Vector2 vector6 = vector2 + num11.ToRotationVector2() * (num8 * 2f);
				float num15 = 1f - num5;
				float num16 = 1f - num15 * num15;
				Vector2 value = Vector2.Lerp(vector5, vector4, num16 * 0.9f + 0.1f);
				Vector2 vector7 = Vector2.Lerp(vector6, value, num16 * 0.7f + 0.3f);
				Vector2 spinningpoint = playerArmPosition + (vector7 - playerArmPosition) * new Vector2(1f, num3);
				float num17 = num6;
				num17 *= num17;
				Vector2 item = spinningpoint.RotatedBy(proj.rotation + 4.712389f * num17 * proj.spriteDirection, playerArmPosition);
				controlPoints.Add(item);
				num10 += num14;
				num12 += num14;
				num11 += num14;
				vector = vector4;
				vector3 = vector5;
				vector2 = vector6;
			}
		}
	}
}
