using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Other {
    public class Syah_Nara : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Syah Nara");
            Tooltip.SetDefault("");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Katana);
            Item.damage = 23;
            Item.DamageType = DamageClass.Melee;
            Item.noUseGraphic = false;
            Item.noMelee = false;
            Item.width = 46;
            Item.height = 52;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 19;
            Item.useAnimation = 19;
            Item.knockBack = 9.5f;
            Item.value = 50000;
            Item.shoot = ProjectileID.None;
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = true;
            Item.scale = 1f;
        }
    }
    /*public class Syah_Nara : ModItem, ICustomDrawItem {
        float angle = 0f;
        bool reverse = false;
        public int Startup(Player player) => player.itemAnimationMax / 5;
        public float GetRotationMult(Player player) {
            float o = (float)Math.Pow(player.itemAnimation / (float)player.itemAnimationMax, 4);
            return reverse ? 1f-o : o;
        }
        public float GetRotationTotal(Player player) => player.itemAnimationMax * 0.2f;
        public float GetRotationCurrent(Player player) => (GetRotationMult(player)-0.5f)*GetRotationTotal(player);
		public override void SetStaticDefaults() {
		    DisplayName.SetDefault("Syah Nara");
		    Tooltip.SetDefault("");
		}
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Katana);
            item.damage = 25;
			item.melee = true;
            item.noUseGraphic = false;
            item.noMelee = false;
            item.width = 46;
            item.height = 52;
            item.useStyle = 1914;//s:19 n:14
            item.useTime = 17;
            item.useAnimation = 17;
            item.knockBack = 9.5f;
            item.value = 100000;
            item.shoot = ProjectileID.None;
			item.rare = ItemRarityID.Blue;
            item.autoReuse = true;
            item.scale = 1f;
        }
        public override bool UseItemFrame(Player player) {
            player.handon = item.handOnSlot;
            Vector2 diff = (Main.MouseWorld - player.MountedCenter);
            player.direction = diff.X < 0 ?-1:1;
            player.itemLocation = Vector2.Zero;
            int startupFrame = player.itemAnimationMax - Startup(player);
            if(player.itemAnimation>startupFrame) {
                player.bodyFrame.Y = 0;
                return true;
            }
            if(player.itemAnimation == startupFrame) {
                angle = diff.ToRotation();
                reverse = !reverse;
            }
            //float rot = (reverse ^ (player.direction == -1)) ? angle : angle-MathHelper.PiOver4;
            float a = GetRotationCurrent(player) + MathHelper.PiOver4 + angle;
            player.bodyFrame.Y = player.bodyFrame.Height * OriginExtensions.GetNearestPlayerFrame(a, player.gravDir);
            return true;
        }
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
            int startupFrame = player.itemAnimationMax-Startup(player);
            Vector2 unit = Vector2.UnitX;
            unit = unit.RotatedBy(GetRotationCurrent(player) + angle);
            //unit.X *= player.direction;
            hitbox = OriginExtensions.BoxOf(player.MountedCenter+unit*8, player.MountedCenter+unit*76*item.scale, 4);
            item.Hitbox = hitbox;
        }
        bool recursionnt = false;
        public override bool? CanHitNPC(Player player, NPC target) {
            if(recursionnt)return null;
            if(!item.Hitbox.Intersects(target.Hitbox))return false;
            recursionnt = true;
            bool cantBeHit = !target.CanBeHitBy(player, item, false);
            recursionnt = false;
            if(cantBeHit)return false;

            int totalDamage = player.GetWeaponDamage(item);

			int critChance = player.rangedCrit;
			ItemLoader.GetWeaponCrit(item, player, ref critChance);
			PlayerHooks.GetWeaponCrit(player, item, ref critChance);
			bool crit = (critChance >= 100 || Main.rand.Next(1, 101) <= critChance);

            float knockBack = item.knockBack;
			ItemLoader.GetWeaponKnockback(item, player, ref knockBack);
			PlayerHooks.GetWeaponKnockback(player, item, ref knockBack);

			int bannerID = Item.NPCtoBanner(target.BannerID());
			if (bannerID >= 0 && player.NPCBannerBuff[bannerID]){
				totalDamage = ((!Main.expertMode) ? ((int)(totalDamage * ItemID.Sets.BannerStrength[Item.BannerToItem(bannerID)].NormalDamageDealt)) : ((int)(totalDamage * ItemID.Sets.BannerStrength[Item.BannerToItem(bannerID)].ExpertDamageDealt)));
			}

			int damage = Main.DamageVar(totalDamage);
			NPCLoader.ModifyHitByItem(target, player, item, ref damage, ref knockBack, ref crit);
			PlayerHooks.ModifyHitNPC(player, item, target, ref damage, ref knockBack, ref crit);
			player.OnHit(target.Center.X, target.Center.Y, target);
			if (player.armorPenetration > 0){
				damage += target.checkArmorPenetration(player.armorPenetration);
			}

            Vector2 oldVel = target.velocity;
			int dmgDealt = (int)target.StrikeNPC(damage, knockBack, player.direction, crit);

			if (bannerID >= 0)player.lastCreatureHit = bannerID;
			if (player.beetleOffense && !target.immortal){
				player.beetleCounter += dmgDealt;
				player.beetleCountdown = 0;
			}

			target.immune[player.whoAmI] = player.itemAnimation > 15 ? 15 : player.itemAnimation;

			ItemLoader.OnHitNPC(item, player, target, dmgDealt, knockBack, crit);
			NPCLoader.OnHitByItem(target, player, item, dmgDealt, knockBack, crit);
			PlayerHooks.OnHitNPC(player, item, target, dmgDealt, knockBack, crit);

			if (Main.netMode != NetmodeID.SinglePlayer){
				if (crit){
					NetMessage.SendData(MessageID.StrikeNPC, -1, -1, null, target.whoAmI, damage, knockBack, player.direction, 1);
				}
				else
				{
					NetMessage.SendData(MessageID.StrikeNPC, -1, -1, null, target.whoAmI, damage, knockBack, player.direction);
				}
			}

			if (player.accDreamCatcher){
				player.addDPS(damage);
			}
            return false;
        }
        public void DrawInHand(Texture2D itemTexture, PlayerDrawInfo drawInfo, Vector2 itemCenter, Vector4 lightColor, Vector2 drawOrigin) {
            Player drawPlayer = drawInfo.drawPlayer;
            DrawData value;
            int startupFrame = (drawPlayer.itemAnimationMax-Startup(drawPlayer))-1;
            if(drawPlayer.itemAnimation > startupFrame)return;
            SpriteEffects spriteEffects = drawInfo.spriteEffects;
            Vector2 origin = new Vector2(4, 52);
            bool reverseRot = reverse;
            float m = 1f;
            if(drawPlayer.direction == -1) {
                spriteEffects ^= SpriteEffects.FlipHorizontally;
                //origin.X = 46 - origin.X;
                reverseRot = !reverseRot;
                m = 1.5f;
            }
            if(!reverse) {
                spriteEffects ^= SpriteEffects.FlipVertically;
                origin.Y = 52 - origin.Y;
            }
            float a = (GetRotationCurrent(drawPlayer) * m) + MathHelper.PiOver4 + angle;
            float rot = reverseRot ? 0 : -MathHelper.PiOver4;
            value = new DrawData(itemTexture, drawPlayer.MountedCenter-Main.screenPosition+new Vector2(8,0).RotatedBy(a), new Rectangle(0, 0, 46, 52), new Color(lightColor), a+rot, origin, item.scale, spriteEffects, 1);
            drawInfo.DrawDataCache.Add(value);
        }
    }*/
}
