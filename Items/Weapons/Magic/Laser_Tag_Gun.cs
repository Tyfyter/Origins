using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
    public class Laser_Tag_Gun : AnimatedModItem, IElementalItem {
        static short glowmask;
        public ushort Element => Elements.Earth;
        static DrawAnimationManual animation;
        public override DrawAnimation Animation => animation;
        public override Color? GetGlowmaskTint(Player player) => Main.teamColor[player.team];
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Laser Tag Gun");
			Tooltip.SetDefault("‘Defective to some, glory to others’");
            animation = new DrawAnimationManual(1);
			Main.RegisterItemAnimation(Item.type, animation);
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.SpaceGun);
			Item.damage = 1;
			Item.DamageType = DamageClasses.RangedMagic;
			Item.noMelee = true;
            Item.crit = 46;
			Item.width = 42;
			Item.height = 14;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.mana = 10;
            Item.shoot = ModContent.ProjectileType<Laser_Tag_Laser>();
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Lime;
            Item.glowMask = glowmask;
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Space_Goo>(), 10);
            recipe.AddIngredient(ModContent.ItemType<Space_Rock>(), 20);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
        public override void UpdateInventory(Player player) {
        }

		static int GetCritMod(Player player) {
            OriginPlayer modPlayer = player.GetModPlayer<OriginPlayer>();
            int critMod = 0;
            if((modPlayer.oldBonuses&1)!=0||modPlayer.fiberglassSet||modPlayer.fiberglassDagger) {
                critMod = -50;
            }
            if((modPlayer.oldBonuses&2)!=0||modPlayer.felnumSet) {
                critMod = -64;
            }
            return critMod;
        }
        public override void ModifyWeaponCrit(Player player, ref float crit) {
            if(player.HeldItem.type != Item.type)crit+= GetCritMod(player);
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(3-(11*Main.player[Item.playerIndexTheItemIsReservedFor].direction),0);
        }
        public override void HoldItem(Player player) {
            if(player.itemAnimation!=0) {
                player.GetModPlayer<OriginPlayer>().itemLayerWrench = true;
            }
            int critMod = GetCritMod(player);
            player.GetCritChance(DamageClass.Ranged) += critMod;
            player.GetCritChance(DamageClass.Magic) += critMod;
        }
    }
    public class Laser_Tag_Laser : ModProjectile {
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.GreenLaser);
            Projectile.light = 0;
            Projectile.aiStyle = 0;
            Projectile.extraUpdates++;
			Projectile.DamageType = DamageClasses.RangedMagic;
        }
        public override void AI() {
            Projectile.rotation = Projectile.velocity.ToRotation();
            try {
                Color color = Main.teamColor[Main.player[Projectile.owner].team];
                Lighting.AddLight(Projectile.Center, Vector3.Normalize(color.ToVector3())*3);
            } catch(Exception) { }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(crit)damage*=199;
        }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit) {
            if(crit)damage*=199;
        }
        public override void OnHitPvp(Player target, int damage, bool crit) {
            target.AddBuff(BuffID.Cursed, 600);
        }
        public override bool PreDraw(ref Color lightColor) {
            Color color = Main.teamColor[Main.player[Projectile.owner].team];
            Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center-Main.screenPosition, null, color, Projectile.rotation, new Vector2(42,1), Projectile.scale, SpriteEffects.None, 1);
            return false;
        }
    }
}
