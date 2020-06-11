using System.IO;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.World.Generation;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Generation;
using System.Linq;


namespace Origins
{
    public class ModNameWorld : ModWorld
    {

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
            if (genIndex == -1)
            {
                return;
            }
            tasks.Insert(genIndex + 1, new PassLegacy("FirstLake", delegate (GenerationProgress progress)
            {
                progress.Message = "Generating Lake";
                for (int i = 0; i < Main.maxTilesX / 2500; i++) //900 is how many biomes. The bigger is the number = less biomes
                {
                    int X = WorldGen.genRand.Next(1, Main.maxTilesX - 300);
                    int Y = WorldGen.genRand.Next((int)Main.worldSurface - 200, Main.maxTilesY - 0);
                    int TileType = 56; //this is the tile u want to use for the biome , if u want to use a vanilla tile then its int TileType = 56;

                    WorldGen.TileRunner(X, Y, 550, WorldGen.genRand.Next(10, 50), TileType, false, 0f, 0f, true, true);

                }

            }));
        }
    }
}