using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MoreBlockSwap
{
    public static class ItemDropUtil
    {
        internal static MethodInfo WorldGen_KillTile_GetItemDrops;

        // This is a copy of WorldGen.KillTile_DropItems
        // heldTile was added because some drops are based off it
        // This also removes the TileLoader.Drop call to allow modded swapping to drop items
        public static void DropItems(int x, int y, Tile tileCache, int? heldTile = null, bool includeLargeObjectDrops = false)
        {
            WorldGen_KillTile_GetItemDrops ??= typeof(WorldGen).GetMethod("KillTile_GetItemDrops", BindingFlags.Static | BindingFlags.NonPublic);
            var args = new object[] { x, y, tileCache, 0, 0, 0, 0, includeLargeObjectDrops };
            
            WorldGen_KillTile_GetItemDrops.Invoke(null, args);

            int dropItem = (int)args[3];
            int dropItemStack = (int)args[4];
            int secondaryItem = (int)args[5];
            int secondaryItemStack = (int)args[6];

            ModTile mTile = TileLoader.GetTile(tileCache.TileType);

            // Drop the items from modded tiles
            if (dropItem == 0 && mTile != null)
            {
                TileLoader.Drop(x, y, mTile.Type, includeLargeObjectDrops); // Test this extensively
                return;
            }

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

        public static int GetItemDrop(int targetTileId, int targetStyle, int x, int y)
        {
            int customDrop = GetCustomDrop(targetTileId, targetStyle);
            if (customDrop != -1)
            {
                return customDrop;
            }

            ModTile mTile = TileLoader.GetTile(targetTileId);
            if(mTile is not null)
            {
                var items = mTile.GetItemDrops(x, y);
                Item? modDrop = items?.FirstOrDefault();

                if (modDrop is not null)
                    return modDrop.type;
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
            if (GetDropForRubblemaker(targetTileId, targetStyle, out int rubbleDrop))
            {
                return rubbleDrop;
            }
            return targetTileId switch
            {
                TileID.Sandcastles => ItemID.SandBlock,
                TileID.TrapdoorOpen => ItemID.Trapdoor,
                TileID.TallGateOpen => ItemID.TallGate,
                _ => -1,
            };
        }

        public static bool GetDropForRubblemaker(int targetTile, int targetStyle, out int drop)
        {
            if (GetDropFromFlexibleTileWand(FlexibleTileWand.RubblePlacementSmall, targetTile, targetStyle, out drop))
                return true;

            if (GetDropFromFlexibleTileWand(FlexibleTileWand.RubblePlacementMedium, targetTile, targetStyle, out drop))
                return true;

            if (GetDropFromFlexibleTileWand(FlexibleTileWand.RubblePlacementLarge, targetTile, targetStyle, out drop))
                return true;

            return false;
        }

        internal static FieldInfo FlexibleTileWand_options;
        internal static FieldInfo OptionBucket_ItemTypeToConsume;
        internal static FieldInfo OptionBucket_Options;

        private static bool GetDropFromFlexibleTileWand(FlexibleTileWand wandData, int targetTile, int targetStyle, out int drop)
        {
            drop = -1;
            FlexibleTileWand_options ??= typeof(FlexibleTileWand)?.GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance);

            // Dictionary<int, FlexibleTileWand.OptionBucket>
            IDictionary dictionary = (IDictionary)FlexibleTileWand_options.GetValue(wandData);

            // typeof(object) is FlexibleTileWand.OptionBucket, but its private :(
            foreach (object entry in dictionary.Values)
            {
                OptionBucket_ItemTypeToConsume ??= entry?.GetType()?.GetField("ItemTypeToConsume", BindingFlags.Instance | BindingFlags.Public);
                OptionBucket_Options ??= entry?.GetType()?.GetField("Options", BindingFlags.Instance | BindingFlags.Public);

                int itemId = (int)OptionBucket_ItemTypeToConsume.GetValue(entry);
                List<FlexibleTileWand.PlacementOption> options = (List<FlexibleTileWand.PlacementOption>)OptionBucket_Options?.GetValue(entry);

                FlexibleTileWand.PlacementOption target = options?.FirstOrDefault(option => option.TileIdToPlace == targetTile && option.TileStyleToPlace == targetStyle, null);

                if (target is not null)
                {
                    drop = itemId;
                    return true;
                }
            }

            return false;
        }
    }
}
