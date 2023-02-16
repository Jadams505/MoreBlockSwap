using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreBlockSwap
{
    public static class ItemDropUtil
    {
        // This is a copy of WorldGen.KillTile_DropItems
        // heldTile was added because some drops are based off it
        // This also removes the TileLoader.Drop call to allow modded swapping to drop items
        public static void DropItems(int x, int y, Tile tileCache, int? heldTile = null, bool includeLargeObjectDrops = false)
        {
            WorldGen.KillTile_GetItemDrops(x, y, tileCache, out var dropItem, out var dropItemStack, out var secondaryItem, out var secondaryItemStack, includeLargeObjectDrops);

            if(heldTile is int targetTile)
            {
                if (includeLargeObjectDrops && BlockSwapUtil.IsConversionCase(tileCache.TileType, targetTile, out _, out int dropOverride))
                {
                    dropItem = dropOverride;
                }
            }

            if (!Main.getGoodWorld || tileCache.HasTile)
            {
                if (dropItem > 0)
                {
                    int index = Item.NewItem(WorldGen.GetItemSource_FromTileBreak(x, y), x * 16, y * 16, 16, 16, dropItem, dropItemStack, noBroadcast: false, -1);
                    Main.item[index].TryCombiningIntoNearbyItems(index);
                }

                if (secondaryItem > 0)
                {
                    int index = Item.NewItem(WorldGen.GetItemSource_FromTileBreak(x, y), x * 16, y * 16, 16, 16, secondaryItem, secondaryItemStack, noBroadcast: false, -1);
                    Main.item[index].TryCombiningIntoNearbyItems(index);
                }
            }
        }

        public static int GetItemDrop(int targetTileId, int targetStyle)
        {
            int customDrop = GetCustomDrop(targetTileId, targetStyle);
            if (customDrop != -1)
            {
                return customDrop;
            }

            ModTile mTile = TileLoader.GetTile(targetTileId);

            if(mTile != null && mTile.ItemDrop > 0)
            {
                return mTile.ItemDrop;
            }

            for (int i = 0; i < ItemID.Count; ++i)
            {
                Item item = new Item();
                item.SetDefaults(i);
                if (item.createTile == targetTileId && item.placeStyle == targetStyle)
                {
                    return i;
                }
            }

            for(int i = ItemID.Count; i < ItemLoader.ItemCount; ++i)
            {
                ModItem mItem = ItemLoader.GetItem(i);
                if(mItem != null)
                {
                    if(mItem.Item.createTile == targetTileId && mItem.Item.placeStyle == targetStyle)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static int GetCustomDrop(int targetTileId, int targetStyle)
        {
            return targetTileId switch
            {
                TileID.Sandcastles => ItemID.SandBlock,
                TileID.TrapdoorOpen => ItemID.Trapdoor,
                TileID.TallGateOpen => ItemID.TallGate,
                _ => -1,
            };
        }
    }
}
