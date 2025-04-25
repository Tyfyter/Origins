using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Balloon)]
	public class Harpy_Horseshoe_Balloon : ModItem {
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 1, silver: 85);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BlueHorseshoeBalloon)
			.AddIngredient<Feathery_Crest>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.jumpBoost = true;
			ref ExtraJumpState jumpState = ref player.GetJumpState<Harpy_Balloon_Jump>();
			jumpState.Enable();
			player.hasLuck_LuckyHorseshoe = true;
			player.noFallDmg = true;
			if ((player.controlJump && !jumpState.Available) || jumpState.Active) {
				player.gravity = 0.15f;
			} else if (player.controlDown && player.velocity.Y != 0f) {
				player.gravity = 1.4f;
			}
		}
	}
	public class Harpy_Balloon_Jump : ExtraJump {
		public override Position GetDefaultPosition() => BeforeBottleJumps;
		public override float GetDurationMultiplier(Player player) => 0.75f;
		public override void OnStarted(Player player, ref bool playSound) {
			int feet = player.height;
			if (player.gravDir == -1f)
				feet = 0;

			for (int num23 = 0; num23 < 10; num23++) {
				Dust dust = Dust.NewDustDirect(new Vector2(player.position.X - 34f, player.position.Y + feet - 16f), 102, 32, DustID.Cloud, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 100, default, 1.5f);
				dust.velocity.X = dust.velocity.X * 0.5f - player.velocity.X * 0.1f;
				dust.velocity.Y = dust.velocity.Y * 0.5f - player.velocity.Y * 0.3f;
			}
			Vector2 startPos = new(player.position.X + ((player.width / 2) - 16f), player.position.Y + feet - 16f);
			Vector2 cloudOffset = new(20, 0);
			for (int i = -1; i <= 1; i++) {
				Gore gore = Gore.NewGoreDirect(null,
					startPos + cloudOffset * i,
					new Vector2(0f - player.velocity.X, 0f - player.velocity.Y),
					Main.rand.Next(11, 14)
				);
				gore.velocity.X = gore.velocity.X * 0.1f - player.velocity.X * 0.1f;
				gore.velocity.Y = gore.velocity.Y * 0.1f - player.velocity.Y * 0.05f;
				/*for (int j = 0; j < 2; j++) {
					gore = Gore.NewGoreDirect(null,
						startPos + cloudOffset * i,
						new Vector2(0f - player.velocity.X, 0f - player.velocity.Y),
						GoreID.TreeLeaf_HallowJim
					);
					gore.velocity.X = gore.velocity.X * 0.1f - player.velocity.X * 0.1f;
					gore.velocity.Y = gore.velocity.Y * 0.1f - player.velocity.Y * 0.05f + 8;
				}*/
			}
		}

		public override void ShowVisuals(Player player) {
			int feet = player.height;
			if (player.gravDir == -1f)
				feet = -6;

			Dust dust = Dust.NewDustDirect(new Vector2(player.position.X - 4f, player.position.Y + feet), player.width + 8, 4, DustID.Cloud, player.velocity.X * -0.5f, player.velocity.Y * 0.5f, 100, default, 1.5f);
			dust.velocity.X = dust.velocity.X * 0.5f - player.velocity.X * 0.1f;
			dust.velocity.Y = dust.velocity.Y * 0.5f - player.velocity.Y * 0.3f;
			if (player.jump % 3 == 0) {
				Gore gore = Gore.NewGoreDirect(null,
					new Vector2(player.position.X - ((player.width / 2) - 16f), player.position.Y + feet),
					new Vector2(0f - player.velocity.X, 0f - player.velocity.Y),
					GoreID.TreeLeaf_HallowJim
				);
				gore.velocity.X = gore.velocity.X * 0.1f - player.velocity.X * 0.1f;
				gore.velocity.Y = gore.velocity.Y * 0.1f - player.velocity.Y * 0.05f + 8;
			}
		}
	}
}
