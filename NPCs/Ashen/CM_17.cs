using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.NPCs.Riven;
using Origins.World.BiomeData;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.Ashen {
	public class CM_17 : Glowing_Mod_NPC, IWikiNPC, IAshenEnemy, IBroken {
		public Rectangle DrawRect => new(0, 0, 142, 90);
		public int AnimationFrames => 6;
		public int FrameDuration => 8;
		public AutoLoadingTexture drillBit = typeof(CM_17).GetDefaultTMLName() + "_Drillbit";
		public AutoLoadingTexture lowerArm = typeof(CM_17).GetDefaultTMLName() + "_Lower";
		public AutoLoadingTexture upperArm = typeof(CM_17).GetDefaultTMLName() + "_Upper";
		protected SpriteEffects SpriteEffects => NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

		public static string BrokenReason => "remove debug info after balance testing";

		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 9;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with {
				Position = new Vector2(50f, 26f),
				PortraitPositionXOverride = 20,
				PortraitPositionYOverride = 0
			};
			GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, BiomeSpawnChance);
		}
		public override void SetDefaults() {
			NPC.lifeMax = 180;
			NPC.defense = 22;
			NPC.damage = 24;
			NPC.width = 142;
			NPC.height = 88;
			NPC.value = Item.buyPrice(0, 0, 6);
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			NPC.knockBackResist = 0.1f;
			SpawnModBiomes = [
				GetInstance<Underground_Ashen_Biome>().Type,
			];
		}
		public override bool? CanFallThroughPlatforms() => NPC.targetRect.Bottom > NPC.position.Y + NPC.height + NPC.velocity.Y;
		public static int TimeToSpawnWatchlings => 2 * 60;
		public override void AI() {
			const int MaxWatchlings = 10; // desired max subtracted by 2
			float accel = 0.15f;
			NPCAimedTarget target = NPC.GetTargetData();
			bool targetInvalid = target.Invalid;
			int currentMoveDirection = float.Sign(NPC.velocity.X);
			if (!NPC.collideY && NPC.velocity.Y == 0) {
				NPC.collideY = Collision.GetTilesIn(NPC.BottomLeft + Vector2.UnitY, NPC.BottomRight + Vector2.UnitY * 16).Any(pos => Framing.GetTileSafely(pos).HasSolidTile());
			}

			Vector2 targetDirection = targetInvalid ? default : NPC.DirectionTo(NPC.targetRect.Center());
			int targetMoveDirection = targetInvalid ? NPC.direction : float.Sign(targetDirection.X);
			Rectangle detectRange = NPC.Hitbox;
			Rectangle fleeRange = NPC.Hitbox;
			detectRange.Inflate(20 * 16, 15 * 16);
			fleeRange.Inflate(8 * 16, 5 * 16);
			detectRange.DrawDebugOutline();
			fleeRange.DrawDebugOutline();
			void AttemptRetarget() {
				if (NPC.ai[3] == 0) accel = 0;
				NPC.ai[0] = 0;
				NPC.TargetClosest(false);
			}
			bool HasMaxWatchings() {
				int count = 0;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc?.ModNPC is Watchling { OwnerID: int OwnerID } && OwnerID == NPC.whoAmI) {
						count++;
					}
					NPC.ai[1] = count; // for debugging
					if (count >= MaxWatchlings) break;
				}
				return count >= MaxWatchlings;
			}
			if (!targetInvalid && (target.Hitbox.Intersects(detectRange) || NPC.ai[3] == 1)) {
				NPC.ai[3] = 1;
				switch (NPC.aiAction) {
					case 0:
					targetMoveDirection = Math.Sign(target.Center.X - NPC.Center.X);
					if ((NPC.ai[2].Cooldown() || NPC.ai[2] == 0) && !HasMaxWatchings()) {
						NPC.aiAction = 1;
						NPC.netUpdate = true;
					}
					break;

					case 1:
					Vector2 pos = NPC.Center + new Vector2(55, -4).Apply(SpriteEffects, default);
					Dust.QuickDust(pos, Color.White);
					if (NPC.ai[0]++ == TimeToSpawnWatchlings * 0.5f) {
						for (int i = 0; i < 3; i++) {
							NPC watchling = NPC.SpawnNPC(null, (int)pos.X, (int)pos.Y, NPCType<Watchling>());
							watchling.velocity = new Vector2(-targetMoveDirection * 2, -2) + Main.rand.NextVector2Circular(3, 3);
						}
					} else if (NPC.ai[0] >= TimeToSpawnWatchlings) {
						NPC.ai[0] = 0;
						NPC.ai[2] = 1 * 60;
						NPC.aiAction = 0;
						NPC.netUpdate = true;
					}
					if (target.Hitbox.Intersects(fleeRange)) {
						targetMoveDirection = -Math.Sign(target.Center.X - NPC.Center.X);
						if (NPC.ai[0] < TimeToSpawnWatchlings * 0.5f) NPC.ai[0].Cooldown(rate: 2);
						else accel = 0;
					} else accel = 0;
					break;
				}
			} else AttemptRetarget();

			HasMaxWatchings();

			if (currentMoveDirection != targetMoveDirection) accel *= 0.25f;
			if (NPC.direction == 0) NPC.direction = -1;
			if (!NPC.collideY) accel *= 0.25f;
			NPC.velocity.X += accel * targetMoveDirection;
			NPC.velocity.X *= NPC.collideY ? 0.93f : 0.98f;
			float preStepOffY = NPC.gfxOffY;
			if (NPC.collideY) {
				Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
				Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			}
			if (accel != 0) NPC.direction = targetMoveDirection;
			if (NPC.collideY && Math.Abs(NPC.velocity.Y) == 0) {
				bool shouldJump = false;
				if (NPC.collideX && preStepOffY == NPC.gfxOffY) shouldJump = true;
				else if (!targetInvalid) {
					if (Math.Abs(NPC.Center.Y - target.Center.Y) <= 8.5f * 16 && Math.Abs(NPC.Center.X - target.Center.X) <= 4) {
						NPC.velocity.X *= 0.2f;
						shouldJump = true;
					} else if (target.Position.Y + target.Height < NPC.position.Y && !NPC.Hitbox.Add(new Vector2(NPC.width * 0.5f, 16)).OverlapsAnyTiles(false)) {
						shouldJump = true;
					}
				}
				if (shouldJump) NPC.velocity.Y -= 8;
			}
			NPC.spriteDirection = NPC.direction;
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			NPC.ai[3] = 1;
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			if (!projectile.npcProj) NPC.ai[3] = 1;
		}
		public static float BiomeSpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			if (spawnInfo.SpawnTileY < Main.rockLayer) return 0;
			return Ashen_Biome.SpawnRates.Watcher * (Main.hardMode ? 1 : 0.5f);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.CustomBestiaryName(Type, this.GetLocalizationKey("FullName"));
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void FindFrame(int frameHeight) {
			NPC.localAI[0] = (NPC.localAI[0] + 0.15f) % 2f;
			if (NPC.aiAction == 1 && NPC.ai[0] > 0) {
				int tmp = (int)(NPC.ai[0] / TimeToSpawnWatchlings * 2) + 6;
				NPC.frame.Y = Math.Min(tmp, 8) * frameHeight;
				return;
			}
			NPC.DoFrames(10, 0..6, Math.Abs(NPC.velocity.X));
			if (Math.Abs(NPC.velocity.X) < 0.3f) NPC.frame.Y = 0;
			if (!NPC.collideY && !NPC.IsABestiaryIconDummy && Math.Abs(NPC.velocity.Y) != 0) NPC.DoFrames(1, 5..6);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemType<Phoenum>(), 1, 1, 3));
			npcLoot.Add(new CommonDrop(ItemType<Exo_Legs>(), 300, 1, 1, 11));
			npcLoot.Add(ItemDropRule.ByCondition(new Journal_Entry_Condition(Journal_Registry.GetJournalEntryByTextKey(GetInstance<Worn_Paper_Smog_Test>().PaperName)), ItemType<Worn_Paper_Smog_Test>(), 40));
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			return base.PreDraw(spriteBatch, screenPos, drawColor);
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2[] offsets = new Vector2[Main.npcFrameCount[Type]];
			offsets[1] = new(0, 2);
			offsets[3] = new(0, 2);
			offsets[4] = new(0, 2);
			offsets[6] = new(0, 4);
			offsets[8] = new(0, 4);
			SpriteEffects effects = SpriteEffects;
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			base.PostDraw(spriteBatch, screenPos, drawColor);

			void SetDrawData(Texture2D texture, Vector2 offset, float rotation = 0f, Rectangle? frame = null) {
				DrawData data = new(texture, default, frame, drawColor, rotation, texture.Size() * 0.5f, NPC.scale, effects);
				if (frame is not null) {
					data.origin = frame.Value.Size() * 0.5f;
				}
				data.position = NPC.Center - screenPos + (offset.Apply(effects, default) + new Vector2(0, NPC.gfxOffY) * NPC.scale);
				data.Draw(spriteBatch);
			}

			SetDrawData(upperArm.Value, new Vector2(13, -15) + offsets[NPC.frame.Y / 90], NPC.rotation);

			Rectangle lowerFrame = lowerArm.Frame(1, 5, 0, NPC.frame.Y / 90);
			if (NPC.frame.Y / 90 > 4) lowerFrame.Y = 0;

			SetDrawData(lowerArm.Value, new Vector2(-28, -38) + offsets[NPC.frame.Y / 90], NPC.rotation, lowerFrame);

			SetDrawData(drillBit.Value, new Vector2(-98, -26), NPC.rotation, drillBit.Frame(1, 2, 0, (int)NPC.localAI[0]));

			spriteBatch.DrawDebugTextAbove(
				$"{NPC.direction} {NPC.spriteDirection}, {TimeToSpawnWatchlings}, {TimeToSpawnWatchlings * 0.5f}\n" +
				$"{NPC.ai[0]}, {NPC.ai[1]}, {NPC.ai[2]}, {NPC.ai[3]}\n" +
				$"{NPC.localAI[0]}, {NPC.localAI[1]}, {NPC.localAI[2]}, {NPC.localAI[3]}",
				NPC.Top - screenPos, scale: 1);
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(NPC.aiAction);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.aiAction = reader.ReadInt32();
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore1");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore2");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore3");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore4");
				for (int i = 0; i < 7; i++) {
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
				}
			} else if (Main.rand.NextBool(5)) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
			}
		}
	}
	public class Watchling : Glowing_Mod_NPC, IWikiNPC, IAshenEnemy, IBroken {
		public Rectangle DrawRect => new(0, 0, 32, 26);
		public int AnimationFrames => 6;
		public static string BrokenReason => "Balance test, change sounds";
		public int OwnerID = -1;
		public int SpawnCounter;
		public static int SpawnCounterMax => 60;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 6;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			NPCID.Sets.PositiveNPCTypesExcludedFromDeathTally[Type] = true;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.width = 28;
			NPC.height = 28;
			SetSharedDefaults();
		}
		public void SetSharedDefaults() {
			NPC.lifeMax = 81;
			NPC.defense = 10;
			NPC.damage = 33;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath24.WithPitch(0.6f);
			this.CopyBanner<CM_17>();
			SpawnModBiomes = [
				GetInstance<Underground_Ashen_Biome>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent { Entity: NPC { ModNPC: CM_17, whoAmI: int owner } }) OwnerID = owner;
		}
		public override bool PreAI() {
			if (!NPC.collideY && NPC.velocity.Y == 0) {
				NPC.collideY = Collision.GetTilesIn(NPC.BottomLeft + Vector2.UnitY, NPC.BottomRight + Vector2.UnitY * 16).Any(pos => Framing.GetTileSafely(pos).HasSolidTile());
			}
			if ((SpawnCounter > 0 || NPC.collideY) && SpawnCounter.Warmup(SpawnCounterMax)) NPC.netUpdate = true;
			if (SpawnCounter < SpawnCounterMax) {
				if (NPC.collideY) NPC.velocity.X *= 0.8f;
				return false;
			}
			return base.PreAI();
		}
		public void Transform<TNPC>() where TNPC : Watchling {
			int frame = NPC.frame.Y / NPC.frame.Height;
			double frameCounter = NPC.frameCounter;
			NPC.Transform(NPCType<TNPC>());
			NPC.frame.Y = frame * NPC.frame.Height;
			NPC.frameCounter = frameCounter;
			TNPC watch = (TNPC)NPC.ModNPC;
			watch.OwnerID = OwnerID;
			watch.SpawnCounter = SpawnCounter;
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) NPC.spriteDirection = NPC.direction;
			//increment frameCounter every frame and run the following code when it exceeds 7 (i.e. run the following code every 8 frames)

			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			if (NPC.velocity.Y == 0f && NPC.NPCCanStickToWalls()) Transform<Watchling_Wall>();
		}
		public override void FindFrame(int frameHeight) {
			if (SpawnCounter < SpawnCounterMax) NPC.frame.Y = (SpawnCounter * 3) / SpawnCounterMax * frameHeight;
			else if (NPC.collideY || NPC.IsABestiaryIconDummy) NPC.DoFrames(4, 3..);
			else NPC.DoFrames(1, 4..5);
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(SpawnCounter);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			SpawnCounter = reader.ReadInt32();
		}
	}
	public class Watchling_Wall : Watchling, ICustomWikiStat {
		bool ICustomWikiStat.CanExportStats => false;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
		}
		public override void SetDefaults() {// could not add stats because 
			NPC.CloneDefaults(NPCID.WallCreeperWall);
			NPC.aiStyle = NPCAIStyleID.Spider;
			NPC.width = 28;
			NPC.height = 28;
			SetSharedDefaults();
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) { }
		public override bool PreAI() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return true;
			if (!NPC.NPCCanStickToWalls()) Transform<Watchling>();
			return true;
		}
		public override void AI() { }
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(4, 3..);
		}
	}
}
