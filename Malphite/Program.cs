#region
using System;
using System.Collections;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Collections.Generic;
using System.Threading;
#endregion

namespace Malphite
{
    
        internal class Program
        {
            public const string ChampionName = "Malphite";
            private static readonly Obj_AI_Hero Player = ObjectManager.Player;
            //Orbwalker instance
            public static Orbwalking.Orbwalker Orbwalker;
            //Spells
            public static List<Spell> SpellList = new List<Spell>();
            public static Spell Q;
            public static Spell E;
            public static Spell W;
            public static Spell R;
            
            private static SpellSlot IgniteSlot;
            private static SpellSlot SmiteSlot;
            public static float SmiteRange = 700f;
            public static int DelayTick = 0;
            //Menu
            public static Menu Config;
            public static Menu MenuTargetedItems;
            private static void Main(string[] args)
            {
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            }
            private static void Game_OnGameLoad(EventArgs args)
            {
                if (Player.BaseSkinName != "Malphite")
                    return;
                if (Player.IsDead)
                    return;
                Q = new Spell(SpellSlot.Q, 625);
                W = new Spell(SpellSlot.W, 0);
                E = new Spell(SpellSlot.E, 400);
                R = new Spell(SpellSlot.R, 1000);
                Q.SetTargetted(0.50f, 75f);
                E.SetTargetted(0.50f, 75f);
                R.SetTargetted(0.50f, 75f);
                SpellList.Add(Q);
                SpellList.Add(W);
                SpellList.Add(E);
                SpellList.Add(R);
                IgniteSlot = Player.GetSpellSlot("SummonerDot");
                SmiteSlot = Player.GetSpellSlot("SummonerSmite");
                //Create the menu
                Config = new Menu("Malphite", "Malphite", true);
                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);
                Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
                Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
                Orbwalker.SetAttack(true);
                // Combo
                Config.AddSubMenu(new Menu("Combo", "Combo"));
                Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombohit", "Use R if hit >=").SetValue(new Slider(1,1,5)));
                Config.SubMenu("Combo")
                .AddItem(
                new MenuItem("ComboActive", "Combo!").SetValue(
                new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
                // Harass
                Config.AddSubMenu(new Menu("Harass", "Harass"));
                Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                Config.SubMenu("Harass")
                .AddItem(new MenuItem("UseQHarassDontUnderTurret", "Don't Under Turret Q").SetValue(true));
                Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
               
                Config.SubMenu("Harass")
                .AddItem(new MenuItem("HarassMana", "Min. Mana Percent: ").SetValue(new Slider(50, 100, 0)));
                Config.SubMenu("Harass")
                .AddItem(
                new MenuItem("HarassActive", "Harass").SetValue(
                new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                // Lane Clear
                Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
                Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "Use Q LastHit").SetValue(false));
                Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "Use E").SetValue(false));
                Config.SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneClearMana", "Min. Mana Percent: ").SetValue(new Slider(50, 100, 0)));
                Config.SubMenu("LaneClear")
                .AddItem(
                new MenuItem("LaneClearActive", "LaneClear").SetValue(
                new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                // Jungling Farm
                Config.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJungleFarm", "Use Q").SetValue(true));
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJungleFarm", "Use W").SetValue(false));
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJungleFarm", "Use E").SetValue(false));
                Config.SubMenu("JungleFarm")
                .AddItem(new MenuItem("JungleFarmMana", "Min. Mana Percent: ").SetValue(new Slider(50, 100, 0)));
                Config.SubMenu("JungleFarm")
                .AddItem(new MenuItem("AutoSmite", "Auto Smite").SetValue(new KeyBind('N', KeyBindType.Toggle)));
                Config.SubMenu("JungleFarm")
                .AddItem(
                new MenuItem("JungleFarmActive", "JungleFarm").SetValue(
                new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                
                // Extras -> Use Items
                Menu menuUseItems = new Menu("Use Items", "menuUseItems");
                Config.SubMenu("Extras").AddSubMenu(menuUseItems);
                // Extras -> Use Items -> Targeted Items
                MenuTargetedItems = new Menu("Targeted Items", "menuTargetItems");
                menuUseItems.AddSubMenu(MenuTargetedItems);
                MenuTargetedItems.AddItem(new MenuItem("item3143", "Randuin's Omen").SetValue(true));
               
                // Drawing
                Config.AddSubMenu(new Menu("Drawings", "Drawings"));
                Config.SubMenu("Drawings")
                .AddItem(
                new MenuItem("DrawQRange", "Q range").SetValue(
                new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                Config.SubMenu("Drawings")
                .AddItem(
                new MenuItem("DrawERange", "E range").SetValue(
                new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                Config.SubMenu("Drawings")
                .AddItem(
                new MenuItem("DrawRRange", "R range").SetValue(
                new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                Config.SubMenu("Drawings")
                .AddItem(
                new MenuItem("DrawSmiteRange", "Smite Range").SetValue(
                new Circle(false, System.Drawing.Color.Indigo)));
                /* [ Damage After Combo ] */
                var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Damage After Combo").SetValue(true);
                Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
                Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
                Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
                dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };
                Config.AddToMainMenu();
                Game.OnGameUpdate += Game_OnGameUpdate;
                Drawing.OnDraw += Drawing_OnDraw;
                GameObject.OnCreate += GameObject_OnCreate;
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
                Game.PrintChat(
                String.Format(
                "<font color='#70DBDB'>FatihSultanMehmed | </font> <font color='#FFFFFF'>{0}</font> <font color='#70DBDB'> Loaded!</font>",
                ChampionName));
            }
            private static void Drawing_OnDraw(EventArgs args)
            {
                var drawQRange = Config.Item("DrawQRange").GetValue<Circle>();
                if (drawQRange.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, Q.Range, drawQRange.Color, 1);
                }

                var drawERange = Config.Item("DrawERange").GetValue<Circle>();
                if (drawERange.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, E.Range, drawERange.Color, 1);
                }

                var drawRRange = Config.Item("DrawRRange").GetValue<Circle>();
                if (drawRRange.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, drawRRange.Color, 1);
                }
                
                var drawSmiteRange = Config.Item("DrawSmiteRange").GetValue<Circle>();
                if (drawSmiteRange.Active && Config.Item("AutoSmite").GetValue<KeyBind>().Active)
                {
                    Render.Circle.DrawCircle(Player.Position, SmiteRange,Color.DarkRed,1);
                }
               
            }
            private static float GetComboDamage(Obj_AI_Base t)
            {
                var fComboDamage = 0d;
                if (Q.IsReady())
                    fComboDamage += ObjectManager.Player.GetSpellDamage(t, SpellSlot.Q);
                if (R.IsReady())
                    fComboDamage += ObjectManager.Player.GetSpellDamage(t, SpellSlot.R);
                if (W.IsReady())
                    fComboDamage += ObjectManager.Player.GetSpellDamage(t, SpellSlot.W);
                if (E.IsReady())
                    fComboDamage += ObjectManager.Player.GetSpellDamage(t, SpellSlot.E);
                if (IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                    fComboDamage += ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);
                
                return (float)fComboDamage;
            }
            
            private static void GameObject_OnCreate(GameObject sender, EventArgs args)
            {
                // if (sender.Name.Contains("Missile") || sender.Name.Contains("Minion"))
            }
            public static void Obj_AI_Base_OnProcessSpellCast(LeagueSharp.Obj_AI_Base obj,
            LeagueSharp.GameObjectProcessSpellCastEventArgs arg)
            {
                
            }
            private static void Game_OnGameUpdate(EventArgs args)
            {
                if (!Orbwalking.CanMove(100))
                    return;
                if (DelayTick - Environment.TickCount <= 250)
                {
                    UseSummoners();
                    DelayTick = Environment.TickCount;
                }
                
                if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                {
                    Combo();
                }
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                {
                    var existsMana = Player.MaxMana / 100 * Config.Item("HarassMana").GetValue<Slider>().Value;
                    if (Player.Mana >= existsMana)
                        Harass();
                }
                if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
                {
                    var existsMana = Player.MaxMana / 100 * Config.Item("LaneClearMana").GetValue<Slider>().Value;
                    if (Player.Mana >= existsMana)
                        LaneClear();
                }
                if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                {
                    var existsMana = Player.MaxMana / 100 * Config.Item("JungleFarmMana").GetValue<Slider>().Value;
                    if (Player.Mana >= existsMana)
                        JungleFarm();
                }
            }
            private static void Combo()
            {
                Console.WriteLine("ComboActive");
                var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                var useIgniteTarget = TargetSelector.GetTarget(500, TargetSelector.DamageType.True);
                var useQ = Config.Item("UseQCombo").GetValue<bool>();
                var useW = Config.Item("UseWCombo").GetValue<bool>();
                var useE = Config.Item("UseECombo").GetValue<bool>();
                int useRhit = Config.Item("UseRCombohit").GetValue<Slider>().Value;
                if (R.IsReady() && rTarget.CountEnemiesInRange(1000)>=useRhit)
                {
                    R.Cast(rTarget.ServerPosition,true);
                }
                if (Q.IsReady() && useQ )
                {
                        Q.Cast(qTarget);
                    
                }
                if (W.IsReady() && useW)
                {
                    W.Cast();
                }
                if (E.IsReady() && useE )
                    E.Cast(eTarget); 
                if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    if (Player.GetSummonerSpellDamage(useIgniteTarget, Damage.SummonerSpell.Ignite) > useIgniteTarget.Health &&
                    ObjectManager.Player.Distance(useIgniteTarget) <= 500)
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, useIgniteTarget);
                    }
                }
                
            }
            private static void Harass()
            {
                var useQ = Config.Item("UseQCombo").GetValue<bool>();
                var useW = Config.Item("UseWCombo").GetValue<bool>();
                var useE = Config.Item("UseECombo").GetValue<bool>();
                var useQDontUnderTurret = Config.Item("UseQHarassDontUnderTurret").GetValue<bool>();
                var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                
                if (Q.IsReady() && E.IsReady() && qTarget != null && useQ && useE)
            {
                if (useQDontUnderTurret)
                    {
                        if (!Utility.UnderTurret(qTarget))
                            {
                                Q.Cast(qTarget);
                                E.Cast(eTarget);
                            }
                       }
                }
                if (Q.IsReady() && E.IsReady() && qTarget != null && eTarget != null && useQ && useE)
                {
                    Q.Cast(qTarget);
                    E.Cast(eTarget);
                }
                if (Q.IsReady() && qTarget != null && useQ)
                {
                    Q.Cast(qTarget);
                }
                if (E.IsReady() && eTarget != null && useE)
                {
                    E.Cast(eTarget);
                }


        }

           
            
            private static void LaneClear()
            {
                var useQ = Config.Item("UseQLaneClear").GetValue<bool>();
                var useE = Config.Item("UseELaneClear").GetValue<bool>();
                var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                var QDamage = ObjectManager.Player.GetSpellDamage(qTarget, SpellSlot.Q);
                var eTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                var EDamage = ObjectManager.Player.GetSpellDamage(eTarget, SpellSlot.E);
                var vMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                foreach (var vMinion in vMinions)
                {

                    if (useQ && Q.IsReady() && Player.Distance(vMinion) > Orbwalking.GetRealAutoAttackRange(Player) && vMinion.Health<QDamage)
                    {
                            Q.Cast(vMinion);
                    }
                    if (useE && E.IsReady() && Player.Distance(vMinion) < E.Range && vMinion.Health < EDamage)
                        E.Cast(vMinion.Position);
                    
                }
            }
            private static void JungleFarm()
            {
                var useQ = Config.Item("UseQJungleFarm").GetValue<bool>();
                var useW = Config.Item("UseWJungleFarm").GetValue<bool>();
                var useE = Config.Item("UseEJungleFarm").GetValue<bool>();
                var mobs = MinionManager.GetMinions(
                Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count <= 0)
                    return;
                if (Q.IsReady() && useQ && Player.Distance(mobs[0]) > Player.AttackRange)
                    Q.Cast(mobs[0]);
                if (W.IsReady() && useW)
                    W.Cast();
                if (E.IsReady() && useE)
                    E.Cast(mobs[0]);
            }
            private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base vTarget, InterruptableSpell args)
            {
                
            }
            private static InventorySlot GetInventorySlot(int ID)
            {
                return
                ObjectManager.Player.InventoryItems.FirstOrDefault(
                item =>
                (item.Id == (ItemId)ID && item.Stacks >= 1) || (item.Id == (ItemId)ID && item.Charges >= 1));
            }
            public static void UseItems(Obj_AI_Hero vTarget)
            {
                if (vTarget == null)
                    return;
                foreach (var itemID in from menuItem in MenuTargetedItems.Items
                                       let useItem = MenuTargetedItems.Item(menuItem.Name).GetValue<bool>()
                                       where useItem
                                       select Convert.ToInt16(menuItem.Name.ToString().Substring(4, 4))
                                           into itemID
                                           where Items.HasItem(itemID) && Items.CanUseItem(itemID) && GetInventorySlot(itemID) != null
                                           select itemID)
                {
                    Items.UseItem(itemID, vTarget);
                }
               
            }
            private static void UseSummoners()
            {
                if (SmiteSlot == SpellSlot.Unknown)
                    return;
                if (!Config.Item("AutoSmite").GetValue<KeyBind>().Active)
                    return;
                string[] monsterNames = { "LizardElder", "AncientGolem", "Worm", "Dragon" };
                var firstOrDefault = Player.Spellbook.Spells.FirstOrDefault(spell => spell.Name.Contains("mite"));
                if (firstOrDefault == null)
                    return;
                var vMonsters = MinionManager.GetMinions(
                Player.ServerPosition, firstOrDefault.SData.CastRange[0], MinionTypes.All, MinionTeam.NotAlly);
                foreach (var vMonster in
                vMonsters.Where(
                vMonster =>
                vMonster != null && !vMonster.IsDead && !Player.IsDead && !Player.IsStunned &&
                SmiteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                .Where(
                vMonster =>
                (vMonster.Health < Player.GetSummonerSpellDamage(vMonster, Damage.SummonerSpell.Smite)) &&
                (monsterNames.Any(name => vMonster.BaseSkinName.StartsWith(name)))))
                {
                    Player.Spellbook.CastSpell(SmiteSlot, vMonster);
                }
            }
        }
       
    }

