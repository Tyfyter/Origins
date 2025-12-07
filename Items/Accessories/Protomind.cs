using Microsoft.Xna.Framework;
using Origins.Dev;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Protomind : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		static Message_Cache[] messagesByType;
		
		public override void Unload() {
			messagesByType = null;
			AchievementsHelper.OnProgressionEvent -= AchievementsHelper_OnProgressionEvent;
		}
		public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
			AchievementsHelper.OnProgressionEvent += AchievementsHelper_OnProgressionEvent;
		}
		static short glowmask;
        public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.accessory = true;
			Item.shoot = ModContent.ProjectileType<Protomind_P>();
			Item.rare = ItemRarityID.LightPurple;
			Item.expert = true;
			Item.value = Item.sellPrice(gold: 3);
            Item.glowMask = glowmask;
        }
		public override void UpdateInventory(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.hasProtOS = true;
			UpdateMoonlordWarningAndIdle(originPlayer);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.hasProtOS = true;
			originPlayer.protomindItem = Item;
			UpdateMoonlordWarningAndIdle(originPlayer, hideVisual);
		}
		static void UpdateMoonlordWarningAndIdle(OriginPlayer originPlayer, bool disableIdle = false) {
			Player player = originPlayer.Player;
			int[] cooldowns = originPlayer.protOSQuoteCooldown;
			if (cooldowns[(int)QuoteType.The_Part_Where_He_Kills_You] <= 0) {
				int index = NPC.FindFirstNPC(NPCID.MoonLordHead);
				if (index != -1 && Main.npc[index].ai[0] == 1f) { //charging up deathray
					PlayRandomMessage(
						QuoteType.The_Part_Where_He_Kills_You,
						cooldowns,
						player.Top
					);
				}
			}
			if (cooldowns[(int)QuoteType.Idle] <= 0) {
				PlayRandomMessage(QuoteType.Idle, cooldowns, player.Top);
			}
			if (player.companionCube && cooldowns[(int)QuoteType.Companion_Cube] <= 0) {
				PlayRandomMessage(QuoteType.Companion_Cube, cooldowns, player.Top);
			}
			if (player.position.Y > Main.UnderworldLayer * 16 && player.oldPosition.Y <= Main.UnderworldLayer * 16) {
				PlayRandomMessage(QuoteType.In_Hell, cooldowns, player.Top);
			}
			if (player.gravDir != originPlayer.lastGravDir) {
				PlayRandomMessage(QuoteType.Gravitation, cooldowns, player.Top);
			}
			int boundNPCType = -1;
			const float boundNPCRange = 50 * 16;
			if (originPlayer.nearbyBoundNPCType == -1) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.active && player.DistanceSQ(npc.Center) < boundNPCRange * boundNPCRange) {
						bool isVanillaBoundNPC = npc.type is 105 or 106 or 123 or 354 or 376 or 579 or 453 or 589;
						if (isVanillaBoundNPC || (npc.aiStyle == 0 && (NPCLoader.CanChat(npc) ?? false))) {
							boundNPCType = npc.type;
						}
					}
				}
			} else {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.active && npc.type == originPlayer.nearbyBoundNPCType && player.DistanceSQ(npc.Center) < boundNPCRange * boundNPCRange) {
						boundNPCType = npc.type;
					}
				}
			}
			originPlayer.nearbyBoundNPCType = boundNPCType;
			if (boundNPCType == -1) {
				originPlayer.nearbyBoundNPCTime = 0;
			} else if (++originPlayer.nearbyBoundNPCTime == 60 * 90) {// 90 seconds
				PlayRandomMessage(QuoteType.Bound_NPC, cooldowns, player.Top);
			}
		}
		public override bool OnPickup(Player player) {
			PlayRandomMessage(QuoteType.Pickup, player.GetModPlayer<OriginPlayer>().protOSQuoteCooldown, player.Top);
			return true;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			int[] cooldownCounter = Main.LocalPlayer.GetModPlayer<OriginPlayer>().protOSQuoteCooldown;
			if (cooldownCounter[(int)QuoteType.Bird] <= 0) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.active && Potato_Battery.IsBird(npc.type) && npc.DistanceSQ(Item.Center) < 160 * 160) {
						PlayRandomMessage(QuoteType.Bird, cooldownCounter, Item.Top);
					}
				}
			}
		}
		static void AchievementsHelper_OnProgressionEvent(int eventID) {
			if (eventID == AchievementHelperID.Events.SmashShadowOrb) {
				OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
				if (originPlayer.hasProtOS) {
					PlayRandomMessage(QuoteType.Smashing_Evil_Tile, originPlayer.protOSQuoteCooldown, Main.LocalPlayer.Top);
				}
			}
		}
		public static string GetRandomVariation(QuoteType type) {
			if ((int)type >= (int)QuoteType.Count) {
				Origins.instance.Logger.Error($"{nameof(Protomind)}.{nameof(GetRandomVariation)} called with invalid parameter {type} ({(int)type})");
				return "misingno";
			}
			if (messagesByType is null || messagesByType.Length != (int)QuoteType.Count) {
				messagesByType = new Message_Cache[(int)QuoteType.Count];
			}
			Message_Cache refreshCache = messagesByType[(int)type];
			if (refreshCache.LastCulture?.IsActive == true && refreshCache.LastGameMode == Main.GameModeInfo.Id) {
				return Main.rand.Next(messagesByType[(int)type].Cache);
			}
			List<string> cache = new();
			static bool TryGetText(string key, out string text) {
				if (Language.Exists(key)) {
					text = Language.GetTextValue(key);
					return true;
				}
				text = key;
				return false;
			}
			if (TryGetText($"Mods.Origins.Dialogue.ProtOS.{type}", out string quote)) {
				cache.Add(quote);
			} else {
				int count = 0;
				while (TryGetText($"Mods.Origins.Dialogue.ProtOS.{type}_{count}", out quote)) {
					cache.Add(quote);
					count++;
				}
			}
			if (Main.expertMode) {
				if (TryGetText($"Mods.Origins.Dialogue.ProtOS.{type}_Expert", out quote)) {
					cache.Add(quote);
				} else {
					int count = 0;
					while (TryGetText($"Mods.Origins.Dialogue.ProtOS.{type}_Expert_{count}", out quote)) {
						cache.Add(quote);
						count++;
					}
				}
			}
			if (Main.masterMode) {
				if (TryGetText($"Mods.Origins.Dialogue.ProtOS.{type}_Master", out quote)) {
					cache.Add(quote);
				} else {
					int count = 0;
					while (TryGetText($"Mods.Origins.Dialogue.ProtOS.{type}_Master_{count}", out quote)) {
						cache.Add(quote);
						count++;
					}
				}
			}
			messagesByType[(int)type] = new(cache.ToArray(), LanguageManager.Instance.ActiveCulture, Main.GameModeInfo.Id);
			if (cache.Count == 0) {
				messagesByType[(int)type] = new([$"missingno (Mods.Origins.Dialogue.ProtOS.{type})"], LanguageManager.Instance.ActiveCulture, Main.GameModeInfo.Id);
				return messagesByType[(int)type].Cache[0];
			}
			return Main.rand.Next(cache);
		}
		public static void PlayRandomMessage(QuoteType type, int[] cooldowns, Vector2 position, Vector2? velocity = null) {
			TryPlayMessage(
				type,
				GetRandomVariation(type),
				cooldowns,
				position,
				null
			);
		}
		public static void TryPlayMessage(QuoteType type, string quote, int[] cooldowns, Vector2 position, Vector2? velocity = null) {
			if (cooldowns[(int)type] <= 0) {
				PlayMessage(
					quote,
					position,
					velocity
				);
				switch (type) {
					case QuoteType.Pickup:
					case QuoteType.In_Hell:
					case QuoteType.Smashing_Evil_Tile:
					cooldowns[(int)type] = Main.rand.Next(1200, 1801);
					break;

					case QuoteType.Death:
					case QuoteType.Kill_Villager:
					case QuoteType.Bound_NPC:// no need to do something here since the cooldown is already at the desired value of 0
					break;

					case QuoteType.Item_Is_Bad:
					case QuoteType.Item_Is_Explosive:
					case QuoteType.Potato_Launcher:
					case QuoteType.Bird:
					cooldowns[(int)type] = Main.rand.Next(200, 481);
					break;

					case QuoteType.Combat:
					cooldowns[(int)type] = Main.rand.Next(240, 361);
					break;

					case QuoteType.Gravitation:
					case QuoteType.The_Part_Where_He_Kills_You:
					cooldowns[(int)type] = Main.rand.Next(600, 901);
					break;
				}
				cooldowns[(int)QuoteType.Idle] = Main.rand.Next(1800, 3601);
				cooldowns[(int)QuoteType.Companion_Cube] = Main.rand.Next(1800, 3601);
			}
		}
		public static void PlayMessage(string text, Vector2 position, Vector2? velocity = null) {
			PopupText.NewText(new AdvancedPopupRequest() {
				Text = text,
				DurationInFrames = 30 + text.Length * 6,
				Velocity = velocity ?? new Vector2(Main.rand.NextFloatDirection() * 7f, -2f + Main.rand.NextFloat() * -2f),
				Color = new Color(242, 250, 255)
			}, position);
			SoundEngine.PlaySound(SoundID.NPCHit34.WithPitch(-1f), position);
			SoundEngine.PlaySound(SoundID.NPCHit26.WithPitch(-1f), position);
			if (Main.rand.NextBool(5)) {
				SoundEngine.PlaySound(SoundID.NPCHit55.WithPitch(1f), position);
			}
			if (Main.rand.NextBool(40)) {
				SoundEngine.PlaySound(SoundID.Zombie108.WithPitch(-1f), position);
			}
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BrainOfConfusion)
			.AddIngredient(ModContent.ItemType<Potato_Battery>())
			.AddTile(TileID.TinkerersWorkbench)
			.AddOnCraftCallback((_, _, _, _) => {
				PlayRandomMessage(
					QuoteType.Craft,
					Main.LocalPlayer.GetModPlayer<OriginPlayer>().protOSQuoteCooldown,
					Main.LocalPlayer.Top
				);
			})
			.Register();
		}
		public enum QuoteType {
			Pickup,
			Craft,
			Bird,
			Death,
			Respawn,
			Combat,
			Falling,
			Gravitation,
			Idle,
			Companion_Cube,
			In_Hell,
			Item_Is_Bad,
			Item_Is_Explosive,
			Smashing_Evil_Tile,
			Kill_Villager,
			Bound_NPC,
			The_Part_Where_He_Kills_You,
			Potato_Launcher,

			Count
		}
		readonly struct Message_Cache(string[] cache, GameCulture lastCulture, int lastGameMode) {
			public string[] Cache { get; init; } = cache;
			public GameCulture LastCulture { get; init; } = lastCulture;
			public int LastGameMode { get; init; } = lastGameMode;
		}
	}
	public class Protomind_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Main.projFrames[ProjectileID.BrainOfConfusion];
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BrainOfConfusion);
			AIType = ProjectileID.BrainOfConfusion;
		}
	}
}
