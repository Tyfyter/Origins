using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Armor.Defiled;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Magic;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Defiled_Flyer : Glowing_Mod_NPC, IDefiledEnemy, IWikiNPC, ITangelaHaver {
		public AssimilationAmount? Assimilation => 0.02f;
		public Rectangle DrawRect => new(-30, 28, 104, 38);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = Vector2.UnitY,
				PortraitPositionXOverride = -25,
				Velocity = 1f
			};
			DefiledGlobalNPC.NPCTransformations.Add(NPCID.Crimera, Type);
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.aiStyle = -1;
			NPC.lifeMax = 48;
			NPC.damage = 20;
			NPC.defense = 5;
			NPC.width = 30;
			NPC.height = 30;
			NPC.catchItem = 0;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.knockBackResist = 0.75f;
			NPC.value = 76;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 35;
		public int MaxManaDrain => 15;
		public float Mana { get; set; }
		public void Regenerate(out int lifeRegen) {
			int factor = 57 / ((NPC.life / 10) + 1);
			lifeRegen = factor;
			Mana -= factor / 180f;
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerFloorY > Main.worldSurface + 50 || spawnInfo.SpawnTileY >= Main.worldSurface - 50) return 0;
			return Defiled_Wastelands.SpawnRates.FlyingEnemyRate(spawnInfo) * Defiled_Wastelands.SpawnRates.Flyer * (spawnInfo.Player.ZoneSkyHeight ? 2 : 1);
		}
		public override int SpawnNPC(int tileX, int tileY) {
			tileY = OriginGlobalNPC.GetAerialSpawnPosition(tileX, tileY, this);
			if (tileY == -1) return Main.maxNPCs;
			return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Latchkey>(), 8, 2, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Krunch_Mix>(), 19));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Infusion>(), 44));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Greaves>(), 525));
		}
		public override void AI() {
			if (Main.rand.NextBool(900)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(1f, 1.1f), NPC.Center);
			NPC.TargetClosest();
			Vector2 direction;
			if (NPC.HasValidTarget) {
				Vector2 target = NPC.GetTargetData().Center;
				if (!CollisionExt.CanHitRay(NPC.Center, target) && Pathfind(NPC.GetTargetData()) is Vector2 pathTarget) {
					target = pathTarget;
				}
				direction = NPC.DirectionTo(target);
				float oldRot = NPC.rotation;
				GeometryUtils.AngularSmoothing(ref NPC.rotation, direction.ToRotation(), 0.1f);
				float diff = GeometryUtils.AngleDif(oldRot, NPC.rotation, out int dir) * 0.75f;
				NPC.velocity = NPC.velocity.RotatedBy(diff * dir) * (1 - diff * 0.1f);
			} else {
				direction = Vector2.UnitX * NPC.direction;
				GeometryUtils.AngularSmoothing(ref NPC.rotation, NPC.direction * MathHelper.PiOver2 + MathHelper.PiOver2, 0.1f);
			}
			NPC.velocity *= 0.98f;
			if (++NPC.ai[0] > 60) {
				NPC.velocity += direction * 7;
				NPC.ai[0] = 0;
			}
			NPC.spriteDirection = Math.Sign(Math.Cos(NPC.rotation));
		}
		public Vector2? Pathfind(NPCAimedTarget target) {
			int bestMatch = -1;
			float bestMatchDist = float.PositiveInfinity;
			Vector2 diff = target.Center - NPC.Center;
			for (int i = 0; i < dirs.Length; i++) {
				float dist = dirs[i].ToVector2().DistanceSQ(diff);
				if (dist < bestMatchDist) {
					bestMatchDist = dist;
					bestMatch = i;
				}
			}
			diff.Normalize();
			Vector2 position = NPC.position;
			position += diff * CollisionExt.Raymarch(position, diff, 16 * 50);
			Vector2 clockwiseTarget = default;
			Vector2 counterclockwiseTarget = default;
			if (Crawl(target, position.ToTileCoordinates(), bestMatch, false) is Point pos1) {
				clockwiseTarget = pos1.ToWorldCoordinates();
				/*Rectangle rect = new(0, 0, 16, 16);
				rect.X = pos1.X * 16;
				rect.Y = pos1.Y * 16;
				OriginExtensions.DrawDebugOutline(rect);*/
			}
			if (Crawl(target, position.ToTileCoordinates(), bestMatch, true) is Point pos2) {
				counterclockwiseTarget = pos2.ToWorldCoordinates();
			}
			if (clockwiseTarget == default && counterclockwiseTarget == default) return null;
			if (target.Center.DistanceSQ(clockwiseTarget) > target.Center.DistanceSQ(counterclockwiseTarget)) {
				return counterclockwiseTarget;
			} else {
				return clockwiseTarget;
			}
		}
		static Point[] dirs = [
			new(0, -1),
			new(1, -1),
			new(1, 0),
			new(1, 1),
			new(0, 1),
			new(-1, 1),
			new(-1, 0),
			new(-1, -1)
		];
		public Point? Crawl(NPCAimedTarget target, Point pos, int dir, bool counterclockwise) {
			const float max_dist = 16 * 50;
			static bool IsValidPosition(Point pos) => !Framing.GetTileSafely(pos).HasFullSolidTile();
			static bool IsOnes(Point p) => p.X is -1 or 1 && p.Y is -1 or 1;
			List<Point> path = [pos];
			//Rectangle rect = new(0, 0, 16, 16);
			test:
			if (CollisionExt.CanHitRay(pos.ToWorldCoordinates(), target.Position)) {
				if (CollisionExt.CanHitRay(NPC.Center, path[^1].ToWorldCoordinates())) return path[^1];
				if (path.Count <= 1) return null;
				Point? nextTarget = path[1];
				for (int i = 1; i < path.Count; i++) {
					/*rect.X = path[i].X * 16;
					rect.Y = path[i].Y * 16;
					OriginExtensions.DrawDebugOutline(rect);*/
					if (CollisionExt.CanHitRay(NPC.Center, path[i].ToWorldCoordinates())) {
						nextTarget = path[i];
					} else {
						Point step = path[i] - nextTarget.Value;
						if (IsOnes(step)) {
							for (int j = 0; j < dirs.Length; j++) {
								if (step == dirs[j]) {
									nextTarget += dirs[(j + (counterclockwise ? -1 : 1) + dirs.Length) % dirs.Length];
									break;
								}
							}
						}
					}
				}
				return nextTarget;
			}
			if (counterclockwise) {
				for (int i = 0; i < dirs.Length; i++) {
					int nextDir = (dir - i + dirs.Length) % dirs.Length;
					if (IsValidPosition(pos + dirs[nextDir])) {
						pos += dirs[nextDir];
						dir = (nextDir + 2) % dirs.Length;
						break;
					}
				}
			} else {
				for (int i = 0; i < dirs.Length; i++) {
					int nextDir = (dir + i) % dirs.Length;
					if (IsValidPosition(pos + dirs[nextDir])) {
						pos += dirs[nextDir];
						dir = (nextDir - 2 + dirs.Length) % dirs.Length;
						break;
					}
				}
			}
			if (path.Contains(pos) || pos.ToWorldCoordinates().DistanceSQ(target.Center) > max_dist * max_dist) return null;
			path.Add(pos);
			goto test;
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void FindFrame(int frameHeight) {
			float frame = (NPC.IsABestiaryIconDummy ? (float)++NPC.frameCounter : NPC.ai[0]) / 60;
			if (NPC.frameCounter > 1) NPC.frameCounter = 0;
			NPC.frame = new Rectangle(0, (38 * (int)(frame * frame * Main.npcFrameCount[Type] + 2)) % 152, 104, 36);
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(yMult: -0.5f), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 2; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Mana);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Mana = reader.ReadSingle();
		}
		public (Rectangle startArea, Predicate<Vector2> customShape)? GetCustomChrysalisShape(NPC chrysalisNPC) {
			Rectangle area = chrysalisNPC.Hitbox;
			area.Width += 70;
			if (chrysalisNPC.direction > 0) area.X -= 70;
			return (area, pos => area.Contains(pos));
		}
		public void OnChrysalisSpawn() {
			if (NPC.direction == -1) {
				NPC.rotation = MathHelper.Pi;
			}
		}
		public int? TangelaSeed { get; set; }
		public AutoLoadingAsset<Texture2D> tangelaTexture = typeof(Defiled_Flyer).GetDefaultTMLName() + "_Tangela";
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects spriteEffects = SpriteEffects.FlipHorizontally;
			if (NPC.spriteDirection == -1) {
				spriteEffects |= SpriteEffects.FlipVertically;
			}
			if (NPC.IsABestiaryIconDummy) NPC.rotation = NPC.velocity.ToRotation();
			Vector2 halfSize = new(GlowTexture.Width * 0.5f, GlowTexture.Height / Main.npcFrameCount[NPC.type] / 2);
			Vector2 position = new(NPC.position.X - screenPos.X + (NPC.width / 2) - GlowTexture.Width * NPC.scale / 2f + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + NPC.height - GlowTexture.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4f + halfSize.Y * NPC.scale + Main.NPCAddHeight(NPC) + NPC.gfxOffY);
			Vector2 origin = new(halfSize.X * 1.6f, halfSize.Y);
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				position,
				NPC.frame,
				drawColor,
				NPC.rotation,
				origin,
				NPC.scale,
				spriteEffects,
			0);
			spriteBatch.Draw(
				GlowTexture,
				position,
				NPC.frame,
				Color.White,
				NPC.rotation,
				origin,
				NPC.scale,
				spriteEffects,
			0);

			TangelaVisual.DrawTangela(
				this,
				tangelaTexture,
				position,
				NPC.frame,
				NPC.rotation,
				origin,
				new(NPC.scale),
				spriteEffects
			);
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }
	}
}
