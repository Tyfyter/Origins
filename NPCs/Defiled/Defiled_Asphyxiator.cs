using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Defiled_Asphyxiator : Glowing_Mod_NPC, IDefiledEnemy, IWikiNPC, ICustomWikiStat {
		public Rectangle DrawRect => new(0, 0, 92, 58);
		public int AnimationFrames => 36;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.04f;
		public const float speedMult = 0.75f;
		//public float SpeedMult => npc.frame.Y==510?1.6f:0.8f;
		//bool attacking = false;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 9;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 475;
			NPC.defense = 28;
			NPC.damage = 49;
			NPC.width = 92;
			NPC.height = 56;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0.5f, 0.75f);
			NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(0.5f, 0.75f);
			NPC.value = 2300;
			NPC.knockBackResist = 0.5f;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Black_Bile>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Latchkey>(), 8, 2, 5));
		}
		public int MaxMana => 100;
		public int MaxManaDrain => 100;
		public float Mana { get; set; }
		public void Regenerate(out int lifeRegen) {
			int factor = 37 / ((NPC.life / 40) + 2);
			lifeRegen = factor;
			Mana -= factor / 90f;// 3 mana for every 2 health regenerated
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if ((spawnInfo.SpawnTileX > WorldGen.oceanDistance && spawnInfo.SpawnTileX < Main.maxTilesX - WorldGen.oceanDistance) || !spawnInfo.Water || spawnInfo.DesertCave) return 0;
			return Defiled_Wastelands.SpawnRates.FlyingEnemyRate(spawnInfo, true) * Defiled_Wastelands.SpawnRates.Asphyxiator;
		}
		public override int SpawnNPC(int tileX, int tileY) {
			tileY = OriginGlobalNPC.GetAerialSpawnPosition(tileX, tileY, this);
			if (tileY == -1) return Main.maxNPCs;
			return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public int Frame {
			get => NPC.frame.Y / 58;
			set => NPC.frame.Y = value * 58;
		}
		public override void AI() {
			NPC.TargetClosestUpgraded();
			if (NPC.HasValidTarget && NPC.HasPlayerTarget) {
				Player _target = Main.player[NPC.target];
				int level = Defiled_Asphyxiator_Debuff.GetLevel(_target);
				NPCAimedTarget target = NPC.GetTargetData();
				int projectileType = ProjectileID.None;
				float speed = 12f;
				float inertia = 128f;
				NPC.rotation = NPC.velocity.X * 0.1f;
				Vector2 vectorToTargetPosition = target.Center - NPC.Center;
				if (NPC.confused) vectorToTargetPosition *= -1;
				NPC.spriteDirection = Math.Sign(vectorToTargetPosition.X);
				float dist = vectorToTargetPosition.Length();
				vectorToTargetPosition /= dist;
				NPC.aiAction = 0;
				switch (level) {
					case 3: {
						NPC.aiAction = 1;
					}
					break;
					case 0:
					projectileType = ModContent.ProjectileType<Defiled_Asphyxiator_P2>();
					goto default;
					case 1:
					projectileType = ModContent.ProjectileType<Defiled_Asphyxiator_P3>();
					goto default;
					case 2:
					projectileType = ModContent.ProjectileType<Defiled_Asphyxiator_P1>();
					goto default;
					default: {
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

						if (++NPC.ai[0] > 90 - 9 * 3) {
							if (NPC.ai[0] > 90) {
								NPC.ai[0] = 0;
								if (Main.netMode != NetmodeID.MultiplayerClient) {
									Vector2 projPos;
									int tries = 0;
									for (int i = Main.rand.Next(2, 4); i-- >0;) {
										do {
											projPos = target.Center + Main.rand.NextVector2CircularEdge(17 * 16, 17 * 16);
										} while (!Collision.CanHitLine(projPos - new Vector2(18), 36, 36, target.Hitbox.TopLeft(), target.Hitbox.Width, target.Hitbox.Height) && ++tries < 100);
										Projectile.NewProjectile(
											NPC.GetSource_FromAI(),
											projPos,
											default,
											projectileType,
											(int)(20 * ContentExtensions.DifficultyDamageMultiplier),
											4,
											ai1: target.Center.X,
											ai2: target.Center.Y
										);
									}
								}
							}
						}
					}
					break;
				}
				vectorToTargetPosition *= speed;
				NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
				Vector2 nextVel = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, true, true);
				if (nextVel.X != NPC.velocity.X) NPC.velocity.X *= -0.9f;
				if (nextVel.Y != NPC.velocity.Y) NPC.velocity.Y *= -0.9f;
			}
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 9) {
				NPC.frameCounter = 0;
				Frame++;
			}
			switch (NPC.aiAction) {
				case 0:
				if (Frame < 0 || Frame >= 4) {
					Frame = 0;
					NPC.frameCounter = 0;
				}
				break;
				case 1:
				if (Frame < 4 || Frame >= 9) {
					Frame = 4;
					NPC.frameCounter = 0;
				}
				break;
			}
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(yMult: -0.5f), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 10; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
	}
	public class Defiled_Asphyxiator_P1 : ModProjectile {
		const int delay = 25;
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 36;
			Projectile.timeLeft = 60;
			Projectile.hostile = false;
			Projectile.hide = true;
		}
		public override bool ShouldUpdatePosition() => Projectile.ai[0] > delay;
		public override void AI() {
			if (++Projectile.ai[0] > delay) {
				Projectile.hide = false;
				Projectile.hostile = true;
			} else {
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch);
				if (Projectile.ai[0] == delay - 5) Projectile.velocity = (new Vector2(Projectile.ai[1], Projectile.ai[2]) - Projectile.Center).SafeNormalize(default) * 10;
				Projectile.hostile = false;
				Projectile.hide = true;
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Defiled_Asphyxiator_Debuff.AddBuff(target);
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 5; i++)
				Origins.instance.SpawnGoreByName(Projectile.GetSource_Death(), Projectile.position + new Vector2(Main.rand.Next(Projectile.width), Main.rand.Next(Projectile.height)), Projectile.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			return true;
		}
	}
	public class Defiled_Asphyxiator_P2 : Defiled_Asphyxiator_P1 { }
	public class Defiled_Asphyxiator_P3 : Defiled_Asphyxiator_P1 { }
	public class Defiled_Asphyxiator_Debuff : ModBuff {
		public override string Texture => typeof(Defiled_Asphyxiator_P2).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() => ID = Type;
		public override void Update(Player player, ref int buffIndex) {
			int level = GetLevel(player, buffIndex);
			switch (level) { // temporary implementation for balance testing
				case 1:
				player.dazed = true;
				break;
				case 2:
				player.sticky = true;
				break;
				case 3:
				OriginPlayer originPlayer = player.OriginPlayer();
				originPlayer.rasterizedTime = 8;
				originPlayer.forceDrown = true;
				if (player.breath > 1) player.breath = 1;
				break;
			}
		}
		public static int GetLevel(Player player, int searchStart = 0) => GetLevel(player, out _, searchStart);
		public static int GetLevel(Player player, out int index, int searchStart = 0) {
			int level1 = ID;
			int level2 = Defiled_Asphyxiator_Debuff_2.ID;
			int level3 = Defiled_Asphyxiator_Debuff_3.ID;
			for (int i = searchStart; i < player.buffType.Length; i++) {
				if (player.buffTime[i] == 0) break;
				int type = player.buffType[i];
				if (type == level1) {
					index = i;
					return 1;
				}
				if (type == level2) {
					index = i;
					return 2;
				}
				if (type == level3) {
					index = i;
					return 3;
				}
			}
			index = -1;
			return 0;
		}
		public static void AddBuff(Player player) {
			const int level_1_time = 120;
			const int level_2_time = 120 + level_1_time;// pointlessly adding and subtracting to show that level 2 will decay into level 1
			const int level_3_time = 150;
			int level = GetLevel(player, out int index);
			if (index != -1) {
				if (level == 1) {
					player.buffType[index] = Defiled_Asphyxiator_Debuff_2.ID;
					player.buffTime[index] = level_3_time;
				} else {
					player.buffType[index] = Defiled_Asphyxiator_Debuff_3.ID;
					player.buffTime[index] = level_2_time - level_1_time;
				}
			} else {
				player.AddBuff(ID, level_1_time);
			}
		}
	}
	public class Defiled_Asphyxiator_Debuff_2 : Defiled_Asphyxiator_Debuff {
		public override string Texture => typeof(Defiled_Asphyxiator_P3).GetDefaultTMLName();
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() => ID = Type;
		public override void Update(Player player, ref int buffIndex) {
			if (player.buffTime[buffIndex] == 1) {
				player.buffType[buffIndex] = Defiled_Asphyxiator_Debuff.ID;
				player.buffTime[buffIndex] = 120;
			}
			base.Update(player, ref buffIndex);
		}
	}
	public class Defiled_Asphyxiator_Debuff_3 : Defiled_Asphyxiator_Debuff {
		public override string Texture => typeof(Defiled_Asphyxiator_P1).GetDefaultTMLName();
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() => ID = Type;
	}
}
