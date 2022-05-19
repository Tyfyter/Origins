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
    public class Toxic_Slash : ModItem, IElementalItem {
        public ushort Element => Elements.Acid;
		public override bool OnlyShootOnSwing => true;
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Toxic Slash");
			Tooltip.SetDefault("");
			glowmask = Origins.AddGlowMask("Weapons/Acid/Toxic_Slash_Glow");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.TrueExcalibur);
			item.damage = 45;
			item.melee = true;
			item.autoReuse = true;
            item.useStyle = 1;
			item.width = 28;
			item.height = 30;
			item.useTime = 27;
			item.useAnimation = 28;
			item.value = 5000;
            item.shoot = ModContent.ProjectileType<Gooey_Exaultion_P>();
			item.rare = ItemRarityID.Lime;
			item.glowMask = glowmask;
		}
		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
			target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.duration);
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			damage -= damage / 3;
			return Main.rand.NextBool();
        }
    }
}
