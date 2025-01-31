using Origins.Buffs;
using Origins.Reflection;
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
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Brine {
	public abstract class Brine_Pool_NPC : ModNPC {
		public int pathfindingTime = 0;
		public bool targetIsRipple = false;
		public Vector2 TargetPos { get; set; }
		public static List<(Vector2 position, float magnitude)> Ripples { get; private set; } = [];
		public override void SetStaticDefaults() {
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<Toxic_Shock_Debuff>()] = true;
		}
		public override void SetDefaults() {
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		public override void Unload() {
			Ripples = null;
		}
		public override bool CanHitNPC(NPC target) {
			return target.type != Type;
		}
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
			}
		}
		public void DoTargeting() {
			const int pathfinding_frequency = 5;
			TargetClosest(NPC.Center.IsWithin(TargetPos, Math.Max(NPC.width, NPC.height)));
			if (NPC.HasValidTarget) {
				NPCAimedTarget targetData = NPC.GetTargetData();
				Vector2 target = targetData.Center;
				if (pathfindingTime < pathfinding_frequency) pathfindingTime++;
				if (CheckTargetLOS(target)) {
					TargetPos = target;
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
		public override int SpawnNPC(int tileX, int tileY) {
			int spawnY = tileY * 16;
			if (Math.Abs(tileY - OriginGlobalNPC.aerialSpawnPosition) < 100) spawnY = OriginGlobalNPC.aerialSpawnPosition * 16 + 8;
			return NPC.NewNPC(null, tileX * 16 + 8, spawnY, NPC.type);
		}
	}
}
