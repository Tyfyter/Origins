using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Accessories.Eyndum_Cores;
using Origins.Items.Armor.Vanity.Terlet.PlagueTexan;
using Origins.Items.Materials;
using Origins.Items.Other.Testing;
using Origins.Items.Weapons.Explosives;
using Origins.Items.Weapons.Summon;
using Origins.Projectiles;
using Origins.Projectiles.Misc;
using Origins.World;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Origins.Items.OriginGlobalItem;
using static Origins.OriginExtensions;

namespace Origins {
    public class OriginPlayer : ModPlayer {
        public const float rivenMaxMult = 0.3f;
        public float rivenMult => (1f-rivenMaxMult)+Math.Max((player.statLife/(float)player.statLifeMax2)*(rivenMaxMult*2), rivenMaxMult);
        
        #region set bonuses
        public bool fiberglassSet = false;
        public bool cryostenSet = false;
        public bool cryostenHelmet = false;
        public bool felnumSet = false;
        public float felnumShock = 0;
        public float oldFelnumShock = 0;
        public bool celestineSet = false;
        public bool minerSet = false;
        public bool defiledSet = false;
        public bool reshapingChunk = false;
        public bool rivenSet = false;
        public bool riftSet = false;
        public bool eyndumSet = false;
        #endregion set bonuses

        #region accessories
        public bool bombHandlingDevice = false;
        public bool dimStarlight = false;
        public byte dimStarlightCooldown = 0;
        public bool madHand = false;
        public bool fiberglassDagger = false;
        public bool advancedImaging = false;
        public bool rasterize = false;
        #endregion

        #region explosive stats
        public float explosiveDamage = 1;
        public int explosiveCrit = 4;
        public float explosiveThrowSpeed = 1;
        public float explosiveSelfDamage = 1;
        #endregion

        #region biomes
        public bool ZoneVoid = false;
        public float ZoneVoidProgress = 0;
        public float ZoneVoidProgressSmoothed = 0;

        public bool ZoneDefiled = false;
        public float ZoneDefiledProgress = 0;
        public float ZoneDefiledProgressSmoothed = 0;

        public bool ZoneRiven = false;
        public float ZoneRivenProgress = 0;
        public float ZoneRivenProgressSmoothed = 0;

        public bool ZoneBrine = false;
        public float ZoneBrineProgress = 0;
        public float ZoneBrineProgressSmoothed = 0;
        #endregion

        #region buffs
        public int rapidSpawnFrames = 0;
        public int rasterizedTime = 0;
        #endregion

        public bool DrawShirt = false;
        public bool DrawPants = false;
        public bool ItemLayerWrench = false;
        public bool PlagueSight = false;

        public Ref<Item> eyndumCore = null;

        internal static bool ItemChecking = false;
        public int cryostenLifeRegenCount = 0;
        internal byte oldBonuses = 0;
        public const int minionSubSlotValues = 3;
        public float[] minionSubSlots = new float[minionSubSlotValues];
        public int wormHeadIndex = -1;
        public int heldProjectile = -1;
        public override void ResetEffects() {
            oldBonuses = 0;
            if(fiberglassSet||fiberglassDagger)oldBonuses|=1;
            if(felnumSet)oldBonuses|=2;
            if(!player.frozen) {
                DrawShirt = false;
                DrawPants = false;
            }
            fiberglassSet = false;
            cryostenSet = false;
            cryostenHelmet = false;
            oldFelnumShock = felnumShock;
            rasterize = false;
            if(!felnumSet) {
                felnumShock = 0;
            } else {
                if(felnumShock > player.statLifeMax2) {
                    if(Main.rand.Next(20)==0) {
                        Vector2 pos = new Vector2(Main.rand.Next(4, player.width-4), Main.rand.Next(4, player.height-4));
                        Projectile proj = Projectile.NewProjectileDirect(player.position + pos, Main.rand.NextVector2CircularEdge(3,3), Felnum_Shock_Leader.ID, (int)(felnumShock*0.1f), 0, player.whoAmI, pos.X, pos.Y);
                        if(proj.modProjectile is Felnum_Shock_Leader shock) {
                            shock.Parent = player;
                            shock.OnStrike += () => felnumShock *= 0.9f;
                        }
                    }
                    felnumShock -= (felnumShock - player.statLifeMax2) / player.statLifeMax2 * 5 + 1;
                }
            }
            felnumSet = false;
            celestineSet = false;
            minerSet = false;
            defiledSet = false;
            reshapingChunk = false;
            rivenSet = false;
            riftSet = false;
            eyndumSet = false;
            bombHandlingDevice = false;
            dimStarlight = false;
            madHand = false;
            fiberglassDagger = false;
            advancedImaging = false;
            explosiveDamage = 1f;
            explosiveCrit = 4;
            explosiveThrowSpeed = 1f;
            explosiveSelfDamage = 1f;
            if(IsExplosive(player.HeldItem)) {
                explosiveCrit += player.HeldItem.crit;
            }
            if(cryostenLifeRegenCount>0)
                cryostenLifeRegenCount--;
            if(dimStarlightCooldown>0)
                dimStarlightCooldown--;
            if(rapidSpawnFrames>0)
                rapidSpawnFrames--;
            int rasterized = player.FindBuffIndex(Rasterized_Debuff.ID);
            if (rasterized >= 0) {
                rasterizedTime = Math.Min(Math.Min(rasterizedTime + 1, 8), player.buffTime[rasterized] - 1);
            }
            player.breathMax = 200;
            PlagueSight = false;
            minionSubSlots = new float[minionSubSlotValues];
        }
        public override void PostUpdate() {
            heldProjectile = -1;
            if (rasterizedTime > 0) {
                player.velocity = Vector2.Lerp(player.velocity, player.oldVelocity, rasterizedTime * 0.06f);
                player.position = Vector2.Lerp(player.position, player.oldPosition, rasterizedTime * 0.06f);
            }
            player.oldVelocity = player.velocity;
        }
        public override void PostUpdateMiscEffects() {
            if(cryostenHelmet) {
                if(player.statLife!=player.statLifeMax2&&(int)Main.time%(cryostenLifeRegenCount>0 ? 5 : 15)==0)
                    for(int i = 0; i < 10; i++) {
                        int num6 = Dust.NewDust(player.position, player.width, player.height, DustID.Frost);
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
        public override void PostUpdateEquips() {
            if (eyndumSet) {
                ApplyEyndumSetBuffs();
                if (eyndumCore?.Value?.modItem is ModItem equippedCore) {
                    equippedCore.UpdateEquip(player);
                }
            }
            player.buffImmune[Rasterized_Debuff.ID] = player.buffImmune[BuffID.Cursed];
        }
        public void ApplyEyndumSetBuffs() {
            #region movement
            float speedMult = (player.moveSpeed - 1) * 0.5f;
            player.runAcceleration += (player.runAcceleration / player.moveSpeed) * speedMult;
            player.maxRunSpeed += (player.maxRunSpeed / player.moveSpeed) * speedMult;
            player.extraFall += player.extraFall / 2;
            player.wingTimeMax += player.wingTimeMax / 2;
            player.jumpSpeedBoost += player.jumpSpeedBoost * 0.5f;
            if (player.spikedBoots == 1) player.spikedBoots = 2;
            #endregion
            #region defence
            player.statLifeMax2 += (player.statLifeMax2 - player.statLifeMax) / 2;
            player.statDefense += player.statDefense / 2;
            player.endurance += player.endurance * 0.5f;
            player.lifeRegen += player.lifeRegen / 2;
            player.thorns += player.thorns * 0.5f;
            player.lavaMax += player.lavaMax / 2;
            #endregion
            #region damage
            player.armorPenetration += player.armorPenetration / 2;

            player.allDamage += (player.allDamage - 1) * 0.5f;
            player.meleeDamage += (player.meleeDamage - 1) * 0.5f;
            player.rangedDamage += (player.rangedDamage - 1) * 0.5f;
            player.magicDamage += (player.magicDamage - 1) * 0.5f;
            player.minionDamage += (player.minionDamage - 1) * 0.5f;

            player.allDamageMult += (player.allDamageMult - 1) * 0.5f;
            player.meleeDamageMult += (player.meleeDamageMult - 1) * 0.5f;
            player.rangedDamageMult += (player.rangedDamageMult - 1) * 0.5f;
            player.magicDamageMult += (player.magicDamageMult - 1) * 0.5f;
            player.minionDamageMult += (player.minionDamageMult - 1) * 0.5f;

            player.arrowDamage += (player.arrowDamage - 1) * 0.5f;
            player.bulletDamage += (player.bulletDamage - 1) * 0.5f;
            player.rocketDamage += (player.rocketDamage - 1) * 0.5f;

            player.meleeSpeed += (player.meleeSpeed - 1) * 0.5f;

            explosiveDamage += (explosiveDamage - 1) * 0.5f;
            explosiveThrowSpeed += (explosiveThrowSpeed - 1) * 0.5f;
            explosiveSelfDamage += (explosiveSelfDamage - 1) * 0.5f;
            #endregion
            #region resources
            player.statManaMax2 += (player.statManaMax2 - player.statManaMax) / 2;
            player.manaCost += (player.manaCost - 1) * 0.5f;
            player.maxMinions += (player.maxMinions - 1) / 2;
            player.maxTurrets += (player.maxTurrets - 1) / 2;
            player.manaRegenBonus += player.manaRegenBonus / 2;
            player.manaRegenDelayBonus += player.manaRegenDelayBonus / 2;
            #endregion
            #region utility
            player.wallSpeed += (player.wallSpeed - 1) * 0.5f;
            player.tileSpeed += (player.tileSpeed - 1) * 0.5f;
            player.pickSpeed *= (player.pickSpeed - 1) * 0.5f;
            player.aggro += player.aggro / 2;
            player.blockRange += player.blockRange / 2;
            #endregion
        }
        public override void UpdateLifeRegen() {
            if(cryostenHelmet)player.lifeRegenCount+=cryostenLifeRegenCount>0 ? 180 : 1;
        }
        #region attacks
        public override void ModifyWeaponDamage(Item item, ref float add, ref float mult, ref float flat) {
            if(IsExplosive(item))add+=explosiveDamage-1;
            bool ammoBased = item.useAmmo != AmmoID.None || (item.ammo != AmmoID.None && player.HeldItem.useAmmo == item.ammo);
            if(fiberglassSet) {
                flat+=ammoBased?2:4;
            }
            if(fiberglassDagger) {
                flat+=ammoBased?4:8;
            }
            if(rivenSet&&item.summon&&!ItemChecking) {
                mult*=rivenMult;
            }
        }
        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit) {
            if(felnumShock>29) {
                damage+=(int)(felnumShock/15);
                felnumShock = 0;
                Main.PlaySound(SoundID.Item, (int)player.Center.X, (int)player.Center.Y, 122, 2f, 1f);
            }
        }
        public override bool Shoot(Item item, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if(advancedImaging) {
                speedX*=1.3f;
                speedY*=1.3f;
            }
            if(IsExplosive(item)) {
                if(item.useAmmo == 0) {
                    speedX*=explosiveThrowSpeed;
                    speedY*=explosiveThrowSpeed;
                }
                if(riftSet) {
                    Fraction dmg = new Fraction(2, 2);
                    int c = (madHand ? 1 : 0) + (Main.rand.Next(2) == 0 ? 1 : 0);
                    dmg.D+=c;
                    damage *= dmg;
                    double rot = Main.rand.Next(2) == 0?-0.1:0.1;
                    Vector2 _position;
                    Vector2 velocity;
                    int _type;
                    int _damage;
                    float _knockBack;
                    for(int i = c; i-->0;) {
                        _position = position;
                        velocity = new Vector2(speedX, speedY).RotatedBy(rot);
                        _type = type;
                        _damage = damage;
                        _knockBack = knockBack;
                        if(ItemLoader.Shoot(item, player, ref _position, ref velocity.X, ref velocity.Y, ref _type, ref _damage, ref _knockBack)) {
                            Projectile.NewProjectile(_position, velocity, _type, _damage, _knockBack, player.whoAmI);
                        }
                        rot = -rot;
                    }
                }
            }
            if(item.shoot>ProjectileID.None&&felnumShock>29) {
                Projectile p = new Projectile();
                p.SetDefaults(type);
                OriginGlobalProj.felnumEffectNext = true;
                if(p.melee || p.aiStyle == 60)
                    return true;
                damage+=(int)(felnumShock/15);
                felnumShock = 0;
                Main.PlaySound(SoundID.Item, (int)player.Center.X, (int)player.Center.Y, 122, 2f, 1f);
            }
            return true;
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(Origins.ExplosiveModOnHit[proj.type]) {
                damage = (int)(damage*(player.allDamage+explosiveDamage-1)*0.7f);
            }
            if(proj.melee && felnumShock > 29) {
                damage+=(int)(felnumShock / 15);
                felnumShock = 0;
                Main.PlaySound(SoundID.Item, (int)proj.Center.X, (int)proj.Center.Y, 122, 2f, 1f);
            }
            if(proj.minion&&rivenSet) {
                damage = (int)(damage*rivenMult);
            }
        }
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) {
            if(crit) {
                if(celestineSet)
                    Item.NewItem(target.Hitbox, Main.rand.Next(Origins.celestineBoosters));
                if(dimStarlight&&dimStarlightCooldown<1) {
                    Item.NewItem(target.position, target.width, target.height, ItemID.Star);
                    dimStarlightCooldown = 90;
                }
            }
            if (rasterize) { 
                target.AddBuff(Rasterized_Debuff.ID, Rasterized_Debuff.duration);
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
            if(crit) {
                if(celestineSet)
                    Item.NewItem(target.Hitbox, Main.rand.Next(Origins.celestineBoosters));
                if(dimStarlight&&dimStarlightCooldown<1) {
                    Item.NewItem(target.position, target.width, target.height, ItemID.Star);
                    dimStarlightCooldown = 90;
                }
            }
            if (rasterize) { 
                target.AddBuff(Rasterized_Debuff.ID, Rasterized_Debuff.duration);//
            }
        }
        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit) {
            if(proj.owner == player.whoAmI && proj.friendly && OriginGlobalProj.IsExplosiveProjectile(proj)) {
                if(minerSet) {
                    explosiveSelfDamage-=0.2f;
                    explosiveSelfDamage*=1/explosiveDamage;
                    //damage = (int)(damage/explosiveDamage);
                    //damage-=damage/5;
                }
                damage = (int)(damage*explosiveSelfDamage);
            }
        }
        public override void OnHitByNPC(NPC npc, int damage, bool crit) {
            if(!player.noKnockback && damage!=0) {
                player.velocity.X *= MeleeCollisionNPCData.knockbackMult;
            }
            MeleeCollisionNPCData.knockbackMult = 1f;
        }
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
            if(player.HasBuff(Solvent_Debuff.ID)&&Main.rand.Next(9)<3) {
                crit = true;
            }
            if(defiledSet) {
                float manaDamage = Math.Max(damage-player.statDefense*(Main.expertMode?0.75f:0.5f), 1) * (reshapingChunk ? 0.25f : 0.15f);
                float costMult = 3;
                float costMult2 = (1/(player.magicDamage+player.allDamage-1f))/(player.magicDamageMult*player.allDamageMult);
                if(player.statMana < manaDamage*costMult*costMult2) {
                    manaDamage = player.statMana/(costMult*costMult2);
                }
                if(player.magicCuffs) {
                    if(costMult2>1)
                        costMult2 = 1/costMult2;
                }
                if(manaDamage*costMult*costMult2>=1f)
                    player.ManaEffect((int)-(manaDamage*costMult*costMult2));
                player.CheckMana((int)Math.Floor(manaDamage*costMult*costMult2), true);
                damage = (int)(damage-manaDamage);
                player.magicCuffs = false;
                player.AddBuff(ModContent.BuffType<Defiled_Exhaustion_Buff>(), 10);
            }else if (reshapingChunk) {
                damage -= damage / 20;
            }
            return damage != 0;
        }
        #endregion
        public override void PostSellItem(NPC vendor, Item[] shopInventory, Item item) {
            if (vendor.type == NPCID.Demolitionist && item.type == ModContent.ItemType<Peat_Moss>()) {
                OriginWorld originWorld = ModContent.GetInstance<OriginWorld>();
                if (originWorld.peatSold < 20 && item.type == ModContent.ItemType<Peat_Moss>()) {
                    if (item.stack >= 20 - originWorld.peatSold) {
                        item.stack -= 20 - originWorld.peatSold;
                        originWorld.peatSold = 20;
                        int nextSlot = 0;
                        for (; ++nextSlot < shopInventory.Length && !shopInventory[nextSlot].IsAir;) ;
                        if (nextSlot < shopInventory.Length) shopInventory[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Grenade>());
                        if (nextSlot < shopInventory.Length) shopInventory[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Bomb>());
                        if (nextSlot < shopInventory.Length) shopInventory[nextSlot].SetDefaults(ModContent.ItemType<Impact_Dynamite>());
                    } else {
                        originWorld.peatSold += item.stack;
                        item.TurnToAir();
                    }
                }
            }
        }
        public override void PreUpdateMovement() {
        }
        /*public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) {
            if(item.type==ModContent.ItemType<Peat_Moss>()) {

            }
        }*/
        public override void Load(TagCompound tag) {
            if (tag.SafeGet<Item>("EyndumCore") is Item eyndumCoreItem) {
                eyndumCore = new Ref<Item>(eyndumCoreItem);
            }
        }
        public override TagCompound Save() {
            TagCompound output = new TagCompound();
            if (!(eyndumCore is null)) {
                output.Add("EyndumCore", eyndumCore.Value);
            }
            return output;
        }
        #region biomes
        public override void UpdateBiomes() {
            ZoneVoid = OriginWorld.voidTiles > 300;
            ZoneVoidProgress = Math.Min(OriginWorld.voidTiles - 200, 200)/300f;

            ZoneDefiled = OriginWorld.defiledTiles > DefiledWastelands.NeededTiles;
            ZoneDefiledProgress = Math.Min(OriginWorld.defiledTiles - (DefiledWastelands.NeededTiles-DefiledWastelands.ShaderTileCount), DefiledWastelands.ShaderTileCount)/DefiledWastelands.ShaderTileCount;

            ZoneRiven = OriginWorld.rivenTiles > RivenHive.NeededTiles;
            ZoneRivenProgress = Math.Min(OriginWorld.rivenTiles - (RivenHive.NeededTiles-RivenHive.ShaderTileCount), RivenHive.ShaderTileCount)/RivenHive.ShaderTileCount;

            ZoneBrine = OriginWorld.brineTiles > BrinePool.NeededTiles;

            LinearSmoothing(ref ZoneVoidProgressSmoothed, ZoneVoidProgress, OriginWorld.biomeShaderSmoothing);
            LinearSmoothing(ref ZoneDefiledProgressSmoothed, ZoneDefiledProgress, OriginWorld.biomeShaderSmoothing);
            LinearSmoothing(ref ZoneRivenProgressSmoothed, ZoneRivenProgress, OriginWorld.biomeShaderSmoothing*0.1f);
        }
        public override bool CustomBiomesMatch(Player other) {
            OriginPlayer modOther = other.GetModPlayer<OriginPlayer>();
            return !((ZoneVoid^modOther.ZoneVoid) || (ZoneDefiled ^ modOther.ZoneDefiled) || (ZoneRiven ^ modOther.ZoneRiven) || (ZoneBrine ^ modOther.ZoneBrine));
        }
        public override void SendCustomBiomes(BinaryWriter writer) {
            byte flags = 0;
            if (ZoneVoid)
                flags |= 1;
            if (ZoneDefiled)
                flags |= 2;
            if (ZoneRiven)
                flags |= 4;
            if (ZoneBrine)
                flags |= 8;
            writer.Write(flags);
        }
        public override void ReceiveCustomBiomes(BinaryReader reader) {
            byte flags = reader.ReadByte();
            ZoneVoid = ((flags & 1) != 0);
            ZoneDefiled = ((flags & 2) != 0);
            ZoneRiven = ((flags & 4) != 0);
            ZoneBrine = ((flags & 8) != 0);
        }
        public override void CopyCustomBiomesTo(Player other) {
            OriginPlayer modOther = other.GetModPlayer<OriginPlayer>();
            //modOther.ZoneVoidTime = ZoneVoidTime;
            modOther.ZoneVoid = ZoneVoid;
            modOther.ZoneDefiled = ZoneDefiled;
            modOther.ZoneRiven = ZoneRiven;
            modOther.ZoneBrine = ZoneBrine;
        }
        public override void UpdateBiomeVisuals() {
            if(ZoneVoidProgressSmoothed > 0)Filters.Scene["Origins:ZoneDusk"].GetShader().UseProgress(ZoneVoidProgressSmoothed);
            if(ZoneDefiledProgressSmoothed > 0)Filters.Scene["Origins:ZoneDefiled"].GetShader().UseProgress(ZoneDefiledProgressSmoothed);
            if(ZoneRivenProgressSmoothed > 0)Filters.Scene["Origins:ZoneRiven"].GetShader().UseProgress(ZoneRivenProgressSmoothed);
            player.ManageSpecialBiomeVisuals("Origins:ZoneDusk", ZoneVoidProgressSmoothed>0, player.Center);
            player.ManageSpecialBiomeVisuals("Origins:ZoneDefiled", ZoneDefiledProgressSmoothed>0, player.Center);
            player.ManageSpecialBiomeVisuals("Origins:ZoneRiven", ZoneRivenProgressSmoothed>0, player.Center);
        }
        #endregion
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
            if (eyndumSet) {
                if (eyndumCore?.Value?.modItem is Eyndum_Core equippedCore) {
                    layers.Insert(layers.IndexOf(PlayerLayer.Body) + 1, CreateEyndumCoreLayer(equippedCore.CoreGlowColor));
                }
            }
            if (Origins.HelmetGlowMasks.TryGetValue(player.head, out Texture2D helmetMask)) {
                layers.Insert(layers.IndexOf(PlayerLayer.Head) + 1, CreateHeadGlowmask(helmetMask));
            }
            if (Origins.BreastplateGlowMasks.TryGetValue(player.Male ? player.body : -player.body, out Texture2D breastplateMask)) {
                layers.Insert(layers.IndexOf(PlayerLayer.Body) + 1, CreateBodyGlowmask(breastplateMask));
            } else if (Origins.BreastplateGlowMasks.TryGetValue(player.Male ? -player.body : player.body, out Texture2D fBreastplateMask)) {
                layers.Insert(layers.IndexOf(PlayerLayer.Body) + 1, CreateBodyGlowmask(fBreastplateMask));
            }
            if (Origins.LeggingGlowMasks.TryGetValue(player.legs, out Texture2D leggingMask)) {
                layers.Insert(layers.IndexOf(PlayerLayer.Legs) + 1, CreateLegsGlowmask(leggingMask));
            }
            if (player.itemAnimation != 0 && player.HeldItem.modItem is ICustomDrawItem) {
                switch(player.HeldItem.useStyle) {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    break;

                    default:
                    case 5:
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = CustomShootLayer;
                    CustomShootLayer.visible = true;
                    break;
                    /*default:
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = SlashWrenchLayer;
                    SlashWrenchLayer.visible = true;
                    break;*/
                }
            }
            if(ItemLayerWrench && !player.HeldItem.noUseGraphic) {
                switch(player.HeldItem.useStyle) {
                    case 5:
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = ShootWrenchLayer;
                    ShootWrenchLayer.visible = true;
                    break;
                    default:
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = SlashWrenchLayer;
                    SlashWrenchLayer.visible = true;
                    break;
                }
            }
            ItemLayerWrench = false;
            if (player.HeldItem.modItem is Chocolate_Bar animator) {
                layers.Add(new PlayerLayer("Origins (debugging tool)", "animator", (v)=>animator.DrawAnimations(v)));
            }
        }
        public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo) {
            if(PlagueSight) drawInfo.eyeColor = new Color(255,215,0);
            //if(drawInfo.drawPlayer.body==Origins.PlagueTexanJacketID) drawInfo.drawHands = true;
        }
        public override void FrameEffects() {
            for(int i = 13; i < 18+player.extraAccessorySlots; i++) {
                if(player.armor[i].type==Plague_Texan_Sight.ID)Plague_Texan_Sight.ApplyVisuals(player);
            }
        }
        //public static PlayerLayer PlagueEyes = new PlayerLayer("Origins", "PlagueEyes", null, (drawInfo)=> {drawInfo.eyeColor = Color.Goldenrod;});
        public static PlayerLayer PlayerShirt = new PlayerLayer("Origins", "PlayerShirt", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            Vector2 Position = drawInfo2.position;
            SpriteEffects spriteEffects = drawInfo2.spriteEffects;
            int skinVariant = drawPlayer.skinVariant;
            DrawData drawData;
            drawData = new DrawData(Main.playerTextures[skinVariant, 14], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo2.legOrigin, new Rectangle?(drawPlayer.legFrame), drawInfo2.shirtColor, drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
            Main.playerDrawData.Add(drawData);
            if(!drawPlayer.Male) {
                drawData = new DrawData(Main.playerTextures[skinVariant, 4], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.underShirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                Main.playerDrawData.Add(drawData);
                drawData = new DrawData(Main.playerTextures[skinVariant, 6], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.shirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                Main.playerDrawData.Add(drawData);
            } else {
                drawData = new DrawData(Main.playerTextures[skinVariant, 4], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.underShirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                Main.playerDrawData.Add(drawData);
                drawData = new DrawData(Main.playerTextures[skinVariant, 6], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.shirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                Main.playerDrawData.Add(drawData);
            }
            drawData = new DrawData(Main.playerTextures[skinVariant, 5], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.bodyColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
            Main.playerDrawData.Add(drawData);
        });
        public static PlayerLayer PlayerPants = new PlayerLayer("Origins", "PlayerPants", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            Vector2 Position = drawInfo2.position;
            SpriteEffects spriteEffects = drawInfo2.spriteEffects;
            int skinVariant = drawPlayer.skinVariant;
            DrawData drawData;
            drawData = new DrawData(Main.playerTextures[skinVariant, 11], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo2.legOrigin, new Rectangle?(drawPlayer.legFrame), drawInfo2.pantsColor, drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
            Main.playerDrawData.Add(drawData);
            drawData = new DrawData(Main.playerTextures[skinVariant, 12], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo2.legOrigin, new Rectangle?(drawPlayer.legFrame), drawInfo2.shoeColor, drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
            Main.playerDrawData.Add(drawData);
        });
        public static PlayerLayer ShootWrenchLayer = new PlayerLayer("Origins", "FiberglassBowLayer", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            Item item = drawPlayer.inventory[drawPlayer.selectedItem];
            Texture2D itemTexture = Main.itemTexture[item.type];
            IAnimatedItem aItem = (IAnimatedItem)item.modItem;
            int drawXPos = 10;
            Vector2 ItemCenter = new Vector2(itemTexture.Width / 2, itemTexture.Height / 2);
            Vector2 drawItemPos = OriginExtensions.DrawPlayerItemPos(drawPlayer.gravDir, item.type);
            drawXPos = (int)drawItemPos.X;
            ItemCenter.Y = drawItemPos.Y;
            Vector2 drawOrigin = new Vector2(-drawXPos, itemTexture.Height / 2);
            if(drawPlayer.direction == -1) {
                drawOrigin = new Vector2(itemTexture.Width + drawXPos, itemTexture.Height / 2);
            }
            drawOrigin.X-=drawPlayer.width/2;
            Vector4 col = drawInfo2.faceColor.ToVector4()/drawPlayer.skinColor.ToVector4();
            DrawData value = new DrawData(itemTexture, new Vector2((int)(drawInfo2.itemLocation.X - Main.screenPosition.X + ItemCenter.X), (int)(drawInfo2.itemLocation.Y - Main.screenPosition.Y + ItemCenter.Y)), aItem.Animation.GetFrame(itemTexture), item.GetAlpha(new Color(col.X, col.Y, col.Z, col.W)), drawPlayer.itemRotation, drawOrigin, item.scale, drawInfo2.spriteEffects, 0);
            Main.playerDrawData.Add(value);
            if(drawPlayer.inventory[drawPlayer.selectedItem].glowMask != -1) {
                value = new DrawData(Main.glowMaskTexture[item.glowMask], new Vector2((int)(drawInfo2.itemLocation.X - Main.screenPosition.X + ItemCenter.X), (int)(drawInfo2.itemLocation.Y - Main.screenPosition.Y + ItemCenter.Y)), aItem.Animation.GetFrame(itemTexture), item.GetAlpha(aItem.GlowmaskTint??new Color(col.X, col.Y, col.Z, col.W)), drawPlayer.itemRotation, drawOrigin, item.scale, drawInfo2.spriteEffects, 0);
                Main.playerDrawData.Add(value);
            }
        });
        public static PlayerLayer SlashWrenchLayer = new PlayerLayer("Origins", "FelnumBroadswordLayer", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            float num77 = drawPlayer.itemRotation + MathHelper.PiOver4 * drawPlayer.direction;
            Item item = drawPlayer.inventory[drawPlayer.selectedItem];
            Texture2D itemTexture = Main.itemTexture[item.type];
            IAnimatedItem aItem = (IAnimatedItem)item.modItem;
            Rectangle frame = aItem.Animation.GetFrame(itemTexture);
            Color currentColor = Lighting.GetColor((int)(drawInfo2.position.X + drawPlayer.width * 0.5) / 16, (int)((drawInfo2.position.Y + drawPlayer.height * 0.5) / 16.0));
            SpriteEffects spriteEffects = (drawPlayer.direction==1 ? 0 : SpriteEffects.FlipHorizontally) | (drawPlayer.gravDir==1f ? 0 : SpriteEffects.FlipVertically);
            DrawData value = new DrawData(itemTexture, new Vector2((int)(drawInfo2.itemLocation.X - Main.screenPosition.X), (int)(drawInfo2.itemLocation.Y - Main.screenPosition.Y)), frame, drawPlayer.inventory[drawPlayer.selectedItem].GetAlpha(currentColor), drawPlayer.itemRotation, new Vector2(frame.Width * 0.5f - frame.Width * 0.5f * drawPlayer.direction, frame.Height), drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
            Main.playerDrawData.Add(value);
            if(drawPlayer.inventory[drawPlayer.selectedItem].color != default) {
                value = new DrawData(itemTexture, new Vector2((int)(drawInfo2.itemLocation.X - Main.screenPosition.X), (int)(drawInfo2.itemLocation.Y - Main.screenPosition.Y)), frame, drawPlayer.inventory[drawPlayer.selectedItem].GetColor(currentColor), drawPlayer.itemRotation, new Vector2(frame.Width * 0.5f - frame.Width * 0.5f * drawPlayer.direction, frame.Height), drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
                Main.playerDrawData.Add(value);
            }
            if(drawPlayer.inventory[drawPlayer.selectedItem].glowMask != -1) {
                value = new DrawData(Main.glowMaskTexture[drawPlayer.inventory[drawPlayer.selectedItem].glowMask], new Vector2((int)(drawInfo2.itemLocation.X - Main.screenPosition.X), (int)(drawInfo2.itemLocation.Y - Main.screenPosition.Y)), frame, aItem.GlowmaskTint??new Color(250, 250, 250, item.alpha), drawPlayer.itemRotation, new Vector2(frame.Width * 0.5f - frame.Width * 0.5f * drawPlayer.direction, frame.Height), drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
                Main.playerDrawData.Add(value);
            }
        });
        internal static PlayerLayer CustomShootLayer => new PlayerLayer("Origins", "RejectAutomationLayer", null, delegate (PlayerDrawInfo drawInfo) {
            Player drawPlayer = drawInfo.drawPlayer;
            Item item = drawPlayer.HeldItem;
            Texture2D itemTexture = Main.itemTexture[item.type];
            ICustomDrawItem aItem = (ICustomDrawItem)item.modItem;
            int drawXPos = 0;
            Vector2 itemCenter = new Vector2(itemTexture.Width / 2, itemTexture.Height / 2);
            Vector2 drawItemPos = DrawPlayerItemPos(drawPlayer.gravDir, item.type);
            drawXPos = (int)drawItemPos.X;
            itemCenter.Y = drawItemPos.Y;
            Vector2 drawOrigin = new Vector2(drawXPos, itemTexture.Height / 2);
            if(drawPlayer.direction == -1) {
                drawOrigin = new Vector2(itemTexture.Width + drawXPos, itemTexture.Height / 2);
            }
            drawOrigin.X-=drawPlayer.width/2;
            Vector4 lightColor = drawInfo.faceColor.ToVector4()/drawPlayer.skinColor.ToVector4();
            aItem.DrawInHand(itemTexture, drawInfo, itemCenter, lightColor, drawOrigin);
        });
        public static PlayerLayer FelnumGlow = new PlayerLayer("Origins", "FelnumGlow", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            Vector2 Position;
            Rectangle? Frame;
            Texture2D Texture;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if(drawPlayer.direction == -1) {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if(drawPlayer.gravDir == -1f) {
                spriteEffects |= SpriteEffects.FlipVertically;
            }
            DrawData item;
            int a = (int)Math.Max(Math.Min((drawPlayer.GetModPlayer<OriginPlayer>().felnumShock*255)/drawPlayer.statLifeMax2, 255), 1);
            if(drawPlayer.head == Origins.FelnumHeadArmorID) {
                Position = new Vector2(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f, drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.headPosition + drawInfo2.headOrigin;
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
                Position = new Vector2(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f, drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.headPosition + drawInfo2.headOrigin;
                Frame = new Rectangle?(drawPlayer.bodyFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Eye");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.headRotation, drawInfo2.headOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
                Main.playerDrawData.Add(item);
            }
            if(drawPlayer.body == Origins.FelnumBodyArmorID) {
                Position = new Vector2(((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo2.bodyOrigin;
                Frame = new Rectangle?(drawPlayer.bodyFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Arms");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
                Main.playerDrawData.Add(item);

                Position = new Vector2(((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo2.bodyOrigin;
                Frame = new Rectangle?(drawPlayer.bodyFrame);
                Texture = ModContent.GetTexture(drawPlayer.Male ? "Origins/Items/Armor/Felnum/Felnum_Glow_Body" : "Origins/Items/Armor/Felnum/Felnum_Glow_FemaleBody");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
                Main.playerDrawData.Add(item);
            }
            if(drawPlayer.legs == Origins.FelnumLegsArmorID) {
                Position = new Vector2((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo2.legOrigin;
                Frame = new Rectangle?(drawPlayer.legFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Legs");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[2].type);
                Main.playerDrawData.Add(item);
            }
        });
        public static PlayerLayer CreateHeadGlowmask(Texture2D texture) => new PlayerLayer("Origins", "HeadGlowmask", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (drawPlayer.direction == -1) {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if (drawPlayer.gravDir == -1f) {
                spriteEffects |= SpriteEffects.FlipVertically;
            }
            Vector2 Position = new Vector2((int)(drawInfo2.position.X + (float)drawPlayer.width / 2f - (float)drawPlayer.bodyFrame.Width / 2f - Main.screenPosition.X), (int)(drawInfo2.position.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f - Main.screenPosition.Y)) + drawPlayer.headPosition + drawInfo2.headOrigin;
            //Vector2 Position = new Vector2(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f, drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.headPosition + drawInfo2.headOrigin;
            Rectangle? Frame = new Rectangle?(drawPlayer.bodyFrame);
            DrawData item = new DrawData(texture, Position, Frame, Color.White, drawPlayer.headRotation, drawInfo2.headOrigin, 1f, spriteEffects, 0);
            item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
            Main.playerDrawData.Add(item);
        }) {visible = true};
        public static PlayerLayer CreateBodyGlowmask(Texture2D texture) => new PlayerLayer("Origins", "BodyGlowmask", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (drawPlayer.direction == -1) {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if (drawPlayer.gravDir == -1f) {
                spriteEffects |= SpriteEffects.FlipVertically;
            }
            Vector2 Position = new Vector2(((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo2.bodyOrigin;
            Rectangle? Frame = new Rectangle?(drawPlayer.bodyFrame);
            DrawData item = new DrawData(texture, Position, Frame, Color.White, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
            item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
            Main.playerDrawData.Add(item);
        }) { visible = true };
        public static PlayerLayer CreateLegsGlowmask(Texture2D texture) => new PlayerLayer("Origins", "LegsGlowmask", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (drawPlayer.direction == -1) {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if (drawPlayer.gravDir == -1f) {
                spriteEffects |= SpriteEffects.FlipVertically;
            }
            Vector2 Position = new Vector2((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo2.legOrigin;
            Rectangle? Frame = new Rectangle?(drawPlayer.legFrame);
            DrawData item = new DrawData(texture, Position, Frame, Color.White, drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
            item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[2].type);
            Main.playerDrawData.Add(item);
        }) { visible = true };
        public static PlayerLayer CreateEyndumCoreLayer(Color color) => new PlayerLayer("Origins", "EyndumCore", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (drawPlayer.direction == -1) {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if (drawPlayer.gravDir == -1f) {
                spriteEffects |= SpriteEffects.FlipVertically;
            }
            Vector2 Position = new Vector2(((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo2.bodyOrigin;
            Rectangle? Frame = new Rectangle?(drawPlayer.bodyFrame);
            DrawData item = new DrawData(Origins.eyndumCoreTexture, Position, Frame, color, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
            item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
            Main.playerDrawData.Add(item);
        }) { visible = true };
    }
}
