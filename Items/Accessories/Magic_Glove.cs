using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Magic_Glove : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Magic Glove");
            Tooltip.SetDefault("Shoots random magic as you swing\n'May require magical capability'");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            sbyte handOnSlot = Item.handOnSlot;
            sbyte handOffSlot = Item.handOffSlot;
            Item.CloneDefaults(ItemID.YoYoGlove);
            //Item.handOffSlot = handOffSlot;
            //Item.handOnSlot = handOnSlot;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Green;

            Item.damage = 10;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.shootSpeed = 5;
            Item.mana = 1;
            Item.UseSound = SoundID.Item4;
        }
        public override void UpdateEquip(Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            originPlayer.gunGlove = true;
            originPlayer.gunGloveItem = Item;
        }
    }
}
