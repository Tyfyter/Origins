using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.Utils;
using Origins.Dev;
using Origins.Items.Tools;
using Origins.Tiles.Brine;
using Origins.Walls;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine {
	public class Airsnatcher : Brine_Pool_NPC, ICustomWikiStat {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[NPC.type] = 6;
			TargetNPCTypes.Add(ModContent.NPCType<King_Crab>());
			TargetNPCTypes.Add(ModContent.NPCType<Sea_Dragon>());
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new Vector2(0f, 12f),
				PortraitPositionYOverride = 32,
				Frame = 1
			};
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 300;
			NPC.defense = 50;
			NPC.damage = 0;
			NPC.npcSlots = 0;
			NPC.width = 38;
			NPC.height = 42;
			NPC.friendly = false;
			NPC.chaseable = false;
			NPC.HitSound = SoundID.Item127;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0f;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			int count = 0;
			const int max_count = 9;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.type == Type && (++count >= max_count || npc.WithinRange(new(spawnInfo.SpawnTileX * 16, (spawnInfo.SpawnTileY - 1) * 16), 16 * 8))) return 0;
			}
			int wallType = ModContent.WallType<Baryte_Wall>();
			HashSet<int> tileTypes = [
				ModContent.TileType<Baryte>(),
				ModContent.TileType<Peat_Moss>()
			];
			bool hasRightWall = false;
			for (int i = 1; i <= 1; i++) {
				Tile tile = Framing.GetTileSafely(spawnInfo.SpawnTileX + i, spawnInfo.SpawnTileY);
				if (!tile.HasTile || tile.BottomSlope || !tileTypes.Contains(tile.TileType)) return 0;
				for (int j = 1; j < NPC.height / 16; j++) {
					tile = Framing.GetTileSafely(spawnInfo.SpawnTileX + i, spawnInfo.SpawnTileY - j);
					if (tile.LiquidAmount < 255 || tile.LiquidType != Liquids.Brine.ID) return 0;
					if (!hasRightWall && tile.WallType == wallType) hasRightWall = true;
				}
			}
			if (!hasRightWall) return 0;
			return Brine_Pool.SpawnRates.Airsnatcher;
		}
		public override int SpawnNPC(int tileX, int tileY) {
			return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			/*npcLoot.Add(new LeadingConditionRule(DropConditions.PlayerInteraction).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Alkaliphiliac_Tissue>(), 1, 1, 4)
			));*/
		}
		public override void AI() {
			const int bubble_time = 60 * 12;
			if (NPC.ai[1] != 0) {
				if (NPC.ai[0] > 0) {
					NPC.ai[0] -= 5;
				} else {
					NPC.ai[0] = 0;
					NPC.ai[1] = 0;
				}
			} else if (NPC.ai[0] < bubble_time) {
				NPC.ai[0]++;
			}
			int frame = (int)((1 - NPC.ai[0] / bubble_time) * Main.npcFrameCount[Type]);
			if (frame == 0 && NPC.ai[1] == 0) frame = 1;
			if (frame >= Main.npcFrameCount[Type]) frame = Main.npcFrameCount[Type] - 1;
			NPC.frame.Y = 44 * frame;
			if (frame == 1 && NPC.ai[1] == 0) {
				const int extend = 16 * 20;
				Rectangle hitbox = NPC.Hitbox;
				hitbox.Y -= extend;
				hitbox.Height += extend;
				foreach (Player player in Main.ActivePlayers) {
					if (hitbox.Intersects(player.Hitbox)) {
						NPC.ai[1] = 1;
						if (Main.netMode != NetmodeID.MultiplayerClient) {
							Projectile.NewProjectile(
								NPC.GetSource_FromAI(),
								NPC.position + new Vector2(19, 2),
								Vector2.Zero,
								ModContent.ProjectileType<Airsnatcher_Bubble>(),
								0,
								0
							);
						}
						break;
					}
				}
			}
			NPC.timeLeft = 60;
		}
		public override void UpdateLifeRegen(ref int damage) {
			if (!NPC.GetWet(Liquids.Brine.ID)) {
				NPC.lifeRegen -= 120;
				if (damage < 0) damage = 0;
				damage += 30;
			}
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter >= 6 * 2) NPC.frameCounter = 0;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				/*Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center,
					Vector2.Zero,
					$"Gores/NPC/{nameof(Carpalfish)}_Gore_3"
				);
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center + GeometryUtils.Vec2FromPolar(-16, NPC.rotation),
					Vector2.Zero,
					$"Gores/NPC/{nameof(Carpalfish)}_Gore_2"
				);
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center + GeometryUtils.Vec2FromPolar(-32, NPC.rotation),
					Vector2.Zero,
					$"Gores/NPC/{nameof(Carpalfish)}_Gore_1"
				);*/
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			int frame = NPC.frame.Y / 44;
			if (NPC.ai[1] == 0 && frame != Main.npcFrameCount[Type] - 1) {
				Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<Airsnatcher_Bubble>()].Value;
				Rectangle rect = texture.Frame(verticalFrames: 3, frameY: (int)NPC.frameCounter / 6);
				if (frame >= 2) {
					rect.Inflate(-2, 0);
				}
				spriteBatch.Draw(
					texture,
					NPC.position + new Vector2(19, 2 + frame * 6) - screenPos,
					rect,
					drawColor,
					0,
					rect.Size() * 0.5f,
					1,
					0,
				0);
			}
			NPC.rotation = 0;
			return true;
		}
	}
	public class Airsnatcher_Bubble : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			ProjectileID.Sets.CanDistortWater[Type] = false;
			Hydrolantern_Force_Global.ProjectileTypes.Add(Type);
		}
		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 22;
		}
		static int PopFrameTime => 4;
		public override bool ShouldUpdatePosition() => Projectile.timeLeft > PopFrameTime;
		public override void AI() {
			if (Projectile.timeLeft <= PopFrameTime) {
				Projectile.frame = 2;
				return;
			}
			if (++Projectile.frameCounter >= 6) {
				Projectile.frame = Projectile.frame ^ 1;
			}
			if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height / 2)) {
				if (Projectile.velocity.Y > -2) Projectile.velocity.Y -= 0.1f;
				Projectile.velocity *= 0.97f;
				Rectangle hitbox = Projectile.Hitbox;
				float x = ContentExtensions.DifficultyDamageMultiplier;
				if (x > 4) x = 4;
				float seconds = 15 - (x - 1) * (5 - (x - 2));
				int time = (int)(seconds * 60 / 3);
				foreach (Player player in Main.ActivePlayers) {
					if (player.Hitbox.Intersects(hitbox)) {
						player.breath += Math.Max(Math.Min(time, (player.breathMax - player.breath) - 1), 0);
						if (Projectile.active) {
							Projectile.Kill();
						}
					}
				}
			} else {
				Projectile.Kill();
			}
		}
		public void Pop() {
			if (Projectile.timeLeft > PopFrameTime) {
				Projectile.timeLeft = PopFrameTime;
			}
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 8; i++) {
				Dust.NewDust(
					Projectile.position - Vector2.One * 2,
					Projectile.width + 4,
					Projectile.height + 4,
					DustID.BreatheBubble
				);
			}
			SoundEngine.PlaySound(SoundID.Drown.WithPitchVarience(0.6f), Projectile.Center);
			SoundEngine.PlaySound(SoundID.Item54.WithPitchRange(-1f, -0.2f), Projectile.Center);
		}
	}
}
