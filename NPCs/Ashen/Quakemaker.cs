using Origins.Gores;
using Origins.Items.Accessories;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.NPCs.Ashen {
	public class Quakemaker_Head : WormHead {
		public override int BodyType => ModContent.NPCType<Quakemaker_Body>();
		public override int TailType => ModContent.NPCType<Quakemaker_Tail>();
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.NPCs.Quakemaker.DisplayName");
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 2;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				CustomTexturePath = "Origins/UI/Quakemaker", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(25f, 32f),
				PortraitPositionXOverride = 4,
				PortraitPositionYOverride = 16f
			};
			ModContent.GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 40;
			NPC.lifeMax = 150;
			NPC.defense = 12;
			NPC.damage = 23;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			//NPC.scale = 0.9f;
			NPC.value = 300;
			SpawnModBiomes = [
				ModContent.GetInstance<Ashen_Biome>().Type,
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns
			);
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerSafe) return 0;
			if (spawnInfo.SpawnTileY < Main.rockLayer) return 0;
			return Ashen_Biome.SpawnRates.Quakemaker;
		}

		public override void Init() {
			MinSegmentLength = 10;
			MaxSegmentLength = 10;
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
		protected override void HeadAI_Movement_PlayDigSounds(float distance) {
			if (NPC.soundDelay == 0 && DigSound.HasValue) {
				// Play sounds quicker the closer the NPC is to the target location
				float delay = distance / 40f;

				if (delay < 10)
					delay = 10f;

				if (delay > 20)
					delay = 20f;

				NPC.soundDelay = (int)delay;

				SoundEngine.PlaySound(DigSound, NPC.Center);
				Rectangle hitbox = NPC.Hitbox.Add((NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * 16);
				hitbox.Inflate(-12, -12);
				if (hitbox.OverlapsAnyTiles()) {
					Main.instance.CameraModifiers.Add(new CameraShakeModifier(
						NPC.Center, 2f, 1f, 10, 500f, -1f, "Quakemaker"
					));
				}
			}
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
				for (int i = 0; i < 4; i++) {
					Gore.NewGorePerfect(
						current.GetSource_Death(),
						current.position,
						(velocity + current.velocity) * 0.5f + Main.rand.NextVector2Circular(2, 2),
						GoreCache.Ashen_Generic
					);
				}
				if (current.type == tailType) break;
				NPC next = Main.npc[(int)current.ai[0]];
				velocity = next.DirectionTo(current.Center) * speed;
				current = next;
			}
		}
	}
	public class Quakemaker_Body : WormBody {
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.NPCs.Quakemaker.DisplayName");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 40;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			//NPC.scale = 0.9f;
		}
		public override void AI() {
			if (!Main.npc.IndexInRange(NPC.realLife)) return;
			NPC.color = HeadSegment.color;
			NPC.alpha = HeadSegment.alpha;
			NPC.GivenName = HeadSegment.GivenName;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			(HeadSegment.ModNPC as Quakemaker_Head)?.TryDeathEffect();
		}
		public override void Init() {
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
	}

	internal class Quakemaker_Tail : WormTail {
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.NPCs.Quakemaker.DisplayName");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 48;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			//NPC.scale = 0.9f;
		}
		public override void AI() {
			if (!Main.npc.IndexInRange(NPC.realLife)) return;
			NPC.color = HeadSegment.color;
			NPC.alpha = HeadSegment.alpha;
			NPC.GivenName = HeadSegment.GivenName;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			(HeadSegment.ModNPC as Quakemaker_Head)?.TryDeathEffect();
		}
		public override void Init() {
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
	}
}