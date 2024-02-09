using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Materials;
using Origins.Items.Other.LootBags;
using Origins.Items.Tools;
using Origins.Items.Weapons.Summoner;
using Origins.LootConditions;
using Origins.Tiles.BossDrops;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.Riven.World_Cracker.World_Cracker_Head;

namespace Origins.NPCs.Riven.World_Cracker {
    [AutoloadBossHead]
	public class World_Cracker_Head : WormHead, ILoadExtraTextures, IRivenEnemy {
		public void LoadTextures() => _ = GlowTexture;
		public virtual string GlowTexturePath => Texture + "_Glow";
		private Asset<Texture2D> _glowTexture;
		public Texture2D GlowTexture => (_glowTexture ??= (ModContent.RequestIfExists<Texture2D>(GlowTexturePath, out var asset) ? asset : null))?.Value;
		public override int BodyType => ModContent.NPCType<World_Cracker_Body>();
		public override int TailType => ModContent.NPCType<World_Cracker_Tail>();
		public static int DifficultyMult => Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
		public static AutoCastingAsset<Texture2D> ArmorTexture { get; private set; }
		public static AutoCastingAsset<Texture2D> HPBarArmorTexture { get; private set; }
		public static int MaxArmorHealth {
			get => 100 + 50 * DifficultyMult;
		}
		internal static IItemDropRule normalDropRule;
		internal static IItemDropRule armorBreakDropRule;
		int ArmorHealth { get => (int)NPC.ai[3]; set => NPC.ai[3] = (int)value; }
		public override void SetStaticDefaults() {
			/*var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { // Influences how the NPC looks in the Bestiary
				CustomTexturePath = "Origins/NPCs/Riven/World_Cracker_Bestiary", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(40f, 24f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = 12f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);*/
			if (!Main.dedServ) {
				ArmorTexture = ModContent.Request<Texture2D>("Origins/NPCs/Riven/World_Cracker/World_Cracker_Armor");
				HPBarArmorTexture = ModContent.Request<Texture2D>("Origins/NPCs/Riven/World_Cracker/World_Cracker_Armor_Health_Bar");
			}
		}
		public override void Unload() {
			ArmorTexture = null;
			HPBarArmorTexture = null;
			normalDropRule = null;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerHead);
			NPC.boss = true;
			NPC.BossBar = ModContent.GetInstance<Boss_Bar_WC>();
			NPC.width = NPC.height = 60;
			NPC.damage = 35;
			NPC.defense = 100;
			NPC.lifeMax = 4000;
			NPC.aiStyle = -1;
			NPC.GravityMultiplier *= 0.5f;
			Music = Origins.Music.RivenBoss;
		}
		public override void AI() {
			float ArmorHealthPercent = ArmorHealth / (float)MaxArmorHealth;
			if (ArmorHealthPercent > 0.5f) {
				NPC.defense = 100;
			} else if (ArmorHealthPercent > 0f) {
				NPC.defense = 50;
			} else {
				NPC.defense = 0;
			}
			//ForcedTargetPosition = Main.MouseWorld;
			//Acceleration = 1;
			SetBaseSpeed();
			Player playerTarget = Main.player[NPC.target];
			ForcedTargetPosition = playerTarget.MountedCenter - playerTarget.velocity * 32;
			float dot = Vector2.Dot(NPC.velocity.SafeNormalize(default), (ForcedTargetPosition.Value - NPC.Center).SafeNormalize(default));
			CanFly = dot > 0.5f;

			ProcessShoot(NPC);

			//Acceleration *= MathF.Max((0.8f -  * 5, 1);
		}
		public static void ProcessShoot(NPC npc) {
			NPC headNPC = npc.realLife >= 0 ? Main.npc[npc.realLife] : npc;
			int target = headNPC.target;
			Player playerTarget = Main.player[target];
			int otherShotDelay = Main.rand.Next(50, 70);
			if (++npc.ai[2] > (Main.expertMode ? 300 : 120) && Collision.CanHitLine(playerTarget.position, playerTarget.width, playerTarget.height, npc.Center, 8, 8)) {
				npc.ai[2] = otherShotDelay;
				Terraria.Audio.SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(0.4f, 0.6f), npc.Center);
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					Projectile.NewProjectileDirect(
						npc.GetSource_FromAI(),
						npc.Center,
						Vector2.Normalize(playerTarget.MountedCenter - npc.Center).RotatedByRandom(0.15f) * 9 * Main.rand.NextFloat(0.9f, 1.1f),
						Amoeball.ID,
						20 + (DifficultyMult * 3), // for some reason NPC projectile damage is just arbitrarily doubled
						0f,
						Main.myPlayer
					);
				}
				NPC current = headNPC;
				int tailType = ModContent.NPCType<World_Cracker_Tail>();
				while (current is not null) {
					current.ai[2] -= otherShotDelay;
					current = current.type == tailType ? null : Main.npc[(int)current.ai[0]];
				}
			}
		}
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			if (ArmorHealth > 0) {
				if (ArmorHealth < MaxArmorHealth) {
					float alpha = Lighting.Brightness((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f));
					Main.instance.DrawHealthBar(position.X, position.Y, ArmorHealth, MaxArmorHealth, alpha, scale);
				}
				return false;
			}
			return true;
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			DamageArmor(NPC, hit, item.ArmorPenetration);
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			DamageArmor(NPC, hit, projectile.ArmorPenetration);
		}
		public static void DamageArmor(NPC npc, NPC.HitInfo hit, int armorPenetration) {
			if (npc.ai[3] <= 0) return;
			int oldArmorHealth = (int)npc.ai[3];
			NPC.HitModifiers apMods = new NPC.HitModifiers();
			apMods.ArmorPenetration += armorPenetration;
			NPCLoader.ModifyIncomingHit(npc, ref apMods);
			npc.ai[3] = (int)Math.Max(npc.ai[3] - Math.Max((hit.SourceDamage * (hit.Crit ? 2 : 1)) - Math.Max(apMods.Defense.ApplyTo(15) - apMods.ArmorPenetration.Value, 0) * (1 - apMods.ScalingArmorPenetration.Value), 0), 0);
			if (!hit.HideCombatText) CombatText.NewText(npc.Hitbox, hit.Crit ? new Color(255, 170, 133) : new Color(255, 210, 173), oldArmorHealth - (int)npc.ai[3], hit.Crit);
			if (npc.ai[3] <= 0) {
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					DropAttemptInfo dropInfo = default(DropAttemptInfo);
					dropInfo.player = Main.LocalPlayer;
					dropInfo.npc = npc;
					dropInfo.IsExpertMode = Main.expertMode;
					dropInfo.IsMasterMode = Main.masterMode;
					dropInfo.IsInSimulation = false;
					dropInfo.rng = Main.rand;
					OriginExtensions.ResolveRule(armorBreakDropRule, dropInfo);
				}
				int halfWidth = npc.width / 2;
				int baseX = hit.HitDirection > 0 ? 0 : halfWidth;
				if (Main.netMode != NetmodeID.Server) {
					Gore.NewGore(
						npc.GetSource_OnHit(npc),
						npc.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(npc.height)),
						hit.GetKnockbackFromHit(),
						Origins.instance.GetGoreSlot("Gores/NPCs/WC_Cracked_Armor" + Main.rand.Next(1, 5))
					);
				}
			}
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.world_cracker_hit);
				packet.Write((ushort)npc.whoAmI);
				packet.Write((int)hit.SourceDamage);
				packet.Write((bool)hit.Crit);
				packet.Write((int)hit.HitDirection);
				packet.Write((float)hit.Knockback);
				packet.Write((int)armorPenetration);
				packet.Send(-1, Main.myPlayer);
			}
		}
		public static void DrawArmor(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor, Rectangle frame, NPC npc) {
			float ArmorHealthPercent = ((int)npc.ai[3]) / (float)MaxArmorHealth;
			if (ArmorHealthPercent <= 0f) return;
			if (ArmorHealthPercent <= 0.5f) {
				frame.Y += 120;
			}
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (npc.spriteDirection == 1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
			Texture2D armorTexture = ArmorTexture;
			Vector2 halfSize = frame.Size() / 2;
			spriteBatch.Draw(
				armorTexture,
				npc.Center - screenPos,
				frame,
				drawColor,
				npc.rotation,
				halfSize,
				npc.scale,
				spriteEffects,
			0);
		}
		internal static void CommonWormInit(Worm worm) {
			// These two properties handle the movement of the worm
			worm.NPC.ai[3] = MaxArmorHealth;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Glowing_Mod_NPC.DrawGlow(spriteBatch, screenPos, GlowTexture, NPC);
			DrawArmor(spriteBatch, screenPos, drawColor, new Rectangle(0, 0, 102, 58), NPC);
		}
		void SetBaseSpeed() {
			MoveSpeed = 15.5f;
			Acceleration = 0.2f + 0.05f * DifficultyMult;
		}
		public override void Init() {
			MinSegmentLength = MaxSegmentLength = 13 + 2 * DifficultyMult;
			SetBaseSpeed();
			CommonWormInit(this);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {

			normalDropRule = new LeadingSuccessRule();

			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Encrusted_Ore_Item>(), 1, 20, 330));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Riven_Carapace>(), 1, 1, 134));
			normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Amoeba_Hook>(), 1));
            normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Teardown>(), 1));

            normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<World_Cracker_Trophy_Item>(), 10));
            normalDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<World_Cracker_Mask>(), 10));

			npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<World_Cracker_Bag>(), 1, 1, 1, null)
			));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Protozoa_Food>(), 4));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<World_Cracker_Relic_Item>()));

            armorBreakDropRule = new LeadingSuccessRule();

			armorBreakDropRule.OnSuccess(new CommonDrop(ModContent.ItemType<Encrusted_Ore_Item>(), 2, 5, 12, 3));
			armorBreakDropRule.OnSuccess(new CommonDrop(ModContent.ItemType<Riven_Carapace>(), 2, 2, 5, 3));
		}
		public override bool SpecialOnKill() {
			Mod.Logger.Info($"SpecialOnKill on {Main.netMode}, life: {NPC.life}");
			int tailType = TailType;
			float dist = float.PositiveInfinity;
			int closest = NPC.whoAmI;
			NPC current = NPC;
			while (current is not null) {
				for (int j = 0; j < Main.maxPlayers; j++) {
					if (Main.player[j].active && !Main.player[j].dead) {
						float currentDist = Main.player[j].DistanceSQ(current.Center);
						if (currentDist < dist) {
							dist = currentDist;
							closest = current.whoAmI;
						}
					}
				}
				DamageArmor(current, new NPC.HitInfo() { SourceDamage = 9999, HideCombatText = true }, 0);
				if (!Main.dedServ) for (int i = 0; i < 10; i++) Gore.NewGore(
					current.GetSource_Death(),
					Main.rand.NextVector2FromRectangle(current.Hitbox),
					current.oldVelocity,
					Origins.instance.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4))
				);
				current = current.type == tailType ? null : Main.npc[(int)current.ai[0]];
			}
			NPC.Center = Main.npc[closest].Center;
			return false;
		}
	}
	public class World_Cracker_Body : WormBody, ILoadExtraTextures, IRivenEnemy {
		public void LoadTextures() => _ = GlowTexture;
		public virtual string GlowTexturePath => Texture + "_Glow";
		private Asset<Texture2D> _glowTexture;
		public Texture2D GlowTexture => (_glowTexture ??= (ModContent.RequestIfExists<Texture2D>(GlowTexturePath, out var asset) ? asset : null))?.Value;
		int ArmorHealth { get => (int)NPC.ai[3]; set => NPC.ai[3] = (int)value; }
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			});
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerBody);
			NPC.width = NPC.height = 48;
			NPC.damage = 20;
			NPC.defense = 100;
			NPC.aiStyle = -1;
		}
		public override void AI() {
			float ArmorHealthPercent = ArmorHealth / (float)MaxArmorHealth;
			if (ArmorHealthPercent > 0.5f) {
				NPC.defense = 100;
			} else if (ArmorHealthPercent > 0f) {
				NPC.defense = 54;
			} else {
				NPC.defense = 8;
			}
			NPC.life = NPC.lifeMax - 1;

			NPC.oldVelocity = NPC.position - NPC.oldPosition;
			if (Main.expertMode) ProcessShoot(NPC);
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			DamageArmor(NPC, hit, item.ArmorPenetration);
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			DamageArmor(NPC, hit, projectile.ArmorPenetration);
		}
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			if (ArmorHealth > 0 && ArmorHealth < MaxArmorHealth) {
				float alpha = Lighting.Brightness((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f));
				Main.instance.DrawHealthBar(position.X, position.Y, ArmorHealth, MaxArmorHealth, alpha, scale);
			}
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Glowing_Mod_NPC.DrawGlow(spriteBatch, screenPos, GlowTexture, NPC);
			DrawArmor(spriteBatch, screenPos, drawColor, new Rectangle(104, (int)(48 * NPC.frameCounter), 62, 58), NPC);
		}
		public override void Init() {
			NPC.frameCounter = Main.rand.Next(2);
			CommonWormInit(this);
		}
	}
	public class World_Cracker_Tail : WormTail, IRivenEnemy {
		int ArmorHealth { get => (int)NPC.ai[3]; set => NPC.ai[3] = (int)value; }
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			});
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerTail);
			NPC.width = NPC.height = 60;
			NPC.damage = 20;
			NPC.defense = 20;
			NPC.aiStyle = -1;
		}
		public override void AI() {
			float ArmorHealthPercent = ArmorHealth / (float)MaxArmorHealth;
			if (ArmorHealthPercent > 0.5f) {
				NPC.defense = 20;
			} else if (ArmorHealthPercent > 0f) {
				NPC.defense = 10;
			} else {
				NPC.defense = 0;
			}
			NPC.life = NPC.lifeMax - 1;

			NPC.oldVelocity = NPC.position - NPC.oldPosition;
			if (Main.expertMode) ProcessShoot(NPC);
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			DamageArmor(NPC, hit, item.ArmorPenetration);
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			DamageArmor(NPC, hit, projectile.ArmorPenetration);
		}
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			if (ArmorHealth > 0 && ArmorHealth < MaxArmorHealth) {
				float alpha = Lighting.Brightness((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f));
				Main.instance.DrawHealthBar(position.X, position.Y, ArmorHealth, MaxArmorHealth, alpha, scale);
			}
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Glowing_Mod_NPC.DrawGlow(spriteBatch, screenPos, TextureAssets.Npc[Type].Value, NPC);
			DrawArmor(spriteBatch, screenPos, drawColor, new Rectangle(168, 0, 52, 56), NPC);
		}
		public override void Init() {
			CommonWormInit(this);
		}
	}
	public class Boss_Bar_WC : ModBossBar {
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			return Asset<Texture2D>.Empty;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
			drawParams.ShowText = false;
			BossBarLoader.DrawFancyBar_TML(spriteBatch, drawParams);
			drawParams.ShowText = true;
			float totalWidth = 0;
			List<Rectangle> frames = new();
			void AddFrame(NPC currentNPC, Rectangle baseFrame) {
				if (currentNPC.ai[3] <= 0) {
					baseFrame.Y = 27;
					baseFrame.Height = 1;
				} else if (currentNPC.ai[3] < MaxArmorHealth * 0.5f) {
					baseFrame.Y += 28;
				}
				totalWidth += baseFrame.Width;
				frames.Add(baseFrame);
			}
			AddFrame(npc, new Rectangle(0, 0, 32, 26));
			totalWidth -= 48;
			NPC current = Main.npc[(int)npc.ai[0]];
			int bodyType = ModContent.NPCType<World_Cracker_Body>();
			while (current.type == bodyType) {
				AddFrame(current, new Rectangle(34, 0, 32, 26));
				current = Main.npc[(int)current.ai[0]];
			}
			AddFrame(npc, new Rectangle(68, 0, 32, 26));
			Vector2 pos = drawParams.BarCenter;
			float scale = 1f;
			float barWidth = drawParams.BarTexture.Width - 48;
			pos.X += barWidth * 0.5f;
			pos.X -= 8;
			pos.Y += 10;
			for (int i = frames.Count - 1; i >= 0; i--) {
				Rectangle frame = frames[i];
				spriteBatch.Draw(HPBarArmorTexture, pos, frame, Color.White, 0, frame.Size() / 2, scale, 0, 0);
				pos.X -= barWidth / frames.Count;
			}
			drawParams.BarTexture = Asset<Texture2D>.DefaultValue;
			BossBarLoader.DrawFancyBar_TML(spriteBatch, drawParams);
			return false;
		}
	}
	public class World_Cracker_Master_Biome : ModBiome {
		const string biomeName = "Origins:WorldCrackerMaster";
		public override void SetStaticDefaults() {
			SkyManager.Instance.Bind(biomeName, new World_Cracker_Master_Sky());
		}
		public override bool IsBiomeActive(Player player) {
			if (Main.LocalPlayer.InModBiome<Riven_Hive>()) {
				return Main.remixWorld || (Main.masterMode && NPC.npcsFoundForCheckActive[ModContent.NPCType<World_Cracker_Head>()]);
			}
			return false;
		}
		public override void SpecialVisuals(Player player, bool isActive) {
			//Main.ColorOfTheSkies = Main.ColorOfTheSkies.MultiplyRGB(new(150, 160, 175));
			if (SkyManager.Instance[biomeName] is CustomSky sky && isActive != sky.IsActive()) {
				if (isActive) {
					SkyManager.Instance.Activate(biomeName);
				} else {
					SkyManager.Instance.Deactivate(biomeName);
				}
			}
			if (Overlays.Scene[biomeName] is Overlay overlay && isActive != overlay.IsVisible()) {
				if (isActive) {
					Overlays.Scene.Activate(biomeName);
				} else {
					Overlays.Scene.Deactivate(biomeName);
				}
			}
		}
	}
	public class World_Cracker_Master_Sky : CustomSky {
		bool isActive;
		public override void Activate(Vector2 position, params object[] args) {
			isActive = true;
		}

		public override void Deactivate(params object[] args) {
			isActive = false;
		}
		public override bool IsActive() => isActive;
		public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) {}
		public override void Reset() { }
		public override float GetCloudAlpha() {
			return 5;
		}
		public override void Update(GameTime gameTime) {
			float newRainCount = Main.screenWidth / 1920f;
			newRainCount *= 25f;
			Vector2 spawnPos = default(Vector2);
			for (int i = 0; i < newRainCount; i++) {
				int stretch = 600;
				if (Main.player[Main.myPlayer].velocity.Y < 0f) {
					stretch += (int)(Math.Abs(Main.player[Main.myPlayer].velocity.Y) * 30f);
				}
				spawnPos.X = Main.rand.Next((int)Main.screenPosition.X - stretch, (int)Main.screenPosition.X + Main.screenWidth + stretch);
				spawnPos.Y = Math.Min(Main.screenPosition.Y - Main.rand.Next(20, 100), (float)(Main.worldSurface * 16));
				spawnPos.X -= Main.windSpeedCurrent * 15f * 40f;
				spawnPos.X += Main.player[Main.myPlayer].velocity.X * 40f;
				if (spawnPos.X < 0f) {
					spawnPos.X = 0f;
				}
				if (spawnPos.X > ((Main.maxTilesX - 1) * 16)) {
					spawnPos.X = (Main.maxTilesX - 1) * 16;
				}
				int tileX = (int)MathHelper.Clamp(spawnPos.X / 16, 0, Main.maxTilesX - 1);
				int tileY = (int)MathHelper.Clamp(spawnPos.Y / 16, 0, Main.maxTilesY - 1);
				if (Main.remixWorld || Main.gameMenu || (!WorldGen.SolidTile(tileX, tileY) && Main.tile[tileX, tileY].WallType <= WallID.None)) {
					Vector2 rainFallVelocity = Rain.GetRainFallVelocity();
					Rain.NewRainForced(spawnPos, rainFallVelocity);
				}
			}
		}
		public override Color OnTileColor(Color inColor) {
			Color c = new(60, 80, 100);
			Main.ColorOfTheSkies = Main.ColorOfTheSkies.MultiplyRGB(c);
			return inColor.MultiplyRGB(c);
		}
	}
	public class Amoeball : ModProjectile {
		public static int ID { get; private set; } = -1;
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Amoeba_Bubble";
		public override string GlowTexture => Texture;
		public AssimilationAmount Assimilation => 0.04f;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Amoeba Bubble");
			Main.projFrames[Projectile.type] = 4;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 5;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 180;
			Projectile.scale = 0.75f;
			Projectile.alpha = 150;
		}
		public override void AI() {
			if (Projectile.timeLeft > 180 - 15) {
				Projectile.position += Projectile.velocity * (Projectile.timeLeft - (180f - 15)) / 15;
			}
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= 7) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Main.expertMode ^ Main.masterMode) return true;
			float speed = oldVelocity.Length();
			Projectile.velocity = Projectile.velocity + Projectile.velocity - oldVelocity;
			Projectile.velocity *= speed / Projectile.velocity.Length();
			Projectile.penetrate--;
			return false;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.GetModPlayer<OriginPlayer>().RivenAssimilation += Assimilation.GetValue(null, target);
			Projectile.Kill();
			//Projectile.penetrate--;
		}
		public override void OnKill(int timeLeft) {
			if (timeLeft > 0 && OriginClientConfig.Instance.ExtraGooeyRivenGores) {
				Gore.NewGore(
					 Projectile.GetSource_Death(),
					 Projectile.Center,
					 Projectile.oldVelocity,
					 Origins.instance.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4))
				 );
			}
		}
		public override Color? GetAlpha(Color lightColor) {
			float timeFactor = 1f;
			if (Projectile.timeLeft < 15) {
				timeFactor = Projectile.timeLeft / 15f;
			} else if (Projectile.timeLeft > 180 - 15) {
				timeFactor = (Projectile.timeLeft - (180f - 15)) / 15;
			}
			return new Color((lightColor.R + 255) / 510f, (lightColor.G + 255) / 510f, (lightColor.B + 255) / 510f, 0.5f) * timeFactor;
		}
	}
}
