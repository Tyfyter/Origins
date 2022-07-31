using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent.Personalities;
using Origins.World.BiomeData;
using Terraria.GameContent.Bestiary;
using Origins.Items.Materials;
using Origins.Projectiles.Weapons;
using Terraria.GameContent.UI;
using Terraria.Utilities;
using Terraria.Localization;
using Terraria.GameContent.Events;

namespace Origins.NPCs.TownNPCs {
	//[AutoloadHead]
	public class Acid_Freak : ModNPC {
		public override void SetStaticDefaults() {
            DisplayName.SetDefault("Acid Freak");
			Main.npcFrameCount[Type] = 25; // The amount of frames the NPC has

			NPCID.Sets.ExtraFramesCount[Type] = 9; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs.
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
			NPCID.Sets.AttackType[Type] = 0;
			NPCID.Sets.AttackTime[Type] = 90; // The amount of time it takes for the NPC's attack animation to be over once it starts.
			NPCID.Sets.AttackAverageChance[Type] = 30;
			NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.
			NPCID.Sets.ActsLikeTownNPC[Type] = true;

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
				IsWet = true
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPC.Happiness
				.SetBiomeAffection<JungleBiome>(AffectionLevel.Like)
				.SetBiomeAffection<JungleBiome>(AffectionLevel.Like)//TODO: make be temple
				.SetBiomeAffection<Brine_Pool>(AffectionLevel.Like)
				.SetBiomeAffection<MushroomBiome>(AffectionLevel.Hate)
				.SetNPCAffection(NPCID.WitchDoctor, AffectionLevel.Love)
				.SetNPCAffection(NPCID.Dryad, AffectionLevel.Like)
				.SetNPCAffection(NPCID.BestiaryGirl, AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.Truffle, AffectionLevel.Hate)
				//.SetNPCAffection(NPCID.Dryad, AffectionLevel.Hate)// Defiled Envoy
			; // < Mind the semicolon!
		}
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.WitchDoctor);
			AnimationType = NPCID.Merchant;
		}
		public override bool PreAI() {
			NPC.wet = false;
			NPC.breathCounter = 0;
			return true;
		}
		public override void PostAI() {
			if (Collision.WetCollision(NPC.position, NPC.width, NPC.height) && !Collision.honey) {
				NPC.wet = true;
			}
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the preferred biomes of this town NPC listed in the bestiary.
				// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
				Brine_Pool.BestiaryBackground,

				// Sets your NPC's flavor text in the bestiary.
				new FlavorTextBestiaryInfoElement("Mods.NPCs.Bestiary.Acid_Freak")
			});
		}
		public override string GetChat() {
			WeightedRandom<string> chat = new();

			int otherNPC = NPC.FindFirstNPC(NPCID.WitchDoctor);
			if (otherNPC >= 0) {
				chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.InteractionWitchDoctor1", Main.npc[otherNPC].GivenName));
				chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.InteractionWitchDoctor2", Main.npc[otherNPC].GivenName));
			}
			if (BirthdayParty.PartyIsUp) {
				chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.InteractionParty", Main.npc[otherNPC].GivenName));
				
				otherNPC = NPC.FindFirstNPC(NPCID.Demolitionist);
				if (otherNPC >= 0) {
					chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.InteractionPartyDemolitionist", Main.npc[otherNPC].GivenName));
				}
			}
			if (Main.WindyEnoughForKiteDrops) {
				chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.InteractionWind"));
			}
			if (Main.IsItStorming) {
				chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.InteractionStorm1"));
				chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.InteractionStorm2"));
			}
			if (Main.bloodMoon) {
				chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.BloodMoon1"));
				chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.BloodMoon2"));
			}
			if (Main.SceneMetrics.EnoughTilesForGraveyard) {
				chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.InteractionGraveYard"));
			}
			// These are things that the NPC has a chance of telling you when you talk to it.
			chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.StandardDialogue1"));
			chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.StandardDialogue2"));
			chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.StandardDialogue3"));
			chat.Add(Language.GetTextValue("Mods.Origins.Dialogue.Acid_Freak.StandardDialogue4"));

			return chat; // chat is implicitly cast to a string.
		}

		public override List<string> SetNPCNameList() {
			return new List<string>() {
				"Jeevus",
				"Blumbo",
				"E",
				"Vide Octavius James"
			};
		}
		public override bool CheckConditions(int left, int right, int top, int bottom) {
			//Terraria.WorldGen.ScoreRoom();
			return base.CheckConditions(left, right, top, bottom);
		}
		public override bool CanGoToStatue(bool toKingStatue) => toKingStatue;
		public override bool CanTownNPCSpawn(int numTownNPCs, int money) { // Requirements for the town NPC to spawn.
			for (int k = 0; k < 255; k++) {
				Player player = Main.player[k];
				if (!player.active) {
					continue;
				}

				// Player has to have either an ExampleItem or an ExampleBlock in order for the NPC to spawn
				if (player.inventory.Any(item => item.type == ModContent.ItemType<Brine_Sample>())) {
					return true;
				}
			}

			return false;
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			damage = 30;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
			projType = ModContent.ProjectileType<Acid_Shot>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 8f;
			randomOffset = 2f;
			gravityCorrection = 30;
		}
	}
}
