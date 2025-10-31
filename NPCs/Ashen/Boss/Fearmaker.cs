using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.LootConditions;
using Origins.Tiles.Ashen;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.Ashen.Boss {
	public class Fearmaker : Trenchmaker {
		public override string Texture => typeof(Trenchmaker).GetDefaultTMLName();
		public override string BossHeadTexture => null;
		public override void Load() {
			const int spin_count = 10;
			for (int i = 0; i <= spin_count; i++) {
				Mod.AddContent(new Spin_Adaptation(i * MathHelper.PiOver4 / spin_count, 1f / (spin_count + 1)));
			}
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 8;
			Origins.NPCOnlyTargetInBiome.Add(Type, ModContent.GetInstance<Ashen_Biome>());
			ContentSamples.NpcBestiaryRarityStars[Type] = 4;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.lifeMax = 600;
			NPC.boss = false;
			NPC.npcSlots = 10;
			NPC.lifeMax = (int)(NPC.lifeMax / 1.5f);
			//todo: make leg split values adapt to best kill the player they spawned on
			legs = [
				new(0, 0, ModContent.GetInstance<Fearmaker_Adaptation_Animation>()),
				new(0, 0, ModContent.GetInstance<Fearmaker_Adaptation_Animation>()),
				new(0, 0, ModContent.GetInstance<Fearmaker_Adaptation_Animation>()),
				new(0, 0, ModContent.GetInstance<Fearmaker_Adaptation_Animation>())
			];
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			if (OriginsModIntegrations.CheckAprilFools() && spawnInfo.Player.InModBiome<Ashen_Biome>() && spawnInfo.Player.position.Y / 16 < Main.rockLayer) return 0.07f;
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(false, true)
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			IItemDropRule normalDropRule = new LeadingSuccessRule();

			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Sanguinite_Ore_Item>(), 1, 70, 165));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<NE8>(), 1, 20, 50));

			npcLoot.Add(normalDropRule);
		}
		public class Fearmaker_Adaptation_Animation : LegAnimation {
			public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) => this;
			public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
				if (!npc.NPC.HasValidTarget) return;
				WeightedRandom<FearmakerAdaptation> adaptations = new();
				foreach (FearmakerAdaptation adaptation in ModContent.GetContent<FearmakerAdaptation>()) {
					adaptations.Add(adaptation, 1 * adaptation.WeightMultiplier);
				}
				adaptations.Get().SetupLegs(npc.legs);
				if (leg.CurrentAnimation != this) leg.CurrentAnimation.Update(npc, ref leg, otherLeg);
			}
		}
		public abstract class FearmakerAdaptation : ModType {
			public sealed override void SetupContent() {
				SetStaticDefaults();
			}
			protected override void Register() {
				ModTypeLookup<FearmakerAdaptation>.Register(this);
			}
			public virtual float WeightMultiplier => 1;
			public abstract void SetupLegs(Leg[] legs);
		}
		[Autoload(false)]
		public class Spin_Adaptation(float split, float weightMultiplier) : FearmakerAdaptation {
			public override float WeightMultiplier => weightMultiplier;
			public override string Name => base.Name + split;
			public override void SetupLegs(Leg[] legs) {
				float normalSplit = legs.Length * MathHelper.PiOver4;
				for (int i = 0; i < legs.Length; i++) {
					legs[i].ThighRot = normalSplit * i;
					legs[i].CurrentAnimation = ModContent.GetInstance<Spin_Animation>();
				}
			}
			class Spin_Animation : LegAnimation {
				public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) => this;
				public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
					leg.ThighRot += (leg.WasStanding || otherLeg.WasStanding) ? 0.4f : 0.3f;
					leg.CalfRot = MathHelper.PiOver4 * -0.5f;
				}
				public override bool HasHitbox(Trenchmaker npc, Leg leg) => true;
			}
		}
		public class Spring_Adaptation : FearmakerAdaptation {
			public override void SetupLegs(Leg[] legs) {
				for (int i = 0; i < legs.Length; i++) {
					legs[i].CurrentAnimation = ModContent.GetInstance<Fear_Standing_Animation>();
				}
			}
			public class Fear_Standing_Animation : LegAnimation {
				public override void Load() {
					defaultLegAnimation = this;
				}
				public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) => ModContent.GetInstance<Fear_Jump_Squat_Animation>();
				public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
					PistonTo(npc, ref leg, 24, 0.2f);
					leg.RotateThigh(-leg.CalfRot, 0.7f);
				}
			}
			public class Fear_Jump_Squat_Animation : LegAnimation {
				public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
					if (PistonLength(npc, leg) < 3) {
						Rectangle hitbox = npc.GetFootHitbox(leg);
						hitbox.Y += hitbox.Height - 4;
						hitbox.Height = 12;
						if (hitbox.OverlapsAnyTiles(false, true)) return ModContent.GetInstance<Fear_Jump_Spring_Animation>();
					}
					if (leg.TimeInAnimation > 60) return ModContent.GetInstance<Fear_Jump_Spring_Animation>();
					return this;
				}

				public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
					PistonTo(npc, ref leg, 0, 0.2f);
					leg.RotateThigh(-leg.CalfRot, 0.7f);
				}
			}
			public class Fear_Jump_Spring_Animation : LegAnimation {
				public override LegAnimation Continue(Trenchmaker npc, Leg leg, Leg otherLeg, Vector2 movement) {
					if (npc.NPC.velocity.Y >= 0 && (leg.WasStanding || leg.TimeStanding < 10)) return ModContent.GetInstance<Fear_Standing_Animation>();
					if (leg.TimeInAnimation > 60) return ModContent.GetInstance<Fear_Standing_Animation>();
					return this;
				}

				public override void Update(Trenchmaker npc, ref Leg leg, Leg otherLeg) {
					PistonTo(npc, ref leg, 48, 0.4f);
					leg.RotateThigh((leg.CalfRot - 0.3f) * -1f, 0.7f);
				}
			}
		}
	}
}
