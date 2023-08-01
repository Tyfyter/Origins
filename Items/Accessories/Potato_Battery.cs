using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Food;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Potato_Battery : ModItem {
		static Message_Type_Count[] messageCountsByType;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Potato Battery");
			// Tooltip.SetDefault("Magic projectiles slightly home towards targets\n'How are you holding up? BECAUSE I'M A POTATO'");
			Item.ResearchUnlockCount = 1;
		}
		public override void Unload() {
			messageCountsByType = null;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.accessory = true;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateInventory(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.hasPotatOS = true;
			UpdateMoonlordWarning(originPlayer.potatOSQuoteCooldown, player.Top);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.potatoBattery = true;
			originPlayer.hasPotatOS = true;
			UpdateMoonlordWarning(originPlayer.potatOSQuoteCooldown, player.Top);
		}
		static void UpdateMoonlordWarning(int[] cooldowns, Vector2 position) {
			int index = NPC.FindFirstNPC(NPCID.MoonLordHead);
			if (index != -1 && Main.npc[index].ai[0] == 1f) {
				PlayRandomMessage(
					QuoteType.The_Part_Where_He_Kills_You,
					cooldowns,
					position
				);
			}
		}
		public override bool OnPickup(Player player) {
			PlayRandomMessage(QuoteType.Pickup, player.GetModPlayer<OriginPlayer>().potatOSQuoteCooldown, player.Top);
			return true;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			int[] cooldownCounter = Main.LocalPlayer.GetModPlayer<OriginPlayer>().potatOSQuoteCooldown;
			if (cooldownCounter[(int)QuoteType.Bird] <= 0) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.active && IsBird(npc.type) && npc.DistanceSQ(Item.Center) < 160 * 160) {
						PlayRandomMessage(QuoteType.Bird, cooldownCounter, Item.Top);
					}
				}
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.CopperBar);
			recipe.AddIngredient(ModContent.ItemType<Potato>());
			recipe.AddIngredient(ModContent.ItemType<Power_Core>());
			recipe.AddTile(TileID.MythrilAnvil); //fabricator
			recipe.Register();
		}
		public static int GetVariationCount(QuoteType type) {
			messageCountsByType ??= new Message_Type_Count[(int)QuoteType.Count];
			if (messageCountsByType[(int)type].LastCulture?.IsActive??false) {
				return messageCountsByType[(int)type].Count;
			} else {
				int count = 0;
				if (Language.Exists($"Mods.Origins.Dialogue.PotatOS.{type}")) {
					count = -1;
				} else while (Language.Exists($"Mods.Origins.Dialogue.PotatOS.{type}_{count}")) {
					count++;
				}
				messageCountsByType[(int)type] = new Message_Type_Count(count, LanguageManager.Instance.ActiveCulture);
				return count;
			}
		}
		public static void PlayRandomMessage(QuoteType type, int[] cooldowns, Vector2 position, Vector2? velocity = null) {
			int variationCount = GetVariationCount(type);
			TryPlayMessage(
				type,
				cooldowns,
				position,
				null,
				variationCount == -1 ? -1 : Main.rand.Next(variationCount)
			);
		}
		public static void TryPlayMessage(QuoteType type, int[] cooldowns, Vector2 position, Vector2? velocity = null, int variation = -1) {
			if (cooldowns[(int)type] <= 0) {
				PlayMessage(
					Language.GetTextValue($"Mods.Origins.Dialogue.PotatOS.{type}{(variation == -1 ? "" : ("_" + variation))}"),
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
				}
			}
		}
		public static void PlayMessage(string text, Vector2 position, Vector2? velocity = null) {
			PopupText.NewText(new AdvancedPopupRequest() {
				Text = text,
				DurationInFrames = 120,
				Velocity = velocity ?? new Vector2(Main.rand.NextFloatDirection() * 7f, -2f + Main.rand.NextFloat() * -2f),
				Color = new Color(242, 250, 255)
			}, position);
			SoundEngine.PlaySound(SoundID.LucyTheAxeTalk, position);
		}
		public static bool IsBird(int npcType) {
			switch (npcType) {
				case NPCID.Bird:
				case NPCID.BirdBlue:
				case NPCID.BirdRed:
				case NPCID.GoldBird:
				case NPCID.Duck:
				case NPCID.Duck2:
				case NPCID.DuckWhite:
				case NPCID.DuckWhite2:
				case NPCID.Grebe:
				case NPCID.Grebe2:
				case NPCID.Owl:
				case NPCID.Penguin:
				case NPCID.PenguinBlack:
				case NPCID.CorruptPenguin:
				case NPCID.CrimsonPenguin:
				case NPCID.Seagull:
				case NPCID.Seagull2:
				return true;

				default:
				return false;
			}
		}
		public enum QuoteType {
			Pickup,
			Death,
			Bird,
			Combat,
			The_Part_Where_He_Kills_You,

			Count
		}
		struct Message_Type_Count {
			public int Count { get; init; }
			public GameCulture LastCulture { get; init; }
			public Message_Type_Count(int count, GameCulture lastCulture) {
				Count = count;
				LastCulture = lastCulture;
			}
		}
	}
}
