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
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Vector2 movement) {
			if (Math.Abs(npc.NPC.GetTargetData().Center.X - npc.NPC.Center.X) > 16 * 10) return ModContent.GetInstance<Jump_Squat_Animation>();
			return this;
		}
		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			PistonTo(npc, ref leg, 24, 0.2f);
			leg.RotateThigh(-leg.CalfRot, 0.7f);
		}
	}
	public class Jump_Squat_Animation : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Vector2 movement) {
			if (PistonLength(npc, leg) < 3) {
				Rectangle hitbox = npc.GetFootHitbox(leg);
				hitbox.Y += hitbox.Height - 4;
				hitbox.Height = 12;
				if (hitbox.OverlapsAnyTiles(false, true)) return ModContent.GetInstance<Jump_Spring_Animation>();
			}
			if (leg.TimeInAnimation > 60) return ModContent.GetInstance<Jump_Spring_Animation>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			PistonTo(npc, ref leg, 0, 0.2f);
			leg.RotateThigh(-leg.CalfRot, 0.7f);
		}
	}
	public class Jump_Spring_Animation : LegAnimation {
		public override LegAnimation Continue(Trenchmaker npc, Leg leg, Vector2 movement) {
			if (npc.NPC.velocity.Y >= 0 && (leg.WasStanding || leg.TimeStanding < 10)) return ModContent.GetInstance<Standing_Animation>();
			if (leg.TimeInAnimation > 60) return ModContent.GetInstance<Standing_Animation>();
			return this;
		}

		public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
			PistonTo(npc, ref leg, 48, 0.4f);
			leg.RotateThigh((leg.CalfRot - 0.3f) * -1f, 0.7f);
		}
	}
}
