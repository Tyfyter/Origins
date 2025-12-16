using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Corrupt {
    public class Optiphage : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 16, 30);
		public int AnimationFrames => 2;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public static new AutoCastingAsset<Texture2D> HeadTexture { get; private set; }
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 2;
			if (!Main.dedServ) {
				HeadTexture = Mod.Assets.Request<Texture2D>("NPCs/Corrupt/Optiphage_Head");
			}
			CorruptGlobalNPC.NPCTypes.Add(Type);
			AssimilationLoader.AddNPCAssimilation<Corrupt_Assimilation>(Type, 0.02f);
		}
		public override void Unload() {
			HeadTexture = null;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DemonEye);
			NPC.aiStyle = Terraria.ID.NPCAIStyleID.StarCell;
			NPC.lifeMax = 14;
			NPC.defense = 2;
			NPC.damage = 14;
			NPC.width = 16;
			NPC.height = 16;
			NPC.friendly = false;
			NPC.noGravity = true;
			NPC.aiStyle = NPCAIStyleID.Demon_Eye;
			NPC.value = 34;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerFloorY > Main.worldSurface + 50 || spawnInfo.SpawnTileY >= Main.worldSurface - 50) return 0;
			if (!spawnInfo.Player.ZoneCorrupt) return 0;
			return 0.05f * (spawnInfo.Player.ZoneSkyHeight ? 2 : 1);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.RottenChunk, 5));
		}
		public override void AI() {
			if (NPC.aiStyle == NPCAIStyleID.Star_Cell) {
				NPC.hide = true;
				if (NPC.ai[0] == 0f) {
					NPC.aiStyle = NPCAIStyleID.Demon_Eye;
				}else if (NPC.ai[0] == 1f) {
					Player targetPlayer = Main.player[NPC.target];
					NPC.BottomRight = targetPlayer.Center;
					targetPlayer.AddBuff(Optiphage_Debuff.ID, 5);
				}
			} else {
				NPCAimedTarget target = NPC.GetTargetData();
				NPC.rotation = NPC.AngleTo(target.Center) + MathHelper.PiOver2;
				if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
				NPC.spriteDirection = NPC.direction;
				NPC.hide = false;
				if (++NPC.frameCounter > 5) {
					NPC.frame = new Rectangle(0, (NPC.frame.Y + 30) % 60, 16, 30);
					NPC.frameCounter = 0;
				}
			}
		}
		public override bool CanHitNPC(NPC target)/* tModPorter Suggestion: Return true instead of null */ {
			return NPC.aiStyle != NPCAIStyleID.Star_Cell;
		}
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
			return NPC.aiStyle != NPCAIStyleID.Star_Cell;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			NPC.aiStyle = NPCAIStyleID.Star_Cell;
			NPC.ai[0] = 1;
		}
		public override void DrawBehind(int index) {
			if (NPC.aiStyle == NPCAIStyleID.Star_Cell) Main.instance.DrawCacheNPCsOverPlayers.Add(index);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (NPC.aiStyle == NPCAIStyleID.Star_Cell) {
				if (NPC.GetGlobalNPC<OriginGlobalNPC>().birdedTime > 0) {
					NPC.aiStyle = NPCAIStyleID.Demon_Eye;
					return true;
				}
				Player targetPlayer = Main.player[NPC.target];
				Main.EntitySpriteDraw(
					HeadTexture,
					(targetPlayer.Top.Floor() + new Vector2(0, 8 + targetPlayer.gfxOffY) + Main.OffsetsPlayerHeadgear[targetPlayer.bodyFrame.Y / targetPlayer.bodyFrame.Height]) - screenPos,
					null,
					drawColor,
					targetPlayer.headRotation,
					new Vector2(8 - targetPlayer.direction * 8, 2),
					NPC.scale,
					targetPlayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				0);
				return false;
			}
			return true;
		}
	}
}
