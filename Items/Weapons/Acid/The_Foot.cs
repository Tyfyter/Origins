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
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.TrueExcalibur);
			item.damage = 90;
			item.melee = true;
			item.autoReuse = true;
            item.useStyle = 1;
			item.width = 28;
			item.height = 30;
			item.useTime = 32;
			item.useAnimation = 32;
			item.value = 5000;
            item.shoot = ProjectileID.None;
			item.rare = ItemRarityID.Lime;
			item.glowMask = glowmask;
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
