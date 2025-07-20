using Origins.Items.Armor.Defiled;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Defiled_Digger_Head : Defiled_Digger {
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				CustomTexturePath = "Origins/UI/Defiled_Digger_Preview", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(25f, 32f),
				PortraitPositionXOverride = 4,
				PortraitPositionYOverride = 16f
			};
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerHead);
			NPC.width = 22;
			NPC.height = 22;
			NPC.lifeMax = 80;
			NPC.defense = 8;
			NPC.damage = 38;
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0.5f, 0.75f);
			NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(0.5f, 0.75f);
			NPC.scale = 0.9f;
			NPC.value = 140;
			SpawnModBiomes = [
				ModContent.GetInstance<Underground_Defiled_Wastelands_Biome>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public override int MaxManaDrain => 18;
		public override float Mana { get; set; }
		public override bool ForceSyncMana => true;
		public override void UpdateLifeRegen(ref int damage) {
			if (NPC.life > 20) {
				NPC.lifeRegen += 24 / (NPC.life / 20);
			}
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.SpawnTileY <= Main.worldSurface || spawnInfo.PlayerSafe || spawnInfo.DesertCave) return 0;
			return Defiled_Wastelands.SpawnRates.FlyingEnemyRate(spawnInfo, true) * Defiled_Wastelands.SpawnRates.Worm;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Latchkey>(), 8, 2, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Greaves>(), 525));
		}
		public override void OnSpawn(IEntitySource source) {
			NPC.spriteDirection = Main.rand.NextBool() ? 1 : -1;
			NPC.ai[3] = NPC.whoAmI;
			NPC.realLife = NPC.whoAmI;
			int current = 0;
			int last = NPC.whoAmI;
			int type = ModContent.NPCType<Defiled_Digger_Body>();
			for (int k = 0; k < 17; k++) {
				current = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI);
				Main.npc[current].ai[3] = NPC.whoAmI;
				Main.npc[current].realLife = NPC.whoAmI;
				Main.npc[current].ai[1] = last;
				Main.npc[current].spriteDirection = Main.rand.NextBool() ? 1 : -1;
				Main.npc[last].ai[0] = current;
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, current);
				last = current;
			}
			current = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Defiled_Digger_Tail>(), NPC.whoAmI);
			Main.npc[current].ai[3] = NPC.whoAmI;
			Main.npc[current].realLife = NPC.whoAmI;
			Main.npc[current].ai[1] = last;
			Main.npc[current].spriteDirection = Main.rand.NextBool() ? 1 : -1;
			Main.npc[last].ai[0] = current;
			NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, current);
		}

		/*public override void Init() {
			base.Init();
			head = true;
		}

		private int attackCounter;
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(attackCounter);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			attackCounter = reader.ReadInt32();
		}

		public override void CustomBehavior() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				if (attackCounter > 0) {
					attackCounter--;
				}

				Player target = Main.player[npc.target];
				if (attackCounter <= 0 && Vector2.Distance(npc.Center, target.Center) < 200 && Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1)) {
					Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
					direction = direction.RotatedByRandom(MathHelper.ToRadians(10));

					int projectile = Projectile.NewProjectile(npc.Center, direction * 1, ProjectileID.ShadowBeamHostile, 5, 0, Main.myPlayer);
					Main.projectile[projectile].timeLeft = 300;
					attackCounter = 500;
					npc.netUpdate = true;
				}
			}
		}*/
	}

	internal class Defiled_Digger_Body : Defiled_Digger {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerBody);
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0.5f, 0.75f);
		}
		public override float Mana {
			get => CheckValidParent() ? (Main.npc[(int)NPC.ai[3]].ModNPC as IDefiledEnemy).Mana : float.NaN;
			set {
				if (CheckValidParent()) (Main.npc[(int)NPC.ai[3]].ModNPC as IDefiledEnemy).Mana = value;
			}
		}
	}

	internal class Defiled_Digger_Tail : Defiled_Digger {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerTail);
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0.5f, 0.75f);
		}
		public override float Mana {
			get => CheckValidParent() ? (Main.npc[(int)NPC.ai[3]].ModNPC as IDefiledEnemy).Mana : float.NaN;
			set {
				if (CheckValidParent()) (Main.npc[(int)NPC.ai[3]].ModNPC as IDefiledEnemy).Mana = value;
			}
		}
	}

	public abstract class Defiled_Digger : Glowing_Mod_NPC, IDefiledEnemy {
		public AssimilationAmount? Assimilation => this is Defiled_Digger_Head ? 0.03f : 0.02f;
		public int MaxMana => 40;
		public virtual int MaxManaDrain => 10;
		public abstract float Mana { get; set; }
		public virtual bool ForceSyncMana => false;
		public override void AI() {
			if (NPC.realLife != NPC.whoAmI && NPC.realLife != -1) {
				NPC head = Main.npc[NPC.realLife];
				NPC.life = head.active ? NPC.lifeMax : 0;
				NPC.immune = head.immune;
			}
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			if (NPC.realLife != NPC.whoAmI && NPC.realLife != -1) {
				if (projectile.usesLocalNPCImmunity) {
					projectile.localNPCImmunity[NPC.realLife] = projectile.localNPCHitCooldown;
					projectile.localNPCImmunity[NPC.whoAmI] = 0;
				} else {
					Main.npc[NPC.realLife].immune[projectile.owner] = NPC.immune[projectile.owner];
				}
			}
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override bool? CanBeHitByProjectile(Projectile projectile) {
			if (NPC.realLife == NPC.whoAmI || NPC.realLife == -1) return null;
			if ((projectile.usesLocalNPCImmunity ? projectile.localNPCImmunity[NPC.realLife] : Main.npc[NPC.realLife].immune[projectile.owner]) > 0) {
				return false;
			}
			return null;
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				NPC current = Main.npc.GetIfInRange(NPC.realLife, NPC);
				while (current.ai[0] != 0) {
					deathEffect(current);
					current = Main.npc[(int)current.ai[0]];
				}
			}
		}
		protected static void deathEffect(NPC npc) {
			Origins.instance.SpawnGoreByName(npc.GetSource_Death(), npc.position, npc.velocity, "Gores/NPCs/DF3_Gore");
			//Gore.NewGore(NPC.GetSource_Death(), npc.position, npc.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
			Origins.instance.SpawnGoreByName(npc.GetSource_Death(), npc.position, npc.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
			//for(int i = 0; i < 3; i++)
		}
		protected bool CheckValidParent() {
			if (Main.npc[(int)NPC.ai[3]].ModNPC is not IDefiledEnemy) {
				NPC.active = false;
				return false;
			}
			return true;
		}
	}
}