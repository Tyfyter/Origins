using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Weapons.Demolitionist;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.Riven {
	public class Goo_Wall : ModNPC, IRivenEnemy/*, IWikiNPC*/ {
		public AssimilationAmount? Assimilation => 0.0001f;
		public override void SetStaticDefaults() {
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.lifeMax = 560;
			NPC.damage = 0;
			NPC.width = 0;
			NPC.height = 0;
			NPC.friendly = false;
			NPC.knockBackResist = 0;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type
			];
		}

		private int minTileRange = (160);
		private int maxTileRange = (160) * 2;
		private List<Vector2> raysAvailable = [];
		public new virtual float SpawnChance(NPCSpawnInfo spawnInfo) {
			minTileRange = 160;
			maxTileRange = 160 * 2;
			int initialDegree = 30;
			int raysToCast = 7;
			raysAvailable = [];
			Vector2 spawn = new(spawnInfo.SpawnTileX * 16, spawnInfo.SpawnTileY * 16 + 16);
			for (int i = -raysToCast; i < raysToCast; i++) {
				Vector2 dir = (-Vector2.UnitY).RotatedBy(i * MathHelper.ToRadians(initialDegree));
				float ray = CollisionExt.Raymarch(spawn, dir, float.BitIncrement(maxTileRange));

				if (ray >= minTileRange && ray <= maxTileRange) {
					Vector2 rayPos = (dir * ray) + spawn;
					Tile tile = Main.tile[rayPos.ToTileCoordinates()];
					if (tile.HasFullSolidTile())
						raysAvailable.Add(rayPos);
				}
			}

			Main.NewText($"Ray count: {raysAvailable.Count}");

			string str = string.Empty;
			for (int i = 0; i < raysAvailable.Count; i++) {
				if (!string.IsNullOrEmpty(str)) str += ", ";
				str += $"index{i}: {raysAvailable[i]}";
			}
			Main.NewText(str);

			return Riven_Hive.SpawnRates.LandEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Wall * Math.Sign(raysAvailable.Count);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ameballoon>(), 1, 5, 8));
		}
		public override int SpawnNPC(int tileX, int tileY) {
			Vector2 pickedRay = raysAvailable[Main.rand.Next(raysAvailable.Count)];

			NPC npc = NPC.NewNPCDirect(null, tileX * 16 + 8, tileY * 16 + 8, Type);
			(npc.ai[1], npc.ai[2]) = pickedRay;
			return npc.whoAmI;
		}

		private List<(Vector2 start, Vector2 end)> lines = new(4);
		private bool InsideWall(Entity tgt) {
			lines.Clear();
			Vector2 anchor = new(NPC.ai[1], NPC.ai[2]);
			Vector2 diff = (NPC.Bottom - anchor).RotatedBy(MathHelper.PiOver2);
			diff.Normalize();
			diff *= 26;

			Vector2 vert1 = NPC.Bottom + diff * 26;
			Vector2 vert2 = anchor - diff * 26;
			Vector2 vert3 = anchor + diff * 26;
			Vector2 vert4 = NPC.Bottom - diff * 26;

			lines.AddRange([(vert1, vert2), (vert2, vert3), (vert3, vert4), (vert4, vert1)]);

			return CollisionExtensions.PolygonIntersectsRect([.. lines], tgt.Hitbox);
		}

		public NPCSpawnInfo SpawnPool(Player plr) {
			Point playerTile = plr.Center.ToTileCoordinates();
			Point npcTile = NPC.Center.ToTileCoordinates();
			NPCSpawnInfo spawnPool = new() {
				PlayerFloorX = playerTile.X,
				PlayerFloorY = playerTile.Y + 8,
				Player = plr,
				SpawnTileX = npcTile.X,
				SpawnTileY = npcTile.Y + 8,
				SpawnTileType = ModContent.TileType<Riven_Flesh>()
			};
			(int x, int y) = (spawnPool.SpawnTileX, spawnPool.SpawnTileY);
			(Tile tile1, Tile tile2) = (Main.tile[x, y + 1], Main.tile[x, y + 2]);
			bool isWet = tile1.LiquidAmount > 0 && tile2.LiquidAmount > 0 && (tile1.LiquidType == LiquidID.Honey || tile1.LiquidType == LiquidID.Water);
			spawnPool.Water = isWet;
			spawnPool.Player = plr;
			return spawnPool;
		}
		private static void PlayEnterSound(Entity tgt) => SoundEngine.PlaySound(SoundID.Shimmer2, tgt.Center);
		public override void AI() {
			NPC.ai[0]--;
			Vector2 anchor = new(NPC.ai[1], NPC.ai[2]);
			FixExploitManEaters.ProtectSpot(NPC.position.ToTileCoordinates().X, NPC.position.ToTileCoordinates().Y);
			FixExploitManEaters.ProtectSpot(anchor.ToTileCoordinates().X, anchor.ToTileCoordinates().Y);
			
			foreach (NPC tgt in Main.ActiveNPCs) {
				if (tgt?.ModNPC is not IRivenEnemy && InsideWall(tgt)) {
					tgt.velocity *= 0.4f;
					if (tgt.TryGetGlobalNPC(out Goo_Wall_Global global) && !global.Walled) PlayEnterSound(tgt);
					tgt.AddBuff(Goo_Wall_Debuff.ID, 60);
					OriginGlobalNPC.InflictTorn(tgt, 180, targetSeverity: 0.001f);
				}
			}

			foreach (Player tgt in Main.ActivePlayers) {
				if (InsideWall(tgt)) {
					if (!tgt.OriginPlayer().walledDebuff) PlayEnterSound(tgt);
					tgt.OriginPlayer().walledDebuff = true;
					tgt.OriginPlayer().InflictAssimilation<Riven_Assimilation>(Assimilation.Value.GetValue(NPC, tgt));
					OriginPlayer.InflictTorn(tgt, 180, targetSeverity: 0.0001f);
					if (NPC.ai[0] <= 0) {
						WeightedRandom<int> pool = new(Main.rand);
						SpawnPool selectedPool = ModContent.GetInstance<Riven_Hive.SpawnRates>();
						for (int k = 0; k < selectedPool.Spawns.Count; k++) {
							(int npcType, SpawnRate condition) = selectedPool.Spawns[k];
							if (npcType != Type) pool.Add(npcType, condition.Rate(SpawnPool(tgt)));
						}
						NPC spawn = NPC.NewNPCDirect(NPC.GetSource_ReleaseEntity(), NPC.Center, pool.Get());
						NetMessage.SendData(MessageID.SyncNPC, number: spawn.whoAmI);
						SoundEngine.PlaySound(SoundID.NPCDeath1.WithPitch(0.4f).WithVolumeScale(0.5f), tgt.Center);
						SoundEngine.PlaySound(SoundID.Item102.WithPitch(1), tgt.Center);
						if (spawn.active) NPC.ai[0] = 8 * 60;
						//if (spawn.active) NPC.ai[0] = 60;
					}
				}
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2 anchor = new(NPC.ai[1], NPC.ai[2]);
			Vector2 origin = NPC.Bottom;
			float rot = origin.DirectionTo(anchor).ToRotation() + MathHelper.PiOver2;
			float dst = origin.Distance(anchor);

			Texture2D texture = TextureAssets.Npc[Type].Value;

			//spriteBatch.DrawLine(Color.White, origin, anchor);

			Rectangle frame = new(0, 182, 52, 18);
			Main.EntitySpriteDraw(texture, origin - screenPos, frame, GetAlpha(drawColor).Value, rot, new(frame.Width * 0.5f, 0), 1, SpriteEffects.None);

			frame = new(0, 20, 52, 160);
			Main.EntitySpriteDraw(texture, anchor - screenPos, frame, GetAlpha(drawColor).Value, rot, new(frame.Width * 0.5f, 0), new Vector2(1, (dst / frame.Height)), SpriteEffects.None);

			frame = new(0, 0, 52, 18);
			Main.EntitySpriteDraw(texture, anchor - screenPos, frame, GetAlpha(drawColor).Value, rot, new(frame.Width * 0.5f, 18), 1, SpriteEffects.None);
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 15; i++) {
					//Vector2.Lerp()
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				}
			}
		}
		public override Color? GetAlpha(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
	}

	public class Goo_Wall_Debuff : ModBuff {
		public override string Texture => typeof(Goo_Wall).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			if (npc.TryGetGlobalNPC(out Goo_Wall_Global global)) global.Walled = true;
		}
	}

	public class Goo_Wall_Global : GlobalNPC {
		public override bool InstancePerEntity => true;
		public bool Walled = false;

		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return !entity.buffImmune[Goo_Wall_Debuff.ID];
		}
		public override void ResetEffects(NPC npc) {
			Walled = false;
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (Walled) npc.lifeRegen -= 2;
		}
	}
}
