﻿using System;
using System.Linq;
using AramBuddy.MainCore.Utility;
using AramBuddy.MainCore.Utility.MiscUtil;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using SharpDX;
using EloBuddy.SDK.Menu.Values;

namespace AramBuddy.Plugins.Champions.MasterYi
{
    internal class MasterYi : Base
    {
        static MasterYi()
        {
            MenuIni = MainMenu.AddMenu(MenuName, MenuName);
            AutoMenu = MenuIni.AddSubMenu("Auto");
            ComboMenu = MenuIni.AddSubMenu("Combo");
            HarassMenu = MenuIni.AddSubMenu("Harass");
            LaneClearMenu = MenuIni.AddSubMenu("LaneClear");
            KillStealMenu = MenuIni.AddSubMenu("KillSteal");     
            HarassMenu.CreateCheckBox(Q.Slot, "Use " + Q.Slot);
            HarassMenu.CreateSlider(Q.Slot + "mana", Q.Slot + " Mana Manager", 60);
            LaneClearMenu.CreateCheckBox(Q.Slot, "Use " + Q.Slot);
            LaneClearMenu.CreateSlider(Q.Slot + "mana", Q.Slot + " Mana Manager", 60);
            LaneClearMenu.CreateCheckBox(E.Slot, "Use " + E.Slot);        
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.CreateCheckBox(Q.Slot,"Use Q");
            ComboMenu.CreateCheckBox(W.Slot,"Use W");
            ComboMenu.CreateCheckBox(E.Slot,"Use E");
            ComboMenu.CreateCheckBox("WAA", "Use W to reset AA");
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.CreateCheckBox("R","Use R");
            KillStealMenu.CreateCheckBox(Q.Slot, "Use " + Q.Slot);

            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit e, EventArgs args)
        {
            if (W.IsReady() && user.Distance(e) <= user.GetAutoAttackRange() - 50 && ComboMenu.CheckBoxValue("WAA") && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                if (W.Cast())
                {
                    Orbwalker.ResetAutoAttack();
                    EloBuddy.Player.IssueOrder(GameObjectOrder.AttackTo, e);
                }
            }
        }

        public override void Active()
        {
            
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null || !target.IsKillable(Q.Range))
                return;
            if (ComboMenu.CheckBoxValue(SpellSlot.Q) && Q.IsReady())
            {
                Q.Cast(target);
            }
            if (ComboMenu.CheckBoxValue(SpellSlot.E) && E.IsReady())
            {
                E.Cast();
            }
            if (ComboMenu.CheckBoxValue(SpellSlot.R) && R.IsReady())
            {
                R.Cast();
            }


        }

        public override void Flee()
        {
            
        }

        public override void Harass()
        {
           
        }

        public override void KillSteal()
        {
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e != null && e.IsKillable()))
            {
                if (Q.IsReady() && target.IsKillable(Q.Range) && Q.WillKill(target) && KillStealMenu.CheckBoxValue(SpellSlot.Q))
                {
                    Q.Cast(target);
                }

            }
        }

        public override void LaneClear()
        {
            foreach (var target in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m != null && m.IsKillable()))
            {
                if(Q.IsReady() && LaneClearMenu.CheckBoxValue(SpellSlot.Q) && LaneClearMenu.CompareSlider(Q.Slot + "mana", user.ManaPercent))
                    Q.Cast(target);
            }
        }
        public static bool getCheckBoxItem(Menu m, string item)
        {
            return m[item].Cast<CheckBox>().CurrentValue;
        }
        public static int getSliderItem(Menu m, string item)
        {
            return m[item].Cast<Slider>().CurrentValue;
        }
    }
}