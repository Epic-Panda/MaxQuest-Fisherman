using UnityEditor;

using EP.Utils.Editor;

public class DataCreator
{
    const string DATA_PATH_ITEMS = "Assets/Game Assets/Data/Items/";
    const string DATA_PATH_VOLATILITY = "Assets/Game Assets/Data/Volatility/";

    [MenuItem("EP Tools/Data/Items/New Item Data")]
    static void CreateItemData()
    {
        EP_CustomAssetManagement.CreateScriptableObjectAsset<ItemData>(DATA_PATH_ITEMS, "new item data.asset");
    }

    [MenuItem("EP Tools/Data/Volatility/New Volatility Data")]
    static void CreateVolatilityData()
    {
        EP_CustomAssetManagement.CreateScriptableObjectAsset<VolatilityData>(DATA_PATH_VOLATILITY, "new volatility data.asset");
    }
}
