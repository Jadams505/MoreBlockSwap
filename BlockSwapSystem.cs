using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreBlockSwap
{
    public class BlockSwapSystem : ModSystem
    {
        public static BlockSwapSystem Instance => ModContent.GetInstance<BlockSwapSystem>();

        public HashSet<int> TilesThatDontWork = new HashSet<int>
        {
            TileID.Campfire, // Wait for 1.4.4
            TileID.GemLocks, // Kills the gem inside
            TileID.SillyBalloonTile, // Same as signs
            TileID.Sandcastles, // Drops a sandcastle bucket instead of nothing/ even better sand
            TileID.GemSaplings, // Same as saplings
        };
    }
}
