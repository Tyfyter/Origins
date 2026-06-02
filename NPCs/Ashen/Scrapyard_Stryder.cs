//#define DRAWPLATFORM //uncomment this to see where the platform is
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Melee;
using Origins.LootConditions;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.Ashen {
	public class Scrapyard_Stryder : ModNPC, IWikiNPC, IAshenEnemy, IPlatformNPC {
		public Rectangle DrawRect => new(0, 0, 34, 46);
		public int AnimationFrames => 3;
		public int FrameDuration => 3;
		public static int PowerUpTime => 18;
		public Vector2 PlatformOffset => new(NPC.direction == 1 ? -14 : 0, -16);
		public float PlatformWidth => 134;
		Vector2 IPlatformNPC.OldPlatformPosition { get; set; }
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 6;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, Ashen_Biome.SpawnRates.ScrapyardStryder);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Unicorn;
			NPC.lifeMax = 475;
			NPC.defense = 24;
			NPC.damage = 48;
			NPC.width = 120;
			NPC.height = 70;
			NPC.value = 230;
			NPC.knockBackResist = 0.5f;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			AIType = NPCID.Unicorn;
			SpawnModBiomes = [
				GetInstance<Ashen_Biome>().Type,
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void FindFrame(int frameHeight) {
			if (NPC.velocity.Y != 0) {
				NPC.frame.Y = NPC.frame.Height * 5;
				NPC.frameCounter = 0;
				return;
			}
			NPC.DoFrames(16, (NPC.position.X - NPC.oldPosition.X) * NPC.direction);
			NPC.spriteDirection = NPC.direction;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ScavengerBonus.Scrap(amountDroppedMinimum: 5, amountDroppedMaximum: 11));
			npcLoot.Add(ItemDropRule.Common(ItemType<BBQ_Skewer>(), 19));
			npcLoot.Add(ItemDropRule.Common(ItemType<The_Muffler>(), 80));
		}
		public override void HitEffect(NPC.HitInfo hit) {
		}
#if DRAWPLATFORM
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2 platformStart = NPC.position + PlatformOffset - screenPos;
			OriginExtensions.DrawDebugLineSprite(platformStart, platformStart + PlatformWidth * Vector2.UnitX, Color.Red);
		}
		struct DebugFlag : IDebugFlag;
#endif
	}
}
