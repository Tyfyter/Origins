using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Armor.Riven;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Tools;
using Origins.Items.Weapons.Summoner;
using Origins.Journal;
using Origins.World.BiomeData;
using PegasusLib;
using System.Collections.Generic;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Misc.Physics;

namespace Origins.NPCs.Riven {
	public class Riven_Fighter : ModNPC, IRivenEnemy, IWikiNPC, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Riven_Protoform_Entry).Name;
		public class Riven_Protoform_Entry : JournalEntry {
			public override string TextKey => "Riven_Protoform";
			public override JournalSortIndex SortIndex => new("Riven", 6);
		}
		public Rectangle DrawRect => new(0, 6, 68, 56);
		public int AnimationFrames => 32;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.09f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 8;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 81;
			NPC.defense = 10;
			NPC.damage = 33;
			NPC.width = 40;
			NPC.height = 46;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath60.WithPitch(1f).WithVolume(0.5f);
			NPC.value = 90;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public new virtual float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Riven_Hive.SpawnRates.LandEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Fighter;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Jam_Sandwich>(), 16));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Symbiote_Skull>(), 40));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Mask>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Coat>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Pants>(), 525));
		}
		public override bool? CanFallThroughPlatforms() => NPC.directionY == 1 && NPC.target >= 0 && NPC.targetRect.Bottom > NPC.position.Y + NPC.height + NPC.velocity.Y;
		public override bool PreAI() {
			if (Main.rand.NextBool(1000)) {
				SoundEngine.PlaySound(SoundID.Zombie12.WithPitch(1f).WithVolume(0.35f), NPC.Center);
				SoundEngine.PlaySound(Origins.Sounds.WCIdle.WithPitchRange(1.75f, 2f).WithVolume(0.35f), NPC.Center);
			}
			NPC.localAI[0]++;
			if (NPC.localAI[0] > 210 && (NPC.collideY || NPC.velocity.Y == 0)) {
				NPC.localAI[0] = 0;
				switch (NPC.aiAction) {
					case 0:
					if (SearchForGrapplePoint(out Vector2 direction)) {
						SoundEngine.PlaySound(SoundID.Item95.WithPitch(1f).WithVolume(0.75f), NPC.Center);
						NPC.ai[2] = NPC.SpawnProjectile(null,
							NPC.Center,
							direction * 16,
							ModContent.ProjectileType<Riven_Protoform_Hook>(),
							0,
							0,
							ai2: NPC.whoAmI
						)?.identity ?? -1;
						NPC.aiAction = 3;
						NPC.netUpdate = true;
						break;
					}
					NPC.aiAction = (int)(Main.GlobalTimeWrappedHourly % 2 + 1);
					if (NPC.aiAction == 2) {
						SoundEngine.PlaySound(SoundID.Item174.WithPitch(1f), NPC.Center);
						NPC.velocity = ((NPC.GetTargetData().Center - new Vector2(0, 16) - NPC.Center) * new Vector2(1, 4)).WithMaxLength(12);
						NPC.collideX = false;
						NPC.collideY = false;
					}
					NPC.netUpdate = true;
					break;
					case 1:
					case 2:
					case 4:
					NPC.aiAction = 0;
					break;
				}
			}
			if (NPC.aiAction != 0) {
				if (NPC.aiAction == 3) {
					Projectile hook = null;
					foreach (Projectile proj in Main.ActiveProjectiles) {
						if (proj.identity == NPC.ai[2] && proj.ModProjectile is Riven_Protoform_Hook) {
							hook = proj;
							break;
						}
					}
					if (hook is not null) {
						if (hook.ai[0] == 2) {
							Rectangle targetHitbox = NPC.GetTargetData().Hitbox;
							DoGrapple(ref NPC.velocity, NPC.Center, hook.Center, out Vector2 normDiff);
							if ((NPC.localAI[0] >= 15 && Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, fallThrough: true).LengthSquared() < 0.5f) || Vector2.Dot(normDiff, NPC.Center.DirectionTo(targetHitbox.Center())) < 0) {
								hook.Kill();
								NPC.aiAction = 4;
							}
						}
					} else {
						NPC.aiAction = 4;
					}
				} else if (NPC.aiAction == 1) {
					NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X + NPC.direction * 0.15f, -6, 6);
					NPC.localAI[0] += NPC.collideX ? 9 : 3;
				} else if (NPC.collideX || NPC.collideY) {
					NPC.localAI[0] = 300;
				}
				return false;
			}
			return true;
		}
		bool SearchForGrapplePoint(out Vector2 direction) {
			direction = default;
			bool Hits(Vector2 grappleCenter) {
				Rectangle targetHitbox = NPC.GetTargetData().Hitbox;
				Vector2 targetCenter = targetHitbox.Center();
				Rectangle hitbox = NPC.Hitbox;
				Vector2 center = NPC.Center;
				Vector2 velocity = NPC.velocity;
				int halfWidth = hitbox.Width / 2;
				int halfHeight = hitbox.Height / 2;
				bool grappling = true;
				for (int i = 0; i < 60; i++) {
					if (grappling) {
						DoGrapple(ref velocity, center, grappleCenter, out Vector2 normDiff);
						if (Vector2.Dot(normDiff, center.DirectionTo(targetCenter)) < 0) grappling = false;
					}
					velocity.Y += NPC.gravity;
					if (velocity.Y > NPC.maxFallSpeed)
						velocity.Y = NPC.maxFallSpeed;
					center += velocity;
					//Dust.NewDustPerfect(center, grappling ? 27 : 6, Vector2.Zero);
					hitbox.X = (int)center.X - halfWidth;
					hitbox.Y = (int)center.Y - halfHeight;
					if (targetHitbox.Intersects(hitbox)) return true;
					if (hitbox.OverlapsAnyTiles()) return false;
				}
				return false;
			}
			for (int i = 0; i < 25; i++) {
				for (int j = 1; j < 35; j++) {
					Point grapplePoint = (NPC.Center + new Vector2(i * NPC.direction, -j) * 16).ToTileCoordinates();
					//Dust.NewDustPerfect(grapplePoint.ToWorldCoordinates(), 6, Vector2.Zero);
					if (!Riven_Protoform_Hook.AI_007_GrapplingHooks_CanTileBeLatchedOnTo(grapplePoint.X, grapplePoint.Y)) continue;
					Vector2 pos = grapplePoint.ToWorldCoordinates();
					if (Hits(pos)) {
						direction = NPC.Center.DirectionTo(pos);
						return true;
					}
				}
			}
			return false;
		}
		static void DoGrapple(ref Vector2 velocity, Vector2 center, Vector2 grappleCenter, out Vector2 normDiff) {
			Vector2 diff = grappleCenter - center;
			normDiff = diff.SafeNormalize(default);
			float dot = Vector2.Dot(normDiff, velocity.SafeNormalize(default));
			velocity = Vector2.Lerp(normDiff * 16, velocity, 0.85f + dot * 0.1f);
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
			}

		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(NPC.localAI[0]);
			writer.Write((byte)NPC.aiAction);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.localAI[0] = reader.ReadSingle();
			NPC.aiAction = reader.ReadByte();
		}
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(NPC.aiAction == 0 ? 7 : 0);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			//spawn gore if npc is dead after being hit
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(1, 4));
			}
			NPC.frame.Y = NPC.frame.Height * 0;
			NPC.frameCounter = 0;
		}
		public static AutoLoadingAsset<Texture2D> glowTexture = typeof(Riven_Fighter).GetDefaultTMLName() + "_Glow";
		public static AutoLoadingAsset<Texture2D> tailTexture = typeof(Riven_Fighter).GetDefaultTMLName() + "_Tail";
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			SpriteEffects effects = NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			DrawData data = new(
				texture,
				NPC.Center - Vector2.UnitY * 2 - screenPos,
				NPC.frame,
				drawColor,
				NPC.rotation,
				new Vector2(22, 27).Apply(effects, texture.Size()),
				NPC.scale,
				effects
			);
			data.Draw(spriteBatch);
			data.texture = glowTexture;
			data.color = Riven_Hive.GetGlowAlpha(drawColor);
			data.Draw(spriteBatch);
			if (NPC.aiAction != 3) {
				data.texture = tailTexture;
				data.Draw(spriteBatch);
			}
			return false;
		}
	}
	public class Riven_Protoform_Hook : ModProjectile {
		public override string Texture => typeof(Amoeba_Hook_P).GetDefaultTMLName();
		public NPC Owner => Main.npc[(int)Projectile.ai[2]];
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Projectile.type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
			Projectile.aiStyle = -1;
			Projectile.netImportant = true;
		}

		// Amethyst Hook is 300, Static Hook is 600
		public override float GrappleRange() {
			return 600f;
		}

		public override void NumGrappleHooks(Player player, ref int numHooks) {
			numHooks = 1;
		}

		// default is 11, Lunar is 24
		public override void GrappleRetreatSpeed(Player player, ref float speed) {
			speed = 11f;
		}

		public override void GrapplePullSpeed(Player player, ref float speed) {
			speed = 0f;
		}
		public override bool PreDrawExtras() {
			NPC owner = Owner;
			Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Flagellash_P>()].Value;
			Vector2 playerCenter = owner.Center + new Vector2(owner.direction * -12, 8);
			Vector2 center = Projectile.Center;
			Vector2 distToProj = playerCenter - Projectile.Center;
			float projRotation = distToProj.ToRotation() - 1.57f;
			float distance = distToProj.Length();
			distToProj.Normalize();
			distToProj *= 8f;
			int progress = -2;
			while (distance > 8f && !float.IsNaN(distance)) {
				center += distToProj;
				distance = (playerCenter - center).Length();
				Color drawColor = Lighting.GetColor(center.ToTileCoordinates());

				float dist = (8 + 2);
				if (progress + dist >= texture.Width - 2) {
					progress = 0;
				}
				Rectangle frame = new(0, progress + 2, 6, (int)dist);

				Main.EntitySpriteDraw(texture,
					center - Main.screenPosition,
					frame,
					GetAlpha(drawColor) ?? drawColor,
					projRotation,
					frame.Size() * 0.5f,
					1,
					SpriteEffects.None
				);
				progress += 8;
			}
			return false;
		}
		public override Color? GetAlpha(Color lightColor) => new Color((lightColor.R + 255) / 510f, (lightColor.G + 255) / 510f, (lightColor.B + 255) / 510f, 0.5f);
		public override void AI() {
			NPC owner = Owner;
			if (!owner.active) {
				Projectile.Kill();
				return;
			}

			Vector2 mountedCenter = owner.Center;
			Vector2 center = Projectile.Center;
			float num = mountedCenter.X - center.X;
			float num2 = mountedCenter.Y - center.Y;
			float num3 = (float)Math.Sqrt(num * num + num2 * num2);
			Projectile.rotation = (float)Math.Atan2(num2, num) - 1.57f;

			if (num3 > 2500f)
				Projectile.Kill();

			if (Projectile.ai[0] == 0f) {
				if (ProjectileLoader.GrappleOutOfRange(num3, Projectile)) {
					Projectile.ai[0] = 1f;
				}

				Vector2 vector3 = center - new Vector2(5f);
				Vector2 vector4 = center + new Vector2(5f);
				Point point = (vector3 - new Vector2(16f)).ToTileCoordinates();
				Point point2 = (vector4 + new Vector2(32f)).ToTileCoordinates();
				int num10 = point.X;
				int num11 = point2.X;
				int num12 = point.Y;
				int num13 = point2.Y;
				if (num10 < 0)
					num10 = 0;

				if (num11 > Main.maxTilesX)
					num11 = Main.maxTilesX;

				if (num12 < 0)
					num12 = 0;

				if (num13 > Main.maxTilesY)
					num13 = Main.maxTilesY;

				Vector2 vector5 = default;
				for (int l = num10; l < num11; l++) {
					for (int m = num12; m < num13; m++) {

						vector5.X = l * 16;
						vector5.Y = m * 16;
						if (!(vector3.X + 10f > vector5.X) || !(vector3.X < vector5.X + 16f) || !(vector3.Y + 10f > vector5.Y) || !(vector3.Y < vector5.Y + 16f))
							continue;

						// TML: Moved nactive check into method, changed from tile to coords
						if (!AI_007_GrapplingHooks_CanTileBeLatchedOnTo(l, m))
							continue;

						WorldGen.KillTile(l, m, fail: true, effectOnly: true);
						//SoundEngine.PlaySound(0, l * 16, m * 16);
						Projectile.velocity.X = 0f;
						Projectile.velocity.Y = 0f;
						Projectile.ai[0] = 2f;
						Projectile.position.X = l * 16 + 8 - Projectile.width / 2;
						Projectile.position.Y = m * 16 + 8 - Projectile.height / 2;
						Rectangle? tileVisualHitbox = WorldGen.GetTileVisualHitbox(l, m);
						if (tileVisualHitbox.HasValue)
							Projectile.Center = tileVisualHitbox.Value.Center.ToVector2();

						Projectile.netUpdate = true;

						break;
					}

					if (Projectile.ai[0] == 2f)
						break;
				}
			} else if (Projectile.ai[0] == 1f) {
				float pullSpeed = 11f;

				if (num3 < 24f)
					Projectile.Kill();

				num3 = pullSpeed / num3;
				num *= num3;
				num2 *= num3;
				Projectile.velocity.X = num;
				Projectile.velocity.Y = num2;
			} else if (Projectile.ai[0] == 2f) {
				Point point4 = center.ToTileCoordinates();
				if (!AI_007_GrapplingHooks_CanTileBeLatchedOnTo(point4.X, point4.Y)) {
					Projectile.ai[0] = 1f;
				}
			}
			if (Projectile.ai[0] != 2f) return;
			NPC player = Owner;
			if (player.Hitbox.Intersects(Projectile.Hitbox)) {
				if (Projectile.ai[1] > 0) Projectile.ai[1] = 26;
			} else {
				Projectile.ai[1]++;
			}
		}
		internal static bool AI_007_GrapplingHooks_CanTileBeLatchedOnTo(int x, int y) {
			Tile tile = Main.tile[x, y];
			return (Main.tileSolid[tile.TileType] || (tile.TileType == TileID.MinecartTrack)) && tile.HasUnactuatedTile;
		}
	}
}
