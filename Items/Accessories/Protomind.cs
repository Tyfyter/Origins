using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Protomind : Potato_Battery {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Protomind");
			Tooltip.SetDefault("Has a chance to create illusions and dodge an attack\nTemporarily increase critical chance after a dodge\nMay confuse nearby enemies after being struck\nMagic projectiles slightly home towards targets");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.accessory = true;
			Item.rare = ItemRarityID.LightPurple;
			Item.value = Item.sellPrice(gold: 3);
		}
		public override void UpdateEquip(Player player) {
			player.BrainOfConfusionDodge();
			player.brainOfConfusionDodgeAnimationCounter++;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.potatoBattery = true;
			//originPlayer.hasProtOS = true;
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
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.BrainOfConfusion);
			recipe.AddIngredient(ModContent.ItemType<Potato_Battery>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
