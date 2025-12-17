using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Core;
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
	public class Goo_Wall : ModNPC, IRivenEnemy, IMultiHitboxNPC/*, IWikiNPC*/ {
		public AssimilationAmount? Assimilation => 0.0001f;
		public Rectangle[] Hitboxes { get; private set; }
		public override void SetStaticDefaults() {
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
			AprilFoolsTextures.AddNPC(this);
		}
		public override void SetDefaults() {
			NPC.lifeMax = 280;
			NPC.damage = 1;
			NPC.width = 26;
			NPC.height = 26;
			NPC.friendly = false;
			NPC.noGravity = true;
			NPC.knockBackResist = 0;
			NPC.HitSound = SoundID.NPCHit9;
			NPC.DeathSound = SoundID.NPCDeath23;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type
			];
		}
		private List<Vector2> raysAvailable = [];
		public Vector2 Anchor1 => Vector2.Lerp(Anchor2, NPC.Center, 2);
		public Vector2 Anchor2 => new(NPC.ai[1], NPC.ai[2]);
		public override void ModifyHoverBoundingBox(ref Rectangle boundingBox) {
			if (Hitboxes is null) {
				boundingBox = default;
				return;
			}
			Rectangle mouseRectangle = new((int)(Main.mouseX + Main.screenPosition.X), (int)(Main.mouseY + Main.screenPosition.Y), 1, 1);
			for (int i = 0; i < Hitboxes.Length; i++) {
				if (Hitboxes[i].Intersects(mouseRectangle)) {
					boundingBox = Hitboxes[i];
					return;
				}
			}
		}
		public new virtual float SpawnChance(NPCSpawnInfo spawnInfo) {
			const int minTileRange = 160;
			const int maxTileRange = 160 * 2;
			Vector2 pos = new(spawnInfo.SpawnTileX * 16 + 8, spawnInfo.SpawnTileY * 16 + 8);
			foreach (NPC other in Main.ActiveNPCs) {
				if (other.ModNPC is Goo_Wall gooWall && (other.WithinRange(pos, maxTileRange) || gooWall.Anchor1.WithinRange(pos, maxTileRange) || gooWall.Anchor2.WithinRange(pos, maxTileRange))) {
					return 0;
				}
			}
			const int rays_to_cast = 7;
			float betweenRays = MathHelper.ToRadians(60) / rays_to_cast;
			raysAvailable = [];
			Vector2 spawn = new(spawnInfo.SpawnTileX * 16, spawnInfo.SpawnTileY * 16 + 16);
			for (int i = -rays_to_cast; i < rays_to_cast; i++) {
				Vector2 dir = -Vector2.UnitY.RotatedBy(i * betweenRays);
				float ray = CollisionExt.Raymarch(spawn, dir, float.BitIncrement(maxTileRange));

				if (ray >= minTileRange && ray <= maxTileRange) {
					Vector2 rayPos = (dir * (ray + 1)) + spawn;
					Tile tile = Main.tile[rayPos.ToTileCoordinates()];
					if (tile.HasFullSolidTile())
						raysAvailable.Add(rayPos);
				}
			}

			return Riven_Hive.SpawnRates.LandEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Wall * Math.Sign(raysAvailable.Count);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ameballoon>(), 1, 7, 11));
		}
		public override bool CanHitNPC(NPC target) => false;
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
		public override int SpawnNPC(int tileX, int tileY) {
			if (!IMultiHitboxNPC.SpawningEnabled) return Main.maxNPCs;
			Vector2 pickedRay = raysAvailable[Main.rand.Next(raysAvailable.Count)];
			Vector2 pos = Vector2.Lerp(new(tileX * 16 + 8, tileY * 16 + 8), pickedRay, 0.5f);
			return NPC.NewNPC(null, (int)pos.X, (int)(pos.Y + NPC.height / 2), Type, ai1: pickedRay.X, ai2: pickedRay.Y);
		}

		private (Vector2 start, Vector2 end)[] lines;
		public bool InsideWall(Entity tgt) {
			if (lines is null) {
				Vector2 anchor = Anchor2;
				Vector2 position = Anchor1;
				Vector2 perp = (position - anchor).RotatedBy(MathHelper.PiOver2).Normalized(out _) * 26;

				Vector2 vert1 = position + perp;
				Vector2 vert2 = anchor + perp;
				Vector2 vert3 = anchor - perp;
				Vector2 vert4 = position - perp;

				lines = [(vert1, vert2), (vert2, vert3), (vert3, vert4), (vert4, vert1)];
			}

			return CollisionExtensions.PolygonIntersectsRect(lines, tgt.Hitbox);
		}

		public NPCSpawnInfo SpawnInfo(Player plr) {
			Point playerTile = plr.Bottom.ToTileCoordinates();
			Point npcTile = NPC.Center.ToTileCoordinates();
			NPCSpawnInfo spawnInfo = new() {
				PlayerFloorX = playerTile.X,
				PlayerFloorY = playerTile.Y + 1,
				Player = plr,
				SpawnTileX = npcTile.X,
				SpawnTileY = npcTile.Y,
				SpawnTileType = ModContent.TileType<Spug_Flesh>()
			};
			(int x, int y) = (spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);
			(Tile tile1, Tile tile2) = (Main.tile[x, y + 1], Main.tile[x, y + 2]);
			bool isWet = tile1.LiquidAmount > 0 && tile2.LiquidAmount > 0 && (tile1.LiquidType == LiquidID.Honey || tile1.LiquidType == LiquidID.Water);
			spawnInfo.Water = isWet;
			spawnInfo.Player = plr;
			return spawnInfo;
		}
		private static void PlayEnterSound(Entity tgt) => SoundEngine.PlaySound(SoundID.Shimmer2, tgt.Center);
		void EnsureHitboxes() {
			if (Hitboxes is null) {
				Vector2 anchor = Anchor2;
				Vector2 position = Anchor1;
				Hitboxes = new Rectangle[12];
				Rectangle box = new(0, 0, 52, 52);

				Vector2 unitSquared = (anchor - position).Normalized(out _).Abs(out Vector2 signs);
				unitSquared *= signs / Math.Max(unitSquared.X, unitSquared.Y);
				Vector2 half = box.Size() * 0.5f;
				for (int i = 0; i < Hitboxes.Length; i++) {
					float progress = i / (float)(Hitboxes.Length - 1);
					Vector2 pos = Vector2.Lerp(position, anchor, progress) - half + Vector2.Lerp(-unitSquared, unitSquared, progress);
					box.X = (int)pos.X;
					box.Y = (int)pos.Y;
					Hitboxes[i] = box;
				}
			}
		}
		public override void AI() {
			Vector2 anchor = Anchor2;
			Vector2 position = Anchor1;
			EnsureHitboxes();
			NPC.ai[0]--;
			FixExploitManEaters.ProtectSpot(position.ToTileCoordinates().X, position.ToTileCoordinates().Y);
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
						NPCSpawnInfo spawnInfo = SpawnInfo(tgt);
						for (int k = 0; k < selectedPool.Spawns.Count; k++) {
							(int npcType, SpawnRate condition) = selectedPool.Spawns[k];
							if (npcType != Type) pool.Add(npcType, condition.Rate(spawnInfo));
						}
						NPC spawn = NPC.NewNPCDirect(NPC.GetSource_ReleaseEntity(), NPC.Center, pool.Get());
						NetMessage.SendData(MessageID.SyncNPC, number: spawn.whoAmI);
						SoundEngine.PlaySound(SoundID.NPCDeath1.WithPitch(0.4f).WithVolumeScale(0.5f), tgt.Center);
						SoundEngine.PlaySound(SoundID.Item102.WithPitch(1), tgt.Center);
						if (spawn.active) NPC.ai[0] = 8 * 60;
					}
				}
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2 anchor = Anchor2;
			Vector2 origin = Anchor1;
			Vector2 scale = new(1);
			Color color = GetAlpha(drawColor).Value;
			if (NPC.IsABestiaryIconDummy) {
				anchor = NPC.Center - new Vector2(0, 50);
				origin = Vector2.Lerp(anchor, NPC.Center, 2);
				scale = new(0.8f, 1);
				color = color.MultiplyRGBA(new(0.9f, 0.9f, 0.9f));
			}
			float rot = origin.DirectionTo(anchor).ToRotation() + MathHelper.PiOver2;
			float dst = origin.Distance(anchor);

			Texture2D texture = TextureAssets.Npc[Type].Value;

			Rectangle frame = new(0, 182, 52, 18);
			Main.EntitySpriteDraw(texture, origin - screenPos, frame, color, rot, new(frame.Width * 0.5f, 0), scale, SpriteEffects.None);

			frame = new(0, 20, 52, 160);
			Main.EntitySpriteDraw(texture, anchor - screenPos, frame, color, rot, new(frame.Width * 0.5f, 0), scale * new Vector2(1, dst / frame.Height), SpriteEffects.None);

			frame = new(0, 0, 52, 18);
			Main.EntitySpriteDraw(texture, anchor - screenPos, frame, color, rot, new(frame.Width * 0.5f, 18), scale, SpriteEffects.None);

			if (Hitboxes is null) return false;
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Vector2 anchor = Anchor2;
				Vector2 position = Anchor1;
				Vector2 perp = (position - anchor).RotatedBy(MathHelper.PiOver2).Normalized(out _) * 26;
				float oneInclusive = float.BitIncrement(1);
				for (int i = 0; i < 15; i++) {
					Origins.instance.SpawnGoreByName(
						NPC.GetSource_Death(),
						Vector2.Lerp(position, anchor, Main.rand.NextFloat(oneInclusive)) + Vector2.Lerp(-perp, perp, Main.rand.NextFloat(oneInclusive)),
						NPC.velocity,
						"Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)
					);
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
