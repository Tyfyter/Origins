using AltLibrary.Common.AltBiomes;
using Origins.Buffs;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.Defiled {
	public class Defiled_Squid : Glowing_Mod_NPC, IDefiledEnemy, IWikiNPC, ICustomWikiStat {
		public Rectangle DrawRect => new(0, -8, 30, 48);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public int MaxMana => 32;
		public int MaxManaDrain => 8;
		public float Mana {
			get => NPC.localAI[3];
			set => NPC.localAI[3] = MathHelper.Clamp(value, 0, MaxMana);
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new(0, 16)
			};
			DefiledGlobalNPC.NPCTransformations.Add(NPCID.Squid, Type);
			BiomeNPCGlobals.assimilationDisplayOverrides.Add(Type, new() {
				[ModContent.GetInstance<Defiled_Assimilation>().AssimilationType] = Squid_Bile_P.assimilation_amount
			});
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.width = 26;
			NPC.height = 26;
			NPC.aiStyle = Terraria.ID.NPCAIStyleID.Jellyfish;
			NPC.damage = 75;
			NPC.lifeMax = 165;
			NPC.defense = 22;
			NPC.knockBackResist = 0;
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(2.1f, 2.35f);
			NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(2.1f, 2.35f);
			NPC.value = 100f;
			NPC.alpha = 20;
			NPC.rarity = 1;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type,
				ModContent.GetInstance<Defiled_Wastelands_Ocean>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!spawnInfo.Water) return 0;
			return Defiled_Wastelands.SpawnRates.FlyingEnemyRate(spawnInfo, true) * Defiled_Wastelands.SpawnRates.Sqid;
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
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.BlackInk));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Latchkey>(), 8, 2, 5));
			npcLoot.Add(ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Black_Bile>(), 1, 1, 3));
		}
		public override bool CanHitNPC(NPC target) {
			if (DefiledGlobalNPC.NPCTransformations.ContainsKey(target.type)) return false;
			return true;
		}
		public override bool PreAI() {
			NPC.friendly = true;
			if (NPC.direction == 0) {
				NPC.TargetClosest(true);
			}
			if (NPC.wet) {
				Mana += 0.01f;
				NPCUtils.TargetSearchResults results = NPCUtils.SearchForTarget(NPC, playerFilter: player => player.wet, npcFilter: npc => npc.wet && DefiledGlobalNPC.NPCTransformations.ContainsKey(npc.type));
				if (!results.FoundTarget) return true;
				float distanceSQ = NPC.DistanceSQ(NPC.Center.Clamp(results.NearestTargetHitbox));
				const float big_range = 18 * 16;
				if (distanceSQ <= big_range * big_range) {
					NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
					bool tooSlow = NPC.velocity.LengthSquared() < 1;
					const float range = 10 * 16;
					if (distanceSQ <= range * range) {
						if (results.NearestTargetType == NPCUtils.TargetType.NPC) {
							NPC.velocity *= 0.98f;
							const float small_range = 2 * 16;
							if (distanceSQ <= small_range * small_range) {
								if (Mana >= 4f) {
									NPC.DoFrames(1);
									NPC.velocity = (NPC.Center - results.NearestNPC.Center).SafeNormalize(NPC.velocity / 8) * 8;
									Mana -= 4f;
									Projectile.NewProjectile(
										NPC.GetSource_FromAI(),
										NPC.Center,
										NPC.velocity * -0.5f,
										ModContent.ProjectileType<Squid_Bile_P>(),
										(int)(12 * ContentExtensions.DifficultyDamageMultiplier),
										3
									);

								}
							} else if (tooSlow) {
								NPC.velocity = (results.NearestNPC.Center - NPC.Center).SafeNormalize(NPC.velocity / 8) * 8;
							}
						} else {
							NPC.velocity *= 0.98f;
							const float small_range = 2 * 16;
							if (tooSlow || distanceSQ <= small_range * small_range) {
								NPC.DoFrames(1);
								NPC.velocity = (NPC.Center - results.NearestTargetHitbox.Center.ToVector2()).SafeNormalize(NPC.velocity / 8) * 8;
								if (Mana >= 4f) {
									Mana -= 4f;
									if (Main.netMode != NetmodeID.MultiplayerClient) {
										Projectile.NewProjectile(
											NPC.GetSource_FromAI(),
											NPC.Center,
											NPC.velocity * -0.5f,
											ModContent.ProjectileType<Squid_Bile_P>(),
											(int)(12 * ContentExtensions.DifficultyDamageMultiplier),
											3
										);
									}
								}
							}
						}
						NPC.DoFrames(7);
						return false;
					} else {
						NPC.DoFrames(13);
					}
				}
				NPC.target = -1;
				NPC.DoFrames(13);
			}
			return true;
		}
		public override void PostAI() {
			NPC.friendly = false;
		}
	}
	public class Squid_Bile_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.VilePowder;
		public const float assimilation_amount = 0.06f;
		public AssimilationAmount Assimilation = assimilation_amount;
		public override void SetStaticDefaults() {
			this.AddAssimilation<Defiled_Assimilation>(Assimilation);
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.VilePowder);
			Projectile.friendly = true;
			Projectile.hostile = true;
			Projectile.aiStyle = 0;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.Darkness, 600);
			target.AddBuff(BuffID.Weak, 240);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (DefiledGlobalNPC.NPCTransformations.TryGetValue(target.type, out int targetType)) {
				target.Transform(targetType);
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			//modifiers.FinalDamage *= 0;
			//modifiers.DisableCrit();
			//modifiers.HideCombatText();
			//target.life += 1;
			modifiers.SetInstantKill();
			target.GetGlobalNPC<OriginGlobalNPC>().transformingThroughDeath = true;
		}
		public override bool? CanHitNPC(NPC target) {
			return DefiledGlobalNPC.NPCTransformations.ContainsKey(target.type);
		}
		public override void AI() {
			Projectile.velocity *= 0.95f;
			Projectile.ai[0] += 1f;
			if (Projectile.ai[0] == 90f) {
				Projectile.Kill();
			}
			if (Projectile.ai[1] == 0f) {
				Projectile.ai[1] = 1f;
				for (int i = 0; i < 60; i++) {
					Dust.NewDust(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						ModContent.DustType<Generic_Powder_Dust>(),
						Projectile.velocity.X,
						Projectile.velocity.Y,
						50,
						new Color(0.3f, 0.28f, 0.35f, 0.9f)
					);
				}
			}
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			int minX = (int)(Projectile.position.X / 16f) - 1;
			int maxX = (int)((Projectile.position.X + Projectile.width) / 16f) + 2;
			int minY = (int)(Projectile.position.Y / 16f) - 1;
			int maxY = (int)((Projectile.position.Y + Projectile.height) / 16f) + 2;
			if (minX < 0) {
				minX = 0;
			}
			if (maxX > Main.maxTilesX) {
				maxX = Main.maxTilesX;
			}
			if (minY < 0) {
				minY = 0;
			}
			if (maxY > Main.maxTilesY) {
				maxY = Main.maxTilesY;
			}
			Vector2 comparePos = default;
			AltBiome biome = ModContent.GetInstance<Defiled_Wastelands_Alt_Biome>();
			for (int x = minX; x < maxX; x++) {
				for (int y = minY; y < maxY; y++) {
					comparePos.X = x * 16;
					comparePos.Y = y * 16;
					if ((Projectile.position.X + Projectile.width > comparePos.X) &&
						(Projectile.position.X < comparePos.X + 16f) &&
						(Projectile.position.Y + Projectile.height > comparePos.Y) &&
						(Projectile.position.Y < comparePos.Y + 16f)
						&& Main.tile[x, y].HasTile) {
						AltLibrary.Core.ALConvert.ConvertTile(x, y, biome);
						AltLibrary.Core.ALConvert.ConvertWall(x, y, biome);
						//WorldGen.Convert(x, y, OriginSystem.origin_conversion_type, 1);
					}
				}
			}
		}
	}
}
