using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.NPCs.Defiled.Boss;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
	public class Defiled_Watcher : Glowing_Mod_NPC, IDefiledEnemy, IWikiNPC, ITangelaHaver, ICustomWikiStat {
		public Rectangle DrawRect => new(0, 0, 74, 68);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.03f;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 3;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			BiomeNPCGlobals.assimilationDisplayOverrides.Add(Type, new() {
				[ModContent.GetInstance<Defiled_Assimilation>().AssimilationType] = Defiled_Watcher_Spikes.assimilation_amount
			});
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.None;
			NPC.lifeMax = 160;
			NPC.defense = 28;
			NPC.damage = 49;
			NPC.width = 74;
			NPC.height = 74;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0.25f, 0.5f);
			NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(0.25f, 0.5f);
			NPC.value = 2300;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0.5f;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Black_Bile>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Latchkey>(), 5, 3, 7));
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
			if (spawnInfo.SpawnTileY < Main.worldSurface || spawnInfo.DesertCave) return 0;
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
			get => NPC.frame.Y / 68;
			set => NPC.frame.Y = value * 68;
		}
		public override bool PreAI() {
			NPC.TargetClosestUpgraded(false);
			float speed = 5f;
			float inertia = 24f;
			Vector2? vectorToTargetPosition = null;
			Vector2 targetPos = default;
			if (NPC.HasValidTarget) {
				NPCAimedTarget target = NPC.GetTargetData();
				targetPos = NPC.Center.Clamp(target.Hitbox);
				vectorToTargetPosition = targetPos - target.Center.Clamp(NPC.Hitbox);
				if (NPC.confused) vectorToTargetPosition *= -1;
			} else {
				if (NPC.velocity.LengthSquared() < 0.001f && Main.rand.NextBool(10)) {
					// If there is a case where it's not moving at all, give it a little "poke"
					NPC.velocity += Main.rand.NextVector2CircularEdge(1, 1) * 0.5f;
				} else {
					NPC.velocity.X = NPC.oldVelocity.X;
					NPC.velocity.Y = NPC.oldVelocity.Y;
				}
			}
			if (NPC.ai[0] > 0) NPC.ai[0]--;
			if (vectorToTargetPosition.HasValue && !vectorToTargetPosition.Value.HasNaNs()) {
				float distance = vectorToTargetPosition.Value.Length();
				Vector2 direction = distance == 0 ? default : vectorToTargetPosition.Value / distance;
				NPC.velocity = (NPC.velocity * (inertia - 1) + direction * speed) / inertia;
				if (distance < 16 * 3 && NPC.ai[0] <= 0) {
					NPC.ai[0] = 67;
					Projectile.NewProjectile(
						NPC.GetSource_FromAI(),
						NPC.Center,
						(targetPos - NPC.Center).SafeNormalize(default) * 8,
						ModContent.ProjectileType<Defiled_Watcher_Spikes>(),
						(int)(20 * ContentExtensions.DifficultyDamageMultiplier),
						3,
						ai2: NPC.whoAmI
					);
				}
			}
			if (NPC.velocity.HasNaNs()) NPC.velocity = default;
			//if (Math.Abs(NPC.velocity.X) < 0.001f) NPC.velocity.X = 0;
			//if (Math.Abs(NPC.velocity.Y) < 0.001f) NPC.velocity.Y = 0;
			NPC.rotation = NPC.velocity.X * 0.1f;
			if (Math.Abs(NPC.velocity.X) <= 0.1f) NPC.direction = NPC.oldDirection;
			else NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.oldDirection = NPC.direction;
			NPC.spriteDirection = NPC.direction;
			return false;
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 9) {
				NPC.frameCounter = 0;
				if (++Frame >= 3) {
					Frame = 0;
					NPC.frameCounter = 0;
				}
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
		public int? TangelaSeed { get; set; }
		public AutoLoadingAsset<Texture2D> tangelaTexture = typeof(Defiled_Watcher).GetDefaultTMLName() + "_Tangela";
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			base.PostDraw(spriteBatch, screenPos, drawColor);
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (NPC.spriteDirection == 1) spriteEffects = SpriteEffects.FlipHorizontally;
			Vector2 halfSize = new(tangelaTexture.Value.Width / 2, tangelaTexture.Value.Height / Main.npcFrameCount[NPC.type] / 2);
			TangelaVisual.DrawTangela(
				this,
				tangelaTexture,
				new Vector2(NPC.position.X - screenPos.X + (NPC.width / 2) - tangelaTexture.Value.Width * NPC.scale / 2f + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + NPC.height - tangelaTexture.Value.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4f + halfSize.Y * NPC.scale + Main.NPCAddHeight(NPC) + NPC.gfxOffY),
				NPC.frame,
				NPC.rotation,
				halfSize,
				new(NPC.scale),
				spriteEffects
			);
		}
	}
	public class Defiled_Watcher_Spikes : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
		public const float assimilation_amount = 0.05f;
		public AssimilationAmount Assimilation = assimilation_amount;
		public override void SetStaticDefaults() {
			this.AddAssimilation<Defiled_Assimilation>(Assimilation);
		}
		public override void SetDefaults() {
			Projectile.timeLeft = 600;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.hide = true;
			Projectile.rotation = Main.rand.NextFloatDirection();
			Projectile.tileCollide = false;
			Projectile.npcProj = true;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Default;
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool? CanHitNPC(NPC target) => false;
		public override bool CanHitPlayer(Player target) => false;
		public override bool CanHitPvp(Player target) => false;
		public override void AI() {
			if (Projectile.localAI[0] < 3) {
				Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Projectile.velocity.RotatedBy(Projectile.localAI[0] * 0.4f) * Main.rand.NextFloat(1.1f, 1.5f),
					Defiled_Spike_Explosion_Spike_Hostile.ID,
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner,
					ai1: Projectile.whoAmI
				);
				if (Projectile.ai[1] != 1) {
					Projectile.NewProjectileDirect(
						Projectile.GetSource_FromThis(),
						Projectile.Center,
						Projectile.velocity.RotatedBy(-Projectile.localAI[0] * 0.4f) * Main.rand.NextFloat(1.1f, 1.5f),
						Defiled_Spike_Explosion_Spike_Hostile.ID,
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner,
						ai1: Projectile.whoAmI
					);
				} else {
					Projectile.ai[1] = 1;
				}
				Projectile.localAI[0]++;
			}
			Projectile.Center = Main.npc[(int)Projectile.ai[2]].Center;
		}
	}
}
