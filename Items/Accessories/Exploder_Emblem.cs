using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Accessories {
    public class Exploder_Emblem : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Exploder Emblem");
            Tooltip.SetDefault("+15% explosive damage");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 28;
            Item.height = 28;
        }
        public override void UpdateEquip(Player player) {
            //GetDamage returns a reference, so you can do this despite it not normally being possible to assign to a method's return value
            player.GetDamage(DamageClasses.Explosive) += 0.15f;
            //that reference is to a StatModifier, which is basically just:
            //Additive percent bonuses (use +=),
            //Multiplicative percent bonuses (use *=),
            //Base damage bonuses (assign to .Base, generally stick to addition/subtraction, applied before percent bonuses), and
            //Flat damage bonuses (assign to .Flat, generally stick to addition/subtraction, applied after percent bonuses)
            //all combined into one
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.AvengerEmblem);
            recipe.AddIngredient(Type);
            recipe.AddIngredient(ItemID.SoulofMight, 5);
            recipe.AddIngredient(ItemID.SoulofSight, 5);
            recipe.AddIngredient(ItemID.SoulofFright, 5);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
		}
	}
}
