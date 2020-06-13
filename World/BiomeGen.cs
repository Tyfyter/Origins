using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using System.Linq;


namespace Origins.World {
    public class BiomeGen : ModWorld {
        internal static List<(Point, int)> HellSpikes = new List<(Point, int)>() {};
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) {
            #region _
            /*genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
            if (genIndex == -1) {
                return;
            }
            tasks.Insert(genIndex + 1, new PassLegacy("TEST Biome", delegate (GenerationProgress progress) {
            progress.Message = "Generating TEST Biome";
            for(int i = 0; i < Main.maxTilesX / 900; i++) {       //900 is how many biomes. the bigger is the number = less biomes
                int X = (int)(Main.maxTilesX*0.4);//WorldGen.genRand.Next(1, Main.maxTilesX - 300);
                int Y = (int)WorldGen.worldSurfaceLow;//WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, Main.maxTilesY - 200);
                int TileType = 56;     //this is the tile u want to use for the biome , if u want to use a vanilla tile then its int TileType = 56; 56 is obsidian block

                WorldGen.TileRunner(X, Y, 350, WorldGen.genRand.Next(100, 200), TileType, false, 0f, 0f, true, true);  //350 is how big is the biome     100, 200 this changes how random it looks.
                WorldGen.CloudLake(X, (int)WorldGen.worldSurfaceHigh);
                WorldGen.AddShadowOrb(X, (int) WorldGen.worldSurfaceHigh-1);
                //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh-1).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh+1).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X-1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X+1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
            }
            }));*/
            #endregion _
            int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Larva"));
            if (genIndex == -1) {
                return;
            }
            tasks.Insert(genIndex + 1, new PassLegacy("HELL Biome", delegate (GenerationProgress progress) {
            progress.Message = "Generating HELL Biome";
            //for(int i = 0; i < Main.maxTilesX / 900; i++) {       //900 is how many biomes. the bigger is the number = less biomes
                int X = (int)(Main.maxTilesX*0.4);//WorldGen.genRand.Next(1, Main.maxTilesX - 300);
                TestRunners.HellRunner(X, Main.maxTilesY-25, 650, WorldGen.genRand.Next(100, 200), TileID.AmberGemspark, false, 0f, 0f, true, true);
                //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh-1).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh+1).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X-1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X+1, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                //Framing.GetTileSafely(X, (int)WorldGen.worldSurfaceHigh).type = TileID.AmberGemspark;
                mod.Logger.Info(HellSpikes.Count+" Void Spikes: "+string.Join(", ", HellSpikes));
                for(;HellSpikes.Count>0;) {
                    (Point, int) i = HellSpikes[0];
                    Point p = i.Item1;
                    HellSpikes.RemoveAt(0);
                    Vector2 vel = new Vector2(0, (p.Y<Main.maxTilesY-150)?2.75f:-2.75f).RotatedByRandom(1.25f);
                    TestRunners.SpikeRunner(p.X, p.Y, TileID.GraniteBlock, vel, i.Item2, randomtwist:true);
                }
            //}
            }));
            tasks.Insert(genIndex + 1, new PassLegacy("FirstLake", delegate (GenerationProgress progress) {
                mod.Logger.Info("Generating Lake");
                progress.Message = "Generating Lake";
                //for (int i = 0; i < Main.maxTilesX / 5000; i++) {
                int X = WorldGen.genRand.Next(50, Main.maxTilesX - 50);
                int Y = WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, (int)WorldGen.worldSurfaceHigh);
                Origins.lake = "LakeGen:"+X+", "+Y;
                mod.Logger.Info(Origins.lake);
                //WorldGen.TileRunner(X, Y, 50, WorldGen.genRand.Next(10, 50), TileID.Stone, true, 8f, 8f, true, true);
                WorldGen.TileRunner(X, Y, 50, WorldGen.genRand.Next(10, 50), TileID.Stone, false, 8f, 8f, true, true);
                //WorldGen.digTunnel(X, 500, 5, 5, 10, 10, true);
                WorldGen.digTunnel(X, Y, 3, 0, 30, 6, true);
                //WorldGen.digTunnel(X, Y, 0, 90, 25, 50, true);
                //}
            }));
        }
    }
}