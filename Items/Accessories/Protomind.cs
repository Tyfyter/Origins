using Microsoft.Xna.Framework;
using Origins.Dev;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Protomind : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat",
			"Info"
		};
		static Message_Cache[] messagesByType;
		
		public override void Unload() {
			messagesByType = null;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.accessory = true;
			Item.shoot = ModContent.ProjectileType<Protomind_P>();
			Item.rare = ItemRarityID.LightPurple;
			Item.value = Item.sellPrice(gold: 3);
			Item.expert = true;
		}
		public override void UpdateInventory(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.hasProtOS = true;
			UpdateMoonlordWarningAndIdle(originPlayer.protOSQuoteCooldown, player);
		}
		public override void UpdateEquip(Player player) {
			//player.brainOfConfusionItem = Item;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.hasProtOS = true;
			originPlayer.protomindItem = Item;
			UpdateMoonlordWarningAndIdle(originPlayer.protOSQuoteCooldown, player);
		}
		static void UpdateMoonlordWarningAndIdle(int[] cooldowns, Player player) {
			if (cooldowns[(int)QuoteType.The_Part_Where_He_Kills_You] <= 0) {
				int index = NPC.FindFirstNPC(NPCID.MoonLordHead);
				if (index != -1 && Main.npc[index].ai[0] == 1f) {
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
			if (player.position.Y > Main.UnderworldLayer * 16 && player.oldPosition.Y <= Main.UnderworldLayer * 16) {
				PlayRandomMessage(QuoteType.InHell, cooldowns, player.Top);
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
		public static string GetRandomVariation(QuoteType type) {
			messagesByType ??= new Message_Cache[(int)QuoteType.Count];
			if (messagesByType[(int)type].LastCulture?.IsActive ?? false) {
				return Main.rand.Next(messagesByType[(int)type].Cache);
			} else {
				List<string> cache = new();
				string quote;
				static bool TryGetText(string key, out string text) {
					if (Language.Exists(key)) {
						text = Language.GetTextValue(key);
						return true;
					}
					text = key;
					return false;
				}
				if (TryGetText($"Mods.Origins.Dialogue.ProtOS.{type}", out quote)) {
					cache.Add(quote);
				} else {
					int count = 0;
					while (TryGetText($"Mods.Origins.Dialogue.ProtOS.{type}_{count}", out quote)) {
						cache.Add(quote);
						count++;
					}
				}
				if (!Main.expertMode) {
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
				if (!Main.masterMode) {
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
				messagesByType[(int)type] = new Message_Cache(cache.ToArray(), LanguageManager.Instance.ActiveCulture);
				return Main.rand.Next(cache);
			}
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
					cooldowns[(int)type] = Main.rand.Next(1200, 1801);
					break;
					case QuoteType.Death:
					cooldowns[(int)type] = 0;
					break;
					case QuoteType.Bird:
					cooldowns[(int)type] = Main.rand.Next(200, 481);
					break;
					case QuoteType.Combat:
					cooldowns[(int)type] = Main.rand.Next(240, 361);
					break;
					case QuoteType.The_Part_Where_He_Kills_You:
					cooldowns[(int)type] = Main.rand.Next(600, 901);
					break;
					case QuoteType.InHell:
					cooldowns[(int)type] = Main.rand.Next(1200, 1801);
					break;
				}
				cooldowns[(int)QuoteType.Idle] = Main.rand.Next(1800, 3601);
			}
		}
		public static void PlayMessage(string text, Vector2 position, Vector2? velocity = null) {
			PopupText.NewText(new AdvancedPopupRequest() {
				Text = text,
				DurationInFrames = 30 + text.Length * 6,
				Velocity = velocity ?? new Vector2(Main.rand.NextFloatDirection() * 7f, -2f + Main.rand.NextFloat() * -2f),
				Color = new Color(242, 250, 255)
			}, position);
			SoundEngine.PlaySound(SoundID.LucyTheAxeTalk, position);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.BrainOfConfusion);
			recipe.AddIngredient(ModContent.ItemType<Potato_Battery>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.AddOnCraftCallback((_, _, _, _) => {
				PlayRandomMessage(
					QuoteType.Craft,
					Main.LocalPlayer.GetModPlayer<OriginPlayer>().protOSQuoteCooldown,
					Main.LocalPlayer.Top
				);
			});
			recipe.Register();
		}
		public enum QuoteType {
			Pickup,
			Death,
			Bird,
			Combat,
			The_Part_Where_He_Kills_You,
			Craft,
			Respawn,
			Falling,
			Gravitation,//←TODO
			Idle,
			CompanionCube,//←TODO
			InHell,
			ItemIsBad,//←TODO
			ItemIsExplosive,//←TODO
			SmashingEvilTile,//←TODO
			KillVillager,//←TODO
			BoundNPCAppearedThrice,//←TODO

			Count
		}
		struct Message_Cache {
			public string[] Cache { get; init; }
			public GameCulture LastCulture { get; init; }
			public Message_Cache(string[] cache, GameCulture lastCulture) {
				Cache = cache;
				LastCulture = lastCulture;
			}
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
