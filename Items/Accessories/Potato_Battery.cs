using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons;
using Origins.NPCs.Defiled.Boss;
using Origins.Tiles.Other;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Potato_Battery : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.MagicBoostAcc
		];
		static Message_Type_Count[] messageCountsByType;
		
		public override void Unload() {
			messageCountsByType = null;
		}
		public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
			OriginsSets.Items.InfoAccessorySlots_IsAMechanicalAccessory[Type] = false;
		}
        static short glowmask;
        public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.accessory = true;
			Item.rare = ItemRarityID.Pink;
			Item.ammo = ModContent.ItemType<Potato>();
			Item.shoot = ModContent.ProjectileType<Potato_Battery_P>();
			Item.value = Item.sellPrice(gold: 1);
            Item.glowMask = glowmask;
			Item.notAmmo = true;
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
			if (index != -1 && Main.npc[index].ai[0] == 1f) { //charging up deathray
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
			Recipe.Create(Type)
			.AddIngredient(ItemID.CopperBar)
			.AddIngredient(ModContent.ItemType<Potato>())
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
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
			SoundEngine.PlaySound(SoundID.NPCHit34.WithPitch(-1f), position);
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
				return OriginsModIntegrations.CheckAprilFools() && npcType == ModContent.NPCType<Defiled_Amalgamation>();
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

		readonly struct Message_Type_Count(int count, GameCulture lastCulture) {
			public int Count { get; init; } = count;
			public GameCulture LastCulture { get; init; } = lastCulture;
		}
	}
}
