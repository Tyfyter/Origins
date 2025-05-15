using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.Graphics.Shaders;
using Origins.Items.Other;
namespace Origins.Items.Mounts {
	public class Chambersite_Minecart_Item : ModItem, ICustomWikiStat {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
        }
        public override void SetDefaults() {
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 1);
			Item.width = 34;
			Item.height = 22;
			Item.mountType = ModContent.MountType<Chambersite_Minecart>();
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Minecart)
			.AddIngredient<Large_Chambersite>()
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Chambersite_Minecart : ModMount {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			MountID.Sets.Cart[Type] = true;
			MountID.Sets.FacePlayersVelocity[Type] = true;

			// Helper method setting many common properties for a minecart
			Mount.SetAsMinecart(
				MountData,
				ModContent.BuffType<Chambersite_Minecart_Buff>(),
				MountData.frontTexture
			);

			// Change properties on MountData here further, for example:
			MountData.spawnDust = 21;

			// Note that runSpeed, dashSpeed, acceleration, jumpHeight, and jumpSpeed will be overridden when the player has used the Minecart Upgrade Kit.
			// To customize the Minecart Upgrade Kit stats, assign values to the MinecartUpgradeX fields:
			MountData.MinecartUpgradeRunSpeed = 40f;
			MountData.MinecartUpgradeDashSpeed = 40f;
			MountData.MinecartUpgradeAcceleration = 0.2f;
			ID = Type;
		}
		public override void UpdateEffects(Player player) {
			if (Main.rand.NextBool(10)) {
				Vector2 randomOffset = Main.rand.NextVector2Square(-1f, 1f) * new Vector2(22f, 10f);
				Vector2 directionOffset = new Vector2(0f, 10f) * player.Directions;
				Vector2 position = player.Center + directionOffset + randomOffset;
				position = player.RotatedRelativePoint(position);
				Dust dust = Dust.NewDustPerfect(position, DustID.GemEmerald);
				dust.noGravity = true;
				dust.fadeIn = 0.6f;
				dust.scale = 0.4f;
				dust.velocity *= 0.25f;
				dust.shader = GameShaders.Armor.GetSecondaryShader(player.cMinecart, player);
			}
		}
	}
	public class Chambersite_Minecart_Buff : ModBuff, ICustomWikiStat {
		public override string Texture => "Origins/Buffs/Chambersite_Minecart_Buff";
		public string CustomStatPath => Name;
		public override LocalizedText DisplayName => Language.GetText("BuffName.MinecartLeft");
		public override LocalizedText Description => Language.GetText("BuffDescription.MinecartLeft");
		protected virtual int MountID => ModContent.MountType<Chambersite_Minecart>();
		public override void SetStaticDefaults() {
			BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
				mountID = MountID
			};
		}
	}
}
