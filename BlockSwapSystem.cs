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
            TileID.Saplings, // Lets you replace always and consumes item probably need to do something with random place styles
            TileID.Signs, // Consumes item
            TileID.Statues, // Facing right does not drop item
            TileID.Lever, // Same as signs
            TileID.Cannon, // Switching portal color causes no item to drop
            TileID.Campfire, // Wait for 1.4.4
            TileID.BubbleMachine, // Fails when toggled off
            TileID.MushroomStatue, // Same as statues
            TileID.ItemFrame, // Same as signs
            TileID.Fireplace, // Same as bubble machine
            TileID.Chimney, // Same as fire place
            TileID.LogicSensor, // Since it is a tile entity it needs special handling when swapped
            TileID.AnnouncementBox, // Same as signs
            TileID.GemLocks, // Kills the gem inside
            TileID.GeyserTrap, // Same as signs
            TileID.SillyBalloonTile, // Same as signs
            TileID.DisplayDoll, // Same as signs
            TileID.WeaponsRack2, // Sames as signs
            TileID.HatRack, // Same as signs
            TileID.ArrowSign, // Sames as signs
            TileID.PaintedArrowSign, // merge with arrow sign
            TileID.Sandcastles, // Drops a sandcastle bucket instead of nothing/ even better sand
            TileID.TatteredWoodSign, // Same as signs
            TileID.GemSaplings, // Same as saplings
            TileID.TeleportationPylon, // Needs checking for only one in the world, plus tile entity handling
        };
    }
}
