using Origins.Buffs;
using Origins.Items.Weapons.Demolitionist;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using PegasusLib;
using Terraria;
using Terraria.Audio;
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
			NPC.width = 52;
			NPC.height = 160;
			NPC.friendly = false;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type
			];
		}
		public new virtual float SpawnChance(NPCSpawnInfo spawnInfo) {
			Point toCheck = new(NPC.width >> 4, NPC.height >> 4);
			int notFullyGrounded = 1;
			for (int i = -toCheck.X; i < toCheck.X; i++) {
				Tile tile = Main.tile[spawnInfo.SpawnTileX + i, spawnInfo.SpawnTileY];
				Tile tile1 = Main.tile[spawnInfo.SpawnTileX + i, spawnInfo.SpawnTileY - toCheck.Y];
				if ((tile.HasTile && tile.HasSolidTile()) || (tile1.HasTile && tile1.HasFullSolidTile())) {
					notFullyGrounded = 0;
					break;
				}
			}
			return (Riven_Hive.SpawnRates.LandEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Wall) * notFullyGrounded;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ameballoon>(), 1, 5, 8));
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
			foreach (NPC tgt in Main.ActiveNPCs) {
				if (tgt?.ModNPC is not IRivenEnemy && NPC.Hitbox.Intersects(tgt.Hitbox)) {
					tgt.velocity *= 0.4f;
					if (tgt.TryGetGlobalNPC(out Goo_Wall_Global global) && !global.Walled) PlayEnterSound(tgt);
					tgt.AddBuff(Goo_Wall_Debuff.ID, 60);
					OriginGlobalNPC.InflictTorn(tgt, 180, targetSeverity: 0.001f);
				}
			}

			foreach (Player tgt in Main.ActivePlayers) {
				if (NPC.Hitbox.Intersects(tgt.Hitbox)) {
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
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 15; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
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
