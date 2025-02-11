using Origins.Buffs;
using Origins.Reflection;
using Origins.Tiles.Brine;
using Origins.Walls;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Brine {
	public abstract class Brine_Pool_NPC : ModNPC {
		public int pathfindingTime = 0;
		public bool targetIsRipple = false;
		public bool canSeeTarget = false;
		public Vector2 TargetPos { get; set; }
		public static List<(Vector2 position, float magnitude)> Ripples { get; private set; } = [];
		[CloneByReference]
		public HashSet<int> TargetNPCTypes { get; private set; } = [];
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<Toxic_Shock_Debuff>()] = true;
			ModContent.GetInstance<Brine_Pool.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		public override void Unload() {
			Ripples = null;
		}
		public new virtual float SpawnChance(NPCSpawnInfo spawnInfo) => 0;
		public override bool CanHitNPC(NPC target) => TargetNPCTypes.Contains(target.type);
		public virtual bool CanTargetNPC(NPC other) {
			if (other.type == NPCID.TargetDummy) return false;
			return other.wet && CanHitNPC(other);
		}
		public virtual bool CanTargetPlayer(Player player) {
			return player.wet && !player.invis;
		}
		public virtual float RippleTargetWeight(float magnitude, float distance) {
			return (magnitude / distance) * 7.5f;
		}
		public virtual bool CheckTargetLOS(Vector2 target) => CollisionExt.CanHitRay(NPC.Center, target);
		public override void PostAI() {
			int specialHitSetter = 1;
			float damageMultiplier = 1f;
			Rectangle baseHitbox = NPC.Hitbox;
			foreach (NPC other in Main.ActiveNPCs) {
				if (other == NPC) continue;
				if (other.active && other.immune[255] == 0 && !(other.dontTakeDamage || other.dontTakeDamageFromHostiles || other.immortal)) {
					Rectangle hurtbox = other.Hitbox;
					Rectangle hitbox = baseHitbox;
					NPC.GetMeleeCollisionData(hurtbox, NPC.whoAmI, ref specialHitSetter, ref damageMultiplier, ref hitbox);
					if (NPCLoader.CanHitNPC(NPC, other) && hitbox.Intersects(hurtbox)) {
						NPCMethods.BeHurtByOtherNPC(other, NPC);
						break;
					}
				}
			}
		}
		public void TargetClosest(bool resetTarget = true, bool faceTarget = true) {
			if (resetTarget) {
				NPC.target = -1;
				TargetPos = default;
			}
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.All, CanTargetPlayer, CanTargetNPC);
			if (searchResults.FoundTarget) {
				NPC.target = searchResults.NearestTargetIndex;
				NPC.targetRect = searchResults.NearestTargetHitbox;
				if (NPC.ShouldFaceTarget(ref searchResults) && faceTarget) {
					NPC.FaceTarget();
				}
				targetIsRipple = false;
			} else if (!NPC.HasValidTarget) {
				float targetStrength = 1f;
				for (int i = 0; i < Ripples.Count; i++) {
					(Vector2 position, float magnitude) = Ripples[i];
					float strength = RippleTargetWeight(magnitude, position.Distance(NPC.Center));
					if (strength > targetStrength) {
						targetStrength = strength;
						TargetPos = position;
						targetIsRipple = true;
					}
				}
				NPC.targetRect = new((int)TargetPos.X - 8, (int)TargetPos.Y - 8, 16, 16);
			}
		}
		public void DoTargeting() {
			const int pathfinding_frequency = 5;
			TargetClosest(NPC.Center.IsWithin(TargetPos, Math.Max(NPC.width * 2, NPC.height * 2)));
			if (NPC.HasValidTarget) {
				NPCAimedTarget targetData = NPC.GetTargetData();
				Vector2 target = targetData.Center;
				if (pathfindingTime < pathfinding_frequency) pathfindingTime++;
				canSeeTarget = false;
				if (CheckTargetLOS(target)) {
					TargetPos = target;
					canSeeTarget = true;
				} else if (pathfindingTime >= pathfinding_frequency) {
					pathfindingTime = 0;
					Vector2 searchSize = new Vector2(48) * 16;
					Vector2 searchStart = NPC.Center - searchSize;
					searchStart = (searchStart / 16).Floor() * 16;
					Point topLeft = searchStart.ToTileCoordinates();
					Point bottomRight = (NPC.Center + searchSize).ToTileCoordinates();
					HashSet<Point> validEnds = [];
					foreach (Point point in Collision.GetTilesIn(targetData.Hitbox.TopLeft(), targetData.Hitbox.BottomRight())) {
						validEnds.Add(point);
					}
					Point[] path = CollisionExtensions.GridBasedPathfinding(
						CollisionExtensions.GeneratePathfindingGrid(topLeft, bottomRight, 1, 1),
						searchSize.ToTileCoordinates(),
						(target - searchStart).ToTileCoordinates(),
						validEnds
					);
					if (path.Length > 0) {
						for (int i = 0; i < path.Length && i < 10; i++) {
							Vector2 pos = path[i].ToWorldCoordinates() + searchStart;
							if (Collision.CanHitLine(NPC.position, 30, 30, pos - Vector2.One * 10, 30, 30)) {
								TargetPos = pos;
							} else {
								break;
							}
						}
					} else {
						TargetPos = default;
					}
				}
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.WritePackedVector2(TargetPos);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			TargetPos = reader.ReadPackedVector2();
		}
		public static bool AnyMossNearSpawn(int tileX, int tileY) {
			Point basePos = new(tileX, tileY);
			Point pos = basePos;
			static bool SearchForMoss(Point pos) {
				int mossType = ModContent.TileType<Peat_Moss>();
				for (int i = -3; i < 4; i++) {
					for (int j = -3; j < 4; j++) {
						if (Framing.GetTileSafely(pos.X + i, pos.Y + j).TileIsType(mossType)) return true;
					}
				}
				return false;
			}
			for (int i = 0; i < 10; i++) {
				pos.Y += 1;
				if (Framing.GetTileSafely(pos).HasTile) {
					if (SearchForMoss(pos)) return true;
					break;
				}
			}
			pos = basePos;
			for (int i = 0; i < 10; i++) {
				pos.X += 1;
				if (Framing.GetTileSafely(pos).HasTile) {
					if (SearchForMoss(pos)) return true;
					break;
				}
			}
			pos = basePos;
			for (int i = 0; i < 10; i++) {
				pos.X -= 1;
				if (Framing.GetTileSafely(pos).HasTile) {
					if (SearchForMoss(pos)) return true;
					break;
				}
			}
			pos = basePos;
			for (int i = 0; i < 10; i++) {
				pos.Y -= 1;
				if (Framing.GetTileSafely(pos).HasTile) {
					if (SearchForMoss(pos)) return true;
					break;
				}
			}
			return false;
		}
		public virtual bool CanSpawnInPosition(int tileX, int tileY) {
			Tile tile = Framing.GetTileSafely(tileX, tileY);
			return tile.LiquidAmount >= 255 && tile.LiquidType == LiquidID.Water && tile.WallType == ModContent.WallType<Baryte_Wall>();
		}
		public override int SpawnNPC(int tileX, int tileY) {
			tileY = OriginGlobalNPC.GetAerialSpawnPosition(tileX, tileY, this, (spawnY) => CanSpawnInPosition(tileX, spawnY));
			if (tileY == -1) return Main.maxNPCs;
			return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
		}
	}
}
