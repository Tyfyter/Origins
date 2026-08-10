#define DRAWPLATFORM //uncomment this to see where the platform is
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Melee;
using Origins.LootConditions;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen {
	public class VV13 : ModNPC, IWikiNPC, IAshenEnemy, IPlatformNPC, ISpecialTargetingNPC {
		public Rectangle DrawRect => new(0, 0, 34, 46);
		public int AnimationFrames => 3;
		public int FrameDuration => 3;
		public Vector2 PlatformOffset => new(0, 0);
		public float PlatformWidth => 128;
		Vector2 IPlatformNPC.OldPlatformPosition { get; set; }
		static AutoLoadingTexture glowTexture = typeof(Scrapyard_Stryder).GetDefaultTMLName("_Glow");
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with {
				Position = new Vector2(35, 0),
				PortraitPositionXOverride = 0,
				PortraitPositionYOverride = 0
			};
			GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, Ashen_Biome.SpawnRates.ScrapyardStryder);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 475;
			NPC.defense = 24;
			NPC.damage = 48;
			NPC.width = 124;
			NPC.height = 46;
			NPC.value = 230;
			NPC.knockBackResist = 0.5f;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			NPC.noGravity = true;
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
			bestiaryEntry.CustomBestiaryName(Type, this.GetLocalizationKey("FullName"));
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void AI() {
			if (Main.rand.NextBool(650)) {
				SoundEngine.PlaySound(Origins.Sounds.VV13Idle, NPC.Center);
			}
			NPC.TargetClosestUpgraded();
			if (NPC.HasValidTarget && NPC.HasPlayerTarget) {
				NPCAimedTarget target = NPC.GetTargetData();
				float speed = 8f;
				float inertia = 128f;
				//NPC.rotation = NPC.velocity.X * 0.1f;
				Vector2 vectorToTargetPosition = (target.Center - NPC.Center).Normalized(out float dist);
				if (NPC.confused) vectorToTargetPosition *= -1;
				const float hover_range = 16 * 13;
				if (dist < hover_range - 32) {
					speed *= -1;
				} else if (dist < hover_range + 32) {
					speed = 0;
					if (NPC.velocity.LengthSquared() < 0.1f) {
						// If there is a case where it's not moving at all, give it a little "poke"
						NPC.velocity += Main.rand.NextVector2Circular(1, 1) * 0.05f;
					}
				}
				NPC.spriteDirection = Math.Sign(vectorToTargetPosition.X);
				vectorToTargetPosition *= speed;
				NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			}
			Vector2 nextVel = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, true, true);
			if (nextVel.X != NPC.velocity.X) NPC.velocity.X *= -0.9f;
			if (nextVel.Y != NPC.velocity.Y) NPC.velocity.Y *= -0.9f;
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.MeatGrinder, 200));
			npcLoot.Add(ScavengerBonus.Scrap(amountDroppedMinimum: 5, amountDroppedMaximum: 11));
			npcLoot.Add(ItemDropRule.Common(ItemType<BBQ_Skewer>(), 19));
			npcLoot.Add(ItemDropRule.Common(ItemType<Phoenum>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ItemType<The_Muffler>(), 80));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			base.HitEffect(hit);
		}
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			npcHitbox.Height -= 22;
			npcHitbox.Y += 22;
			return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Color glowColor = NPC.GetTintColor(Color.White);

			spriteBatch.DrawGlowingNPCPart(
				TextureAssets.Npc[Type].Value,
				glowTexture,
				NPC.Bottom + Vector2.UnitY * 2 - screenPos,
				NPC.frame,
				NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor)),
				glowColor,
				NPC.rotation,
				NPC.frame.Size() * new Vector2(0.5f, 1),
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			NPC.DrawConfused();
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
