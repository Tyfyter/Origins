using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Projectiles.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Acid {
    public class The_Foot : ModItem, IElementalItem {
        public ushort Element => Elements.Acid;
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("The Foot");
			Tooltip.SetDefault("");
			glowmask = Origins.AddGlowMask(this);
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.TrueExcalibur);
			Item.damage = 90;
			Item.DamageType = DamageClass.Melee;
			Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Swing;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.value = 5000;
            Item.shoot = ProjectileID.None;
			Item.rare = ItemRarityID.Lime;
			Item.glowMask = glowmask;
		}
		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
			target.AddBuff(Toxic_Shock_Debuff.ID, 180);
			target.AddBuff(BuffID.Venom, 600);
			target.AddBuff(BuffID.Bleeding, 600);
			target.AddBuff(BuffID.CursedInferno, 600);
			target.AddBuff(BuffID.Ichor, 600);
		}
    }
}
