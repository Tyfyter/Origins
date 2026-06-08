using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Core;
using Origins.Dev;
using Origins.World.BiomeData;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen {
	public class Packhunter : ModNPC, IAshenEnemy, ISpecialTargetingNPC {
		static AutoLoadingTexture glowTexture = typeof(Packhunter).GetDefaultTMLName("_Glow");
		static AutoLoadingTexture headTexture = typeof(Packhunter).GetDefaultTMLName("_Head");
		static AutoLoadingTexture headGlowTexture = typeof(Packhunter).GetDefaultTMLName("_Head_Glow");
		Vector2 NeckPosition => NPC.Center + new Vector2(NPC.direction * (NPC.width * 0.5f - 12), DrawOffsetY - 7);
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 9;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with { Position = new Vector2(2, 0) };
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 40;
			NPC.defense = 6;
			NPC.damage = 25;
			NPC.width = 44;
			NPC.height = 30;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			NPC.knockBackResist = 0.3f;
			NPC.value = 75;
			NPC.target = Main.maxPlayers;
			SpawnModBiomes = [
				ModContent.GetInstance<Ashen_Biome>().Type,
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
		}
		public override bool? CanFallThroughPlatforms() => NPC.targetRect.Bottom > NPC.position.Y + NPC.height + NPC.velocity.Y;
		public void TargetClosest(bool faceTarget = true, Vector2? checkPosition = null) {
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.Players, SearchFilter);
			seesTarget = searchResults.FoundTarget;
			if (searchResults.FoundTarget) {
				NPC.target = searchResults.NearestTargetIndex;
				NPC.targetRect = searchResults.NearestTargetHitbox;
				if (faceTarget && NPC.ShouldFaceTarget(ref searchResults)) NPC.FaceTarget();
			}
		}
		float GetPlayerAggro(Player player) {
			float aggro = player.aggro;
			switch (NPC.aiAction) {
				case 1:
				if (player.whoAmI == NPC.target) aggro += 600;
				else aggro -= 300;
				break;
				case 2:
				case 3:
				if (player.whoAmI == NPC.target) aggro += 300;
				break;
				case 4:
				aggro = 0;
				break;
			}
			return aggro;
		}
		bool SearchFilter(Player player) {
			Rectangle playerHitbox = player.Hitbox;
			return NPC.Hitbox.Intersects(playerHitbox) || GetViewTriangle(GetPlayerAggro(player)).Intersects(playerHitbox);
		}

		Vector2 viewPos;
		Vector2 viewDirection;
		bool seesTarget;
		Triangle GetViewTriangle(float aggro) {
			float viewDist = 16 * 25 + aggro * 0.5f;
			float baseRatio = 0.5f + NPC.ai[2];
			return new(
				viewPos,
				viewPos + (viewDirection + viewDirection.Perpendicular(1) * baseRatio) * viewDist,
				viewPos + (viewDirection + viewDirection.Perpendicular(-1) * baseRatio) * viewDist
			);
		}
		public override void AI() {
			viewPos = NeckPosition + viewDirection * 16;
			NPC.TargetClosest(false);
			NPCAimedTarget target = NPC.GetTargetData();

			if (!NPC.collideY && NPC.velocity.Y == 0) {
				NPC.collideY = Collision.GetTilesIn(NPC.BottomLeft + Vector2.UnitY, NPC.BottomRight + Vector2.UnitY * 16).Any(pos => Framing.GetTileSafely(pos).HasSolidTile());
			}
			Vector2 diffFromTarget = NPC.Center.Clamp(NPC.targetRect) - NPC.targetRect.Center().Clamp(NPC.Hitbox);

			bool targetInvalid = NPC.GetTargetData().Invalid;
			Vector2 targetDirection = targetInvalid ? default : NPC.DirectionTo(NPC.targetRect.Center());
			int currentMoveDirection = float.Sign(NPC.velocity.X);

			float acceleration = 0.15f;
			switch (NPC.aiAction) {
				case 0:// walking
				GeometryUtils.AngularSmoothing(ref NPC.rotation, 1.57f * NPC.direction - MathHelper.PiOver2, 0.05f);
				if (targetInvalid) {
					if (NPC.ai[3] >= 60) {
						NPC.direction *= -1;
						NPC.ai[3] = 0;
					}
				} else {
					if (Math.Abs(diffFromTarget.X) < 16 * 5) {
						NPC.aiAction = 2;
						NPC.ai[0] = 0;
						NPC.netUpdate = true;
					}
				}
				break;
				case 1:// running
				if (targetInvalid) {
					NPC.aiAction = 0;
					NPC.netUpdate = true;
					goto case 0;
				}
				if (!seesTarget) {
					NPC.aiAction = 3;
					NPC.netUpdate = true;
					break;
				}
				acceleration = 0.4f;
				NPC.rotation = targetDirection.ToRotation();
				break;
				case 2:// looking
				acceleration = 0f;
				GeometryUtils.AngularSmoothing(ref NPC.rotation, targetDirection.ToRotation(), 0.05f);
				if (seesTarget) {
					NPC.ai[0]++;
					NPC.ai[1] = 0;
				} else {
					NPC.ai[1]++;
				}
				if (NPC.ai[0] > 60) {
					NPC.aiAction = 4;
					NPC.ai[0] = 0;
					NPC.netUpdate = true;
				} else if (NPC.ai[1] > 90) {
					NPC.target = Main.maxPlayers;
					NPC.aiAction = 0;
					NPC.ai[0] = 0;
					NPC.netUpdate = true;
				}
				break;
				case 3:// searching for lost target
				GeometryUtils.AngularSmoothing(ref NPC.rotation, 1.57f * NPC.direction - MathHelper.PiOver2, 0.05f);
				acceleration = 0.18f;
				if (seesTarget) {
					NPC.aiAction = 1;
					NPC.ai[0] = 0;
					NPC.netUpdate = true;
					break;
				}
				if (NPC.collideX) NPC.direction *= -1;
				if (targetInvalid || ++NPC.ai[0] > 60 * 10) {
					NPC.target = Main.maxPlayers;
					NPC.aiAction = 0;
					NPC.ai[0] = 0;
					NPC.netUpdate = true;
				}
				targetInvalid = true;
				break;
				case 4:// flashing
				acceleration = 0f;
				GeometryUtils.AngularSmoothing(ref NPC.rotation, targetDirection.ToRotation(), 0.05f);
				if (++NPC.ai[0] > 15) {
					foreach (Player player in Main.ActivePlayers) {
						if (GetViewTriangle(GetPlayerAggro(player)).Intersects(player.Hitbox)) {
							player.AddBuff(Flashbang_Debuff.ID, 65);
							if (player.whoAmI == Main.myPlayer && OriginsModIntegrations.CheckAprilFools() && TextUtils.LanguageTree.Find("Mods.Origins.AprilFools.Buffs.Flashbang_Debuff.DogDescription") is LanguageTree desc) {
								Flashbang_Debuff.descriptionOverride = desc.value;
							}
						}
					}
					NPC.aiAction = 1;
					NPC.ai[0] = 0;
					NPC.netUpdate = true;
				}
				break;
			}
			MathUtils.LinearSmoothing(ref NPC.ai[2], (NPC.aiAction == 3).Mul(0.5f), 0.01f + NPC.ai[2] * 0.1f);
			if (NPC.direction == 0) NPC.direction = -1;
			int targetMoveDirection = targetInvalid ? NPC.direction : float.Sign(targetDirection.X);

			if (currentMoveDirection != targetMoveDirection) acceleration *= 0.25f;
			if (!NPC.collideY) acceleration *= 0.25f;

			NPC.velocity.X += acceleration * targetMoveDirection;

			if (NPC.collideY) NPC.velocity.X *= 0.97f;
			if (currentMoveDirection == targetMoveDirection) NPC.velocity.X *= NPC.collideY ? 0.93f : 0.98f;

			float preStepOffY = NPC.gfxOffY;
			if (NPC.collideY) {
				Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
				Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			}

			if (NPC.direction.TrySet(targetMoveDirection)) {
				switch (NPC.aiAction) {
					case 0:
					case 3:
					NPC.rotation = MathHelper.PiOver2 - 1.57f * NPC.direction;
					break;
				}
			}
			if ((NPC.collideX || Math.Abs(NPC.velocity.X) < 0.05f) && targetInvalid) {
				if (NPC.collideY || NPC.ai[3] > 0) NPC.ai[3]++;
			} else NPC.ai[3] = 0;
			if (NPC.collideY && Math.Abs(NPC.velocity.Y) == 0) {
				bool shouldJump = false;
				if (NPC.collideX && preStepOffY == NPC.gfxOffY) shouldJump = true;
				else if (!targetInvalid) {
					if (Math.Abs(NPC.Center.Y - target.Center.Y) <= 8.5f * 16 && Math.Abs(NPC.Center.X - target.Center.X) <= 4) {
						NPC.velocity.X *= 0.2f;
						shouldJump = true;
					} else if (target.Position.Y + target.Height < NPC.position.Y && !NPC.Hitbox.Add(new Vector2(NPC.width * 0.5f, 16)).OverlapsAnyTiles(false)) {
						shouldJump = true;
					}
				}
				if (shouldJump) NPC.velocity.Y -= 8;
			}
			NPC.spriteDirection = NPC.direction;
			viewDirection = NPC.rotation.ToRotationVector2();
		}
		public override void FindFrame(int frameHeight) {
			DrawOffsetY = 0;
			switch (NPC.aiAction) {
				case 2:
				NPC.frame.Y = NPC.frame.Height * 8;
				return;
			}
			if (!NPC.collideY && NPC.velocity.Y == 0) {
				if (OriginsModIntegrations.CheckAprilFools()) {
					DrawOffsetY = 16;
				} else {
					NPC.collideY = Collision.GetTilesIn(NPC.BottomLeft + Vector2.UnitY, NPC.BottomRight + Vector2.UnitY * 16).Any(pos => Framing.GetTileSafely(pos).HasSolidTile());
				}
			}
			if (NPC.collideY || NPC.IsABestiaryIconDummy) {
				float speed = Math.Abs(NPC.velocity.X);
				switch (NPC.aiAction) {
					case 0:
					case 3:
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
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			Color glowColor = Color.White;
			using (NPC.oiled.ScopedOverride(false)) {
				NPCLoader.DrawEffects(NPC, ref glowColor);
				glowColor = NPC.GetNPCColorTintedByBuffs(glowColor);
			}
			spriteBatch.DrawGlowingNPCPart(
				TextureAssets.Npc[Type].Value,
				glowTexture,
				NPC.Bottom + Vector2.UnitY * (4 + DrawOffsetY) - screenPos,
				NPC.frame,
				drawColor,
				glowColor,
				0,
				NPC.frame.Size() * new Vector2(0.5f, 1),
				NPC.scale,
				NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
			if (NPC.frame.Y == NPC.frame.Height * 8) {
				spriteBatch.DrawGlowingNPCPart(
					headTexture,
					headGlowTexture,
					NeckPosition - screenPos,
					null,
					drawColor,
					glowColor,
					NPC.rotation + MathHelper.Pi,
					headTexture.Value.Size() * new Vector2(1, 0.5f),
					NPC.scale,
					NPC.spriteDirection == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None
				);
			}
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (NPC.aiAction == 1) return;
			GameShaders.Misc["Origins:Identity"]
			.UseImage0(TextureAssets.MagicPixel)
			.UseSamplerState(SamplerState.LinearClamp)
			.Apply();
			Triangle viewTriangle = GetViewTriangle(GetPlayerAggro(Main.LocalPlayer));
			//screenPos.Y -= NPC.gfxOffY;
			switch (NPC.frame.Y / NPC.frame.Height) {
				case 2:
				case 3:
				case 6:
				screenPos.Y -= 2;
				break;
			}
			Color color = new Color(96, 72, 48, 0);
			switch (NPC.aiAction) {
				case 4:
				if (NPC.ai[0] < 10) {
					color *= (1f - NPC.ai[0] * 0.03f);
				} else {
					color *= 1 + NPC.ai[0] * 0.2f;
				}
				break;
			}
			vertices[0].Color = color;
			vertices[1].Color = new Color(0, 0, 0, 0);
			vertices[2].Color = new Color(0, 0, 0, 0);
			vertices[0].Position = new(viewTriangle.a - screenPos, 0);
			vertices[1].Position = new(viewTriangle.b - screenPos, 0);
			vertices[2].Position = new(viewTriangle.c - screenPos, 0);
			Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length, dices, 0, 2);
		}
	}
}
