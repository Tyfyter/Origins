using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Water;
using Origins.World.BiomeData;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.Riven.World_Cracker.World_Cracker_Head;

namespace Origins.NPCs.Riven.World_Cracker {
	public class World_Cracker_Head : WormHead {
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
			ForcedTargetPosition = Main.MouseWorld;
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
		public static void DamageArmor(NPC npc, NPC.HitInfo hit,  int armorPenetration) {
			npc.ai[3] = (int)Math.Max(npc.ai[3] - Math.Max(hit.SourceDamage - Math.Max(15 - armorPenetration, 0), 0), 0);
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
			worm.MoveSpeed = 15.5f;
			worm.Acceleration = 0.3f;
			worm.NPC.ai[3] = MaxArmorHealth;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Glowing_Mod_NPC.DrawGlow(spriteBatch, screenPos, GlowTexture, NPC);
			DrawArmor(spriteBatch, screenPos, drawColor, new Rectangle(0, 0, 102, 58), NPC);
		}
		public override void Init() {
			MinSegmentLength = MaxSegmentLength = 13 + 2 * DifficultyMult;
			CommonWormInit(this);
		}
	}
	public class World_Cracker_Body : WormBody {
		public virtual string GlowTexturePath => Texture + "_Glow";
		private Asset<Texture2D> _glowTexture;
		public Texture2D GlowTexture => (_glowTexture ??= (ModContent.RequestIfExists<Texture2D>(GlowTexturePath, out var asset) ? asset : null))?.Value;
		int ArmorHealth { get => (int)NPC.ai[3]; set => NPC.ai[3] = (int)value; }
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
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
	public class World_Cracker_Tail : WormTail {
		int ArmorHealth { get => (int)NPC.ai[3]; set => NPC.ai[3] = (int)value; }
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
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
					baseFrame.Y = 58;
					baseFrame.Height = 2;
				} else if (currentNPC.ai[3] < MaxArmorHealth * 0.5f) {
					baseFrame.Y += 60;
				}
				totalWidth += baseFrame.Width;
				frames.Add(baseFrame);
			}
			AddFrame(npc, new Rectangle(0, 0, 50, 36));
			totalWidth -= 60;
			NPC current = Main.npc[(int)npc.ai[0]];
			int tailType = ModContent.NPCType<World_Cracker_Tail>();
			while (current.type != tailType) {
				AddFrame(current, new Rectangle(52, 0, 30, 36));
				current = Main.npc[(int)current.ai[0]];
			}
			AddFrame(npc, new Rectangle(84, 0, 26, 36));
			Vector2 pos = drawParams.BarCenter;
			float scale = 1f;
			float barWidth = drawParams.BarTexture.Width - 60;
			pos.X += barWidth * 0.5f;
			pos.X -= 8;
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
}
