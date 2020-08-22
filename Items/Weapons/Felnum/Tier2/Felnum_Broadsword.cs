using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.World;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Items.Weapons.Felnum.Tier2 {
    //this took seven and a half hours to make
	public class Felnum_Broadsword : ModItem, IAnimatedItem {
        public override bool CloneNewInstances => true;
        internal static DrawAnimationManual animation;
        public DrawAnimation Animation {
            get {
                animation.Frame = frame;
                return animation;
            }
        }
        public int charge = 0;
        internal int frame = 5;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Broadsword");
			Tooltip.SetDefault("Behold\nHold right click to stab");
            animation = new DrawAnimationManual(6);
            animation.Frame = 5;
			Main.RegisterItemAnimation(item.type, animation);
		}
		public override void SetDefaults() {
			item.damage = 58;
			item.melee = true;
			item.width = 42;
			item.height = 42;
			item.useTime = 16;
			item.useAnimation = 16;
			item.useStyle = 1;
			item.knockBack = 9;
			item.value = 5000;
			item.autoReuse = true;
            item.useTurn = true;
			item.rare = ItemRarityID.Lime;
			item.UseSound = SoundID.Item1;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 12);
            recipe.AddIngredient(ItemID.OrichalcumBar, 7);
            recipe.SetResult(this);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.AddRecipe();
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 12);
            recipe.AddIngredient(ItemID.MythrilBar, 7);
            recipe.SetResult(this);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.AddRecipe();
        }

        public override bool AltFunctionUse(Player player) {
            return true;
        }

        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse == 2) {
			    item.useTime = 1;
			    item.useAnimation = 16;
			    item.useStyle = 5;
			    item.knockBack = 19;
                item.shoot = ModContent.ProjectileType<Felnum_Broadsword_Stab>();
                item.shootSpeed = 3.4f;
                item.noUseGraphic = true;
                item.noMelee = true;
			    item.UseSound = null;
            } else {
			    item.useTime = 16;
			    item.useAnimation = 16;
			    item.useStyle = 1;
			    item.knockBack = 9;
                item.shoot = 0;
                item.shootSpeed = 0;
                item.noUseGraphic = false;
                item.noMelee = false;
			    item.UseSound = SoundID.Item1;
            }
            return base.CanUseItem(player);
        }

        public override void HoldItem(Player player) {
            if(player.itemAnimation!=0 && player.altFunctionUse != 2) {
                player.GetModPlayer<OriginPlayer>().ItemLayerWrench = true;
            }
        }

        public override void Load(TagCompound tag) {
            frame = tag.GetInt("frame");
        }
        public override TagCompound Save() {
            return new TagCompound() {{"frame", frame}};
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
            OriginExtensions.FixedUseItemHitbox(item, player, ref hitbox, ref noHitbox);
            if(frame==5/*!ModContent.GetInstance<OriginWorld>().felnumBroadswordStab*/) {
                hitbox = new Rectangle(0,0,0,0);
                //if(animation.Frame==0) ModContent.GetInstance<OriginWorld>().felnumBroadswordStab = true;
            }
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
            if(player.controlUseTile&&(charge>=15||player.CheckMana(4, true))){
                player.itemAnimation = 5;
                if(charge<15){
                    if(++charge>=15)for(int i = 0; i < 3; i++){
                        int a = Dust.NewDust(position-new Vector2(speedX,speedY), 0, 0, 92);
                        Main.dust[a].noGravity = true;
                    }
                }else if(Main.GameUpdateCount%12<=1){
                    int a = Dust.NewDust(position-new Vector2(speedX,speedY), 0, 0, 92);
                    Main.dust[a].noGravity = true;
                }
                return false;
            }
            if(charge>=15) {
                Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, ai1:animation.Frame>0?0:-1);
                charge = 0;
                player.itemAnimation = 16;
                player.itemAnimationMax = 16;
                if(frame==5) {
                    Main.PlaySound(2, (int)position.X, (int)position.Y, 122, 0.5f, 1f);
                }
            }
            return false;
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            Texture2D texture = Main.itemTexture[item.type];
            spriteBatch.Draw(texture, position, Animation.GetFrame(texture), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
            damage+=(damage-21)/2;
        }
        public override void GetWeaponDamage(Player player, ref int damage) {
            if(!OriginPlayer.ItemChecking)damage+=(damage-21)/2;
        }
    }
    public class Felnum_Broadsword_Stab : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Felnum/Tier2/Felnum_Broadsword_B";
        public override bool CloneNewInstances => true;
        int stabee = -1;
        bool noGrow = false;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Broadsword");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Spear);
            projectile.timeLeft = 16;
			projectile.width = 32;
			projectile.height = 32;
        }
        public float movementFactor{
			get => projectile.ai[0];
			set => projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			projectile.direction = projOwner.direction;
			projOwner.heldProj = projectile.whoAmI;
			projOwner.itemTime = projOwner.itemAnimation;
			projectile.position.X = ownerMountedCenter.X - (projectile.width / 2);
			projectile.position.Y = ownerMountedCenter.Y - (projectile.height / 2);
			if (!projOwner.frozen) {
				if (movementFactor == 0f){
                    movementFactor = 4.7f;
                    if(projectile.ai[1]==-1)noGrow = true;
                    projectile.ai[1] = 4;
					projectile.netUpdate = true;
				}
				if (projOwner.itemAnimation < 3){
					movementFactor-=1.7f;
				} else if (projectile.ai[1]>0){
					movementFactor+=1.3f;
                    projectile.ai[1]--;
                }
			}
			projectile.position += projectile.velocity * movementFactor;
			if (projOwner.itemAnimation == 0) {
				projectile.Kill();
			}
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
            if (projectile.spriteDirection == 1) {
				projectile.rotation -= MathHelper.Pi/2f;
			}
            if(stabee >= 0) {
                if(!Main.npc[stabee].active) {
                    stabee = -2;
                    return;
                }
                NPC victim = Main.npc[stabee];
                victim.AddBuff(ModContent.BuffType<ImpaledBuff>(), 1);
                victim.position+=projectile.position-projectile.oldPosition;
                victim.Center = Vector2.Lerp(victim.Center,projectile.Center+projectile.velocity, 0.035f);
                victim.oldPosition = victim.position;
            }
		}
        public override bool? CanHitNPC(NPC target) {
			Player player = Main.player[projectile.owner];
            if(stabee >= 0) {
                return target.whoAmI == stabee && player.itemAnimation == 3;
            }
            return base.CanHitNPC(target);
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(stabee == -1) knockback = 0;
            else crit = true;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(stabee>=0)return;
            if(target.boss || target.type == NPCID.TargetDummy || stabee == -2) {
                stabee = -2;
                return;
            }
            Player player = Main.player[projectile.owner];
            player.itemAnimation+=player.itemAnimationMax;
            player.itemAnimationMax*=2;
            projectile.timeLeft = player.itemAnimation;
            stabee = target.whoAmI;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            if(noGrow) {
                spriteBatch.Draw(mod.GetTexture("Items/Weapons/Felnum/Tier2/Felnum_Broadsword_B"), (projectile.Center - projectile.velocity*2) - Main.screenPosition, new Rectangle(0, 0, 40, 40), lightColor, projectile.rotation, new Vector2(20, 20), 1f, SpriteEffects.None, 0f);
                return false;
            }
            Texture2D texture = mod.GetTexture("Items/Weapons/Felnum/Tier2/Felnum_Broadsword");
            if(Main.player[projectile.owner].HeldItem.modItem is Felnum_Broadsword sword) {
                Rectangle frame = sword.Animation.GetFrame(texture);
                if(sword.frame>0)sword.frame--;
                spriteBatch.Draw(texture, (projectile.Center - projectile.velocity*2) - Main.screenPosition, frame, lightColor, projectile.rotation, new Vector2(20, 20), 1f, SpriteEffects.None, 0f);
                return false;
            }
            return true;
        }
    }
    public class ImpaledBuff : ModBuff {
        public override bool Autoload(ref string name, ref string texture) {
            texture = "Terraria/Buff_160";
            return true;
        }
        public override void SetDefaults() {
            DisplayName.SetDefault("Impaled");
        }
    }
}
