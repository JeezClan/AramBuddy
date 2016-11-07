using System;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;

namespace AramBuddy.MainCore.Utility.MiscUtil.Caching
{
    internal class Cache
    {
        private static float LastUpdate;

        public static List<Interuptables> InteruptablesCache = new List<Interuptables>();
        public static List<Gapclosers> GapclosersCache = new List<Gapclosers>();
        public static void Init()
        {
            Game.OnTick += Game_OnTick;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Core.GameTickCount - LastUpdate > 250)
            {
                InteruptablesCache.RemoveAll(s => Game.Time - s.Args.EndTime >= 0
                || (s.Sender != null && (s.Sender.IsDead || (!s.Sender.Spellbook.IsCastingSpell && !s.Sender.Spellbook.IsChanneling && !s.Sender.Spellbook.IsCharging))));
                GapclosersCache.RemoveAll(
                    s => Core.GameTickCount - s.Args.TickCount > 1000
                    || s.Sender != null && (s.Args.End.Equals(s.Sender.Position) || s.Args.End.Equals(s.Sender.ServerPosition) || s.Sender.IsDead || (s.IsDash && !s.Sender.IsDashing())));
                LastUpdate = Core.GameTickCount;
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender != null)
            {
                var info = new Gapclosers(sender, e, sender.IsDashing());
                if(!GapclosersCache.Contains(info))
                    GapclosersCache.Add(info);
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender != null)
            {
                var info = new Interuptables(sender, e);
                if (!InteruptablesCache.Contains(info))
                    InteruptablesCache.Add(info);
            }
        }
    }
}
