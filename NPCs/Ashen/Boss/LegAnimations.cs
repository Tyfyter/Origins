using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.Boss.Trenchmaker;

namespace Origins.NPCs.Ashen.Boss {
	//todo: force chee to become an animator
	public class Standing_Animation : LegAnimation {
		public override void Load() {
			defaultLegAnimation = this;
		}
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			float dist;
			NPCAimedTarget target = npc.NPC.GetTargetData();
			if (target.Center.X > npc.NPC.Center.X) {
				dist = target.Position.X - (npc.NPC.position.X + npc.NPC.width);
			} else {
				dist = npc.NPC.position.X - (target.Position.X + target.Width);
			}
			AIState aiState = npc.GetState() as AIState;
			if (dist > (aiState?.WalkDist ?? 10 * 16)) {
				if (otherLeg.WasStanding && otherLeg.CurrentAnimation is Standing_Animation or Walk_Animation_2) {
					return ModContent.GetInstance<Walk_Animation_1>();
				}
			} else if (aiState is Teabag_State && otherLeg.CurrentAnimation is Standing_Animation or Teabag_Animation_1) {
				return ModContent.GetInstance<Teabag_Animation_1>();
			}
			return this;
		}
		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-leg.CalfRot, 0.02f);
			PistonTo(npc, ref leg, 24, 0.2f);
		}
	}
	public class Walk_Animation_1 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (leg.ThighRot == -0.5f && PistonLength(npc, leg) < 3) return ModContent.GetInstance<Walk_Animation_2>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(-0.5f, 0.04f);
			PistonTo(npc, ref leg, 0, 0.2f);
		}
	}
	public class Walk_Animation_2 : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (leg.ThighRot == 1.1f && PistonLength(npc, leg) >= 30) return ModContent.GetInstance<Standing_Animation>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			leg.RotateThigh(1.1f, 0.04f);
			PistonTo(npc, ref leg, 32, 0.2f);
		}
	}
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
			if (npc.NPC.HasValidTarget) return ModContent.GetInstance<Teabag_Animation_Finish>();
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
			if (PistonLength(npc, leg) >= 43) npc.NPC.noGravity = false;
		}
	}
}
