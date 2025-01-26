using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Items.Weapons.Demolitionist;
using Origins.NPCs.Riven;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod.Items.HealerItems;

namespace Origins.NPCs.MiscE {
	public class Slime_Worm_Head : WormHead, ICustomWikiStat {
		public override int BodyType => ModContent.NPCType<Slime_Worm_Body>();
		public override int TailType => ModContent.NPCType<Slime_Worm_Tail>();
		//public override void Load() => this.AddBanner();
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.NPCs.Slime_Worm.DisplayName");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				CustomTexturePath = "Origins/UI/Slime_Worm_Preview", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(0f, 8f),
				PortraitPositionYOverride = 0f
			};
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 24;
			NPC.lifeMax = 50;
			NPC.defense = 7;
			NPC.damage = 23;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath23;
			//NPC.scale = 0.9f;
			NPC.value = 70;
			NPC.alpha = 175;
		}
		public override void OnFinishSpawning() {
			NPC.life = NPC.lifeMax = (SegmentCount + 1) * 10;
		}
		public override void AI() {
			NPC.lifeMax = (SegmentCount + 1) * 10;
			string colorName = "Blue";
			switch (SegmentCount) {
				case 3:
				NPC.color = new(0, 220, 40, 100);
				colorName = "Green";
				break;
				case 4:
				NPC.color = new(0, 80, 255, 100);
				colorName = "Blue";
				break;
				case 5:
				NPC.color = new(255, 30, 0, 100);
				colorName = "Red";
				break;
				case 6:
				NPC.color = new(200, 0, 255, 150);
				colorName = "Purple";
				break;
				case 7:
				NPC.color = new(255, 255, 0, 100);
				colorName = "Yellow";
				break;
				case 8:
				NPC.color = new(0, 0, 0, 50);
				colorName = "Black";
				break;
			}
			NPC.GivenName = Language.GetOrRegister("Mods.Origins.NPCs.Slime_Worm.DisplayNameFormattable").Format(Language.GetOrRegister("Mods.Origins.NPCs.Slime_Worm." + colorName));
		}
		public override bool SpecialOnKill() {
			int bodyType = BodyType;
			NPC current = NPC;
			int slimeType = NPCID.BlueSlime;
			switch (SegmentCount) {
				case 3:
				slimeType = NPCID.GreenSlime;
				break;
				case 4:
				slimeType = NPCID.BlueSlime;
				break;
				case 5:
				slimeType = NPCID.RedSlime;
				break;
				case 6:
				slimeType = NPCID.PurpleSlime;
				break;
				case 7:
				slimeType = NPCID.YellowSlime;
				break;
				case 8:
				slimeType = NPCID.BlackSlime;
				break;
			}
			while (current is not null) {
				NPC.NewNPC(NPC.GetSource_Death(), (int)current.Center.X, (int)current.Center.Y, slimeType);
				current = (current.type == bodyType || current == NPC) ? Main.npc[(int)current.ai[0]] : null;
			}
			return false;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.SlimeRain,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground
			);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.SpawnTileY <= Main.worldSurface || spawnInfo.PlayerSafe || spawnInfo.DesertCave || !Main.slimeRain) return 0;
			return 0.09f;
		}
		public void ModifyWikiStats(JObject data) {
			data["SpriteWidth"] = 108;
		}

		public override void Init() {
			MinSegmentLength = 3;
			MaxSegmentLength = 8;
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			TryDeathEffect();
		}
		public void TryDeathEffect() {
			if (NPC.life > 0 || NPC.aiAction == 1) return;
			NPC.aiAction = 1;
			NPC current = NPC;
			Vector2 velocity = NPC.velocity * 1.25f;
			float speed = velocity.Length();
			HashSet<int> indecies = [];
			int tailType = TailType;
			while (current.ai[0] != 0) {
				if (!indecies.Add(current.whoAmI)) break;
				OriginExtensions.LerpEquals(
					ref Dust.NewDustPerfect(
						current.position,
						DustID.t_Slime,
						velocity + Main.rand.NextVector2Circular(4, 4),
						newColor: NPC.color
					).velocity,
					current.velocity,
					0.5f
				);
				if (current.type == tailType) break;
				NPC next = Main.npc[(int)current.ai[0]];
				velocity = next.DirectionTo(current.Center) * speed;
				current = next;
			}
		}
	}
	public class Slime_Worm_Body : WormBody {
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.NPCs.Slime_Worm.DisplayName");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 24;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath23;
			//NPC.scale = 0.9f;
		}
		public override void AI() {
			if (!Main.npc.IndexInRange(NPC.realLife)) return;
			NPC.color = HeadSegment.color;
			NPC.alpha = HeadSegment.alpha;
			NPC.GivenName = HeadSegment.GivenName;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			(HeadSegment.ModNPC as Slime_Worm_Head)?.TryDeathEffect();
		}
		public override void Init() {
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
	}

	internal class Slime_Worm_Tail : WormTail {
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.NPCs.Slime_Worm.DisplayName");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 24;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath23;
			//NPC.scale = 0.9f;
		}
		public override void AI() {
			if (!Main.npc.IndexInRange(NPC.realLife)) return;
			NPC.color = HeadSegment.color;
			NPC.alpha = HeadSegment.alpha;
			NPC.GivenName = HeadSegment.GivenName;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			(HeadSegment.ModNPC as Slime_Worm_Head)?.TryDeathEffect();
		}
		public override void Init() {
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
	}
}