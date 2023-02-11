using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public static class SwapValidityUtil
    {
        public static bool IsValidForReplacementCustom(int heldTileId, int heldPlaceStyle, Tile tileToReplace)
        {
            int tileToReplacePlaceStyle = BlockSwapUtil.GetItemPlaceStyleFromTile(tileToReplace);
            TileObjectData tileToReplaceData = TileObjectData.GetTileData(tileToReplace);
            int tileToReplaceRealStyle = TileObjectData.GetTileStyle(tileToReplace);
            int closeDoorId = TileLoader.CloseDoorID(tileToReplace);

            if (closeDoorId != -1)
            {
                if (heldTileId == closeDoorId)
                {
                    if (tileToReplacePlaceStyle != -1)
                    {
                        return heldPlaceStyle != tileToReplacePlaceStyle;
                    }
                }
            }

            if (heldTileId == TileID.Saplings && tileToReplace.TileType == TileID.Saplings)
            {
                return true;
            }

            if (heldTileId == TileID.GemSaplings && tileToReplace.TileType == TileID.GemSaplings)
            {
                return true;
            }

            return false;
        }

        public static bool IsValidForCrossTypeReplacement(int heldTileId, int heldPlaceStyle, int targetX, int targetY, Tile tileToReplace)
        {
            TileObjectData heldData = TileObjectData.GetTileData(heldTileId, heldPlaceStyle);
            TileObjectData replaceData = TileObjectData.GetTileData(tileToReplace);

            if (heldData != null && replaceData != null &&
                heldData.Width == replaceData.Width && heldData.Height == replaceData.Height &&
                heldTileId != tileToReplace.TileType)
            {
                Point replaceTopLeft = BlockSwapUtil.TopLeftOfMultiTile(targetX, targetY, tileToReplace);
                bool canPlace = TileObject.CanPlace(replaceTopLeft.X + heldData.Origin.X, replaceTopLeft.Y + heldData.Origin.Y, heldTileId, heldPlaceStyle, 0, out _, onlyCheck: false, checkStay: true);

                return canPlace;
            }

            return false;
        }

        // This is used to stop swapping to and from tile entity like tiles
        // This is because they require clean up and set up that simple swapping can't handle
        public static bool IsInvalidTileEntityLikeTile(int heldTile, int heldStyle, int x, int y)
        {
            Tile tileToReplace = Framing.GetTileSafely(x, y);
            TileObjectData replaceData = TileObjectData.GetTileData(tileToReplace);
            TileObjectData heldData = TileObjectData.GetTileData(heldTile, heldStyle);
            Point16 potentialTileEntityPos = new Point16(x, y);

            // Holding a tile entity tile
            if (heldData != null && heldData.HookPostPlaceMyPlayer.hook != null)
            {
                return true;
            }

            if(replaceData != null)
            {
                // Vanilla tile entities/chests/dressers use this
                // Chests and dressers are not tile entities so this is important
                if (replaceData.HookPostPlaceMyPlayer.hook != null)
                {
                    return true;
                }

                potentialTileEntityPos = BlockSwapUtil.TileEntityCoordinates(x, y, replaceData.CoordinateWidth + replaceData.CoordinatePadding, replaceData.Width, replaceData.Height);
            }

            // Secondary check for actual tile entities
            if (TileEntity.ByPosition.TryGetValue(potentialTileEntityPos, out _))
            {
                return true;
            }

            return false;
        }

        public static bool IsInvalidForReplacement(Tile tileToReplace)
        {
            return tileToReplace.TileType switch
            {
                TileID.DemonAltar => true,
                TileID.GemLocks => tileToReplace.TileFrameY >= 54, // prevents swappping gem locks when full to prevent networking issues
                TileID.Boulder => true, // forces you to break them and deal with consequences :)
                var x when TileID.Sets.BreakableWhenPlacing[x] => true, // These tiles are supposed to break so replacement is effectively already achieved
                var x when Main.tileCut[x] => true, // These tiles break when swinging so same as above
                _ => false,
            };
        }
    }
}
