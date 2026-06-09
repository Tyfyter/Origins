//#define DRAWPLATFORM //uncomment this to see where the platform is
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Melee;
using Origins.LootConditions;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen {
	public class Scrapyard_Stryder : ModNPC, IWikiNPC, IAshenEnemy, IPlatformNPC, ISpecialTargetingNPC {
		public Rectangle DrawRect => new(0, 0, 34, 46);
		public int AnimationFrames => 3;
		public int FrameDuration => 3;
		public static int PowerUpTime => 18;
		public Vector2 PlatformOffset => new(NPC.direction * -28 - NPC.width * 0.5f, -22);
		public float PlatformWidth => 134;
		Vector2 IPlatformNPC.OldPlatformPosition { get; set; }
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 6;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with { Position = new(15, 45), PortraitPositionXOverride = -5, PortraitPositionYOverride = 0 };
			GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, Ashen_Biome.SpawnRates.ScrapyardStryder);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Unicorn;
			NPC.lifeMax = 475;
			NPC.defense = 24;
			NPC.damage = 48;
			NPC.width = 64;
			NPC.height = 64;
			NPC.value = 230;
			NPC.knockBackResist = 0.5f;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			AIType = NPCID.Unicorn;
			SpawnModBiomes = [
				GetInstance<Ashen_Biome>().Type,
			];
		}
		public void TargetClosest(bool faceTarget = true, Vector2? checkPosition = null) {
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.Players, SearchFilter);
			if (searchResults.FoundTarget) {
				NPC.target = searchResults.NearestTargetIndex;
				NPC.targetRect = searchResults.NearestTargetHitbox;
				if (faceTarget && NPC.ShouldFaceTarget(ref searchResults)) NPC.FaceTarget();
			}
		}
		bool SearchFilter(Player player) => player?.OriginPlayer()?.standingOnPlatformNPC != NPC;
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
			if (NPC.IsABestiaryIconDummy) NPC.DoFrames(16, 6);
			else NPC.DoFrames(16, (NPC.position.X - NPC.oldPosition.X) * NPC.direction);
			NPC.spriteDirection = NPC.direction;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ScavengerBonus.Scrap(amountDroppedMinimum: 5, amountDroppedMaximum: 11));
			npcLoot.Add(ItemDropRule.Common(ItemType<BBQ_Skewer>(), 19));
			npcLoot.Add(ItemDropRule.Common(ItemType<Phoenum>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ItemType<The_Muffler>(), 80));
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				NPC.Bottom - screenPos,
				NPC.frame,
				NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor)),
				NPC.rotation,
				NPC.frame.Size() * new Vector2(0.5f + (NPC.spriteDirection * 0.15f), 1),
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
			0f);
			return false;
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
