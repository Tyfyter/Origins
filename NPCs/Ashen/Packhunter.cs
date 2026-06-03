using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Core;
using Origins.Dev;
using Origins.Gores.NPCs;
using Origins.Items.Weapons.Summoner;
using PegasusLib;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.Packhunter;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen {
	public class Packhunter : ModNPC, IWikiNPC, IAshenEnemy, ISpecialTargetingNPC {
		public Rectangle DrawRect => new(0, 0, 82, 50);
		public int AnimationFrames => 120;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public bool wasHit;
		public bool turning;
		public int turnDirection;
		public int turnTime;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 9;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with { Position = new Vector2(7, 0)};
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 40;
			NPC.defense = 6;
			NPC.damage = 25;
			NPC.width = 54;
			NPC.height = 50;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath38.WithPitch(2f);
			NPC.knockBackResist = 0.3f;
			NPC.value = 75;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			if (Main.rand.NextBool()) target.AddBuff(BuffID.Bleeding, 20);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit) {
			if (Main.rand.NextBool()) target.AddBuff(BuffID.Bleeding, 20);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
		}
		public override bool? CanFallThroughPlatforms() => NPC.targetRect.Bottom > NPC.position.Y + NPC.height + NPC.velocity.Y;
		public void TargetClosest(bool faceTarget = true, Vector2? checkPosition = null) {
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.Players, SearchFilter);
			if (searchResults.FoundTarget) {
				NPC.target = searchResults.NearestTargetIndex;
				NPC.targetRect = searchResults.NearestTargetHitbox;
				if (faceTarget && NPC.ShouldFaceTarget(ref searchResults)) NPC.FaceTarget();
			}
		}
		bool SearchFilter(Player player) => viewTriangle.Intersects(player.Hitbox);
		Triangle viewTriangle;
		void SetViewTriangle(Vector2 head, Vector2 direction) {
			const float view_dist = 16 * 25;
			const float sqrt_half = 0.5f;
			viewTriangle = new(
				head,
				head + (direction + direction.Perpendicular(1) * sqrt_half) * view_dist,
				head + (direction + direction.Perpendicular(-1) * sqrt_half) * view_dist
			);
		}
		public override void AI() {
			NPC.TargetClosest(false);
			NPCAimedTarget target = NPC.GetTargetData();

			if (!NPC.collideY && NPC.velocity.Y == 0) {
				NPC.collideY = Collision.GetTilesIn(NPC.BottomLeft + Vector2.UnitY, NPC.BottomRight + Vector2.UnitY * 16).Any(pos => Framing.GetTileSafely(pos).HasSolidTile());
			}
			float distanceFromTarget = NPC.targetRect.Center().Clamp(NPC.Hitbox).Distance(NPC.Center.Clamp(NPC.targetRect));

			bool targetInvalid = NPC.GetTargetData().Invalid;
			int currentMoveDirection = (NPC.velocity.X >= 0).ToDirectionInt();
			int targetMoveDirection = targetInvalid ? NPC.direction : (NPC.DirectionTo(NPC.targetRect.Center()).X >= 0).ToDirectionInt();

			float acceleration = 0.15f;
			switch (NPC.aiAction) {
				case 0:
				break;
				case 1:
				acceleration = 0.4f;
				break;
			}
			if (currentMoveDirection != targetMoveDirection) {
				if (turnDirection != targetMoveDirection) {
					turnDirection = targetMoveDirection;
					turnTime = 0;
				}
				acceleration *= 0.25f;
			}
			if (!NPC.collideY) acceleration *= 0.25f;

			NPC.velocity.X += acceleration * targetMoveDirection;

			if (NPC.collideY) NPC.velocity.X *= 0.97f;
			if (currentMoveDirection == targetMoveDirection) NPC.velocity.X *= 0.93f;

			if (NPC.collideY) {
				Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
				Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			}

			if (++turnTime >= 6 * 2) {
				NPC.direction = targetMoveDirection;
			}
			SetViewTriangle(NPC.Center + new Vector2(NPC.direction * NPC.width * 0.5f, 4), new Vector2(NPC.direction, 0));
			NPC.spriteDirection = NPC.direction;
		}
		public override void FindFrame(int frameHeight) {
			if (turnTime < 6 * 2 && !NPC.IsABestiaryIconDummy) {
				NPC.frame.Y = NPC.frame.Height * 8;
			} else if (NPC.collideY) {
				float speed = Math.Abs(NPC.velocity.X);
				switch (NPC.aiAction) {
					case 0:
					NPC.DoFrames(9, 0..4, speed);
					break;
					case 1:
					NPC.DoFrames(18, 4..8, speed);
					break;
				}
			} else {
				NPC.frame.Y = NPC.frame.Height * 6;
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
		}
		static readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[3];
		static readonly short[] dices = [0, 1, 2];
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			GameShaders.Misc["Origins:Identity"]
			.UseImage0(TextureAssets.MagicPixel)
			.UseSamplerState(SamplerState.LinearClamp)
			.Apply();
			for (int i = 0; i < vertices.Length; i++) vertices[i].Color = Color.Yellow;
			vertices[0].Position = new(viewTriangle.a - screenPos, 0);
			vertices[1].Position = new(viewTriangle.b - screenPos, 0);
			vertices[2].Position = new(viewTriangle.c - screenPos, 0);
			Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length, dices, 0, 2);
		}
	}
}
