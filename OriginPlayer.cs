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
using Terraria.ModLoader;
using static Origins.Items.OriginGlobalItem;

namespace Origins {
    public class OriginPlayer : ModPlayer {
        public bool Fiberglass_Set = false;
        public bool Cryosten_Set = false;
        public bool Cryosten_Helmet = false;
        public float Explosive_Damage = 1;
        public bool Miner_Set = false;
        public bool DrawShirt = false;
        public bool DrawPants = false;
        public bool ZoneVoid = false;
        public int cryostenLifeRegenCount = 0;
        public override void ResetEffects() {
            DrawShirt = false;
            DrawPants = false;
            Fiberglass_Set = false;
            Cryosten_Set = false;
            Cryosten_Helmet = false;
            Miner_Set = false;
            Explosive_Damage = 1f;
            if(cryostenLifeRegenCount>0)cryostenLifeRegenCount--;
        }
        public override void PostUpdateMiscEffects() {
			if (Cryosten_Helmet){
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
			if (Cryosten_Helmet)player.lifeRegenCount+=cryostenLifeRegenCount>0?180:1;
        }
        public override void ModifyWeaponDamage(Item item, ref float add, ref float mult, ref float flat) {
            if(IsExplosive(item))add+=Explosive_Damage-1;
            if(Fiberglass_Set) {
                flat+=4;
            }
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(Origins.ExplosiveModOnHit.Contains(proj.type)) {
                damage = (int)(damage*(player.allDamage+Explosive_Damage-1)*0.7f);
            }
            if(Origins.ExplosiveProjectiles[proj.type]) {
                damage+=target.defense/10;
            }
            if(Fiberglass_Set) {
                damage+=4;
            }
        }
		public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit) {
            if(Miner_Set)if(proj.owner == player.whoAmI) {
                damage = (int)(damage/Explosive_Damage);
                damage-=damage/5;
            }
        }
		public override void UpdateBiomes() {
			ZoneVoid = OriginWorld.voidTiles > 200;
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
    }
}
