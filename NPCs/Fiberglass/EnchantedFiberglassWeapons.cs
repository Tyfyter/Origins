using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.LootConditions;
using Origins.Tiles.Other;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Origins.OriginExtensions;

namespace Origins.NPCs.Fiberglass {
	public class Enchanted_Fiberglass_Bow : ModNPC, IWikiNPC {
		Color[] oldColor = new Color[10];
		int[] oldDir = new int[10];
		public Rectangle DrawRect => new(0, 0, 18, 36);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			NPCID.Sets.TrailingMode[NPC.type] = 3;
			NPCID.Sets.NoMultiplayerSmoothingByType[NPC.type] = true;
		}
		public override void SetDefaults() {
			NPC.noGravity = true;
			NPC.damage = 10;
			NPC.life = NPC.lifeMax = 95;
			NPC.defense = 10;
			NPC.aiStyle = Terraria.ID.NPCAIStyleID.FaceClosestPlayer;//104;//10,
			NPC.width = NPC.height = 27;
			NPC.alpha = NPC.IsABestiaryIconDummy ? 0 : 200;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0.4f;
			NPC.value = 500f;
			SpawnModBiomes = [
				ModContent.GetInstance<Fiberglass_Undergrowth>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void AI() {
			NPC.velocity *= 0.85f;
			NPC.TargetClosest();
			NPC.spriteDirection = NPC.direction;
			NPC.rotation = (NPC.Center - Main.player[NPC.target].Center).ToRotation();
			Vector2 speed = new Vector2(-12, 0).RotatedBy(Main.rand.NextFloat(NPC.rotation - 0.05f, NPC.rotation + 0.05f));
			Vector2 pos = NPC.Center + speed;
			if (Collision.CanHit(pos, 1, 1, Main.player[NPC.target].Center, 1, 1) && Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.ai[0] += 1f;
				if (NPC.ai[0] >= 120f) {
					Projectile.NewProjectile(NPC.GetSource_FromAI(), pos.X, pos.Y, speed.X, speed.Y, ProjectileID.WoodenArrowHostile, 16, 0f);
					NPC.ai[0] = 0f;
					teleport();
				}
			} else NPC.ai[0] = 0f;
			if (NPC.spriteDirection == 1) NPC.rotation += MathHelper.Pi;
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			if (damageDone <= 1) return;
			NPC.ai[0] = -15f;
			teleport();
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			if (damageDone <= 1) return;
			NPC.ai[0] = 0f;
			teleport();
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Broken_Fiberglass_Bow>(), 10));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Shaped_Glass>(), 25));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Item>(), 10, 3, 8));
			npcLoot.Add(ItemDropRule.Common(ItemID.Vine, 7));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Shard>(), 1, 1, 7));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			NPC.velocity.X += hit.HitDirection * 3;
			if (hit.Damage > NPC.life * 2f) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPCs/FG{Main.rand.Next(3) + 1}_Gore");
			}
			if (NPC.life <= 0) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG1_Gore");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG2_Gore");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG3_Gore");
			} else if (hit.Damage > NPC.lifeMax * 0.5f) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPCs/FG{Main.rand.Next(3) + 1}_Gore");
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			for (int i = NPC.oldPos.Length - 1; i > 0; i--) {
				spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.oldPos[i] + new Vector2(13.5f, 13.5f) - Main.screenPosition, new Rectangle(0, 0, 18, 36), oldColor[i].MultiplyRGBA(new Color(new Vector4(1 - i / 10f))), NPC.oldRot[i], new Vector2(9, 18), 1f, oldDir[i] != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 1f);
				oldDir[i] = oldDir[i - 1];
				oldColor[i] = oldColor[i - 1];
			}
			oldDir[0] = NPC.spriteDirection;
			oldColor[0] = drawColor;
		}
		void teleport() {
			float rot = NPC.rotation + MathHelper.Pi / 2f;
			WeightedRandom<Vector2> options = new();
			for (int i = 0; i < 16; i++) {
				Vector2 unit = new Vector2(4, 0).RotatedBy(rot + Main.rand.NextFloat(-0.05f, 0.05f));
				Vector2 pos = GetLoSLength(Main.player[NPC.target].Center, new Point(8, 8), unit, new Point(8, 8), 75, out int length);
				pos -= unit * (length < 75 ? 4 : 1);
				if (length >= 32) {
					options.Add(pos, length * Main.rand.NextFloat(0.9f, 1.1f));
					//Main.npc[NPC.NewNPC((int)pos.X,(int)pos.Y,NPCID.WaterSphere)].velocity = Vector2.Zero; //accidental miniboss if no velocity change
				}
				rot += MathHelper.Pi / 16f;
			}
			if (options.elements.Count == 0) for (int i = 0; i < 16; i++) {
					Vector2 unit = new Vector2(4, 0).RotatedBy(rot + Main.rand.NextFloat(-0.05f, 0.05f));
					Vector2 pos = GetLoSLength(Main.player[NPC.target].Center, new Point(8, 8), unit, new Point(8, 8), 75, out int length);
					pos -= unit * (length < 75 ? 4 : 1);
					if (length >= 24) {
						options.Add(pos, length * Main.rand.NextFloat(0.9f, 1.1f));
						//Main.npc[NPC.NewNPC((int)pos.X,(int)pos.Y,NPCID.WaterSphere)].velocity = Vector2.Zero;
					}
					rot += MathHelper.Pi / 16f;
				}
			if (options.elements.Count == 0) {
				//npc.active = false;
				return;
			}
			NPC.Center = options.Get();
		}
		public override int SpawnNPC(int tileX, int tileY) {
			tileY = OriginGlobalNPC.GetAerialSpawnPosition(tileX, tileY, this);
			if (tileY == -1) return Main.maxNPCs;
			return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
		}
	}
	public class Enchanted_Fiberglass_Pistol : ModNPC, IWikiNPC {
		Color[] oldColor = new Color[10];
		int[] oldDir = new int[10];
		public Rectangle DrawRect => new(0, 0, 38, 22);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			NPCID.Sets.TrailingMode[NPC.type] = 3;
			NPCID.Sets.NoMultiplayerSmoothingByType[NPC.type] = true;
		}
		public override void SetDefaults() {
			NPC.aiStyle = Terraria.ID.NPCAIStyleID.FaceClosestPlayer;
			NPC.damage = 10;
			NPC.life = NPC.lifeMax = 57;
			NPC.defense = 10;
			NPC.noGravity = true;
			NPC.width = NPC.height = 27;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0.4f;
			NPC.value = 500f;
			SpawnModBiomes = [
				ModContent.GetInstance<Fiberglass_Undergrowth>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void AI() {
			NPC.velocity *= 0.85f;
			NPC.TargetClosest();
			NPC.spriteDirection = NPC.direction;
			NPC.rotation = (NPC.Center - Main.player[NPC.target].Center).ToRotation();
			if (Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1) && Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.localAI[0] += 1f;
				if (NPC.rotation == NPC.oldRot[0]) NPC.localAI[0] += 2f;
				if (NPC.localAI[0] >= 180f) {
					Vector2 speed = new Vector2(-8, 0).RotatedBy(Main.rand.NextFloat(NPC.rotation - 0.1f, NPC.rotation + 0.1f));
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y, speed.X, speed.Y, ProjectileID.BulletDeadeye, 18, 0f);
					NPC.life = 0;
					NPC.checkDead();
				}
			} else NPC.localAI[0] = 0f;
			if (NPC.spriteDirection == 1) NPC.rotation += MathHelper.Pi;
		}
		public override void OnKill() {
			if (NPC.localAI[0] < 180f) {
				Item.NewItem(NPC.GetSource_Death(), NPC.Center, ModContent.ItemType<Fiberglass_Shard>(), Main.rand.Next(4) + 4);
			} else {
				Vector2 speed = new Vector2(8, 0).RotatedBy(NPC.rotation);
				for (int i = Main.rand.Next(4, 7); i >= 0; i--) {
					speed = speed.RotatedByRandom(0.25f);
					int proj =
					Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, speed, ModContent.ProjectileType<Fiberglass_Shard_P>(), (int)(9 * ContentExtensions.DifficultyDamageMultiplier), 3f, NPC.target);
					Main.projectile[proj].hostile = true;
					Main.projectile[proj].friendly = false;
					Main.projectile[proj].hide = false;
				}
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.ByCondition(new AnyPlayerInteraction(), ModContent.ItemType<Shaped_Glass>(), 25));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Item>(), 10, 3, 8));
			npcLoot.Add(ItemDropRule.Common(ItemID.Vine, 7));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Shard>(), 1, 1, 7));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			NPC.velocity.X += hit.HitDirection * 3;
			if (hit.Damage > NPC.life * 2f) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPCs/FG{Main.rand.Next(3) + 1}_Gore");
			}
			if (NPC.life <= 0) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG1_Gore");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG2_Gore");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG3_Gore");
			} else if (hit.Damage > NPC.lifeMax * 0.5f) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPCs/FG{Main.rand.Next(3) + 1}_Gore");
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			for (int i = NPC.oldPos.Length - 1; i > 0; i--) {
				spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.oldPos[i] + new Vector2(13.5f, 19) - Main.screenPosition, new Rectangle(0, 0, 38, 22), oldColor[i].MultiplyRGBA(new Color(new Vector4(1 - i / 10f))), NPC.oldRot[i], new Vector2(19, 11), 1f, oldDir[i] != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 1f);
				oldDir[i] = oldDir[i - 1];
				oldColor[i] = oldColor[i - 1];
			}
			oldDir[0] = NPC.spriteDirection;
			oldColor[0] = drawColor;
		}
		public override int SpawnNPC(int tileX, int tileY) {
			tileY = OriginGlobalNPC.GetAerialSpawnPosition(tileX, tileY, this);
			if (tileY == -1) return Main.maxNPCs;
			return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
		}
	}
	public class Enchanted_Fiberglass_Sword : ModNPC, IWikiNPC {
		Color[] oldColor = new Color[10];
		int[] oldDir = new int[10];
		int stuck = 0;
		Vector2 stuckVel = Vector2.Zero;
		public Rectangle DrawRect => new(0, 0, 44, 48);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			NPCID.Sets.TrailingMode[NPC.type] = 3;
		}
		public override void SetDefaults() {
			NPC.damage = 10;
			NPC.life = NPC.lifeMax = 110;
			NPC.aiStyle = Terraria.ID.NPCAIStyleID.HoveringFighter;
			NPC.noGravity = true;
			NPC.width = NPC.height = 42;
			NPC.HitSound = SoundID.DD2_CrystalCartImpact;
			//npc.DeathSound = SoundID.DD2_DefeatScene;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0.1f;
			NPC.value = 500f;
			SpawnModBiomes = [
				ModContent.GetInstance<Fiberglass_Undergrowth>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override bool PreAI() {
			if (stuck > 0) {
				stuck--;
				if (stuck <= 0) {
					NPC.noTileCollide = false;
					NPC.velocity = -stuckVel / 3;
					NPC.position += NPC.velocity;
				}
				return false;
			}
			return true;
		}
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
			if (stuck > 0) return false;
			return base.CanHitPlayer(target, ref cooldownSlot);
		}
		public override void AI() {
			if (NPC.localAI[0] < 30 && stuck <= 0) {
				NPC.TargetClosest();
			}
			NPC.damage = stuck > 0 ? 0 : NPC.localAI[0] > 30 ? 50 : 10;
			NPC.defense = stuck > 0 ? 0 : NPC.localAI[0] > 30 ? 10 : 20;
			NPC.spriteDirection = NPC.direction;
			float targetRot = NPC.rotation;
			float rotSpeed = 0.15f;
			Player target = NPC.HasValidTarget ? Main.player[NPC.target] : null;
			if (NPC.HasValidTarget && (NPC.Center - target.Center).Length() < 80 + 42 && Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.localAI[0] += 1f;
				if (NPC.localAI[0] < 30) {
					bool close = (NPC.Center - Main.player[NPC.target].Center).Length() < 16 + 42;
					targetRot = -1.5f * NPC.direction;
					if (NPC.direction == -1) {
						NPC.velocity.X -= 0.2f;
						if (NPC.velocity.X < (close ? target.velocity.X - 0.2f : -1)) {
							NPC.velocity.X = (close ? target.velocity.X - 0.2f : -1);
						}
						if (NPC.velocity.X > 1) {
							NPC.velocity.X = 1;
						}
					} else if (NPC.direction == 1) {
						NPC.velocity.X += 0.2f;
						if (NPC.velocity.X > 1) {
							NPC.velocity.X = 1;
						}
						if (NPC.velocity.X < (close ? target.velocity.X + 0.2f : -1)) {
							NPC.velocity.X = (close ? target.velocity.X + 0.2f : -1);
						}
					}
				} else if (NPC.localAI[0] < 60) {
					bool close = NPC.direction != oldDir[0];
					rotSpeed = 0.3f;
					if (!close) {
						targetRot = 3 * NPC.direction;
						if (NPC.direction == -1) {
							NPC.velocity.X -= 0.4f;
						} else if (NPC.direction == 1) {
							NPC.velocity.X += 0.4f;
						}
					} else {
						NPC.direction = oldDir[0];
						NPC.localAI[0] = 90;
						targetRot -= rotSpeed * NPC.direction;
						NPC.velocity = NPC.oldVelocity;
						NPC.aiStyle = Terraria.ID.NPCAIStyleID.FaceClosestPlayer;
						if (NPC.collideX) {
							getStuck();
						}
					}
				} else {
					rotSpeed = 0.3f;
					NPC.localAI[0] = 90;
					targetRot -= rotSpeed * NPC.direction;
					NPC.velocity = NPC.oldVelocity;
					NPC.aiStyle = Terraria.ID.NPCAIStyleID.FaceClosestPlayer;
					if (NPC.collideX) {
						getStuck();
					}
				}
			} else {
				if (NPC.localAI[0] < 30) {
					NPC.localAI[0] = 0f;
					targetRot = 0f;
					NPC.aiStyle = Terraria.ID.NPCAIStyleID.HoveringFighter;
				} else {
					rotSpeed = 0.3f;
					targetRot -= rotSpeed * NPC.direction;
					NPC.localAI[0]--;
					NPC.aiStyle = Terraria.ID.NPCAIStyleID.FaceClosestPlayer;
					NPC.velocity = NPC.oldVelocity;
					NPC.target = -1;
					if (NPC.collideX) {
						getStuck();
					}
					if (NPC.localAI[0] < 30) NPC.rotation %= MathHelper.Pi;
				}
			}
			NPC.rotation += MathHelper.Pi / 2f;
			targetRot += MathHelper.Pi / 2f;
			if (NPC.rotation > targetRot) {
				NPC.rotation -= rotSpeed;
				if (NPC.rotation < targetRot) NPC.rotation = targetRot;
			} else if (NPC.rotation < targetRot) {
				NPC.rotation += rotSpeed;
				if (NPC.rotation > targetRot) NPC.rotation = targetRot;
			}
			targetRot -= MathHelper.Pi / 2f;
			NPC.rotation -= MathHelper.Pi / 2f;
		}
		void getStuck() {
			stuck = Main.rand.Next(80, 100);
			NPC.position += NPC.velocity;
			stuckVel = NPC.velocity;
			NPC.velocity *= 0;
			NPC.localAI[0] = 0;
			NPC.rotation %= MathHelper.Pi;
			NPC.noTileCollide = true;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Broken_Fiberglass_Sword>(), 10));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Shaped_Glass>(), 25));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Item>(), 10, 3, 8));
			npcLoot.Add(ItemDropRule.Common(ItemID.Vine, 7));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Shard>(), 1, 1, 7));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			NPC.velocity.X += hit.HitDirection * 3;
			if (hit.Damage > NPC.life * 2f) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPCs/FG{Main.rand.Next(3) + 1}_Gore");
			}
			if (NPC.life <= 0) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG1_Gore");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG2_Gore");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG3_Gore");
			} else if (hit.Damage > NPC.lifeMax * 0.5f) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPCs/FG{Main.rand.Next(3) + 1}_Gore");
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			for (int i = NPC.oldPos.Length - 1; i > 0; i--) {
				spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.oldPos[i] + new Vector2(21, 21) - Main.screenPosition, new Rectangle(0, 0, 42, 42), oldColor[i].MultiplyRGBA(new Color(new Vector4(1 - i / 10f))), NPC.oldRot[i], new Vector2(21, 21), 1f, oldDir[i] != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 1f);
				oldDir[i] = oldDir[i - 1];
				oldColor[i] = oldColor[i - 1];
			}
			oldDir[0] = NPC.spriteDirection;
			oldColor[0] = drawColor;
		}
	}
}
