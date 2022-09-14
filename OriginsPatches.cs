using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using Origins.Buffs;
using Origins.Gores.NPCs;
using Origins.Items;
using Origins.Items.Accessories;
using Origins.Items.Armor.Felnum;
using Origins.Items.Armor.Rift;
using Origins.Items.Armor.Vanity.Terlet.PlagueTexan;
using Origins.Items.Materials;
using Origins.Items.Weapons.Explosives;
using Origins.Items.Weapons.Felnum.Tier2;
using Origins.NPCs.TownNPCs;
using Origins.Projectiles;
using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.UI;
using Origins.World;
using Origins.World.BiomeData;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.Utilities;
using static Origins.OriginExtensions;
using MC = Terraria.ModLoader.ModContent;

namespace Origins {
	public partial class Origins : Mod {
		void ApplyPatches() {
            On.Terraria.NPC.UpdateCollision += (orig, self) => {
                int realID = self.type;
                if (self.ModNPC is ISandsharkNPC shark) {
                    self.type = NPCID.SandShark;
                    try {
                        shark.PreUpdateCollision();
                        orig(self);
                    } finally {
                        shark.PostUpdateCollision();
                    }
                    self.type = realID;
                    return;
                }
                ITileCollideNPC tcnpc = self.ModNPC as ITileCollideNPC;
                self.type = tcnpc?.CollisionType ?? realID;
                orig(self);
                self.type = realID;
            };
            On.Terraria.NPC.GetMeleeCollisionData += NPC_GetMeleeCollisionData;
            On.Terraria.WorldGen.GERunner += OriginSystem.GERunnerHook;
            On.Terraria.WorldGen.Convert += OriginSystem.ConvertHook;
            Defiled_Tree.Load();
            OriginSystem worldInstance = MC.GetInstance<OriginSystem>();
            if (!(worldInstance is null)) {
                worldInstance.defiledResurgenceTiles = new List<(int, int)> { };
                worldInstance.defiledAltResurgenceTiles = new List<(int, int, ushort)> { };
            }
            //IL.Terraria.WorldGen.GERunner+=OriginWorld.GERunnerHook;
            On.Terraria.Main.DrawInterface_Resources_Breath += FixedDrawBreath;
            On.Terraria.WorldGen.CountTiles += WorldGen_CountTiles;
            On.Terraria.WorldGen.AddUpAlignmentCounts += WorldGen_AddUpAlignmentCounts;
            Terraria.IO.WorldFile.OnWorldLoad += () => {
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    for (int i = 0; i < Main.maxTilesX; i++) WorldGen.CountTiles(i);
                }
            };
            On.Terraria.Lang.GetDryadWorldStatusDialog += Lang_GetDryadWorldStatusDialog;
            HookEndpointManager.Add(typeof(TileLoader).GetMethod("MineDamage", BindingFlags.Public | BindingFlags.Static), (hook_MinePower)MineDamage);

            On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayerInternal += LegacyPlayerRenderer_DrawPlayerInternal;
            On.Terraria.Projectile.GetWhipSettings += Projectile_GetWhipSettings;
            On.Terraria.Recipe.Condition.RecipeAvailable += (On.Terraria.Recipe.Condition.orig_RecipeAvailable orig, Recipe.Condition self, Recipe recipe) => {
                if (self == Recipe.Condition.NearWater && Main.LocalPlayer.GetModPlayer<OriginPlayer>().ZoneBrine) {
                    return false;
                }
                return orig(self, recipe);
            };
            HookEndpointManager.Add(typeof(CommonCode).GetMethod("DropItem", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(DropAttemptInfo), typeof(int), typeof(int), typeof(bool) }), (hook_DropItem)CommonCode_DropItem);
            On.Terraria.WorldGen.ScoreRoom += (On.Terraria.WorldGen.orig_ScoreRoom orig, int ignoreNPC, int npcTypeAskingToScoreRoom) => {
                npcScoringRoom = npcTypeAskingToScoreRoom;
                orig(ignoreNPC, npcTypeAskingToScoreRoom);
            };
            On.Terraria.WorldGen.GetTileTypeCountByCategory += (On.Terraria.WorldGen.orig_GetTileTypeCountByCategory orig, int[] tileTypeCounts, TileScanGroup group) => {
                if (group == TileScanGroup.TotalGoodEvil) {
                    int defiledTiles = tileTypeCounts[MC.TileType<Defiled_Stone>()] + tileTypeCounts[MC.TileType<Defiled_Grass>()] + tileTypeCounts[MC.TileType<Defiled_Sand>()] + tileTypeCounts[MC.TileType<Defiled_Ice>()];
                    int rivenTiles = tileTypeCounts[MC.TileType<Tiles.Riven.Riven_Flesh>()];

                    if (npcScoringRoom == MC.NPCType<Acid_Freak>()) {
                        return orig(tileTypeCounts, TileScanGroup.Hallow);
                    }

                    return orig(tileTypeCounts, group) - (defiledTiles + rivenTiles);
                }
                return orig(tileTypeCounts, group);
            };
            On.Terraria.GameContent.ShopHelper.BiomeNameByKey += (On.Terraria.GameContent.ShopHelper.orig_BiomeNameByKey orig, string biomeNameKey) => {
                lastBiomeNameKey = biomeNameKey;
                return orig(biomeNameKey);
            };

            On.Terraria.Localization.Language.GetTextValueWith += (On.Terraria.Localization.Language.orig_GetTextValueWith orig, string key, object obj) => {
                if (key.EndsWith("Biome")) {
                    try {
                        string betterKey = key + "_" + lastBiomeNameKey.Split('.')[^1];
                        if (Language.Exists(betterKey)) {
                            key = betterKey;
                        } else if (!Language.Exists(lastBiomeNameKey)) {
                            obj = new {
                                BiomeName = "the " + lastBiomeNameKey.Split('.')[^1].Replace('_', ' ')
                            };
                        }
                    } catch (RuntimeBinderException) { }
                }
                return orig(key, obj);
            };

            On.Terraria.GameContent.ShopHelper.IsPlayerInEvilBiomes += (On.Terraria.GameContent.ShopHelper.orig_IsPlayerInEvilBiomes orig, ShopHelper self, Player player) => {
                bool retValue = false;
                IShoppingBiome[] orig_dangerousBiomes = (IShoppingBiome[])dangerousBiomes.GetValue(self);
                try {
                    IShoppingBiome[] _dangerousBiomes;
                    if (Main.npc[player.talkNPC].type == MC.NPCType<Acid_Freak>()) {
                        _dangerousBiomes = new IShoppingBiome[] { orig_dangerousBiomes[2] };
                    } else {
                        _dangerousBiomes = orig_dangerousBiomes.WithLength(orig_dangerousBiomes.Length + 2);
                        _dangerousBiomes[^2] = new Defiled_Wastelands();
                        _dangerousBiomes[^1] = new Riven_Hive();
                    }
                    dangerousBiomes.SetValue(self, _dangerousBiomes);
                    retValue = orig(self, player);
                } finally {
                    dangerousBiomes.SetValue(self, orig_dangerousBiomes);

                }
                return retValue;
            };
            On.Terraria.Main.GetProjectileDesiredShader += (orig, index) => {
                if (Main.projectile[index].TryGetGlobalProjectile(out OriginGlobalProj originGlobalProj) && originGlobalProj.isFromMitosis) {
                    return GameShaders.Armor.GetShaderIdFromItemId(ItemID.StardustDye);
                }
                if (Main.projectile[index].ModProjectile is IShadedProjectile shadedProjectile) {
                    return shadedProjectile.Shader;
                }
                return orig(index);
            };
            On.Terraria.Graphics.Light.TileLightScanner.GetTileLight += TileLightScanner_GetTileLight;
            On.Terraria.GameContent.UI.Elements.UIWorldListItem.GetIcon += UIWorldListItem_GetIcon;
            On.Terraria.GameContent.UI.Elements.UIGenProgressBar.DrawSelf += UIGenProgressBar_DrawSelf;
            /*HookEndpointManager.Add(typeof(PlantLoader).GetMethod("ShakeTree", BindingFlags.Public | BindingFlags.Static), 
                (hook_ShakeTree)((orig_ShakeTree orig, int x, int y, int type, ref bool createLeaves) => {
					if (orig(x, y, type, ref createLeaves)) {
                        PlantLoader_ShakeTree(x, y, type, ref createLeaves);
                        return true;
					}
                    return false;
                })
            );*/
            On.Terraria.WorldGen.ShakeTree += WorldGen_ShakeTree;
        }

        private AutoCastingAsset<Texture2D> _texOuterDefiled;
        private AutoCastingAsset<Texture2D> _texOuterRiven;
        private AutoCastingAsset<Texture2D> _texOuterLower;
        private FieldInfo _visualOverallProgress;
        private FieldInfo _targetOverallProgress;
        private FieldInfo _visualCurrentProgress;
        private FieldInfo _targetCurrentProgress;
        private MethodInfo _drawFilling2;
        private void UIGenProgressBar_DrawSelf(On.Terraria.GameContent.UI.Elements.UIGenProgressBar.orig_DrawSelf orig, Terraria.GameContent.UI.Elements.UIGenProgressBar self, SpriteBatch spriteBatch) {
            byte evil = OriginSystem.WorldEvil;
            if (evil > 4) {

                if (_texOuterDefiled.Value is null) _texOuterDefiled = Assets.Request<Texture2D>("UI/WorldGen/Outer_Defiled");
                if (_texOuterRiven.Value is null) _texOuterRiven = Assets.Request<Texture2D>("UI/WorldGen/Outer_Riven");
                if (_texOuterLower.Value is null) _texOuterLower = Main.Assets.Request<Texture2D>("Images/UI/WorldGen/Outer_Lower");
                if (_visualOverallProgress is null) _visualOverallProgress = typeof(Terraria.GameContent.UI.Elements.UIGenProgressBar).GetField("_visualOverallProgress", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_targetOverallProgress is null) _targetOverallProgress = typeof(Terraria.GameContent.UI.Elements.UIGenProgressBar).GetField("_targetOverallProgress", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_visualCurrentProgress is null) _visualCurrentProgress = typeof(Terraria.GameContent.UI.Elements.UIGenProgressBar).GetField("_visualCurrentProgress", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_targetCurrentProgress is null) _targetCurrentProgress = typeof(Terraria.GameContent.UI.Elements.UIGenProgressBar).GetField("_targetCurrentProgress", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_drawFilling2 is null) _drawFilling2 = typeof(Terraria.GameContent.UI.Elements.UIGenProgressBar).GetMethod("DrawFilling2", BindingFlags.NonPublic | BindingFlags.Instance);

                const int _smallBarWidth = 508;

                const int _longBarWidth = 570;
                bool flag = evil == OriginSystem.evil_riven;
                if (WorldGen.drunkWorldGen && Main.rand.NextBool(2)) {
                    flag = !flag;
                }
                _visualOverallProgress.SetValue(self, _targetOverallProgress.GetValue(self));
                _visualCurrentProgress.SetValue(self, _targetCurrentProgress.GetValue(self));
                CalculatedStyle dimensions = self.GetDimensions();
                int completedWidth = (int)((float)_visualOverallProgress.GetValue(self) * _longBarWidth);
                int completedWidthSmall = (int)((float)_visualCurrentProgress.GetValue(self) * _smallBarWidth);
                Vector2 value = new(dimensions.X, dimensions.Y);
                Color color = default(Color);
                color.PackedValue = (flag ? 4294946846u : 4289374890u);
                _drawFilling2.Invoke(self, new object[] { spriteBatch, value + new Vector2(20f, 40f), 16, completedWidth, _longBarWidth, color, Color.Lerp(color, Color.Black, 0.5f), new Color(48, 48, 48) });
                color.PackedValue = 4290947159u;
                _drawFilling2.Invoke(self, new object[] { spriteBatch, value + new Vector2(50f, 60f), 8, completedWidthSmall, _smallBarWidth, color, Color.Lerp(color, Color.Black, 0.5f), new Color(33, 33, 33) });
                Rectangle r = dimensions.ToRectangle();
                r.X -= 8;
                spriteBatch.Draw(flag ? _texOuterRiven : _texOuterDefiled, r.TopLeft(), Color.White);
                spriteBatch.Draw(_texOuterLower.Value, r.TopLeft() + new Vector2(44f, 60f), Color.White);
            } else {
                orig(self, spriteBatch);
            }
        }

        private FieldInfo _UIWorldListItem_data;
        private FieldInfo _worldIcon;
        FieldInfo UIWorldListItem_Data => _UIWorldListItem_data ??=
            typeof(Terraria.GameContent.UI.Elements.UIWorldListItem)
            .GetField("_data", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo UIWorldListItem_WorldIcon => _worldIcon ??=
            typeof(Terraria.GameContent.UI.Elements.UIWorldListItem)
            .GetField("_worldIcon", BindingFlags.NonPublic | BindingFlags.Instance);
        private Asset<Texture2D> UIWorldListItem_GetIcon(On.Terraria.GameContent.UI.Elements.UIWorldListItem.orig_GetIcon orig, Terraria.GameContent.UI.Elements.UIWorldListItem self) {
            void changeWorldIcon() {
                try {
                    WorldFileData data = (WorldFileData)UIWorldListItem_Data.GetValue(self);
                    if (!data.DrunkWorld && !data.ForTheWorthy && !data.NotTheBees && !data.Anniversary && !data.DontStarve) {
                        string path = Path.ChangeExtension(data.Path, ".twld");
                        if (!FileUtilities.Exists(path, data.IsCloudSave)) {
                            return;
                        }
                        byte[] buf = FileUtilities.ReadAllBytes(path, data.IsCloudSave);
                        if (buf[0] == 31 && buf[1] == 139) {
                            TagCompound tag = TagIO.FromStream(new MemoryStream(buf));
                            TagCompound worldTag = tag.GetList<TagCompound>("modData")
                            .Where(v => v.GetString("mod") == Name).First();
                            OriginSystem originSystem = new OriginSystem();
                            originSystem.LoadWorldData(worldTag.GetCompound("data"));
                            if (UIWorldListItem_WorldIcon.GetValue(self) is not Terraria.GameContent.UI.Elements.UIImage image) return;
                            image.AllowResizingDimensions = false;
                            switch (originSystem.worldEvil) {
                                case OriginSystem.evil_wastelands:
                                image.SetImage(Assets.Request<Texture2D>("UI/WorldGen/IconDefiled" + (data.IsHardMode ? "Hallow" : "")));
                                break;
                                case OriginSystem.evil_riven:
                                image.SetImage(Assets.Request<Texture2D>("UI/WorldGen/IconRiven" + (data.IsHardMode ? "Hallow" : "")));
                                break;
                            }
                        }
                    }
                } catch (Exception) { }
            }
            Task.Run(changeWorldIcon);
            return orig(self);
        }

        delegate bool orig_ShakeTree(int x, int y, int type, ref bool createLeaves);
        delegate bool hook_ShakeTree(orig_ShakeTree orig, int x, int y, int type, ref bool createLeaves);
        delegate void GetTreeBottom(int i, int j, out int x, out int y);
        GetTreeBottom _getTreeBottom;
        static GetTreeBottom getTreeBottom => instance._getTreeBottom ??=
            typeof(WorldGen).GetMethod("GetTreeBottom", BindingFlags.NonPublic | BindingFlags.Static)
            .CreateDelegate<GetTreeBottom>(null);
        private void WorldGen_ShakeTree(On.Terraria.WorldGen.orig_ShakeTree orig, int i, int j) {
            getTreeBottom(i, j, out var x, out var y);
            int num = y;
            int tileType = Main.tile[x, y].TileType;
            TreeTypes treeType = WorldGen.GetTreeType(tileType);
            if (treeType == TreeTypes.None) {
                return;
            }
            bool edgeC = false;
            for (int k = 0; k < WorldGen.numTreeShakes; k++) {
                if (WorldGen.treeShakeX[k] == x && WorldGen.treeShakeY[k] == y) {
                    edgeC = true;
                }
            }
            y--;
            while (y > 10 && Main.tile[x, y].HasTile && TileID.Sets.IsShakeable[Main.tile[x, y].TileType]) {
                y--;
            }
            y++;
            if (!WorldGen.IsTileALeafyTreeTop(x, y)) {
                return;
            }
            bool edgeB = WorldGen.numTreeShakes == WorldGen.maxTreeShakes;
            bool edgeA = Collision.SolidTiles(x - 2, x + 2, y - 2, y + 2);
            if (PlantLoader_ShakeTree(x, y, tileType, edgeA || edgeB || edgeC)) {
                if (WorldGen.genRand.NextBool(20)) {
                    switch (WorldGen.genRand.Next(2)) {
                        case 0:
                        Item.NewItem(new EntitySource_ShakeTree(i, j), i * 16, j * 16, 16, 16, MC.ItemType<Tree_Sap>(), WorldGen.genRand.Next(1, 3));
                        break;
                        case 1:
                        Item.NewItem(new EntitySource_ShakeTree(i, j), i * 16, j * 16, 16, 16, MC.ItemType<Bark>(), WorldGen.genRand.Next(1, 3));
                        break;
                    }
                }
            }
            orig(i, j);
        }
        static bool PlantLoader_ShakeTree(int x, int y, int type, bool useRealRand = false) {
            //getTreeBottom(i, j, out var x, out var y);
            UnifiedRandom genRand = useRealRand ? WorldGen.genRand : WorldGen.genRand.Clone();
            TreeTypes treeType = WorldGen.GetTreeType(type);
            if (Main.getGoodWorld && genRand.NextBool(15)) return false;
            if (genRand.NextBool(300) && treeType == TreeTypes.Forest) return false;
            if (genRand.NextBool(300) && treeType == TreeTypes.Forest) return false;
            if (genRand.NextBool(200) && treeType == TreeTypes.Jungle) return false;
            if (genRand.NextBool(200) && treeType == TreeTypes.Jungle) return false;
            if (genRand.NextBool(1000) && treeType == TreeTypes.Forest) return false;
            if (genRand.NextBool(7) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Snow || treeType == TreeTypes.Hallowed)) return false;
            if (genRand.NextBool(8) && treeType == TreeTypes.Mushroom) return false;
            if (genRand.NextBool(35) && Main.halloween) return false;
            if (genRand.NextBool(12)) return false;
            if (genRand.NextBool(20)) return false;
            if (genRand.NextBool(15) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed)) return false;
            if (genRand.NextBool(50) && treeType == TreeTypes.Hallowed && !Main.dayTime) return false;
            if (genRand.NextBool(50) && treeType == TreeTypes.Forest && !Main.dayTime) return false;
            if (genRand.NextBool(40) && treeType == TreeTypes.Forest && !Main.dayTime && Main.halloween) return false;
            if (genRand.NextBool(50) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed)) return false;
            if (genRand.NextBool(40) && treeType == TreeTypes.Jungle) return false;
            if (genRand.NextBool(20) && (treeType == TreeTypes.Palm || treeType == TreeTypes.PalmCorrupt || treeType == TreeTypes.PalmCrimson || treeType == TreeTypes.PalmHallowed) && !WorldGen.IsPalmOasisTree(x)) return false;
            if (genRand.NextBool(30) && (treeType == TreeTypes.Crimson || treeType == TreeTypes.PalmCrimson)) return false;
            if (genRand.NextBool(30) && (treeType == TreeTypes.Corrupt || treeType == TreeTypes.PalmCorrupt)) return false;
            if (genRand.NextBool(30) && treeType == TreeTypes.Jungle && !Main.dayTime) return false;
            if (genRand.NextBool(40) && treeType == TreeTypes.Jungle) return false;
            if (genRand.NextBool(20) && (treeType == TreeTypes.Forest || treeType == TreeTypes.Hallowed) && !Main.raining && !NPC.TooWindyForButterflies && Main.dayTime) return false;
            if (genRand.NextBool(15) && treeType == TreeTypes.Forest) return false;
            if (genRand.NextBool(15) && treeType == TreeTypes.Snow) return false;
            if (genRand.NextBool(15) && treeType == TreeTypes.Jungle) return false;
            if (genRand.NextBool(15) && (treeType == TreeTypes.Palm || treeType == TreeTypes.PalmCorrupt || treeType == TreeTypes.PalmCrimson || treeType == TreeTypes.PalmHallowed) && !WorldGen.IsPalmOasisTree(x)) return false;
            if (genRand.NextBool(15) && (treeType == TreeTypes.Corrupt || treeType == TreeTypes.PalmCorrupt)) return false;
            if (genRand.NextBool(15) && (treeType == TreeTypes.Hallowed || treeType == TreeTypes.PalmHallowed)) return false;
            if (genRand.NextBool(15) && (treeType == TreeTypes.Crimson || treeType == TreeTypes.PalmCrimson)) return false;
            return true;
        }
        private void TileLightScanner_GetTileLight(On.Terraria.Graphics.Light.TileLightScanner.orig_GetTileLight orig, Terraria.Graphics.Light.TileLightScanner self, int x, int y, out Vector3 outputColor) {
            orig(self, x, y, out outputColor);
            Tile tile = Main.tile[x, y];

            if (tile.LiquidType == LiquidID.Water && LoaderManager.Get<WaterStylesLoader>().Get(Main.waterStyle) is IGlowingWaterStyle glowingWaterStyle) {
                glowingWaterStyle.AddLight(ref outputColor, tile.LiquidAmount);
            }
        }

        static int npcScoringRoom = -1;
        static string lastBiomeNameKey;
        #region drop rules
        private static void CommonCode_DropItem(ItemDropper orig, DropAttemptInfo info, int item, int stack, bool scattered = false) {
            (itemDropper ?? orig)(info, item, stack, scattered);
        }
        public static void ResolveRuleWithHandler(IItemDropRule rule, DropAttemptInfo dropInfo, ItemDropper handler) {
            try {
                itemDropper += handler;
                OriginExtensions.ResolveRule(rule, dropInfo);
            } finally {
                itemDropper = null;
            }
        }
        static event ItemDropper itemDropper;
        #endregion
        private void Projectile_GetWhipSettings(On.Terraria.Projectile.orig_GetWhipSettings orig, Projectile proj, out float timeToFlyOut, out int segments, out float rangeMultiplier) {
            if (proj.ModProjectile is IWhipProjectile whip) {
                whip.GetWhipSettings(out timeToFlyOut, out segments, out rangeMultiplier);
            } else {
                orig(proj, out timeToFlyOut, out segments, out rangeMultiplier);
            }
        }
        internal static bool isDrawingShadyDupes = false;
        private void LegacyPlayerRenderer_DrawPlayerInternal(On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.orig_DrawPlayerInternal orig, Terraria.Graphics.Renderers.LegacyPlayerRenderer self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float alpha, float scale, bool headOnly) {
            bool shaded = false;
            try {
                OriginPlayer originPlayer = drawPlayer.GetModPlayer<OriginPlayer>();
                if (originPlayer.amebicVialVisible) {
                    PlayerShaderSet shaderSet = new PlayerShaderSet(drawPlayer);
                    new PlayerShaderSet(amebicProtectionShaderID).Apply(drawPlayer);
                    int playerHairDye = drawPlayer.hairDye;
                    drawPlayer.hairDye = amebicProtectionHairShaderID;

                    const float offset = 2;
                    int itemAnimation = drawPlayer.itemAnimation;
                    drawPlayer.itemAnimation = 0;
                    isDrawingShadyDupes = true;
                    amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(offset, 0));
                    orig(self, camera, drawPlayer, position + new Vector2(offset, 0), rotation, rotationOrigin, shadow, alpha, scale, headOnly);

                    amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(-offset, 0));
                    orig(self, camera, drawPlayer, position + new Vector2(-offset, 0), rotation, rotationOrigin, shadow, alpha, scale, headOnly);

                    amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(0, offset));
                    orig(self, camera, drawPlayer, position + new Vector2(0, offset), rotation, rotationOrigin, shadow, alpha, scale, headOnly);

                    amebicProtectionShader.Shader.Parameters["uOffset"].SetValue(new Vector2(0, -offset));
                    orig(self, camera, drawPlayer, position + new Vector2(0, -offset), rotation, rotationOrigin, shadow, alpha, scale, headOnly);
                    shaderSet.Apply(drawPlayer);
                    drawPlayer.hairDye = playerHairDye;
                    drawPlayer.itemAnimation = itemAnimation;
                    isDrawingShadyDupes = false;
                }
                int rasterizedTime = originPlayer.rasterizedTime;
                if (rasterizedTime > 0) {
                    shaded = true;
                    rasterizeShader.Shader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
                    rasterizeShader.Shader.Parameters["uOffset"].SetValue(drawPlayer.velocity.WithMaxLength(4) * 0.0625f * rasterizedTime);
                    rasterizeShader.Shader.Parameters["uWorldPosition"].SetValue(drawPlayer.position);
                    rasterizeShader.Shader.Parameters["uSecondaryColor"].SetValue(new Vector3(40, 1120, 0));
                    Main.graphics.GraphicsDevice.Textures[1] = cellNoiseTexture;
                    Main.spriteBatch.Restart(SpriteSortMode.Immediate, effect: rasterizeShader.Shader);
                }
                orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, alpha, scale, headOnly);
            } finally {
                isDrawingShadyDupes = false;
                if (shaded) {
                    Main.spriteBatch.Restart();
                }
            }
        }

        private string Lang_GetDryadWorldStatusDialog(On.Terraria.Lang.orig_GetDryadWorldStatusDialog orig) {
            const int good = 1;
            const int evil = 2;
            const int blood = 4;
            const int defiled = 8;
            const int riven = 16;
            string text = "";
            int tGood = WorldGen.tGood;
            int tEvil = WorldGen.tEvil;
            int tBlood = WorldGen.tBlood;
            int tDefiled = OriginSystem.tDefiled;
            int tRiven = OriginSystem.tRiven;
            int tBad = tEvil + tBlood + tDefiled + tRiven;
            if (tDefiled == 0 && tRiven == 0) {
                return orig();
            }
            int tHas = (tGood > 0 ? good : 0) | (tEvil > 0 ? evil : 0) | (tBlood > 0 ? blood : 0) | (tDefiled > 0 ? defiled : 0) | (tRiven > 0 ? riven : 0);
            switch (tHas & (good | evil | blood)) {
                case good | evil | blood:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusAll", Main.worldName, tGood, tEvil, tBlood);
                break;
                case good | evil:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusHallowCorrupt", Main.worldName, tGood, tEvil, tBlood);
                break;
                case good | blood:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusHallowCrimson", Main.worldName, tGood, tEvil, tBlood);
                break;
                case evil | blood:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusCorruptCrimson", Main.worldName, tGood, tEvil, tBlood);
                break;
                case evil:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusCorrupt", Main.worldName, tGood, tEvil, tBlood);
                break;
                case blood:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusCrimson", Main.worldName, tGood, tEvil, tBlood);
                break;
                case good:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusHallow", Main.worldName, tGood, tEvil, tBlood);
                break;
                case 0:
                text = Language.GetTextValue("DryadSpecialText.WorldStatusPure", Main.worldName, tGood, tEvil, tBlood);
                break;
            }
            //temp fix, unlocalized and never grammatically correct
            if (tDefiled > 0) text += $" and {tDefiled}% defiled wastelands";
            if (tRiven > 0) text += $" and {tRiven}% riven";
            string str = (tGood * 1.2 >= tBad && tGood * 0.8 <= tBad) ?
                Language.GetTextValue("DryadSpecialText.WorldDescriptionBalanced") : ((tGood >= tBad) ?
                Language.GetTextValue("DryadSpecialText.WorldDescriptionFairyTale") : ((tBad > tGood + 20) ?
                Language.GetTextValue("DryadSpecialText.WorldDescriptionGrim") : ((tBad <= 10) ?
                Language.GetTextValue("DryadSpecialText.WorldDescriptionClose") :
                Language.GetTextValue("DryadSpecialText.WorldDescriptionWork"))));
            return text + " " + str;
        }
        #region mining power
        private delegate void orig_MinePower(int minePower, ref int damage);
        private delegate void hook_MinePower(orig_MinePower orig, int minePower, ref int damage);
        private void MineDamage(orig_MinePower orig, int minePower, ref int damage) {
            ModTile modTile = MC.GetModTile(Main.tile[Player.tileTargetX, Player.tileTargetY].TileType);
            if (modTile is null) {
                damage += minePower;
            } else if (modTile is IComplexMineDamageTile damageTile) {
                damageTile.MinePower(Player.tileTargetX, Player.tileTargetY, minePower, ref damage);
            } else {
                damage += ((int)(minePower / modTile.MineResist));
            }
        }
        #endregion
        #region tile counts
        private void WorldGen_CountTiles(On.Terraria.WorldGen.orig_CountTiles orig, int X) {
            if (X == 0) OriginSystem.UpdateTotalEvilTiles();
            orig(X);
        }

        private void WorldGen_AddUpAlignmentCounts(On.Terraria.WorldGen.orig_AddUpAlignmentCounts orig, bool clearCounts) {
            int[] tileCounts = WorldGen.tileCounts;
            if (clearCounts) {
                OriginSystem.totalDefiled2 = 0;
                OriginSystem.totalRiven2 = 0;
            }
            OriginSystem.totalDefiled2 += tileCounts[MC.TileType<Defiled_Stone>()] + tileCounts[MC.TileType<Defiled_Grass>()] + tileCounts[MC.TileType<Defiled_Sand>()] + tileCounts[MC.TileType<Defiled_Ice>()];
            OriginSystem.totalRiven2 += tileCounts[MC.TileType<Tiles.Riven.Riven_Flesh>()];
            orig(clearCounts);
        }
        #endregion
    }
}
