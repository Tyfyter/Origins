using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Graphics;
using Origins.Items.Weapons.Ammo;
using Origins.Tiles;
using Origins.World.BiomeData;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.Defiled {
	//Very important comment, do not delete: 4265737420706F6E79
	public class Defiled_Chrysalis : ModNPC, IDefiledEnemy {
		public override string Texture => "Terraria/Images/NPC_0";
		public float Mana { get; set; }
		static Asset<Texture2D>[] textures;
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			textures = new Asset<Texture2D>[3];
			for (int i = 0; i < textures.Length; i++) textures[i] = ModContent.Request<Texture2D>("Origins/Gores/NPCs/DF_Effect_Medium" + (i + 1));
		}
		public override void SetDefaults() {
			NPC.lifeMax = 1000;
			NPC.SuperArmor = true;
			NPC.noGravity = true;
			NPC.immortal = true;
			NPC.knockBackResist = 0;
			NPC.behindTiles = true;
		}
		NPC referenceNPC;
		ChunkData[] chunks;
		int animationTimeMax = 0;
		public override void OnSpawn(IEntitySource source) {
			if (NPC.ai[0] == 0) NPC.ai[0] = ModContent.NPCType<Defiled_Cyclops>();
			if (NPC.ai[1] != 0) {
				animationTimeMax = (int)NPC.ai[1];
				NPC.ai[1] = 0;
			}
			if (NPC.ai[2] == 0) NPC.ai[2] = Main.rand.Next();
			NPC.netUpdate = true;
		}
		public override void AI() {
			if (Main.rand.NextBool(8)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitch(-0.5f).WithVolume(0.4f), NPC.Center);
			if (Main.rand.NextBool(4)) SoundEngine.PlaySound(SoundID.Item48.WithPitch(-1f).WithVolume(0.55f), NPC.Center);
			if (NPC.ai[0] <= 0) {
				NPC.active = false;
				return;
			}
			if (referenceNPC is null) {
				referenceNPC = new();
				referenceNPC.SetDefaults((int)NPC.ai[0]);
				if (animationTimeMax == 0) {
					animationTimeMax = (int)MathF.Pow(referenceNPC.width * referenceNPC.height, 0.333f) * 10;
				}
				Vector2 bottom = NPC.Bottom;
				NPC.Size = referenceNPC.Size;
				NPC.Bottom = bottom;
			}
			if (chunks is null) {
				NPC.TargetClosest();
				UnifiedRandom rand = new((int)NPC.ai[2]);
				List<Vector2> vectors;
				if (referenceNPC.ModNPC is IDefiledEnemy defiledEnemy && defiledEnemy.GetCustomChrysalisShape(NPC) is (Rectangle startArea, Predicate<Vector2> customShape)) {
					vectors = Main.rand.PoissonDiskSampling(startArea, customShape, 12);
				} else {
					vectors = Main.rand.PoissonDiskSampling(NPC.Hitbox, 12);
				}
				chunks = new ChunkData[vectors.Count];
				for (int i = 0; i < chunks.Length; i++) {
					chunks[i] = new(rand, vectors[i], animationTimeMax);
				}
			}
			if (++NPC.ai[1] > animationTimeMax) {
				NPC.Transform(referenceNPC.netID);
				if (NPC.ModNPC is IDefiledEnemy defiledEnemy) defiledEnemy.OnChrysalisSpawn();
				IEntitySource source = NPC.GetSource_FromThis();
				Vector2 from = new(NPC.position.X + NPC.width  * 0.5f, NPC.position.Y + NPC.height * 0.75f);
				for (int i = 0; i < chunks.Length; i++) {
					chunks[i].Explode(source, from);
				}
			}
			if (Main.masterMode) {
				int chanked = 0;
				for (int i = 0; i < chunks.Length; i++) {
					if (NPC.ai[1] >= chunks[i].animationTimeEnd) chanked += 1;
				}
				foreach (Player player in Main.ActivePlayers) {
					if (NPC.Center.Clamp(player.Hitbox).IsWithin(NPC.Center, chanked * 12 * 0.5f)) player.AddBuff(Rasterized_Debuff.ID, chanked);
				}
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			// fix DragonLens spawning npcs bc i keep accidentally softlocking myself
			if (NPC.IsABestiaryIconDummy) return false;
			//no clue how I missed the 'u' key so badly
			int chanked = 0;
			for (int i = 0; i < chunks.Length; i++) {
				chunks[i].Draw((int)NPC.ai[1]);
				if (NPC.ai[1] >= chunks[i].animationTimeEnd) chanked += 1;
			}
			if (Main.masterMode) {
				Mask_Rasterize.QueueDrawData(new(
					TextureAssets.Projectile[Bile_Dart_Aura.ID].Value,
					NPC.Center - Main.screenPosition,
					null,
					new Color(
						0.5f,
						0.5f,
					0f),
					0,
					new Vector2(36),
					chanked * 12f / TextureAssets.Projectile[Bile_Dart_Aura.ID].Width(),
					0
				));
			}
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(animationTimeMax);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			animationTimeMax = reader.ReadInt32();
		}
		public readonly struct ChunkData {
			readonly Vector2 basePos;
			readonly Vector2 groundPos;
			readonly int animationTimeStart;
			public readonly int animationTimeEnd;
			readonly int texture;
			public ChunkData(UnifiedRandom rand, Vector2 basePos, int animationTimeMax) {
				this.basePos = basePos;
				texture = rand.Next(textures.Length);
				int tries = 100;
				float dist = 0;
				const float max_dist = 16 * 10;
				while (tries > 0) {
					Vector2 direction = rand.NextFloat(MathHelper.TwoPi).ToRotationVector2();
					dist = CollisionExt.Raymarch(basePos, direction, max_dist);
					Vector2 endPoint = basePos + direction * dist;
					Tile tileSafely = Framing.GetTileSafely(endPoint + direction);
					if ((tileSafely.HasTile && (TileLoader.GetTile(tileSafely.TileType) is IDefiledTile || tries <= 15)) || --tries <= 0) {
						groundPos = endPoint + direction * 16;
						break;
					}
				}
				animationTimeStart = rand.Next(animationTimeMax - 20);
				animationTimeEnd = animationTimeStart + (int)((dist / max_dist + rand.NextFloat()) * 0.5f * 20);
			}
			public readonly void Draw(int progress) {
				Vector2 pos = Vector2.Lerp(groundPos, basePos, Utils.Remap(progress, animationTimeStart, animationTimeEnd, 0, 1));
				Main.spriteBatch.Draw(
					textures[texture].Value,
					pos - Main.screenPosition,
					null,
					Lighting.GetColor(pos.ToTileCoordinates()),
					0,
					textures[texture].Size() * 0.5f,
					1,
					SpriteEffects.None,
				0);
			}
			public readonly void Explode(IEntitySource source, Vector2 from) {
				Origins.instance.SpawnGoreByName(
					source,
					basePos,
					(basePos - from).SafeNormalize(default).RotatedByRandom(0.1f) * Main.rand.NextFloat(1, 4),
					"Gores/NPCs/DF_Effect_Medium" + (texture + 1)
				);
			}
		}
	}
	public class Defiled_Chrysalis_Nearby_Spawn : ModNPC, IDefiledEnemy {
		public override string Texture => "Terraria/Images/NPC_0";
		public float Mana { get; set; }
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			lastSpawnInfo = spawnInfo;
			return Defiled_Wastelands.SpawnRates.LandEnemyRate(spawnInfo, false) * Defiled_Wastelands.SpawnRates.Nearby * ContentExtensions.DifficultyDamageMultiplier;
		}
		static NPCSpawnInfo lastSpawnInfo;
		public override int SpawnNPC(int tileX, int tileY) {
			Vector2 playerTilePosition = lastSpawnInfo.Player.position / 16f;
			int minX = (int)playerTilePosition.X - NPC.safeRangeX;
			int maxX = (int)playerTilePosition.X + NPC.safeRangeX;
			int minY = (int)playerTilePosition.Y - NPC.safeRangeY;
			int maxY = (int)playerTilePosition.Y + NPC.safeRangeY;
			int spawnTileType = 0;
			int newTileX = 0;
			int newTileY = 0;
			for (int i = 0; i < 50; i++) {
				bool foundValidSpot = false;
				int x = Main.rand.Next(minX, maxX);
				int y = Main.rand.Next(minY, maxY);
				if (!Main.tile[x, y].HasUnactuatedTile || !Main.tileSolid[Main.tile[x, y].TileType]) {
					for (int y1 = y; y1 < Main.maxTilesY && y1 < maxY; y1++) {
						if (Main.tile[x, y1].HasUnactuatedTile && Main.tileSolid[Main.tile[x, y1].TileType]) {
							if (!playerTilePosition.IsWithin(new(x, y1), 10)) {
								spawnTileType = Main.tile[x, y1].TileType;
								newTileX = x;
								newTileY = y1 - 1;
								foundValidSpot = true;
							}
							break;
						}
					}
					if (foundValidSpot) {
						int num22 = newTileX - 3 / 2;
						int num23 = newTileX + 3 / 2;
						int num24 = newTileY - 3;
						int num25 = newTileY;
						if (num22 < 0) {
							foundValidSpot = false;
						}
						if (num23 > Main.maxTilesX) {
							foundValidSpot = false;
						}
						if (num24 < 0) {
							foundValidSpot = false;
						}
						if (num25 > Main.maxTilesY) {
							foundValidSpot = false;
						}
						if (foundValidSpot) {
							for (int x1 = num22; x1 < num23; x1++) {
								for (int y1 = num24; y1 < num25; y1++) {
									if (Main.tile[x1, y1].HasUnactuatedTile && Main.tileSolid[Main.tile[x1, y1].TileType]) {
										foundValidSpot = false;
										break;
									}
									if (Main.tile[x1, y1].LiquidType == LiquidID.Lava) {
										foundValidSpot = false;
										break;
									}
								}
							}
						}
					}
				}
				if (foundValidSpot) {
					break;
				}
			}

			lastSpawnInfo.SpawnTileX = newTileX;
			lastSpawnInfo.SpawnTileY = newTileY;
			lastSpawnInfo.SpawnTileType = spawnTileType;
			WeightedRandom<int> rand = new();
			List<(int npcType, SpawnRate condition)> spawns = ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().Spawns;
			for (int i = 0; i < spawns.Count; i++) {
				(int npcType, SpawnRate condition) = spawns[i];
				if (npcType == Type) continue;
				if (npcType == ModContent.NPCType<Ancient_Defiled_Cyclops>()) continue;
				if (npcType == ModContent.NPCType<Ancient_Defiled_Flyer>()) continue;
				rand.Add(npcType, condition.Rate(lastSpawnInfo));
			}
			return NPC.NewNPC(null, newTileX * 16 + 8, newTileY * 16, ModContent.NPCType<Defiled_Chrysalis>(), ai0: rand.Get());
		}
	}
}
