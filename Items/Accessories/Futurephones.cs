using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Face)]
	public class Futurephones : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
        }
        static short glowmask;
        public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Pink;
            Item.glowMask = glowmask;
        }
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().futurephones = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Headphones>()
			.AddIngredient<Snipers_Mark>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Futurephones_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
	}
}
