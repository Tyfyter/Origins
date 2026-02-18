using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.Utils;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Tiles.Brine;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine {
	public class Leaf_Snapper : Brine_Pool_NPC, ICustomWikiStat {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[NPC.type] = 14;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f,
				IsWet = true
			};
			TargetNPCTypes.Add(ModContent.NPCType<Carpalfish>());
			TargetNPCTypes.Add(ModContent.NPCType<Mildew_Creeper>());
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 140;
			NPC.defense = 50;
			NPC.damage = 0;
			NPC.width = 46;
			NPC.height = 28;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit19;
			NPC.DeathSound = SoundID.NPCDeath22;
			NPC.knockBackResist = 0.95f;
			NPC.value = 250;
			NPC.noGravity = true;
			NPC.chaseable = OriginsModIntegrations.CheckAprilFools();
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Brine_Pool.SpawnRates.Turtle * 0.5f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new LeadingConditionRule(DropConditions.PlayerInteraction).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Alkaliphiliac_Tissue>(), 1, 1, 2)
			));
		}
		public override bool CanTargetNPC(NPC other) {
			return base.CanTargetNPC(other) && other.WithinRange(NPC.Center, 16 * 12 + NPC.width + Math.Max(other.width, other.height));
		}
		public override bool CanTargetPlayer(Player player) {
			return base.CanTargetPlayer(player) && player.WithinRange(NPC.Center, 16 * 12 + NPC.width + Math.Max(player.width, player.height));
		}
		public override void AI() {
			PathfindingTime = 0;
			DoTargeting();
			Vector2 direction;
			if (NPC.GetWet(Liquids.Brine.ID)) {
				NPC.width = (int)(28 * NPC.scale);
				NPC.height = (int)(28 * NPC.scale);
				NPC.aiStyle = NPCAIStyleID.ActuallyNone;
				NPC.noGravity = true;
				if (TargetPos != default) {
					if (NPC.HasNPCTarget) {
						NPC target = Main.npc[NPC.TranslatedTargetIndex];
						if (!target.active || !CanTargetNPC(target)) TargetPos = default;
					} else if (NPC.HasPlayerTarget) {
						Player target = Main.player[NPC.target];
						if (!target.active || target.dead || !CanTargetPlayer(target)) TargetPos = default;
					} else if (NPC.HasNPCTarget) {
						NPC target = Main.npc[NPC.TranslatedTargetIndex];
						if (!target.active || !CanTargetNPC(target)) TargetPos = default;
					}
				}
				if (TargetPos != default) {
					direction = NPC.DirectionFrom(TargetPos);
				} else {
					NPC.ai[0] = 0;
					if (NPC.collideX && Math.Abs(NPC.velocity.X) > Math.Abs(NPC.velocity.Y) * 0.25f) {
						NPC.velocity.X = -NPC.direction;
						NPC.rotation += MathHelper.Pi;
					}
					NPC.direction = Math.Sign(NPC.velocity.X);
					if (NPC.direction == 0) NPC.direction = 1;
					direction = Vector2.UnitX * NPC.direction;
				}
				bool doTileAvoidance = true;
				if (NPC.ai[3] > 300) {
					float dist = 16 * 25;
					int vineType = ModContent.TileType<Brineglow>();
					int vineType2 = ModContent.TileType<Underwater_Vine>();
					for (int i = -3; i < 3; i++) {
						Vector2 dir = Vector2.UnitX.RotatedBy(NPC.rotation + i * 0.5f * NPC.direction);
						float newDist = CollisionExtensions.Raymarch(NPC.Center, dir, tile => !tile.HasTile || (tile.TileType != vineType && tile.TileType != vineType2), dist);
						//OriginExtensions.DrawDebugLine(NPC.Center, NPC.Center + dir * newDist);
						if (TargetPos == default) {
							newDist *= 1 + (i * 0.01f);
						} else {
							newDist *= 1 - Vector2.Dot(direction, dir);
						}
						if (newDist < dist) {
							Tile tile = Framing.GetTileSafely(NPC.Center + dir * (newDist + 2));
							if (tile.HasTile && (tile.TileType == vineType || tile.TileType == vineType2)) {
								dist = newDist;
								direction = dir;
								doTileAvoidance = false;
							}
						}
					}
					if (dist < 28) {
						NPC.ai[3] = Main.rand.NextFloat(0, 90);
						NPC.netUpdate = true;
					}
				} else {
					NPC.ai[3]++;
				}
				if (doTileAvoidance) {
					const float dist = 16 * 4;
					float tileAvoidance = 0;
					for (int i = -3; i < 4; i++) {
						if (i == 0) continue;
						Vector2 dir = Vector2.UnitX.RotatedBy(NPC.rotation + i * 0.5f * NPC.direction);
						float newDist = CollisionExt.Raymarch(NPC.Center, dir, dist);
						//OriginExtensions.DrawDebugLine(NPC.Center, NPC.Center + dir * newDist);
						if (newDist < dist && Framing.GetTileSafely(NPC.Center + dir * (newDist + 2)).HasFullSolidTile()) {
							tileAvoidance += dist / (newDist * i + 1);
						}
					}
					if (tileAvoidance != 0) {
						direction = direction.RotatedBy(Math.Clamp(tileAvoidance * -0.5f * NPC.direction, -MathHelper.PiOver2, MathHelper.PiOver2));
					}
				}
				float oldRot = NPC.rotation;
				direction = Vector2.Lerp(direction, new Vector2(Math.Sign(direction.X), 0), Math.Abs(direction.Y) - 0.5f).SafeNormalize(direction);
				GeometryUtils.AngularSmoothing(ref NPC.rotation, direction.ToRotation(), 0.1f);
				float diff = GeometryUtils.AngleDif(oldRot, NPC.rotation, out int turnDir) * 0.75f;
				NPC.velocity = NPC.velocity.RotatedBy(diff * turnDir) * (1 - diff * 0.1f);
				NPC.velocity *= 0.94f;
				NPC.velocity += direction * 0.2f;
				if (!Collision.WetCollision(NPC.position + NPC.velocity, 20, 20)) {
					NPC.velocity += direction;
				}
				NPC.spriteDirection = Math.Sign(Math.Cos(NPC.rotation));
			} else {
				NPC.noGravity = false;
				NPC.aiStyle = NPCAIStyleID.Passive;
				NPC.rotation = 0;
				if (NPC.velocity.X != 0) NPC.spriteDirection = Math.Sign(NPC.velocity.X);
				NPC.width = (int)(46 * NPC.scale);
				NPC.height = (int)(28 * NPC.scale);
			}
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void FindFrame(int frameHeight) {
			if (NPC.GetWet(Liquids.Brine.ID)) NPC.DoFrames(6, 7..^1);
			else if (NPC.velocity.X != 0) NPC.DoFrames(6, 0..7);
			else NPC.frame.Y = 0;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(12 * NPC.direction, 11).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Leaf_Snapper_Gore3")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-8 * NPC.direction, 11).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Leaf_Snapper_Gore3")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-14 * NPC.direction, 5).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Leaf_Snapper_Gore2")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(1 * NPC.direction, 5).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Leaf_Snapper_Gore1")
				);
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (!NPC.GetWet(Liquids.Brine.ID)) return true;
			SpriteEffects spriteEffects = SpriteEffects.FlipHorizontally;
			if (NPC.spriteDirection != 1) {
				spriteEffects |= SpriteEffects.FlipVertically;
			}
			if (NPC.IsABestiaryIconDummy) NPC.rotation = NPC.velocity.ToRotation();
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Vector2 halfSize = new(texture.Width * 0.5f, (texture.Height / Main.npcFrameCount[NPC.type]) * 0.5f);
			Vector2 position = new(NPC.position.X - screenPos.X + (NPC.width / 2) - texture.Width * NPC.scale / 2f + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + NPC.height - texture.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4f + halfSize.Y * NPC.scale + NPC.gfxOffY);
			Vector2 origin = new(halfSize.X, halfSize.Y);
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				position - Vector2.UnitY * 4,
				NPC.frame,
				drawColor,
				NPC.rotation,
				origin,
				NPC.scale,
				spriteEffects,
			0);
			return false;
		}
	}
	public class Leaf_Snapper_Alt : Leaf_Snapper, IInteractableNPC {
		public bool NeedsSync => false;
		public override bool CanChat() {
			if (Main.mouseRight && Main.npcChatRelease && (Main.LocalPlayer.HeldItem?.pick ?? 0) > 0) {
				Interact();
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					ModPacket packet = Mod.GetPacket();
					packet.Write(Origins.NetMessageType.entity_interaction);
					packet.Write((byte)NPC.whoAmI);
					packet.Send(-1, Main.myPlayer);
				}
			}
			return false;
		}
		public void Interact() {
			NPC.Transform(ModContent.NPCType<Leaf_Snapper>());
			int item = Item.NewItem(NPC.GetSource_Loot("interaction"), NPC.Hitbox, ModContent.ItemType<Baryte_Item>(), Main.rand.Next(6, 11));
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
			}
		}
	}
}
