using AltLibrary.Core;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Magic;
using Origins.Tiles.MusicBoxes;
using Origins.World.BiomeData;
using ReLogic.Content;
using System;
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
	public class Defiled_Effigy : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 40, 54);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 1; // The amount of frames the NPC has, walk frame count (15) + ExtraFramesCount

			NPCID.Sets.ExtraFramesCount[Type] = 0; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs.
			NPCID.Sets.AttackFrameCount[Type] = 0;
			NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
			NPCID.Sets.AttackType[Type] = 2;
			NPCID.Sets.AttackTime[Type] = 90; // The amount of time it takes for the NPC's attack animation to be over once it starts.
			NPCID.Sets.AttackAverageChance[Type] = 30;
			NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.
			NPC.Happiness
				.SetBiomeAffection<Defiled_Wastelands>(AffectionLevel.Love)
				.SetNPCAffection(NPCID.Dryad, AffectionLevel.Hate)// Defiled Envoy
			; // < Mind the semicolon!
			InvalidateNPCHousing.NPCTypeIgnoresSpecificBiome.Add(Type, [ModContent.GetInstance<Defiled_Wastelands_Alt_Biome>()]);

			//TODO: remove when added
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiaryUnimplemented;
			TownNPCStayingHomeless = true;
			BetterDialogue.BetterDialogue.SupportedNPCs.Add(Type);
			BetterDialogue.BetterDialogue.RegisterShoppableNPC(Type);
		}
		public override void SetDefaults() {
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = Terraria.ID.NPCAIStyleID.Passive;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.knockBackResist = 0.5f;
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.HitSound = SoundID.Dig;
			NPC.DeathSound = SoundID.Dig;
			NPC.knockBackResist = 0;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void PostAI() {
			NPC.position.X -= NPC.velocity.X;
			NPC.spriteDirection = -Math.Sign(NPC.velocity.X);
		}
		public override string GetChat() {
			WeightedRandom<string> chat = new();

			chat.AddNPCDialogue(Name, "Standard");
			chat.AddOtherNPCDialogue(Name, NPCID.Guide);
			chat.AddOtherNPCDialogue(Name, NPCID.Dryad);
			if (chat.AddOtherNPCDialogue(Name, NPCID.PartyGirl) && BirthdayParty.PartyIsUp) {
				chat.AddNPCDialogue(Name, "Party");
			}
			if (OriginSystem.tDefiled == 0) chat.AddNPCDialogue(Name, "DefiledNotPresent");
			if (Main.IsItStorming) chat.AddNPCDialogue(Name, "Storm");
			else if (Main.WindyEnoughForKiteDrops) chat.AddNPCDialogue(Name, "Wind");
			if (Main.LocalPlayer.ZoneGraveyard) chat.AddNPCDialogue(Name, "Graveyard");
			if (Main.bloodMoon) chat.AddNPCDialogue(Name, "BloodMoon");
			
			return chat; // chat is implicitly cast to a string.
		}
		public override bool CheckConditions(int left, int right, int top, int bottom) {
			//Terraria.WorldGen.ScoreRoom();
			return base.CheckConditions(left, right, top, bottom);
		}
		public override bool CanGoToStatue(bool toKingStatue) => false;

		public override void SetChatButtons(ref string button, ref string button2) { // What the chat buttons are when you open up the chat UI
			button = Language.GetTextValue("LegacyInterface.28");
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shopName) {
			if (firstButton) {
				shopName = "Shop";
			}
		}

		public override void AddShops() {
			return; // temp, these show up in shop browsers
			new NPCShop(Type)
			.Add<Nerve_Flan_Food>()
			.Add(Music_Box.ItemType<Otherworldly_Music_Box_DW>())
			.Register();
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
			projType = ModContent.ProjectileType<Low_Signal_P>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 8f;
			randomOffset = 0f;
			gravityCorrection = 0;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			NPC.spriteDirection = 1;
			return true;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			NPC.spriteDirection = -Math.Sign(NPC.velocity.X);
		}
		public override ITownNPCProfile TownNPCProfile() {
			return new Defiled_Effigy_Profile();
		}
	}
	public class Defiled_Effigy_Profile : ITownNPCProfile {
		public int RollVariation() => 0;
		public string GetNameForVariant(NPC npc) => npc.getNewNPCName();
		public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) {
			if (npc.IsABestiaryIconDummy && !npc.ForcePartyHatOn)
				return ModContent.Request<Texture2D>("Origins/NPCs/TownNPCs/Defiled_Effigy");

			//if (npc.altTexture == 1)
			//return ModContent.Request<Texture2D>("Origins/NPCs/TownNPCs/Acid_Freak_Party");

			return ModContent.Request<Texture2D>("Origins/NPCs/TownNPCs/Defiled_Effigy");
		}

		public int GetHeadTextureIndex(NPC npc) => ModContent.GetModHeadSlot("Origins/NPCs/TownNPCs/Defiled_Effigy_Head");
	}
}
