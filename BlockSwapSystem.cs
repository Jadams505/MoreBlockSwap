/*
using Terraria;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public class BlockSwapSystem : ModSystem
    {
        public static BlockSwapSystem Instance => ModContent.GetInstance<BlockSwapSystem>();

        public static Dictionary<(int width, int height), List<int>> TileObjectPairs = new();

        public static List<int> NonSolidTiles = new();

        public static List<int> SolidTopTiles = new();

        public static List<int> NotFrameImportantButHaveTOD = new();

        public override void PostAddRecipes()
        {
            for(int i = 0; i < TileID.Count; ++i)
            {
                TileObjectData data = TileObjectData.GetTileData(i, 0);

                if (data != null)
                {
                    var key = (data.Width, data.Height);
                    if (TileObjectPairs.ContainsKey(key))
                    {
                        TileObjectPairs[key].Add(i);
                    }
                    else
                    {
                        TileObjectPairs.Add(key, new List<int> { i });
                    }

                    if (!Main.tileFrameImportant[i])
                    {
                        NotFrameImportantButHaveTOD.Add(i);
                    }
                }

                if (!Main.tileSolid[i] && !Main.tileFrameImportant[i])
                {
                    NonSolidTiles.Add(i);
                }

                if (Main.tileSolidTop[i] && !Main.tileFrameImportant[i])
                {
                    SolidTopTiles.Add(i);
                }
            }
        }
    }
}
*/