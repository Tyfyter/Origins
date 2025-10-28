using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.Boss.Trenchmaker;

namespace Origins.NPCs.Ashen.Boss {
	//todo: force chee to become an animator
	public class Standing_Animation : LegAnimation {
		public override void Load() {
			defaultLegAnimation = this;
		}
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
			if (Math.Abs(npc.NPC.GetTargetData().Center.X - npc.NPC.Center.X) > 16 * 10 && otherLeg.WasStanding && otherLeg.CurrentAnimation is Standing_Animation or Walk_Animation_2) {
				return ModContent.GetInstance<Walk_Animation_1>();
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
}
