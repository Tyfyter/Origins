using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Ashen.Boss {
	public class Fearmaker : Trenchmaker {
		public override string Texture => typeof(Trenchmaker).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.lifeMax = 600;
			NPC.boss = false;
			//todo: make leg split values adapt to best kill the player they spawned on
			const float split = MathHelper.PiOver4 * 0.5f;
			legs = [
				new(-split, 0, ModContent.GetInstance<SpinLegAnimation>()),
				new(MathHelper.Pi - split, 0, ModContent.GetInstance<SpinLegAnimation>()),
				new(split, 0, ModContent.GetInstance<SpinLegAnimation>()),
				new(MathHelper.Pi + split, 0, ModContent.GetInstance<SpinLegAnimation>())
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) { }
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			for (int i = 0; i < legs.Length; i++) {
				GetLegPositions(legs[i], out _, out _, out Vector2 footPos);
				footPos -= new Vector2(27, 7).Apply(SpriteEffects, new Vector2(54, 30));
				Rectangle footHitbox = new((int)footPos.X, (int)footPos.Y, 54, 30);
				if (footHitbox.Intersects(victimHitbox)) {
					npcHitbox = footHitbox;
					return false;
				}
			}
			return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
		}
		class SpinLegAnimation : LegAnimation {
			public override LegAnimation Continue(Trenchmaker npc, Leg leg, Vector2 movement) => this;
			public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
				leg.ThighRot += (leg.WasStanding || otherLeg.WasStanding) ? 0.4f : 0.3f;
				leg.CalfRot = MathHelper.PiOver4 * -0.5f;
			}
		}
	}
}
