using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Origins.Tiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.Boss.Spawn_Trenchmaker_Action;
using static Origins.NPCs.Ashen.Boss.Trenchmaker;
using static Origins.NPCs.StateBossMethods<Origins.NPCs.Ashen.Boss.Trenchmaker>;

namespace Origins.NPCs.Ashen.Boss {
	//todo: force chee to become an animator
	public class Standing_Animation : LegAnimation {
		public override void Load() {
			defaultLegAnimation = this;
		}
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			float dist = npc.DistToTarget;
			AIState aiState = npc.GetState() as AIState;
			if (aiState.ForceAnimation(npc, leg, otherLeg) is LegAnimation forced) return forced;
			if (npc.stuckCount >= 19 && otherLeg.CurrentAnimation is not Stomp_Animation_1 and not Stomp_Animation_2 and not Stomp_Animation_3) {
				npc.stuckCount = 0;
				return ModContent.GetInstance<Stomp_Animation_1>();
			} else if (dist > (aiState?.WalkDist ?? 10 * 16)) {
				/*if (dist > (aiState?.WalkDist ?? 10 * 16) * 2) {
					if (otherLeg.WasStanding && otherLeg.CurrentAnimation is Walk_Animation_2 or Walk_Animation_3 or Run_Animation_2 or Run_Animation_3) {
						return ModContent.GetInstance<Run_Animation_1>();
					}
				}*/
				//if (dist > (aiState?.WalkDist ?? 10 * 16) * 2) return ModContent.GetInstance<Pogo_Animation_1>();
				if (otherLeg.WasStanding && otherLeg.CurrentAnimation is Standing_Animation or Walk_Animation_2 or Walk_Animation_3 or Step_Down_Crouch_Animation) {
					return ModContent.GetInstance<Walk_Animation_1>();
				} else if (!otherLeg.WasStanding && otherLeg.TimeStanding >= 62 && otherLeg.CurrentAnimation is Walk_Animation_1 or Walk_Animation_2 or Walk_Animation_3) {
					return ModContent.GetInstance<Step_Down_Crouch_Animation>();
				}
			} else if (aiState is Teabag_State && otherLeg.CurrentAnimation is Standing_Animation or Teabag_Animation_1) {
				return ModContent.GetInstance<Teabag_Animation_1>();
			} else if (dist <= 0 && npc.NPC.targetRect.Y > npc.NPC.BottomLeft.Y && otherLeg.CurrentAnimation is not Stomp_Animation_1 and not Stomp_Animation_2 and not Stomp_Animation_3) {
				SoundEngine.PlaySound(Origins.Sounds.PowerStompCharge, npc.NPC.Center);
				return ModContent.GetInstance<Stomp_Animation_1>();
			}
			return this;
		}
		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-leg.CalfRot, 0.02f);
			PistonTo(npc, ref leg, otherLeg.CurrentAnimation is Stomp_Animation_1 or Stomp_Animation_2 or Stomp_Animation_3 ? 48 : 24, 0.2f);
		}
	}
	#region walking
	public class Walk_Animation_1 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (leg.ThighRot == -0.5f && PistonLength(npc, leg) < 3) {
				return ModContent.GetInstance<Walk_Animation_2>();
			}
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-0.5f, 0.04f);
			PistonTo(npc, ref leg, 0, 0.2f);
		}
	}
	public class Walk_Animation_2 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (leg.WasStanding || (leg.ThighRot == 0f && PistonLength(npc, leg) >= 36)) return ModContent.GetInstance<Walk_Animation_3>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(0f, 0.03f);
			PistonTo(npc, ref leg, 38, 0.2f);
		}
	}
	public class Walk_Animation_3 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (leg.ThighRot == 1.1f && PistonLength(npc, leg) >= 36) return ModContent.GetInstance<Standing_Animation>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(1.1f, 0.04f);
			PistonTo(npc, ref leg, 38, 0.2f);
		}
	}
	public class Step_Down_Crouch_Animation : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (otherLeg.WasStanding) return ModContent.GetInstance<Standing_Animation>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			if (leg.TimeInAnimation > 5 * 60) {
				if (leg.TimeInAnimation > 15 * 60 && npc.GetState() is PhaseOneIdleState) {
					npc.SetAIState(StateIndex<Carpet_Bomb_State>());
					leg.CurrentAnimation = ModContent.GetInstance<Standing_Animation>();
					leg.CurrentAnimation.Reset();
					leg.TimeInAnimation = 0;
					leg.NetUpdate = true;
					npc.NPC.netUpdate = true;
					return;
				}
				leg.ThighRot += 0.07f * (leg.TimeInAnimation % 16 < 8).ToDirectionInt();
				leg.CalfRot += 0.07f * (leg.TimeInAnimation % 16 < 8).ToDirectionInt();
				return;
			}
			if (leg.ThighRot == 2) {
				PistonTo(npc, ref leg, 32, 0.2f);
			} else if (PistonLength(npc, leg) < 3) {
				leg.RotateThigh(2, 0.02f);
			} else {
				PistonTo(npc, ref leg, 0, 0.4f);
			}
		}
	}
	#endregion
	#region running
	public class Run_Animation_1 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (leg.ThighRot == -0.75f && PistonLength(npc, leg) < 3) return ModContent.GetInstance<Run_Animation_2>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-0.75f, 0.2f);
			PistonTo(npc, ref leg, 0, 0.2f);
		}
	}
	public class Run_Animation_2 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (leg.WasStanding || (leg.ThighRot == -0.75f && PistonLength(npc, leg) >= 22)) return ModContent.GetInstance<Run_Animation_3>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-0.75f, 0.045f);
			PistonTo(npc, ref leg, 24, 0.2f);
		}
	}
	public class Run_Animation_3 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (PistonLength(npc, leg) < 16) {
				if (npc.DistToTarget > 10 * 16 * 2) ModContent.GetInstance<Run_Animation_1>();
				return ModContent.GetInstance<Standing_Animation>();
			}
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			switch ((leg.RotateThigh(1.5f, 0.06f), leg.ThighRot > 0.7f)) {
				default:
				PistonTo(npc, ref leg, 32, 0.2f);
				break;

				case (false, true):
				PistonTo(npc, ref leg, 44);
				break;

				case (true, true):
				PistonTo(npc, ref leg, 0, 0.3f);
				break;
			}
		}
	}
	#endregion
	#region sproinging
	public class Pogo_Animation_1 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if ((leg.WasStanding || otherLeg.WasStanding) && leg.ThighRot == 0.5f && PistonLength(npc, leg) < 3) return ModContent.GetInstance<Pogo_Animation_2>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(0.5f, 0.04f);
			PistonTo(npc, ref leg, 0, 0.4f);
		}
	}
	public class Pogo_Animation_2 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (leg.ThighRot == 1.3f && PistonLength(npc, leg) >= 32) return ModContent.GetInstance<Standing_Animation>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			if (leg.ThighRot > 1f)
				PistonTo(npc, ref leg, 48, 2);
			leg.RotateThigh(1.3f, 0.3f);
		}
	}
	#endregion
	#region stomp
	public class Stomp_Animation_1 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (PistonLength(npc, leg) < 3) return ModContent.GetInstance<Stomp_Animation_2>();
			return this;
		}
		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-leg.CalfRot, 0.02f);
			PistonTo(npc, ref leg, 0, 0.2f);
		}
	}
	public class Stomp_Animation_2 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (PistonLength(npc, leg) >= 40) {
				npc.GetLegPositions(leg, out _, out _, out Vector2 footPos);
				npc.SpawnProjectile(null,
					footPos,
					Vector2.Zero,
					ModContent.ProjectileType<Trenchmaker_Stomp_P>(),
					20,
					0,
					ai1: npc.NPC.direction
				);
				return ModContent.GetInstance<Stomp_Animation_3>();
			}
			return this;
		}
		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(MathHelper.PiOver4, 0.2f);
			leg.RotateCalf(-MathHelper.PiOver4, 0.2f);
		}
		public override bool TileCollide(Trenchmaker npc, Leg leg) => false;
		public override bool HasHitbox(Trenchmaker npc, Leg leg) => true;
	}
	public class Stomp_Animation_3 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (PistonLength(npc, leg) <= 22 && !npc.GetFootHitbox(leg).OverlapsAnyTiles()) return ModContent.GetInstance<Standing_Animation>();
			return this;
		}
		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-leg.CalfRot, 0.02f);
			PistonTo(npc, ref leg, 0, 0.2f);
			if (PistonLength(npc, leg) < 3 && (npc.GetFootHitbox(leg).OverlapsAnyTiles() || npc.GetFootHitbox(otherLeg).OverlapsAnyTiles())) {
				npc.NPC.velocity.Y -= 1;
			}
		}
		public override bool TileCollide(Trenchmaker npc, Leg leg) => !npc.GetFootHitbox(leg).OverlapsAnyTiles();
	}
	public class Trenchmaker_Stomp_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2OgreSmash;
		public override void SetDefaults() {
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 120;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
		}
		public override void AI() {
			float num = 15f;
			if (Projectile.ai[0] == 0) SoundEngine.PlaySound(Origins.Sounds.PowerStomp, Projectile.Center);
			Projectile.ai[0] += 1f;
			if (Projectile.ai[0] > 9f) {
				Projectile.Kill();
				return;
			}

			Projectile.velocity = Vector2.Zero;
			Projectile.position = Projectile.Center;
			Projectile.Size = new Vector2(16f, 16f) * MathHelper.Lerp(5f, num, Utils.GetLerpValue(0f, 9f, Projectile.ai[0]));
			Projectile.Center = Projectile.position;
			if (!NPC.downedBoss2) {
				Rectangle smashHitbox = Projectile.Hitbox;
				smashHitbox.Width /= 2;
				if (Projectile.ai[1] == 1) smashHitbox.X += smashHitbox.Width;

				smashHitbox.Height /= 3;
				smashHitbox.Y += 16;
				if (smashHitbox.OverlapsAnyTiles(out List<Point> tiles)) {
					foreach (Point tile in tiles) {
						if (TileLoader.GetTile(Main.tile[tile].TileType) is not IAshenTile) WorldGen.KillTile(tile.X, tile.Y);
					}
				}
			}
			Point point = Projectile.TopLeft.ToTileCoordinates();
			Point point2 = Projectile.BottomRight.ToTileCoordinates();
			int num2 = point.X / 2 + point2.X / 2;
			int num3 = Projectile.width / 2;
			if ((int)Projectile.ai[0] % 3 != 0)
				return;

			int num4 = (int)Projectile.ai[0] / 3;
			for (int i = point.X; i <= point2.X; i++) {
				for (int j = point.Y; j <= point2.Y; j++) {
					if (Vector2.Distance(Projectile.Center, new Vector2(i * 16, j * 16)) > (float)num3)
						continue;

					Tile tileSafely = Framing.GetTileSafely(i, j);
					if (!tileSafely.HasTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType] || Main.tileFrameImportant[tileSafely.TileType])
						continue;

					Tile tileSafely2 = Framing.GetTileSafely(i, j - 1);
					if (tileSafely2.HasTile && Main.tileSolid[tileSafely2.TileType] && !Main.tileSolidTop[tileSafely2.TileType])
						continue;

					int num5 = WorldGen.KillTile_GetTileDustAmount(fail: true, tileSafely, i, j);
					for (int k = 0; k < num5; k++) {
						Dust obj = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
						obj.velocity.Y -= 3f + (float)num4 * 1.5f;
						obj.velocity.Y *= Main.rand.NextFloat();
						obj.velocity.Y *= 0.75f;
						obj.scale += (float)num4 * 0.03f;
					}

					if (num4 >= 2) {
						for (int m = 0; m < num5 - 1; m++) {
							Dust obj2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
							obj2.velocity.Y -= 1f + (float)num4;
							obj2.velocity.Y *= Main.rand.NextFloat();
							obj2.velocity.Y *= 0.75f;
						}
					}

					if (num5 <= 0 || Main.rand.Next(3) == 0)
						continue;

					float num7 = (float)Math.Abs(num2 - i) / (num / 2f);
					Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.position, Vector2.Zero, 61 + Main.rand.Next(3), 1f - (float)num4 * 0.15f + num7 * 0.5f);
					gore.velocity.Y -= 0.1f + (float)num4 * 0.5f + num7 * (float)num4 * 1f;
					gore.velocity.Y *= Main.rand.NextFloat();
					gore.position = new Vector2(i * 16 + 20, j * 16 + 20);
				}
			}
		}
		public override bool CanHitPlayer(Player target) => target.OriginPlayer().collidingY;
		public override bool? CanHitNPC(NPC target) {
			if (!target.collideY) return false;
			return base.CanHitNPC(target);
		}
	}
	#endregion
	#region jumping
	public class Jump_Preparation_Animation : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (otherLeg.CurrentAnimation is Jump_Preparation_Animation or Jump_Squat_Animation) return ModContent.GetInstance<Jump_Squat_Animation>();
			return this;
		}
		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-leg.CalfRot, 0.02f);
			PistonTo(npc, ref leg, 24, 0.2f);
		}
	}
	public class Jump_Squat_Animation : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (PistonLength(npc, leg) < 4) return ModContent.GetInstance<Jump_Extend_Animation>();
			return this;
		}
		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			PistonTo(npc, ref leg, 0, 0.2f);
			leg.RotateThigh(0.4f - leg.CalfRot, 0.1f);
		}
	}
	public class Jump_Extend_Animation : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (PistonLength(npc, leg) > 40) return ModContent.GetInstance<Jump_Air_Animation>();
			return this;
		}
		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			PistonTo(npc, ref leg, 48, 0.5f);
			leg.RotateThigh(0.7f - leg.CalfRot, 0.25f);
		}
	}
	public class Jump_Air_Animation : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (Math.Abs(PistonLength(npc, leg) - 24) < 2 && (leg.WasStanding || otherLeg.WasStanding)) return ModContent.GetInstance<Standing_Animation>();
			return this;
		}
		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-leg.CalfRot, 0.02f);
			PistonTo(npc, ref leg, 24, 0.2f);
		}
	}
	#endregion
	#region teabag
	public class Teabag_Animation_1 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (PistonLength(npc, leg) < 3 && PistonLength(npc, otherLeg) < 3) return ModContent.GetInstance<Teabag_Animation_2>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-leg.CalfRot, 0.02f);
			PistonTo(npc, ref leg, 0, 0.2f);
		}
	}
	public class Teabag_Animation_2 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (npc.NPC.HasValidTarget || Main.player[npc.NPC.target].respawnTimer < 60 * 2) return ModContent.GetInstance<Teabag_Animation_Finish>();
			if (PistonLength(npc, leg) > 30 && PistonLength(npc, otherLeg) > 30) return ModContent.GetInstance<Teabag_Animation_1>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-leg.CalfRot, 0.02f);
			PistonTo(npc, ref leg, 32, 0.2f);
		}
	}
	public class Teabag_Animation_Finish : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-leg.CalfRot, 0.2f);
			PistonTo(npc, ref leg, 48, 0.8f);
			if (PistonLength(npc, leg) >= 43) {
				npc.NPC.noGravity = false;
				npc.NPC.EncourageDespawn(60);
			}
		}
	}
	#endregion
}
