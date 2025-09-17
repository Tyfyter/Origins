using MonoMod.Cil;
using Origins.Buffs;
using Origins.Projectiles;
using Origins.Reflection;
using Origins.Tiles.Brine;
using Origins.Walls;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Brine {
	public abstract class Brine_Pool_NPC : ModNPC, IBrinePoolNPC {
		public static float ScalingArmorPenetrationToCompensateForTSNerf => 0.1f;
		public int PathfindingTime { get; set; } = 0;
		public bool TargetIsRipple { get; set; } = false;
		public bool CanSeeTarget { get; set; } = false;
		public Vector2 TargetPos { get; set; }
		public virtual bool AggressivePathfinding => false;
		public static List<(Vector2 position, float magnitude)> Ripples { get; private set; } = [];
		[CloneByReference]
		public HashSet<int> TargetNPCTypes { get; private set; } = [];
		protected override bool CloneNewInstances => true;
		public override void Load() {
			if (Mod.FileExists($"Tiles/Banners/{Name}_Banner_Item.rawimg")) this.AddBanner();
		}
		public override void SetStaticDefaults() {
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][Toxic_Shock_Debuff.ID] = true;
			ModContent.GetInstance<Brine_Pool.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public static void DisableRipples(ILContext il) {
			ILCursor c = new(il);
			try {
				int loc = -1;
				ILLabel label = default;
				c.GotoNext(MoveType.After,
					i => i.MatchLdsfld<Main>(nameof(Main.npc)),
					i => i.MatchLdloc(out loc),
					i => i.MatchLdelemRef(),
					i => i.MatchBrfalse(out label)
				);

				c.EmitLdloc(loc);
				c.EmitDelegate((int i) => Main.npc[i].ModNPC is Brine_Pool_NPC or IBrinePoolNPC);
				c.EmitBrtrue(label);
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(DisableRipples), e)) throw;
			}
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
		public override bool CanHitNPC(NPC target) => TargetNPCTypes.Contains(target.type) || target.ModNPC is not IBrinePoolNPC;
		public virtual bool CanTargetNPC(NPC other) {
			if (OriginsSets.NPCs.TargetDummies[other.type]) return false;
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
			HitOtherNPCs(NPC);
		}
		public static void HitOtherNPCs(NPC self) {
			int specialHitSetter = 1;
			float damageMultiplier = 1f;
			Rectangle baseHitbox = self.Hitbox;
			foreach (NPC other in Main.ActiveNPCs) {
				if (other == self) continue;
				if (other.active && other.immune[255] == 0 && !(other.dontTakeDamage || other.dontTakeDamageFromHostiles || other.immortal)) {
					Rectangle hurtbox = other.Hitbox;
					Rectangle hitbox = baseHitbox;
					NPC.GetMeleeCollisionData(hurtbox, self.whoAmI, ref specialHitSetter, ref damageMultiplier, ref hitbox);
					if (NPCLoader.CanHitNPC(self, other) && hitbox.Intersects(hurtbox)) {
						NPCMethods.BeHurtByOtherNPC(other, self);
						break;
					}
				}
			}
		}
		public void TargetClosest(bool resetTarget = true, bool faceTarget = true) => TargetClosest(this, resetTarget, faceTarget);
		public static void TargetClosest(IBrinePoolNPC self, bool resetTarget = true, bool faceTarget = true) {
			NPC NPC = ((ModNPC)self).NPC;
			if (resetTarget) {
				NPC.target = -1;
				self.TargetPos = default;
			}
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.All, self.CanTargetPlayer, self.CanTargetNPC);
			if (searchResults.FoundTarget) {
				NPC.target = searchResults.NearestTargetIndex;
				NPC.targetRect = searchResults.NearestTargetHitbox;
				if (NPC.ShouldFaceTarget(ref searchResults) && faceTarget) {
					NPC.FaceTarget();
				}
				self.TargetIsRipple = false;
			} else if (!NPC.HasValidTarget) {
				float targetStrength = 1f;
				for (int i = 0; i < Ripples.Count; i++) {
					(Vector2 position, float magnitude) = Ripples[i];
					float strength = self.RippleTargetWeight(magnitude, position.Distance(NPC.Center));
					if (strength > targetStrength) {
						targetStrength = strength;
						self.TargetPos = position;
						self.TargetIsRipple = true;
					}
				}
				NPC.targetRect = new((int)self.TargetPos.X - 8, (int)self.TargetPos.Y - 8, 16, 16);
			}
		}
		public void DoTargeting() => DoTargeting(this);
		public static void DoTargeting(IBrinePoolNPC self) {
			NPC NPC = ((ModNPC)self).NPC;
			const int pathfinding_frequency = 5;
			bool resetTarget;
			if (NPC.HasValidTarget) {
				if (self.TargetIsRipple) resetTarget = NPC.Center.IsWithin(self.TargetPos, Math.Max(NPC.width * 2, NPC.height * 2));
				else if (NPC.HasNPCTarget) resetTarget = !self.CanTargetNPC(Main.npc[NPC.TranslatedTargetIndex]);
				else resetTarget = !self.CanTargetPlayer(Main.player[NPC.target]);
			} else {
				resetTarget = true;
			}
			TargetClosest(self, resetTarget);
			if (NPC.HasValidTarget) {
				NPCAimedTarget targetData = NPC.GetTargetData();
				Vector2 target = targetData.Center;
				if (self.PathfindingTime < pathfinding_frequency) self.PathfindingTime++;
				self.CanSeeTarget = false;
				if (self.CheckTargetLOS(target)) {
					self.TargetPos = target;
					self.CanSeeTarget = true;
				} else if (self.PathfindingTime >= pathfinding_frequency) {
					self.PathfindingTime = 0;
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
								self.TargetPos = pos;
							} else {
								break;
							}
						}
					} else {
						if (self.AggressivePathfinding) path = CollisionExtensions.GridBasedPathfinding(
							CollisionExtensions.GeneratePathfindingGrid(topLeft, bottomRight, 0, 0),
							searchSize.ToTileCoordinates(),
							(target - searchStart).ToTileCoordinates(),
							validEnds
						);
						if (path.Length > 0) {
							for (int i = 0; i < path.Length && i < 10; i++) {
								Vector2 pos = path[i].ToWorldCoordinates() + searchStart;
								if (CollisionExt.CanHitRay(NPC.Center, pos + new Vector2(8))) {
									self.TargetPos = pos;
								} else {
									break;
								}
							}
						} else {
							self.TargetPos = default;
						}
					}
				}
			}
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.ScalingArmorPenetration += Brine_Pool_NPC.ScalingArmorPenetrationToCompensateForTSNerf;
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
			return tile.LiquidAmount >= 255 && tile.LiquidType == LiquidID.Water && (tile.WallType == ModContent.WallType<Baryte_Wall>() || Brine_Pool.forcedBiomeActive);
		}
		public override int SpawnNPC(int tileX, int tileY) {
			tileY = OriginGlobalNPC.GetAerialSpawnPosition(tileX, tileY, this, (spawnY) => CanSpawnInPosition(tileX, spawnY));
			if (tileY == -1) return Main.maxNPCs;
			return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
		}
	}
	public interface IBrinePoolNPC {
		public int PathfindingTime { get; set; }
		public bool TargetIsRipple { get; set; }
		public bool CanSeeTarget { get; set; }
		public Vector2 TargetPos { get; set; }
		public bool AggressivePathfinding { get; }
		[CloneByReference]
		public HashSet<int> TargetNPCTypes { get; }
		public bool CheckTargetLOS(Vector2 target);
		public bool CanTargetNPC(NPC other);
		public bool CanTargetPlayer(Player player) {
			return player.wet && !player.invis;
		}
		public float RippleTargetWeight(float magnitude, float distance) {
			return (magnitude / distance) * 7.5f;
		}
	}
}
