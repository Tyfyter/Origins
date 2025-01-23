using AltLibrary.Core;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Consumables.Broths;
using Origins.Projectiles.Weapons;
using Origins.Tiles.Brine;
using Origins.World.BiomeData;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.TownNPCs {
	[AutoloadHead]
	public class Brine_Fiend : ModNPC {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 23; // The amount of frames the NPC has, walk frame count (15) + ExtraFramesCount

			NPCID.Sets.ExtraFramesCount[Type] = 8; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs.
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
			NPCID.Sets.AttackType[Type] = 0;
			NPCID.Sets.AttackTime[Type] = 90; // The amount of time it takes for the NPC's attack animation to be over once it starts.
			NPCID.Sets.AttackAverageChance[Type] = 30;
			NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
				IsWet = true
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
			NPC.Happiness
				.SetBiomeAffection<JungleBiome>(AffectionLevel.Like)
				.SetBiomeAffection<TempleBiome>(AffectionLevel.Love)
				.SetBiomeAffection<Brine_Pool>(AffectionLevel.Like)
				.SetBiomeAffection<MushroomBiome>(AffectionLevel.Hate)
				.SetBiomeAffection<CorruptionBiome>(AffectionLevel.Like)
				.SetBiomeAffection<CrimsonBiome>(AffectionLevel.Like)
				.SetBiomeAffection<Defiled_Wastelands>(AffectionLevel.Like)
				.SetBiomeAffection<Riven_Hive>(AffectionLevel.Like)
				.SetNPCAffection(NPCID.WitchDoctor, AffectionLevel.Love)
				.SetNPCAffection(NPCID.Dryad, AffectionLevel.Like)
				.SetNPCAffection(NPCID.BestiaryGirl, AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.Truffle, AffectionLevel.Hate)
			//.SetNPCAffection(NPCID.Dryad, AffectionLevel.Hate)// Defiled Envoy
			; // < Mind the semicolon!
			InvalidateNPCHousing.NPCTypeIgnoresAllEvil.Add(Type);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.WitchDoctor);
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.HitSound = SoundID.NPCHit26;
			NPC.DeathSound = SoundID.NPCDeath29;
			AnimationType = NPCID.BestiaryGirl;
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
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
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void AddShops() {
			new NPCShop(Type)
			.Add<Hearty_Broth>()
			.Add<Sour_Broth>()
			.Add<Foul_Broth>()
			.Register();
		}
		public override string GetChat() {
			WeightedRandom<string> chat = new();

			chat.AddOtherNPCDialogue(Name, NPCID.WitchDoctor);

			if (BirthdayParty.PartyIsUp) {

				chat.AddNPCDialogue(Name, "Party");
				if (NPC.FindFirstNPC(NPCID.Demolitionist) >= 0) {
					chat.AddNPCDialogue(Name, "PartyDemolitionist");
				}
			}
			if (Main.WindyEnoughForKiteDrops) {
				if (ChildSafety.Disabled && OriginsModIntegrations.CheckAprilFools()) {
					chat.Add(Language.GetTextValue("Mods.Origins.NPCs.Brine_Fiend.Dialogue.April_Fools.InteractionWimd"));
				} else {
					chat.AddNPCDialogue(Name, "Wind");
				}
			}
			if (Main.IsItStorming) {
				chat.AddNPCDialogue(Name, "Storm");
			}
			if (Main.bloodMoon) {
				chat.AddNPCDialogue(Name, "BloodMoon");
			}
			if (Main.SceneMetrics.EnoughTilesForGraveyard) {
				chat.AddNPCDialogue(Name, "Graveyard");
			}

			chat.AddNPCDialogue(Name, "Standard");

			return chat; // chat is implicitly cast to a string.
		}

		public override List<string> SetNPCNameList() => this.GetGivenName().ToList();
		public override bool CanGoToStatue(bool toKingStatue) => toKingStatue;
		public override bool CanTownNPCSpawn(int numTownNPCs) { // Requirements for the town NPC to spawn.
			if (OriginSystem.Instance.unlockedBrineNPC) return true;
			for (int k = 0; k < 255; k++) {
				Player player = Main.player[k];
				if (!player.active) {
					continue;
				}

				if (player.HasItemInAnyInventory(ModContent.ItemType<Eitrite_Ore_Item>())) {
					OriginSystem.Instance.unlockedBrineNPC = true;
					return true;
				}
			}

			return false;
		}

		public override void SetChatButtons(ref string button, ref string button2) { // What the chat buttons are when you open up the chat UI
			button = Language.GetTextValue("LegacyInterface.28");
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shopName) {
			if (firstButton) {
				shopName = "Shop";
			}
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
			projType = ModContent.ProjectileType<Brine_Droplet>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 8f;
			randomOffset = 2f;
			gravityCorrection = 30;
		}

		public override ITownNPCProfile TownNPCProfile() {
			return new Brine_Fiend_Profile();
		}
	}
	public class Brine_Fiend_Profile : ITownNPCProfile {
		public int RollVariation() => 0;
		public string GetNameForVariant(NPC npc) => npc.getNewNPCName();
		public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) {
			if (npc.IsABestiaryIconDummy && !npc.ForcePartyHatOn)
				return ModContent.Request<Texture2D>("Origins/NPCs/TownNPCs/Brine_Fiend");

			//if (npc.altTexture == 1)
			//return ModContent.Request<Texture2D>("Origins/NPCs/TownNPCs/Brine_Fiend_Party");

			return ModContent.Request<Texture2D>("Origins/NPCs/TownNPCs/Brine_Fiend");
		}

		public int GetHeadTextureIndex(NPC npc) => ModContent.GetModHeadSlot("Origins/NPCs/TownNPCs/Brine_Fiend_Head");
	}
}
