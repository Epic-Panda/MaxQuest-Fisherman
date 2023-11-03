using UnityEditor;

using EP.Utils.Editor;

public class ItemDataCreator
{
    const string DATA_PATH = "Assets/Game Assets/Data/Items/";

    [MenuItem("EP Tools/Data/Items/New Item Data")]
    static void CreateItemData()
    {
        EP_CustomAssetManagement.CreateScriptableObjectAsset<ItemData>(DATA_PATH, "new item.asset");
    }
}
