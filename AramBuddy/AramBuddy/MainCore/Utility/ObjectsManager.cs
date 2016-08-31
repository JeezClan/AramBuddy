﻿using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using static AramBuddy.Config;
namespace AramBuddy.MainCore.Utility
{
    internal class ObjectsManager
    {
        public static void Init()
        {
            // Clears and adds new HealthRelics and bardhealthshrines.
            HealthRelics.Clear();
            foreach (var hr in ObjectManager.Get<GameObject>().Where(o => o != null && o.Name.ToLower().Contains("healthrelic") && o.IsValid && !o.IsDead))
            {
                HealthRelics.Add(hr);
            }

            // Clears and adds new EnemyTraps.
            EnemyTraps.Clear();
            foreach (var trap in ObjectManager.Get<Obj_AI_Minion>().Where(trap => trap.IsEnemy && !trap.IsDead && trap.IsValid))
            {
                if (TrapsNames.Contains(trap.Name))
                {
                    var ttrap = new traps { Trap = trap, IsSpecial = false };
                    EnemyTraps.Add(ttrap);
                } /*
                if (SpecialTrapsNames.Contains(trap.Name))
                {
                    var ttrap = new traps { Trap = trap, IsSpecial = true };
                    EnemyTraps.Add(ttrap);
                }*/
            }

            Game.OnTick += delegate
                {
                    HealthRelics.AddRange(
                        ObjectManager.Get<GameObject>()
                            .Where(o => o.Name.Equals("bardhealthshrine", StringComparison.CurrentCultureIgnoreCase) && o.IsAlly && o.IsValid && !o.IsDead)
                            .Where(hr => hr != null && !HealthRelics.Contains(hr) && Logger.Send("Added " + hr.Name, Logger.LogLevel.Info)));

                    // Removes HealthRelics and Enemy Traps.
                    
                    HealthRelics.RemoveAll(h => h == null || !h.IsValid || h.IsDead || EntityManager.Heroes.AllHeroes.Any(a => !a.IsDead && a.IsValidTarget() && a.Distance(h) <= a.BoundingRadius + h.BoundingRadius));
                    EnemyTraps.RemoveAll(t => t.Trap == null || !t.Trap.IsValid || t.Trap.IsDead || EntityManager.Heroes.Allies.Any(a => !a.IsDead && a.IsValidTarget() && a.Distance(t.Trap) <= a.BoundingRadius + t.Trap.BoundingRadius));
                };

            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        /// <summary>
        ///     Checks if healthrelic or traps are created and add them to the list.
        /// </summary>
        public static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var caster = sender as Obj_AI_Base;
            if (caster != null)
            {
                if (TrapsNames.Contains(sender.Name) && sender.IsEnemy)
                {
                    var trap = new traps { Trap = caster, IsSpecial = false };
                    EnemyTraps.Add(trap);
                    Logger.Send("Create " + sender.Name, Logger.LogLevel.Info);
                } /*
                if (SpecialTrapsNames.Contains(caster.Name) && caster.IsEnemy)
                {
                    var trap = new traps { Trap = caster, IsSpecial = true };
                    EnemyTraps.Add(trap);
                    Logger.Send("Create " + sender.Name, Logger.LogLevel.Info);
                }*/
            }
            if (sender.Name.ToLower().Contains("healthrelic"))
            {
                HealthRelics.Add(sender);
                Logger.Send("Create  " + sender.Name, Logger.LogLevel.Info);
            }
        }

        /// <summary>
        ///     Checks if healthrelic or traps are deleted and remove them from the list.
        /// </summary>
        public static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            var caster = sender as Obj_AI_Base;
            if (caster != null)
            {
                var trap = new traps { Trap = caster, IsSpecial = false };
                //var Specialtrap = new traps { Trap = caster, IsSpecial = true };
                if (EnemyTraps.Contains(trap) && trap.Trap.IsEnemy)
                {
                    EnemyTraps.Remove(trap);
                    Logger.Send("Delete " + sender.Name, Logger.LogLevel.Info);
                } /*
                if (EnemyTraps.Contains(Specialtrap) && caster.IsEnemy)
                {
                    EnemyTraps.Remove(Specialtrap);
                    Logger.Send("Delete " + sender.Name, Logger.LogLevel.Info);
                }*/
            }
            if (sender.Name.ToLower().Contains("healthrelic"))
            {
                HealthRelics.Remove(sender);
                Logger.Send("Delete " + sender.Name, Logger.LogLevel.Info);
            }
        }

        /// <summary>
        ///     traps struct.
        /// </summary>
        public struct traps
        {
            public Obj_AI_Base Trap;
            public bool IsSpecial;
        }

        /// <summary>
        ///     Traps Names.
        /// </summary>
        public static List<string> TrapsNames = new List<string> { "Cupcake Trap", "Noxious Trap", "Jack In The Box", "Ziggs_Base_E_placedMine.troy" };

        /// <summary>
        ///     Special Traps Names.
        /// </summary>
        public static List<string> SpecialTrapsNames = new List<string>
        {
            "Fizz_Base_R_OrbitFish.troy", "Gragas_Base_Q_Enemy", "Lux_Base_E_tar_aoe_green.troy", "Soraka_Base_E_rune.troy", "Ziggs_Base_W_aoe_green.troy",
            "Viktor_Catalyst_green.troy", "Viktor_base_W_AUG_green.troy", "Barrel"
        };

        /// <summary>
        ///     BardChimes list.
        /// </summary>
        public static IEnumerable<GameObject> BardChimes
        {
            get
            {
                return ObjectManager.Get<GameObject>().Where(o => o.Name.ToLower().Contains("bardchimeminion") && o.IsAlly && o.IsValid && !o.IsDead);
            }
        }

        /// <summary>
        ///     HealthRelics and BardHealthShrines list.
        /// </summary>
        public static List<GameObject> HealthRelics = new List<GameObject>();

        /// <summary>
        ///     EnemyTraps list.
        /// </summary>
        public static List<traps> EnemyTraps = new List<traps>();

        /// <summary>
        ///     Returns Valid HealthRelic and BardHealthShrine.
        /// </summary>
        public static GameObject HealthRelic
        {
            get
            {
                return
                    HealthRelics.OrderBy(e => e.Distance(Player.Instance))
                        .FirstOrDefault(e => e.IsValid && ((e.Distance(Player.Instance) < 3000 && e.CountEnemiesInRange(SafeValue) < 1) || (e.Distance(Player.Instance) <= 500)));
            }
        }

        /// <summary>
        ///     Returns Thresh Lantern.
        /// </summary>
        public static Obj_AI_Base ThreshLantern
        {
            get
            {
                return
                    ObjectManager.Get<Obj_AI_Base>()
                        .FirstOrDefault(
                            l =>
                            l.IsValid && !l.IsDead && Player.Instance.Hero != Champion.Thresh
                            && (l.CountEnemiesInRange(1000) > 0 && Player.Instance.Distance(l) < 500 || l.CountEnemiesInRange(SafeValue) < 1) && l.IsAlly && l.Name.Equals("ThreshLantern"));
            }
        }

        /// <summary>
        ///     Returns BardChime.
        /// </summary>
        public static GameObject BardChime
        {
            get
            {
                return
                    BardChimes.OrderBy(c => c.Distance(Player.Instance))
                        .FirstOrDefault(
                            l =>
                            l.IsValid && !l.IsDead && Player.Instance.Hero == Champion.Bard && (!l.Position.UnderEnemyTurret() || l.Position.UnderEnemyTurret() && Misc.SafeToDive) && l.IsAlly
                            && (l.CountEnemiesInRange(1000) > 0 && Player.Instance.Distance(l) < 600 || l.CountEnemiesInRange(SafeValue) < 1));
            }
        }

        /// <summary>
        ///     Returns DravenAxe.
        /// </summary>
        public static GameObject DravenAxe
        {
            get
            {
                var axe = ObjectManager.Get<GameObject>().Where(a => a != null && a.IsValid && 1000 > a.Distance(Player.Instance) / Player.Instance.MoveSpeed * 1000 && a.Name.Contains("Draven_Base_Q_reticle"))
                    .OrderBy(a => Misc.TeamTotal(a.Position)).FirstOrDefault(a => a.CountEnemiesInRange(SafeValue) <= a.CountAlliesInRange(SafeValue));
                return Player.Instance.Hero == Champion.Draven ? axe : null;
            }
        }

        /// <summary>
        ///     Returns Nearest Enemy.
        /// </summary>
        public static AIHeroClient NearestEnemy
        {
            get
            {
                return EntityManager.Heroes.Enemies.OrderBy(e => e.Distance(Player.Instance)).ThenByDescending(e => e.CountAlliesInRange(1250)).FirstOrDefault(e => e.IsKillable() && e.CountAlliesInRange(SafeValue) > 1 && !e.IsDead && !e.IsZombie);
            }
        }

        /// <summary>
        ///     Returns Nearest Ally.
        /// </summary>
        public static AIHeroClient NearestAlly
        {
            get
            {
                return EntityManager.Heroes.Allies.OrderBy(e => e.Distance(Player.Instance)).FirstOrDefault(e => e.IsValidTarget() && !e.IsDead && !e.IsMe);
            }
        }

        /// <summary>
        ///     Returns a Melee Ally Fighting an Enemy.
        /// </summary>
        public static AIHeroClient MeleeAllyFighting
        {
            get
            {
                AIHeroClient ally = null;
                if (NearestEnemy != null)
                {
                    ally =
                        EntityManager.Heroes.Allies.OrderBy(a => a.Distance(NearestEnemy)).FirstOrDefault(a => a.IsValidTarget() && a.IsAttackPlayer() && !a.IsMe && a.IsMelee && a.HealthPercent > 15);
                }
                return ally;
            }
        }

        /// <summary>
        ///     Returns Best Allies To Follow.
        /// </summary>
        public static IEnumerable<AIHeroClient> BestAlliesToFollow
        {
            get
            {
                return
                    EntityManager.Heroes.Allies.OrderByDescending(a => Misc.TeamTotal(a.PredictPosition()))
                        .ThenByDescending(a => a.Distance(AllyNexues))
                        .Where(
                            a =>  Player.Instance.HealthPercent > 10 && //!a.Added() &&
                            a.IsValidTarget() && ((a.UnderEnemyTurret() && Misc.SafeToDive) || !a.UnderEnemyTurret()) && a.CountAlliesInRange(SafeValue + 100) > 1 && a.HealthPercent > 10
                            && !a.IsInFountainRange() && !a.IsDead && !a.IsZombie && !a.IsMe
                            && (a.Spellbook.IsCharging || a.Spellbook.IsChanneling || a.Spellbook.IsAutoAttacking || a.IsAttackPlayer() || a.Spellbook.IsCastingSpell
                            || (a.Path.LastOrDefault().Distance(a) > 35 && a.IsMoving)));
            }
        }

        /// <summary>
        ///     Returns Farthest Ally To Follow.
        /// </summary>
        public static AIHeroClient FarthestAllyToFollow
        {
            get
            {
                return BestAlliesToFollow.FirstOrDefault();
            }
        }

        /// <summary>
        ///     Returns Closets Ally.
        /// </summary>
        public static AIHeroClient ClosestAlly
        {
            get
            {
                return EntityManager.Heroes.Allies.OrderBy(a => a.Distance(Player.Instance)).FirstOrDefault(a => a.Distance(AllySpawn) > 5000 && a.IsValidTarget() && !a.IsMe);
            }
        }

        /// <summary>
        ///     Returns Best Safest Ally To Follow For Melee.
        /// </summary>
        public static AIHeroClient SafestAllyToFollow
        {
            get
            {
                return BestAlliesToFollow.OrderBy(a => a.Distance(Player.Instance)).FirstOrDefault(a => Misc.TeamTotal(a.PredictPosition()) - Misc.TeamTotal(a.PredictPosition(), true) > 0);
            }
        }

        /// <summary>
        ///     Returns Best Safest Ally To Follow For Ranged.
        /// </summary>
        public static AIHeroClient SafestAllyToFollow2
        {
            get
            {
                return BestAlliesToFollow.OrderByDescending(a => Misc.TeamTotal(a.PredictPosition()) - Misc.TeamTotal(a.PredictPosition(), true))
                        .FirstOrDefault(a => a.CountAlliesInRange(SafeValue) + 1 >= a.CountEnemiesInRange(SafeValue));
            }
        }

        /// <summary>
        ///     Returns farthest Ally Minion.
        /// </summary>
        public static Obj_AI_Minion Minion
        {
            get
            {
                return EntityManager.MinionsAndMonsters.AlliedMinions.OrderByDescending(a => a.Distance(AllyNexues))
                        .FirstOrDefault(
                        m =>
                        m.CountAlliesInRange(SafeValue) - m.CountEnemiesInRange(SafeValue) >= 0
                        && ((m.UnderEnemyTurret() && Misc.SafeToDive) || !m.UnderEnemyTurret()) && m.IsValidTarget(2500)
                        && m.IsValid && m.IsHPBarRendered && !m.IsDead && !m.IsZombie && m.HealthPercent > 25
                        && Misc.TeamTotal(m.PredictPosition()) - Misc.TeamTotal(m.PredictPosition(), true) >= 0);
            }
        }

        /// <summary>
        ///     Returns Nearest Ally Minion.
        /// </summary>
        public static Obj_AI_Minion NearestMinion
        {
            get
            {
                return
                    EntityManager.MinionsAndMonsters.AlliedMinions.OrderBy(a => a.Distance(Player.Instance))
                        .FirstOrDefault(
                            m =>
                            m.CountAlliesInRange(SafeValue) - m.CountEnemiesInRange(SafeValue) >= 0 && ((m.UnderEnemyTurret() && Misc.SafeToDive) || !m.UnderEnemyTurret()) && m.IsValidTarget(2500)
                            && m.IsValid && m.IsHPBarRendered && !m.IsDead && !m.IsZombie && m.HealthPercent > 25
                            && Misc.TeamTotal(m.PredictPosition()) - Misc.TeamTotal(m.PredictPosition(), true) >= 0);
            }
        }

        /// <summary>
        ///     Returns Nearest Enemy Minion.
        /// </summary>
        public static Obj_AI_Minion NearestEnemyMinion
        {
            get
            {
                return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Distance(Player.Instance)).FirstOrDefault(m => m.IsKillable() && m.Health > 0);
            }
        }

        /// <summary>
        ///     Returns Nearest Enemy Minion To AllySpawn.
        /// </summary>
        public static Obj_AI_Minion EnemyMinion4Push
        {
            get
            {
                return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Distance(AllySpawn)).FirstOrDefault(m => m.IsKillable(4000) && m.Health > 0);
            }
        }

        /// <summary>
        ///     Returns Second Tier Turret.
        /// </summary>
        public static Obj_AI_Turret SecondTurret
        {
            get
            {
                var name = Player.Instance.Team == GameObjectTeam.Order ? "ha_ap_orderturret" : "ha_ap_chaosturret";
                return EntityManager.Turrets.Allies.FirstOrDefault(t => t.IsValidTarget() && !t.IsDead && t.BaseSkinName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        /// <summary>
        ///     Returns Closeset Ally Turret.
        /// </summary>
        public static Obj_AI_Turret ClosesetAllyTurret
        {
            get
            {
                return EntityManager.Turrets.Allies.OrderBy(t => t.Distance(Player.Instance)).FirstOrDefault(t => t.IsValidTarget() && !t.IsDead);
            }
        }

        /// <summary>
        ///     Returns Safest Ally Turret.
        /// </summary>
        public static Obj_AI_Turret SafeAllyTurret
        {
            get
            {
                return
                    EntityManager.Turrets.Allies.OrderBy(t => t.Distance(Player.Instance))
                        .FirstOrDefault(t => t.IsValidTarget() && !t.IsDead && t.CountAlliesInRange(t.GetAutoAttackRange()) > t.CountEnemiesInRange(t.GetAutoAttackRange()));
            }
        }

        /// <summary>
        ///     Returns Farthest ally turret from spawn.
        /// </summary>
        public static Obj_AI_Turret FarthestAllyTurret
        {
            get
            {
                return EntityManager.Turrets.Allies.OrderBy(t => t.Distance(AllySpawn)).FirstOrDefault(t => t.IsValidTarget() && !t.IsDead && t.Health > 0 && t.HealthPercent > 5);
            }
        }

        /// <summary>
        ///     Returns Nearest Object.
        /// </summary>
        public static GameObject NearestEnemyObject
        {
            get
            {
                var list = new List<GameObject>();
                list.Clear();
                if (EnemyNexues != null)
                    list.Add(EnemyNexues);
                if (EnemyInhb != null)
                    list.Add(EnemyInhb);
                if (EnemyTurret != null)
                    list.Add(EnemyTurret);

                return list.OrderBy(o => o.Distance(AllySpawn)).FirstOrDefault(o => o.IsValid && !o.IsDead);
            }
        }

        /// <summary>
        ///     Returns Closest Enemy Turret.
        /// </summary>
        public static Obj_AI_Turret EnemyTurret
        {
            get
            {
                return EntityManager.Turrets.Enemies.OrderBy(t => t.Distance(Player.Instance)).FirstOrDefault(t => !t.IsDead && t.IsValid && t.Health > 0);
            }
        }

        /// <summary>
        ///     Returns Closest Enemy Inhbitor.
        /// </summary>
        public static Obj_BarracksDampener EnemyInhb
        {
            get
            {
                return ObjectManager.Get<Obj_BarracksDampener>().FirstOrDefault(i => i.IsEnemy && !i.IsDead && i.Health > 0);
            }
        }

        /// <summary>
        ///     Returns Enemy Nexues.
        /// </summary>
        public static Obj_HQ EnemyNexues
        {
            get
            {
                return ObjectManager.Get<Obj_HQ>().FirstOrDefault(i => i.IsEnemy);
            }
        }

        /// <summary>
        ///     Returns Ally Nexues.
        /// </summary>
        public static Obj_HQ AllyNexues
        {
            get
            {
                return ObjectManager.Get<Obj_HQ>().FirstOrDefault(i => i.IsAlly);
            }
        }

        /// <summary>
        ///     Returns Ally SpawnPoint.
        /// </summary>
        public static Obj_SpawnPoint AllySpawn
        {
            get
            {
                return ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(i => i.IsAlly);
            }
        }
    }
}
