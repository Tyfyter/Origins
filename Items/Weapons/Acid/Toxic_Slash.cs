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
            Item.CloneDefaults(ItemID.TrueExcalibur);
			Item.damage = 45;
			Item.melee = true;
			Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Swing;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 27;
			Item.useAnimation = 28;
			Item.value = 5000;
            Item.shoot = ModContent.ProjectileType<Gooey_Exaultion_P>();
			Item.rare = ItemRarityID.Lime;
			Item.glowMask = glowmask;
		}
		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
			target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.default_duration);
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			damage -= damage / 3;
			return Main.rand.NextBool();
        }
    }
}
