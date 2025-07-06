using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Armor.Defiled;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Demolitionist;
using Origins.Journal;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Defiled_Brute : Glowing_Mod_NPC, IDefiledEnemy, IWikiNPC, IJournalEntrySource {
		public Rectangle DrawRect => new(0, 4, 74, 62);
		public int AnimationFrames => 16;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.04f;
		public float SpeedMult => NPC.aiAction == 0 ? 0.85f : (NPC.frame.Y / NPC.frame.Height == 10 ? 2f : 0.15f);
		//public float SpeedMult => npc.frame.Y==510?1.6f:0.8f;
		//bool attacking = false;
		public string EntryName => "Origins/" + typeof(Defiled_Krusher_Entry).Name;
		public class Defiled_Krusher_Entry : JournalEntry {
			public override string TextKey => "Defiled_Krusher";
			public override JournalSortIndex SortIndex => new("The_Defiled", 3);
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 14;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 160;
			NPC.defense = 9;
			NPC.damage = 39;
			NPC.width = 38;
			NPC.height = 54;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0.5f, 0.75f);
			NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(0.5f, 0.75f);
			NPC.value = 103;
			NPC.knockBackResist = 0.5f;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 200;
		public int MaxManaDrain => 24;
		public float Mana { get; set; }
		public void Regenerate(out int lifeRegen) {
			int factor = 37 / ((NPC.life / 40) + 2);
			lifeRegen = factor;
			Mana -= factor / 90f;// 3 mana for every 2 health regenerated
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.SpawnTileY > Main.worldSurface || spawnInfo.DesertCave) return 0;
			return Defiled_Wastelands.SpawnRates.LandEnemyRate(spawnInfo, false) * Defiled_Wastelands.SpawnRates.Brute;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Latchkey>(), 8, 2, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bombardment>(), 48));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Lousy_Liver>(), 87));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled2_Greaves>(), 525));
		}

		public override bool? CanFallThroughPlatforms() => NPC.directionY == 1 && NPC.target >= 0 && NPC.targetRect.Bottom > NPC.position.Y + NPC.height + NPC.velocity.Y;
		public override bool PreAI() {
			if (Main.rand.NextBool(1000)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(0.5f, 0.75f), NPC.Center);
			//if(!attacking) {
			if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X /= SpeedMult;
			//npc.Hitbox = new Rectangle((int)npc.position.X+(npc.oldDirection == 1 ? 70 : 52), (int)npc.position.Y, 56, npc.height);
			//}
			return true;
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					if (NPC.aiAction == 0) {
						Vector2 diff = NPC.GetTargetData().Center - NPC.Center;
						float dist = diff.Length();
						if (dist > 192 && ++NPC.localAI[0] > 60) {
							NPC.localAI[0] = 0;
							NPC.aiAction = 1 + Main.rand.Next(3);
							NPC.localAI[1] = 1;
							NPC.netUpdate = true;
						}
					} else if (NPC.localAI[1] > 0 && ++NPC.localAI[1] >= 4 * 7) {
						NPC.localAI[1] = 0;
						int projType;
						switch (NPC.aiAction) {
							default:
							projType = ModContent.ProjectileType<Defiled_Krusher_P1>();
							break;
							case 2:
							projType = ModContent.ProjectileType<Defiled_Krusher_P2>();
							break;
							case 3:
							projType = ModContent.ProjectileType<Defiled_Krusher_P3>();
							break;
						}
						Vector2 pos = NPC.Center + new Vector2(4 * NPC.direction, -52);
						Vector2 velocity;
						const float speed = 12;
						if (GeometryUtils.AngleToTarget(NPC.GetTargetData().Center - pos, speed, 0.1f) is float angle) {
							velocity = angle.ToRotationVector2() * speed;
						} else {
							float val = 0.70710678119f;
							velocity = new Vector2(val * NPC.direction, -val) * speed;
						}
						Projectile.NewProjectile(
							NPC.GetSource_FromAI(),
							pos,
							velocity,
							projType,
							(int)(30 * ContentExtensions.DifficultyDamageMultiplier),
							8
						);
					}
				}
			}
			if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X *= SpeedMult;
		}
		public override void FindFrame(int frameHeight) {
			if (NPC.aiAction == 0) {
				NPC.DoFrames(7, ..6);
			} else {
				if (NPC.frame.Y / NPC.frame.Height < 6) NPC.frame.Y = NPC.frame.Height * 6;
				NPC.DoFrames(7, 6..(Main.npcFrameCount[Type] + 1));
				if (NPC.frame.Y / NPC.frame.Height == Main.npcFrameCount[Type]) {
					NPC.aiAction = 0;
					NPC.frame.Y = 0;
				}
			}
		}
		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
			modifiers.Knockback.Flat -= 1.2f;
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 10; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)NPC.aiAction);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.aiAction = reader.ReadByte();
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			base.PostDraw(spriteBatch, screenPos, drawColor);
			Texture2D texture = null;
			switch (NPC.aiAction) {
				case 1:
				texture = TextureAssets.Projectile[ModContent.ProjectileType<Defiled_Asphyxiator_P1>()].Value;
				break;
				case 2:
				texture = TextureAssets.Projectile[ModContent.ProjectileType<Defiled_Asphyxiator_P2>()].Value;
				break;
				case 3:
				texture = TextureAssets.Projectile[ModContent.ProjectileType<Defiled_Asphyxiator_P3>()].Value;
				break;
			}
			if (texture is null) return;
			Vector2? pos = null;
			float rot = 0;
			switch (NPC.frame.Y / NPC.frame.Height) {
				case 6:
				pos = new(17, 24);
				break;
				case 7:
				pos = new(21, 17);
				break;
				case 8:
				pos = new(-10, -32);
				break;
				case 9:
				pos = new(-4, -52);
				break;
			}
			if (pos is Vector2 offset) {
				offset.X *= NPC.spriteDirection;
				spriteBatch.Draw(
					texture,
					NPC.Center + offset - screenPos,
					null,
					drawColor,
					rot,
					texture.Size() * 0.5f,
					1,
					SpriteEffects.None,
				0);
			}
		}
	}
	public class Defiled_Krusher_P1 : ModProjectile {
		public override string Texture => "Origins/NPCs/Defiled/Defiled_Asphyxiator_P" + GetType().Name[^1];
		public override void SetStaticDefaults() {
			AssimilationLoader.AddProjectileAssimilation<Defiled_Assimilation>(Type, 0.04f);
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 36;
			Projectile.timeLeft = 180;
			Projectile.hostile = true;
		}
		public override void AI() {
			Projectile.rotation += Projectile.velocity.X * 0.04f;
			Projectile.velocity.Y += 0.1f;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Projectile.Kill();
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			for (int i = 0; i < 5; i++)
				Origins.instance.SpawnGoreByName(Projectile.GetSource_Death(), Projectile.position + new Vector2(Main.rand.Next(Projectile.width), Main.rand.Next(Projectile.height)), Projectile.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
		}
	}
	public class Defiled_Krusher_P2 : Defiled_Krusher_P1 { }
	public class Defiled_Krusher_P3 : Defiled_Krusher_P1 { }
}
