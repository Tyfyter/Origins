using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using static Origins.Items.OriginGlobalItem;
using Terraria.ID;
using Origins.Projectiles;

namespace Origins {
    public class OriginPlayer : ModPlayer {
        public bool fiberglassSet = false;
        public bool cryostenSet = false;
        public bool cryostenHelmet = false;
        public bool felnumSet = false;
        public float felnumShock = 0;
        public float oldFelnumShock = 0;
        //public const int FelnumMax = 100;
        public float explosiveDamage = 1;
        public bool minerSet = false;
        public bool ZoneVoid = false;
        public bool DrawShirt = false;
        public bool DrawPants = false;
        public bool ItemLayerWrench = false;
        internal static bool ItemChecking = false;
        public int cryostenLifeRegenCount = 0;
        public override void ResetEffects() {
            DrawShirt = false;
            DrawPants = false;
            fiberglassSet = false;
            cryostenSet = false;
            cryostenHelmet = false;
            oldFelnumShock = felnumShock;
            if(!felnumSet) {
                felnumShock = 0;
            } else if (felnumShock>player.statLifeMax2){
                felnumShock-=(felnumShock-player.statLifeMax2)/player.statLifeMax2*5+1;
            }
            felnumSet = false;
            minerSet = false;
            explosiveDamage = 1f;
            if(cryostenLifeRegenCount>0)cryostenLifeRegenCount--;
        }
        public override void PostUpdateMiscEffects() {
			if (cryostenHelmet){
				if(player.statLife!=player.statLifeMax2&&(int)Main.time%(cryostenLifeRegenCount>0?5:15)==0)for (int i = 0; i < 10; i++){
					int num6 = Dust.NewDust(player.position, player.width, player.height, 92);
					Main.dust[num6].noGravity = true;
					Main.dust[num6].velocity *= 0.75f;
					int num7 = Main.rand.Next(-40, 41);
					int num8 = Main.rand.Next(-40, 41);
					Main.dust[num6].position.X += num7;
					Main.dust[num6].position.Y += num8;
					Main.dust[num6].velocity.X = -num7 * 0.075f;
					Main.dust[num6].velocity.Y = -num8 * 0.075f;
				}
			}
        }
        public override void UpdateLifeRegen() {
			if (cryostenHelmet)player.lifeRegenCount+=cryostenLifeRegenCount>0?180:1;
        }
        public override void ModifyWeaponDamage(Item item, ref float add, ref float mult, ref float flat) {
            if(IsExplosive(item))add+=explosiveDamage-1;
            if(fiberglassSet) {
                flat+=4;
            }
        }
        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit) {
            if(felnumShock>19) {
                damage+=(int)(felnumShock/15);
                felnumShock = 0;
				Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 122, 2f, 1f);
            }
        }
        public override bool Shoot(Item item, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if(item.shoot>ProjectileID.None&&felnumShock>19) {
                Projectile p = new Projectile();
                p.SetDefaults(item.shoot);
                OriginGlobalProj.felnumEffectNext = true;
                if(p.melee)return true;
                damage+=(int)(felnumShock/15);
                felnumShock = 0;
				Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 122, 2f, 1f);
            }
            return true;
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(Origins.ExplosiveModOnHit.Contains(proj.type)) {
                damage = (int)(damage*(player.allDamage+explosiveDamage-1)*0.7f);
            }
            if(Origins.ExplosiveProjectiles[proj.type]) {
                damage+=target.defense/10;
            }
            if(fiberglassSet) {
                damage+=4;
            }
            if(proj.melee&&felnumShock>19) {
                damage+=(int)(felnumShock/15);
                felnumShock = 0;
				Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 122, 2f, 1f);
            }
        }
		public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit) {
            if(minerSet)if(proj.owner == player.whoAmI && proj.friendly) {
                damage = (int)(damage/explosiveDamage);
                damage-=damage/5;
            }
        }
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
            return damage != 0;
        }
        public override void UpdateBiomes() {
			ZoneVoid = OriginWorld.voidTiles > 200;
		}
        public override bool PreItemCheck() {
            ItemChecking = true;
            return true;
        }
        public override void PostItemCheck() {
            ItemChecking = false;
        }
        public override void ModifyDrawLayers(List<PlayerLayer> layers) {
            if(DrawShirt) {
                int itemindex = layers.IndexOf(PlayerLayer.HeldItem);
                PlayerLayer itemlayer = layers[itemindex];
                layers.RemoveAt(itemindex);
                layers.Insert(layers.IndexOf(PlayerLayer.MountFront), itemlayer);
                layers.Insert(layers.IndexOf(PlayerLayer.MountFront), PlayerShirt);
                PlayerShirt.visible = true;
            }
            if(DrawPants) {
                layers.Insert(layers.IndexOf(PlayerLayer.Legs), PlayerPants);
                PlayerPants.visible = true;
            }
            if(felnumShock>0) {
                layers.Add(FelnumGlow);
                FelnumGlow.visible = true;
            }
            if(ItemLayerWrench && !player.HeldItem.noUseGraphic) {
                layers[layers.IndexOf(PlayerLayer.HeldItem)] = FiberglassBowLayer;
                FiberglassBowLayer.visible = true;
            }
            ItemLayerWrench = false;
        }
        public static PlayerLayer PlayerShirt = new PlayerLayer("Origins", "PlayerShirt", null, delegate(PlayerDrawInfo drawInfo2){
            Player drawPlayer = drawInfo2.drawPlayer;
	        Vector2 Position = drawInfo2.position;
            SpriteEffects spriteEffects = drawInfo2.spriteEffects;
	        int skinVariant = drawPlayer.skinVariant;
            DrawData drawData;
			drawData = new DrawData(Main.playerTextures[skinVariant, 14], new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.legFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.legFrame.Height + 4f))) + drawPlayer.legPosition + drawInfo2.legOrigin, new Rectangle?(drawPlayer.legFrame), drawInfo2.shirtColor, drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
			Main.playerDrawData.Add(drawData);
			if (!drawPlayer.Male){
				drawData = new DrawData(Main.playerTextures[skinVariant, 4], new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.underShirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
				Main.playerDrawData.Add(drawData);
				drawData = new DrawData(Main.playerTextures[skinVariant, 6], new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.shirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
				Main.playerDrawData.Add(drawData);
			}else{
				drawData = new DrawData(Main.playerTextures[skinVariant, 4], new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.underShirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
				Main.playerDrawData.Add(drawData);
				drawData = new DrawData(Main.playerTextures[skinVariant, 6], new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.shirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
				Main.playerDrawData.Add(drawData);
			}
			drawData = new DrawData(Main.playerTextures[skinVariant, 5], new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.bodyColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
			Main.playerDrawData.Add(drawData);
        });
        public static PlayerLayer PlayerPants = new PlayerLayer("Origins", "PlayerPants", null, delegate(PlayerDrawInfo drawInfo2){
            Player drawPlayer = drawInfo2.drawPlayer;
	        Vector2 Position = drawInfo2.position;
            SpriteEffects spriteEffects = drawInfo2.spriteEffects;
	        int skinVariant = drawPlayer.skinVariant;
            DrawData drawData;
			drawData = new DrawData(Main.playerTextures[skinVariant, 11], new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.legFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.legFrame.Height + 4f))) + drawPlayer.legPosition + drawInfo2.legOrigin, new Rectangle?(drawPlayer.legFrame), drawInfo2.pantsColor, drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
			Main.playerDrawData.Add(drawData);
			drawData = new DrawData(Main.playerTextures[skinVariant, 12], new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.legFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.legFrame.Height + 4f))) + drawPlayer.legPosition + drawInfo2.legOrigin, new Rectangle?(drawPlayer.legFrame), drawInfo2.shoeColor, drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
			Main.playerDrawData.Add(drawData);
		    //drawData = new DrawData(Main.playerTextures[skinVariant, 7], new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.bodyColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
		    //Main.playerDrawData.Add(drawData);
		    //drawData = new DrawData(Main.playerTextures[skinVariant, 8], new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.underShirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
		    //Main.playerDrawData.Add(drawData);
		    //drawData = new DrawData(Main.playerTextures[skinVariant, 13], new Vector2((float)((int)(Position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.shirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
		    //Main.playerDrawData.Add(drawData);
        });
        public static PlayerLayer FiberglassBowLayer = new PlayerLayer("Origins", "FiberglassBowLayer", null, delegate(PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            float num77 = drawPlayer.itemRotation + 0.785f * (float)drawPlayer.direction;
            Item item = drawPlayer.inventory[drawPlayer.selectedItem];
            Texture2D itemTexture = Main.itemTexture[item.type];
            IAnimatedItem aItem = (IAnimatedItem)item.modItem;
			int num80 = 10;
			Vector2 vector7 = new Vector2(itemTexture.Width / 2, itemTexture.Height / 2);
            Vector2 vector8 = OriginExtensions.DrawPlayerItemPos(drawPlayer.gravDir, item.type);
			num80 = (int)vector8.X;
			vector7.Y = vector8.Y;
			Vector2 origin4 = new Vector2(-num80, itemTexture.Height / 2);
			if (drawPlayer.direction == -1) {
				origin4 = new Vector2(itemTexture.Width + num80, itemTexture.Height / 2);
			}
            origin4.X-=drawPlayer.width/2;
            Vector4 col = drawInfo2.faceColor.ToVector4()/drawPlayer.skinColor.ToVector4();
			DrawData value = new DrawData(itemTexture, new Vector2((int)(drawInfo2.itemLocation.X - Main.screenPosition.X + vector7.X), (int)(drawInfo2.itemLocation.Y - Main.screenPosition.Y + vector7.Y)), aItem.Animation.GetFrame(itemTexture), item.GetAlpha(new Color(col.X,col.Y,col.Z,col.W)), drawPlayer.itemRotation, origin4, item.scale, drawInfo2.spriteEffects, 0);
			Main.playerDrawData.Add(value);
		});
        public static PlayerLayer FelnumGlow = new PlayerLayer("Origins", "FelnumGlow", null, delegate(PlayerDrawInfo drawInfo2){
            Player drawPlayer = drawInfo2.drawPlayer;
            Vector2 Position;
            Rectangle? Frame;
            Texture2D Texture;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (drawPlayer.direction == -1){
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if (drawPlayer.gravDir == -1f){
                spriteEffects |= SpriteEffects.FlipVertically;
            }
            DrawData item;
            int a = (int)Math.Max(Math.Min((drawPlayer.GetModPlayer<OriginPlayer>().felnumShock*255)/drawPlayer.statLifeMax2, 255), 1);
            if(drawPlayer.head == Origins.FelnumHeadArmorID) {
                Position = new Vector2((float)((int)(drawInfo2.position.X - Main.screenPosition.X - (float)drawPlayer.bodyFrame.Width / 2f + (float)drawPlayer.width / 2f)), (float)((int)(drawInfo2.position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.headPosition + drawInfo2.headOrigin;
                Frame = new Rectangle?(drawPlayer.bodyFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Head");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.headRotation, drawInfo2.headOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
                Main.playerDrawData.Add(item);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Eye");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.headRotation, drawInfo2.headOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
                Main.playerDrawData.Add(item);
            } else if(drawInfo2.drawHair||drawInfo2.drawAltHair||drawPlayer.head == ArmorIDs.Head.FamiliarWig) {
                Position = new Vector2((float)((int)(drawInfo2.position.X - Main.screenPosition.X - (float)drawPlayer.bodyFrame.Width / 2f + (float)drawPlayer.width / 2f)), (float)((int)(drawInfo2.position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.headPosition + drawInfo2.headOrigin;
                Frame = new Rectangle?(drawPlayer.bodyFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Eye");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.headRotation, drawInfo2.headOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
                Main.playerDrawData.Add(item);
            }
            if(drawPlayer.body == Origins.FelnumBodyArmorID) {
                Position = new Vector2((float)((int)(drawInfo2.position.X - Main.screenPosition.X - (float)drawPlayer.bodyFrame.Width / 2f + (float)drawPlayer.width / 2f)), (float)((int)(drawInfo2.position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + drawInfo2.bodyOrigin;
                Frame = new Rectangle?(drawPlayer.bodyFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Arm");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
                Main.playerDrawData.Add(item);

                Position = new Vector2((float)((int)(drawInfo2.position.X - Main.screenPosition.X - (float)drawPlayer.bodyFrame.Width / 2f + (float)drawPlayer.width / 2f)), (float)((int)(drawInfo2.position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + drawInfo2.bodyOrigin;
                Frame = new Rectangle?(drawPlayer.bodyFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Body");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
                Main.playerDrawData.Add(item);
            }
            if(drawPlayer.legs == Origins.FelnumLegsArmorID) {
                Position = new Vector2((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo2.legOrigin;
                Frame = new Rectangle?(drawPlayer.legFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Leg");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[2].type);
                Main.playerDrawData.Add(item);
            }
        });
    }
}
