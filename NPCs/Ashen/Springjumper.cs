using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Dev;
using Origins.Items.Armor.Ashen;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.LootConditions;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.Ashen {
	public class Springjumper : Glowing_Mod_NPC, IAshenEnemy {
		//public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 6;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.lifeMax = 80;
			NPC.defense = 22;
			NPC.damage = 24;
			NPC.width = 50;
			NPC.height = 30;
			NPC.value = Item.buyPrice(0, 0, 2);
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.knockBackResist = 0.5f;
			SpawnModBiomes = [
				GetInstance<Ashen_Biome>().Type,
			];
		}
		public override void AI() {
			void SetFrame(int frame) {
				NPC.ai[0] = frame;
				NPC.ai[1] = 0;
			}
			NPC.TargetClosest();
			Rectangle hitbox = NPC.Hitbox;
			hitbox.Inflate(-hitbox.Width / 10, 0);
			hitbox.Y += hitbox.Height - 8;
			hitbox.Height = 8;
			switch ((int)NPC.ai[0]) {
				default: {
					if (!NPC.collideY && NPC.velocity.Y > 0) break;
					hitbox.Y += 8;
					if (hitbox.OverlapsAnyTiles(false)) {
						Min(ref NPC.velocity.Y, 0);
						NPC.velocity.Y -= 8;
						if (Math.Sign(NPC.velocity.X) == -NPC.direction) {
							NPC.velocity.X += NPC.direction * 4;
						} else {
							NPC.velocity.X = NPC.direction * 4;
						}
						SetFrame(1);
					}
					break;
				}

				case 1: {
					NPC.ai[1]++;
					int i = 0;
					for (; i < 4; i++) {
						hitbox.Y += 8;
						if (hitbox.OverlapsAnyTiles(false)) {
							if (i == 0 && NPC.ai[1] > 5) SetFrame(0);
							break;
						}
					}
					if (i == 4) SetFrame(2);
					break;
				}

				case 2: {
					NPC.ai[1]++;
					int i = 0;
					for (; i < 4; i++) {
						hitbox.Y += 8;
						if (hitbox.OverlapsAnyTiles(false)) break;
					}
					if (i != 4 || NPC.ai[1] > 6) SetFrame(3);
					break;
				}

				case 3: {
					NPC.ai[1]++;
					int i = 0;
					for (; i < 2; i++) {
						hitbox.Y += 8;
						if (hitbox.OverlapsAnyTiles(false)) break;
					}
					if (i != 2 || NPC.ai[1] > 6) SetFrame(4);
					break;
				}

				case 4: {
					NPC.ai[1]++;
					hitbox.Y += 8;
					if (NPC.ai[1] > 6 && hitbox.OverlapsAnyTiles(false)) SetFrame(0);
					break;
				}
			}
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void FindFrame(int frameHeight) {
			NPC.frame.Y = 70 * (int)NPC.ai[0];
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ScavengerBonus.Scrap(1, 1, 2, 5));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore1");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore2");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore3");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore4");
				for (int i = 0; i < 7; i++) {
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
				}
				if (!NetmodeActive.MultiplayerClient) {
					NPC bunny = NPC.NewNPCDirect(
						NPC.GetSource_Death(),
						NPC.Center,
						ModContent.NPCType<Scared_Bunny>()
					);
					bunny.velocity = NPC.velocity;
					bunny.direction = hit.HitDirection;
					bunny.netUpdate = true;
				}
			} else if (Main.rand.NextBool(5)) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				NPC.position - screenPos,
				NPC.frame,
				drawColor,
				0,
				default,
				NPC.scale,
				NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
			0);
			return false;
		}
	}
	public class Scared_Bunny : ModNPC {
		public static float SpeedMultiplier => 2f;
		public override string Texture => "Terraria/Images/NPC_" + NPCID.Bunny;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Bunny];
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Bunny);
			AnimationType = NPCID.Bunny;
		}
		public override bool PreAI() {
			NPC.velocity.X /= SpeedMultiplier;
			return base.PreAI();
		}
		bool init;
		public override void AI() {
			if (init.TrySet(true)) NPC.spriteDirection = NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.ai[0] = 1f;
			NPC.ai[1] = 300;
			NPC.ai[2] = 0f;
		}
		public override void PostAI() {
			NPC.velocity.X *= SpeedMultiplier;
		}
		public override void AddShops() {
			ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[Type] = ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[NPCID.Bunny];
		}
	}
}
