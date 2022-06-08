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
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Acid {
    public class Toxic_Slash : ModItem, IElementalItem {
        public ushort Element => Elements.Acid;
		//public override bool OnlyShootOnSwing => true;
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Toxic Slash");
			Tooltip.SetDefault("");
			glowmask = Origins.AddGlowMask("Weapons/Acid/Toxic_Slash_Glow");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.TrueExcalibur);
			Item.damage = 45;
			Item.DamageType = DamageClass.Melee;
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
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			damage -= damage / 3;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			return Main.rand.NextBool();
		}
    }
}
