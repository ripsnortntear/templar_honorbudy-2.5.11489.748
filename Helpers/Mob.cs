﻿using System.Linq;
using System.Numerics;
using Styx;
using Styx.Common;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Templar.GUI.Tabs;

namespace Templar.Helpers {
    public class Mob {

        // ===========================================================
        // Constants
        // ===========================================================

        // ===========================================================
        // Fields
        // ===========================================================

        // ===========================================================
        // Constructors
        // ===========================================================

        // ===========================================================
        // Getter & Setter
        // ===========================================================

        public static WoWUnit GetNextMob {
            get {
                return PullMob();
            }
        }

        public static WoWUnit GetLootMob {
            get {
                return LootMob();
            }
        }

        public static WoWUnit GetSkinMob {
            get {
                return SkinMob();
            }
        }

        // ===========================================================
        // Methods for/from SuperClass/Interfaces
        // ===========================================================

        // ===========================================================
        // Methods
        // ===========================================================

        // ===========================================================
        // Inner and Anonymous Classes
        // ===========================================================

        private static WoWUnit PullMob() {
            return !GeneralSettings.Instance.MultiPull ? ObjectManager.GetObjectsOfTypeFast<WoWUnit>().Where(SinglePull).OrderBy(u => Vector3.Distance(StyxWoW.Me.Location, u.Location)).FirstOrDefault() : ObjectManager.GetObjectsOfTypeFast<WoWUnit>().Where(MultiPull).OrderBy(u => Vector3.Distance(StyxWoW.Me.Location, u.Location)).FirstOrDefault();
        }

        private static WoWUnit LootMob() {
            return ObjectManager.GetObjectsOfTypeFast<WoWUnit>().Where(LootNext).OrderBy(u => Vector3.Distance(StyxWoW.Me.Location, u.Location)).FirstOrDefault();
        }

        private static WoWUnit SkinMob() {
            return !GeneralSettings.Instance.NinjaSkin ? ObjectManager.GetObjectsOfTypeFast<WoWUnit>().Where(SkinNextNoNinja).OrderBy(u => Vector3.Distance(StyxWoW.Me.Location, u.Location)).FirstOrDefault() : ObjectManager.GetObjectsOfTypeFast<WoWUnit>().Where(SkinNext).OrderBy(u => Vector3.Distance(StyxWoW.Me.Location, u.Location)).FirstOrDefault();
        }

        private static bool GenericContains(WoWUnit u) {
            return u != StyxWoW.Me && u.IsValid && u.Name != "Summon Enabler Stalker" && u.CanSelect && !u.IsPetBattleCritter && !u.IsTotem && !u.IsCritter && !u.IsQuestGiver && !u.IsPlayer && !u.IsPet;
        }

        private static bool GenericAliveMobContains(WoWUnit u) {
            var _base = GenericContains(u) && u.Attackable && u.IsAlive && (u.IsHostile || u.IsNeutral) && !u.TaggedByOther &&
                        !CustomBlacklist.Contains(u.Guid) && BlacklistSettings.Instance.BlacklistedUnits.All(b => b.Entry != u.Entry);

            return GeneralSettings.Instance.IgnoreElites ? _base && !u.Elite : _base;
        }

        private static bool SkinNext(WoWUnit u) {
            return GenericContains(u) && u.CanSkin && u.Skinnable && u.SkinType == WoWCreatureSkinType.Leather && GeneralSettings.Instance.SkinMobs && !CustomBlacklist.Contains(u.Guid);
        }

        private static bool SkinNextNoNinja(WoWUnit u) {
            return SkinNext(u) && u.KilledByMe;
        }

        private static bool LootNext(WoWUnit u) {
            return GenericContains(u) && u.IsDead && u.Lootable && u.CanLoot && !CustomBlacklist.Contains(u.Guid);
        }

        private static bool Whitelisted(WoWObject u) {
            return WhitelistSettings.Instance.WhitelistedUnits.Any(b => b.Entry == u.Entry);
        }

        private static bool WithinDistance(WoWObject u) {
            return Vector3.Distance(u.Location, Variables.StartLocation) <= GeneralSettings.Instance.StartingLocationMaxDistance;
        }

        private static bool SinglePull(WoWUnit u) {
            return !WhitelistSettings.Instance.AttackWhitelistOnly ? SinglePullWithinDistance(u) : SinglePullWithinDistance(u) && Whitelisted(u);
        }

        private static bool MultiPull(WoWUnit u) {
            return !WhitelistSettings.Instance.AttackWhitelistOnly ? MultiPullWithinDistance(u) : MultiPullWithinDistance(u) && Whitelisted(u);
        }

        private static bool SinglePullWithinDistance(WoWUnit u) {
            return GenericAliveMobContains(u) && WithinDistance(u);
        }

        private static bool MultiPullWithinDistance(WoWUnit u) {
            return GenericAliveMobContains(u) && WithinDistance(u) && !u.TaggedByMe && !u.TaggedByOther && !u.IsTargetingMeOrPet && !(u.IsTargetingAnyMinion && u.OwnedByUnit == StyxWoW.Me);
        }
    }
}
