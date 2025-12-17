public static class GameData
{
    // 這些變數是 static (靜態) 的，代表它們跨場景存在，不會消失
    public static int savedCount = 0;      // 拯救的糖豆數量
    public static float timeSpent = 0f;    // 耗時 (秒)
    public static bool isPlayerAlive = true; // 玩家是否存活
}