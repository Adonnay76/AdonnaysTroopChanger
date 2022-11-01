using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;

namespace AdonnaysTroopChanger.Extensions
{
    public delegate void OnLordPartySpawnedDelegate(CampaignEventDispatcher instance, MobileParty spawnedParty);
    public static class CampaignEventDispatcherExtensions
    {
        private static OnLordPartySpawnedDelegate _onLordPartySpawnedDelegateInstance;

        public static void OnLordPartySpawnedInvoker(this CampaignEventDispatcher instance, MobileParty spawnedParty)
        {
            if (_onLordPartySpawnedDelegateInstance == null)
            {
                _onLordPartySpawnedDelegateInstance = GetOnLordPartySpawnedDelegate();
            }
            _onLordPartySpawnedDelegateInstance(instance, spawnedParty);
        }

        private static OnLordPartySpawnedDelegate GetOnLordPartySpawnedDelegate()
        {
            MethodInfo onLordPartySpawnedMethodInfo = typeof(CampaignEventDispatcher).GetMethod("OnLordPartySpawned", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return (OnLordPartySpawnedDelegate)onLordPartySpawnedMethodInfo?.CreateDelegate(typeof(OnLordPartySpawnedDelegate));
        }
    }
}
