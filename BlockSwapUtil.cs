using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public static class BlockSwapUtil
    {
        public static int GetItemPlaceStyleFromTile(Tile tile)
        {
            TileObjectData data = TileObjectData.GetTileData(tile);
            if (data == null)
            {
                return 0;
            }

            if(GetPlaceStyleForDoor(tile, out int style))
            {
                return style;
            }

            int tileObjectStyle = TileObjectData.GetTileStyle(tile);
            int placementStyle = data.CalculatePlacementStyle(tileObjectStyle, 0, 0);

            if (data.RandomStyleRange > 0)
            {
                return placementStyle / data.RandomStyleRange; // normalize random style
            }

            if (data.StyleMultiplier > 1)
            {
                return tileObjectStyle;
            }

            int swl = data.StyleWrapLimit;

            if (swl > 0)
            {
                int startStyle = placementStyle / (swl * data.StyleLineSkip) * swl;
                int styleOffset = placementStyle % swl;

                return startStyle + styleOffset;
            }

            int col = tile.TileFrameX / data.CoordinateFullWidth;
            int row = tile.TileFrameY / data.CoordinateFullHeight;

            return data.StyleHorizontal ? col : row;
        }

        public static bool GetPlaceStyleForDoor(Tile tile, out int style)
        {
            TileObjectData data = TileObjectData.GetTileData(tile);
            ModTile mTile = TileLoader.GetTile(tile.TileType);

            if ((mTile != null && mTile.OpenDoorID != -1) || tile.TileType == TileID.ClosedDoor)
            {
                int row = tile.TileFrameY / data.CoordinateFullHeight;
                int col = tile.TileFrameX / (data.CoordinateFullWidth * 3);

                style =  row + col * data.StyleWrapLimit;
                return true;
            }
            else if ((mTile != null && mTile.CloseDoorID != -1) || tile.TileType == TileID.OpenDoor)
            {
                int row = tile.TileFrameY / data.CoordinateFullHeight;
                int col = tile.TileFrameX / (data.CoordinateFullWidth * 2);
                style =  row + col * 36;
                return true;
            }
            style = 0;
            return false;
        }

        public static int GetItemDrop(int targetTileId, int targetStyle)
        {
            for (int i = 0; i < ItemID.Count; ++i)
            {
                Item item = new Item();
                item.SetDefaults(i);
                if (item.createTile == targetTileId && item.placeStyle == targetStyle)
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool ShouldVanillaHandleSwap(int targetType, Tile tileToReplace)
        {
            TileObjectData toReplaceData = TileObjectData.GetTileData(tileToReplace);
            TileObjectData heldTileData = TileObjectData.GetTileData(targetType, 0);
            int replaceType = tileToReplace.TileType;
            return toReplaceData == null || heldTileData == null ||
                TileID.Sets.BasicChest[targetType] || TileID.Sets.BasicChest[replaceType] ||
                TileID.Sets.BasicDresser[targetType] || TileID.Sets.BasicDresser[replaceType] ||
                /*targetType == TileID.Campfire || replaceType == TileID.Campfire ||*/
                TileID.Sets.Platforms[targetType] || TileID.Sets.Platforms[replaceType] ||
                TileID.Sets.Torch[targetType] || TileID.Sets.Torch[replaceType];
        }
    }
}
