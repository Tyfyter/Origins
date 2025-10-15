using Origins.Dev;
using Origins.Layers;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOff)]
    public class Bug_Trapper : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Vitality
        ];
        static short glowmask;
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<HandsOff_Glow_Layer>(Item.handOffSlot, Texture + "_HandsOff_Glow");
		}
        public override void SetDefaults() {
            Item.DefaultToAccessory(38, 20);
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
            Item.glowMask = glowmask;
        }
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ItemID.Shackle)
            .AddIngredient(ModContent.ItemType<Primordial_Soup>())
            .AddTile(TileID.TinkerersWorkbench)
			.Register();
        }
        public override void UpdateAccessory(Player player, bool isHidden) {
			player.OriginPlayer().bugZapper = true;
			if (!isHidden) UpdateVanity(player);
        }
		public override void UpdateVanity(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			if (originPlayer.bugZapperFlyTime <= 0) {
				ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.PooFly, new ParticleOrchestraSettings {
					PositionInWorld = player.MountedCenter + new Vector2(12 * player.direction, 12 * player.gravDir)
				});
				originPlayer.bugZapperFlyTime = 15;
			}
		}
	}
}
