using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.MiscE {
	public class Abandoned_Bomb : Glowing_Mod_NPC {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 12;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.lifeMax = 120;
			NPC.damage = 65;
			NPC.width = 30;
			NPC.height = 48;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.noTileCollide = false;
			NPC.friendly = false;
		}
		public int Frame {
			get {
				NPC.frame.Height = 624 / Main.npcFrameCount[NPC.type];
				return NPC.frame.Y / NPC.frame.Height;
			}
			set => NPC.frame.Y = NPC.frame.Height * value;
		}
		Guid owner = Guid.Empty;
		bool CanTargetPlayer(Player player) => NPC.WithinRange(player.Center, 16 * 25) && (owner == Guid.Empty || owner == player.OriginPlayer().guid);
		public override void AI() {

			#region Find target
			// Starting search distance
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.Players, CanTargetPlayer);
			if (searchResults.FoundTarget) {
				NPC.target = searchResults.NearestTargetIndex;
				NPC.targetRect = searchResults.NearestTargetHitbox;
				if (NPC.ShouldFaceTarget(ref searchResults)) NPC.FaceTarget();
			}


			//projectile.friendly = foundTarget;
			#endregion

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 6f;
			float inertia = 8f;

			NPCAimedTarget target = NPC.GetTargetData();
			if (!target.Invalid) {
				Vector2 direction = target.Center - NPC.Center;
				int directionX = Math.Sign(direction.X);
				NPC.spriteDirection = directionX;
				bool wallColliding = NPC.collideX;
				float YRatio = direction.Y / (direction.X * -directionX + 0.1f);
				if (direction.Y < 160 && (wallColliding || YRatio > 1) && NPC.collideY) {
					float jumpStrength = 6;
					if (wallColliding) NPC.TryJumpOverObstacles(directionX);
					else if (YRatio > 1.1f) {
						jumpStrength++;
						if (YRatio > 1.2f) {
							jumpStrength++;
							if (YRatio > 1.3f) jumpStrength++;
						}
					}
					NPC.velocity.Y = -jumpStrength;
				}
				Rectangle hitbox = new((int)NPC.Center.X - 24, (int)NPC.Center.Y - 24, 48, 48);
				int targetMovDir = Math.Sign(target.Velocity.X);
				Rectangle targetFutureHitbox = target.Hitbox;
				targetFutureHitbox.X += (int)(target.Velocity.X * 10);
				bool waitForStart = targetMovDir == directionX && !hitbox.Intersects(targetFutureHitbox);
				if (!target.Center.IsWithin(NPC.Center, 48) || !hitbox.Intersects(target.Hitbox) || waitForStart && NPC.ai[0] <= 0f) {
					NPC.ai[0] = 0f;
					direction.Normalize();
					direction *= speed;
					NPC.velocity.X = (NPC.velocity.X * (inertia - 1) + direction.X) / inertia;
				} else {
					NPC.velocity.X = NPC.velocity.X * (inertia - 1) / inertia;
					if (NPC.ai[0] <= 0f) NPC.ai[0] = 1;
				}
			}
			#endregion

			#region Animation and visuals
			if (target.Invalid) {
				NPC.velocity.X *= 0.93f;
				Frame = 11;
			} else if(NPC.ai[0] > 0) {
				Frame = 7 + (int)(NPC.ai[0] / 9f);
				if (++NPC.ai[0] > 36) {
					Rectangle npcHitbox = NPC.Hitbox;
					npcHitbox.X += npcHitbox.Width / 2;
					npcHitbox.Y += npcHitbox.Height / 2;
					npcHitbox.Width = 128;
					npcHitbox.Height = 128;
					npcHitbox.X -= npcHitbox.Width / 2;
					npcHitbox.Y -= npcHitbox.Height / 2;
					ExplosiveGlobalProjectile.ExplosionVisual(npcHitbox, SoundID.Item62);
					NPC.active = false;
				}
			} else if (NPC.collideY) {
				NPC.localAI[1]--;
				const int frameSpeed = 4;
				if (Math.Abs(NPC.velocity.X) < 0.01f) NPC.velocity.X = 0f;
				if (NPC.velocity.X != 0 ^ NPC.oldVelocity.X != 0) NPC.frameCounter = 0;
				if (NPC.velocity.X != 0) {
					NPC.frameCounter++;
					if (NPC.frameCounter >= frameSpeed) {
						NPC.frameCounter = 0;
						Frame++;
						if (Frame >= 6) Frame = 0;
					}
				} else {
					NPC.frameCounter++;
					if (NPC.frameCounter >= frameSpeed) {
						NPC.frameCounter = 0;
						Frame = 0;
					}
				}
				Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
				Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			} else if (Frame > 6) {
				Frame = 1;
			}

			// Some visuals here
			if (Frame > 11) {
				Lighting.AddLight(NPC.Center, Color.CornflowerBlue.ToVector3() * 0.18f);
			} else if (Frame < 9) {
				Lighting.AddLight(NPC.Center, Color.Red.ToVector3() * 0.24f);
			}
			#endregion
		}
		public override bool NeedSaving() => true;
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			if (NPC.ai[0] >= 36) {
				npcHitbox.X += npcHitbox.Width / 2;
				npcHitbox.Y += npcHitbox.Height / 2;
				npcHitbox.Width = 128;
				npcHitbox.Height = 128;
				npcHitbox.X -= npcHitbox.Width / 2;
				npcHitbox.Y -= npcHitbox.Height / 2;
			} else {
				npcHitbox = default;
			}
			return true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[NPC.type].Value;
			spriteBatch.Draw(
				texture,
				NPC.Center - screenPos,
				NPC.frame,
				drawColor,
				NPC.rotation,
				new Vector2(28, 25),
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
			0);
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Rectangle frame = NPC.frame;
			if (owner == Guid.Empty) frame.X += frame.Width;
			spriteBatch.Draw(
				GlowTexture,
				NPC.Center - screenPos,
				frame,
				Color.White,
				NPC.rotation,
				new Vector2(28, 25),
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
			0);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground
			);
		}
		static (Point pos, Guid owner) chosenSource;
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			const int dist = 30;
			const int distSQ = dist * dist;
			List<(Point pos, Guid owner)> bombPositions = ModContent.GetInstance<Abandoned_Bomb_System>().BombPositions;
			(Point pos, Guid owner)[] validSources = bombPositions.Where(p => {
				int aSQ = p.pos.X - spawnInfo.SpawnTileX;
				int bSQ = p.pos.Y - spawnInfo.SpawnTileY;
				return (aSQ * aSQ) + (bSQ * bSQ) < distSQ;
			}).ToArray();
			if (validSources.Length <= 0) return 0;
			chosenSource = Main.rand.Next(validSources);
			return 5f;
		}
		public override void OnSpawn(IEntitySource source) {
			owner = chosenSource.owner;
			List<(Point pos, Guid owner)> bombPositions = ModContent.GetInstance<Abandoned_Bomb_System>().BombPositions;
			int index = bombPositions.FindIndex(chosenSource.Equals);
			if (index > -1) bombPositions.RemoveAt(index);
			chosenSource = default;
		}
		public override bool CheckActive() {
			int activeRangeX = (int)(NPC.sWidth * 2.1);
			int activeRangeY = (int)(NPC.sHeight * 2.1);
			bool keepActive = false;
			Rectangle rectangle = new((int)(NPC.position.X + NPC.width / 2 - activeRangeX), (int)(NPC.position.Y + NPC.height / 2 - activeRangeY), activeRangeX * 2, activeRangeY * 2);
			Rectangle rectangle2 = new((int)((double)(NPC.position.X + NPC.width / 2) - NPC.sWidth * 0.5 - NPC.width), (int)((double)(NPC.position.Y + NPC.height / 2) - NPC.sHeight * 0.5 - NPC.height), NPC.sWidth + NPC.width * 2, NPC.sHeight + NPC.height * 2);
			foreach (Player player in Main.ActivePlayers) {
				Rectangle hitbox = player.Hitbox;
				if (rectangle.Intersects(hitbox)) {
					keepActive = true;
				}
				if (rectangle2.Intersects(hitbox)) {
					NPC.timeLeft = NPC.activeTime;
					NPC.despawnEncouraged = false;
				}
			}
			if (--NPC.timeLeft <= 0) {
				keepActive = false;
			}
			if (!keepActive && Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.active = false;
				if (Main.netMode == NetmodeID.Server) {
					NPC.netSkip = -1;
					NPC.life = 0;
					NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);
				}
				ModContent.GetInstance<Abandoned_Bomb_System>().BombPositions.Add((NPC.Center.ToTileCoordinates(), owner));
			}
			return false;
		}
	}
	public class Abandoned_Bomb_System : ModSystem {
		List<(Point pos, Guid owner)> bombPositions = [];
		public List<(Point pos, Guid owner)> BombPositions => bombPositions ??= [];
		public override void LoadWorldData(TagCompound tag) {
			bombPositions = [];
			if (tag.TryGet("positions", out List<TagCompound> positions)) {
				foreach (TagCompound position in positions) {
					bombPositions.Add((position.Get<Vector2>("pos").ToPoint(), Guid.Parse(position.Get<string>("owner"))));
				}
			}
		}
		public override void SaveWorldData(TagCompound tag) {
			bombPositions ??= [];
			tag["positions"] = bombPositions.Select(p => new TagCompound() {
				["pos"] = p.pos.ToVector2(),
				["owner"] = p.owner.ToString()
			}).ToList();
		}
		public override void ClearWorld() {
			bombPositions = [];
		}
	}
}