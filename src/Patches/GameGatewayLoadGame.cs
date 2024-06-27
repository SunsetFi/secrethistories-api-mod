namespace SHRestAPI.Patches
{
    using HarmonyLib;
    using SecretHistories.Infrastructure;

    [HarmonyPatch(typeof(GameGateway), "LoadGame")]
    internal static class GameGatewayLoadGame
    {
        static void Postfix()
        {
            GameEventSource.RaiseGameStarted();
        }
    }
}