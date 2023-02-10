﻿using Microsoft.Xna.Framework;
using Terraria;
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

        public static bool IsInvalidForReplacement(Tile tileToReplace)
        {
            switch (tileToReplace.TileType)
            {
                case TileID.DemonAltar: 
                    return true;
                case TileID.GemLocks:
                    return tileToReplace.TileFrameY >= 54; // prevents swappping gem locks when full to prevent networking issues
            }

            return false;
        }
    }
}