using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.NPCExtensions;

namespace Origins.NPCs.Ashen {
	public class Repairboy : ModNPC, IAshenEnemy {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.UsesNewTargetting[Type] = true;
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 45;
			NPC.defense = 8;
			NPC.damage = 18;
			NPC.width = 40;
			NPC.height = 26;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Ashen_Biome>().Type,
			];
		}
		AdvancedTargetSearchResults target;
		const float target_dist_min = 16;
		const float target_dist_max = 32;
		void MoveTowards(Vector2 targetPos, out Vector2 targetDir, out float dist) {
			const float acceleration = 0.35f;
			Vector2 movementDir = NPC.velocity.Normalized(out float speed);
			targetDir = (targetPos - NPC.Center).Normalized(out dist);
			if (dist < target_dist_min) {
				NPC.velocity -= targetDir * acceleration;
			} else if (dist > target_dist_max) {
				float speedInRightDir = Vector2.Dot(movementDir, targetDir) * speed;
				float predictedTime = (float.Sqrt(2 * (dist - target_dist_max) * -acceleration + speedInRightDir * speedInRightDir) + speedInRightDir) / acceleration;
				if (!float.IsNaN(predictedTime)) {
					NPC.velocity -= targetDir * acceleration;
				} else {
					NPC.velocity += targetDir * acceleration;
				}
			}
			NPC.direction = targetDir.X < 0 ? -1 : 1;
		}
		public void TargetClosest(bool resetTarget = true, bool faceTarget = true) {
			AdvancedTargetSearchResults searchResults = NPC.SearchForTarget(
				new Rectangle(0, 0, 1920 * 2, 1080 * 2).Recentered(NPC.Center),
				TargetSearchTypes.All,
				playerSearcher: static (player, area, searcher) => searcher.Distance(player.Center) + (!searcher.playerInteraction[player.whoAmI]).Mul(800) - player.aggro,
				npcSearcher: NPCSearcher,
				tileSearcher: TileSearcher
			);
			if (searchResults.HasTarget || resetTarget) target = searchResults;
			NPC.targetRect = target.TargetRect;
			if (faceTarget) NPC.FaceTarget();
		}
		static float? NPCSearcher(NPC target, Rectangle area, NPC searcher) {
			if ((target.type == searcher.type || target.ModNPC is not IAshenEnemy || target.life >= target.lifeMax)) {
				if (target.friendly || target.CountsAsACritter) return searcher.Distance(target.Center);
				return null;
			}
			return searcher.Distance(target.Center) + (target.life - target.lifeMax);
		}
		static (float cost, int id, Rectangle hitbox)? TileSearcher(Rectangle area, NPC searcher) {
			Point low = area.TopLeft().ToTileCoordinates();
			Point high = area.BottomRight().ToTileCoordinates();
			Max(ref low.X, 0);
			Max(ref low.Y, 0);
			Min(ref high.X, Main.maxTilesX);
			Min(ref high.Y, Main.maxTilesY);

			float bestCost = float.PositiveInfinity;
			Rectangle bestHitbox = default;
			Tile bestTile = default;

			Rectangle hitbox = default;
			Vector2 center = searcher.Center / 16;
			for (int j = low.Y; j < high.Y; j++) {
				for (int i = low.X; i < high.X; i++) {
					Tile tile = Main.tile[i, j];
					if (!tile.HasTile || TileLoader.GetTile(tile.TileType) is not IReparableTile reparableTile || TileObjectData.GetTileData(tile) is not TileObjectData data) continue;
					TileUtils.GetMultiTileTopLeft(i, j, data, out hitbox.X, out hitbox.Y);
					if (i != hitbox.X || j != hitbox.Y || !reparableTile.NeedsRepair(i, j)) continue;
					hitbox.Width = data.Width;
					hitbox.Height = data.Height;
					if (Minimize(ref bestCost, center.DistanceSQ(hitbox.Center()))) {
						bestHitbox = hitbox;
						bestTile = tile;
					}
				}
			}
			if (bestHitbox == default) return null;
			return (float.Sqrt(bestCost) * 16, Unsafe.BitCast<Tile, int>(bestTile), bestHitbox.Scaled(16));
		}
		public override void AI() {
			if (!target.HasTarget || NPC.life < NPC.lifeMax || target.TargetType == TargetSearchTypes.Players) TargetClosest();
			if (target.HasTarget) {
				NPC.targetRect = target.TargetRect;
				MoveTowards(NPC.Center.Clamp(NPC.targetRect), out Vector2 targetDir, out float dist);
				if (target.TargetType != TargetSearchTypes.Tiles) NPC.ai[1] = 0;
				if (dist <= target_dist_max + 16) {
					if (NPC.ai[0].CycleUp(30)) {
						NPC.velocity += new Vector2(targetDir.Y, -targetDir.X) * Main.rand.NextBool().ToDirectionInt();
						NPC.netUpdate = true;
					}
					if (target.TargetTile is Tile tile) {
						if (NPC.ai[1].CycleUp(60)) {
							bool canKeepTarget = false;
							if (tile.HasTile && TileLoader.GetTile(tile.TileType) is IReparableTile reparableTile) {
								(int i, int j) = tile.GetTilePosition();
								reparableTile.Repair(i, j);
								canKeepTarget = reparableTile.NeedsRepair(i, j);
							}
							if (!canKeepTarget) TargetClosest();
						}
					}
				} else NPC.ai[0] = 0;
			}
			NPC.velocity *= 0.97f;
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects effects = NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			drawColor = NPC.GetTintColor(drawColor);
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				NPC.Center - screenPos,
				NPC.frame,
				drawColor,
				0,
				new Vector2(32, 21).Apply(effects, NPC.frame.Size()),
				NPC.scale,
				effects,
			0);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			target.Write(writer);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			target = AdvancedTargetSearchResults.Read(reader);
		}
		public interface IReparableTile {
			public bool NeedsRepair(int i, int j);
			public void Repair(int i, int j);
		}
	}
}
