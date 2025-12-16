using AltLibrary.Core;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Other.Consumables.Broths;
using Origins.Items.Weapons.Melee;
using Origins.Journal;
using Origins.Questing;
using Origins.Tiles.Brine;
using Origins.Tiles.MusicBoxes;
using Origins.World.BiomeData;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.TownNPCs {
	[AutoloadHead]
	public class Brine_Fiend : ModNPC, IWikiNPC, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Brine_Fiend_Entry).Name;
		public class Brine_Fiend_Entry : JournalEntry {
			public override string TextKey => "Brine_Fiend";
			public override JournalSortIndex SortIndex => new("Brine_Fiend", 0);
		}
		public Rectangle DrawRect => new(0, 6, 36, 48);
		public int AnimationFrames => 16;
		public int FrameDuration => 2;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 23; // The amount of frames the NPC has, walk frame count (15) + ExtraFramesCount

			NPCID.Sets.ExtraFramesCount[Type] = 8; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs.
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
			NPCID.Sets.AttackType[Type] = 1;
			NPCID.Sets.AttackTime[Type] = 90; // The amount of time it takes for the NPC's attack animation to be over once it starts.
			NPCID.Sets.AttackAverageChance[Type] = 30;
			NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() {
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
			.Add<Plain_Broth>()
			.Add<Hearty_Broth>()
			.Add<Sour_Broth>()
			.Add<Foul_Broth>()
			.Add<Sharp_Broth>()
			.Add<Minty_Broth>()
			.Add<Umami_Broth>()
			.Add<Light_Broth>()
			.Add<Sweet_Broth>()
			.Add<Greasy_Broth>()
			.Add<Savory_Broth>()
			.Add<Spicy_Broth>()
			.Add<Minishark_Broth>(OriginsModIntegrations.AprilFools)
			.Add<Akaliegis>(Quest.QuestCondition<Alkaliegis_Quest>())
			.Add(Music_Box.ItemType<Ancient_Music_Box_BP>(), Quest.QuestCondition<Old_Brine_Music_Box_Quest>())
			.Register();
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Boomboom>()));
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
		public override bool CanGoToStatue(bool toKingStatue) => !toKingStatue;
		public override bool CanTownNPCSpawn(int numTownNPCs) => Boss_Tracker.Instance.downedLostDiver;

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
			projType = ModContent.ProjectileType<Brine_Fiend_Boomboom>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 8f;
			randomOffset = 0f;
			gravityCorrection = 0;
		}

		public override ITownNPCProfile TownNPCProfile() {
			return new Brine_Fiend_Profile();
		}
	}
	public class Brine_Fiend_Boomboom : Boomboom_P {
		public override string Texture => typeof(Boomboom_P).GetDefaultTMLName();
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
		}
		public override void OnSpawn(IEntitySource source) {
			base.OnSpawn(source);
			if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC npc) Projectile.localAI[2] = npc.whoAmI;
		}
		public override void AI() {
			if (!Main.npc.IndexInRange((int)Projectile.localAI[2])) {
				Projectile.Kill();
				return;
			}
			NPC owner = Main.npc[(int)Projectile.localAI[2]];
			if (!owner.active) {
				Projectile.Kill();
				return;
			}
			Projectile.DoBoomerangAI(owner);

			base.AI();
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((int)Projectile.localAI[2]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[2] = reader.ReadInt32(); 
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			base.OnHitNPC(target, hit, damageDone);
			if (Projectile.ai[0] == 0f) {
				Projectile.velocity = -Projectile.velocity;
				Projectile.netUpdate = true;
			}

			Projectile.ai[0] = 1f;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.ai[0] == 0f) {
				Projectile.velocity = -Projectile.oldVelocity;
				Projectile.netUpdate = true;
			}

			Projectile.ai[0] = 1f;
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			return false;
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
