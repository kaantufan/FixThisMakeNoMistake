using System;
using System.Text;
using UnityEngine;

namespace TooLateToPanic
{
    /// <summary>Read-only presentation. The existing world and modal owners keep every action.</summary>
    public sealed partial class PanicHomeHud
    {
        GUIStyle uiCaptionV57, uiValueV57, uiActionV57, uiMessageV57;
        public string UiDesignVersionV57 { get { return "5.7"; } }
        public Rect DamageCardBoundsV57 { get { return new Rect(888, 20, 256, 76); } }
        public Rect BagNeedsBoundsV57 { get { return new Rect(1250, 20, 326, 76); } }

        void EnsureDesignStylesV57()
        {
            if (uiCaptionV57 != null) return;
            EnsureStyles();
            EnsureCompactStylesV5();
            uiCaptionV57 = MakeStyle(_condensedFont, 20, FontStyle.Bold, SoftText, TextAnchor.MiddleLeft);
            uiValueV57 = MakeStyle(_displayFont, 30, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            uiActionV57 = MakeStyle(_condensedFont, 26, FontStyle.Bold, MainText, TextAnchor.MiddleLeft);
            uiMessageV57 = MakeStyle(_bodyFont, 22, FontStyle.Normal, MainText, TextAnchor.MiddleLeft);
            uiMessageV57.wordWrap = true;
        }

        void DesignPanelV57(Rect rect, Color accent)
        {
            Frame(rect, Night, Line, 1f);
            Fill(new Rect(rect.x, rect.y + 12, 3, rect.height - 24), accent);
            Region(rect);
        }

        static string DisasterSymbolKeyV57(DisasterType type)
        {
            switch (type)
            {
                case DisasterType.Earthquake: return "earthquake";
                case DisasterType.Flood: return "flood";
                case DisasterType.Fire: return "fire";
                default: return "storm";
            }
        }

        void DrawCompactStatusDesignV57()
        {
            EnsureDesignStylesV57();
            var model = World.Model;
            Rect wave = WaveCardBounds;
            DesignPanelV57(wave, Brass);
            PanicUiSymbolsV57.Draw(new Rect(wave.x + 15, wave.y + 22, 34, 34), DisasterSymbolKeyV57(model.Disaster), Brass);
            string heading = (World.JuryActive ? "HAZIRLIK" : "DALGA " + model.Wave) + " · " + PhaseName(model.Phase);
            Label(new Rect(wave.x + 61, wave.y + 7, wave.width - 77, 24), heading, uiCaptionV57);
            FittedLabel(new Rect(wave.x + 61, wave.y + 32, wave.width - 77, 34), model.ActiveDisasterLabelV54, uiValueV57, MainText, 23);

            bool preparing = model.Phase == PanicPhase.Preparing || model.Phase == PanicPhase.Tutorial;
            bool timed = preparing || model.Phase == PanicPhase.Disaster || World.JuryActive;
            float value = World.JuryActive && World.JurySnapshot != null ? World.JurySnapshot.StageRemainingSeconds :
                preparing ? PreparationTimerRemaining : model.PhaseTime;
            bool held = _paused || model.IsPaused || WaveLedgerVisibleV56 || model.KeyboardHelpOpenV56 || KeyboardHelpBlockingV56 || WaveLedgerInputBlockedV56;
            string stage = held ? "DURAKLATILDI" : preparing && model.FreezePreparationTime ? "SÜRE SABİT" :
                CampaignPrecautionTimer ? "ÖNLEM SÜRESİ" : preparing ? "HAZIRLIK" : PhaseName(model.Phase);
            DrawDesignTimerV57(stage, value, timed, preparing && value <= 15f);

            Rect budget = ResourceCardBounds;
            DesignPanelV57(budget, Brass);
            PanicUiSymbolsV57.Draw(new Rect(budget.x + 15, budget.y + 23, 32, 32), "budget", Brass);
            Label(new Rect(budget.x + 58, budget.y + 7, budget.width - 72, 24), "BÜTÇE", uiCaptionV57);
            FittedLabel(new Rect(budget.x + 58, budget.y + 33, budget.width - 72, 34),
                model.InfiniteBudget ? "SINIRSIZ" : "$ " + model.Budget.ToString("N0"), uiValueV57, MainText, 24);

            Rect damage = DamageCardBoundsV57;
            Color damageInk = model.TotalDamage >= PanicModel.DamageLossLimit ? Ember : SoftText;
            DesignPanelV57(damage, damageInk);
            PanicUiSymbolsV57.Draw(new Rect(damage.x + 14, damage.y + 22, 34, 34), "damage", damageInk);
            Label(new Rect(damage.x + 59, damage.y + 7, damage.width - 70, 24), "EV HASARI · SINIR %75", uiCaptionV57);
            Label(new Rect(damage.x + 59, damage.y + 33, damage.width - 70, 34),
                "%" + Mathf.Clamp(model.TotalDamage, 0f, 100f).ToString("0.#"), ColorText(uiValueV57, damageInk));
        }

        void DrawDesignTimerV57(string stage, float value, bool timed, bool urgent)
        {
            Rect timer = TimerCardBounds;
            Color accent = urgent ? Brass : SoftText;
            DesignPanelV57(timer, accent);
            PanicUiSymbolsV57.Draw(new Rect(timer.x + 14, timer.y + 23, 32, 32), urgent ? "warning" : "clock", accent);
            FittedLabel(new Rect(timer.x + 57, timer.y + 7, timer.width - 69, 24), stage, uiCaptionV57, SoftText, 20);
            Label(new Rect(timer.x + 57, timer.y + 32, timer.width - 69, 35),
                timed ? FormatTime(Mathf.Max(0f, value)) : "--:--", ColorText(uiValueV57, urgent ? Brass : MainText));
            // A steady warning shape and explicit time replace the old continuously pulsing frame.
            if (urgent) Fill(new Rect(timer.x + 1, timer.yMax - 4, timer.width - 2, 3), Brass);
        }

        void DrawFinalBagStatusDesignV57()
        {
            EnsureDesignStylesV57();
            var model = World.Model;
            Rect task = new Rect(24, 20, 484, 76);
            DesignPanelV57(task, model.BagReady ? Sea : Brass);
            PanicUiSymbolsV57.Draw(new Rect(40, 42, 34, 34), model.BagReady ? "shelter" : "bag", model.BagReady ? Sea : Brass);
            Label(new Rect(86, 27, 404, 24), "AFET ÇANTASI", uiCaptionV57);
            FittedLabel(new Rect(86, 52, 404, 34), model.BagReady ? World.JuryActive ? "Hazır · güvenli alana geç" :
                "Hazır · " + World.ActiveShelterLabelV5 : "Su ve gıdayı çantana al", uiValueV57, MainText, 23);
            bool held = _paused || model.IsPaused || WaveLedgerVisibleV56 || model.KeyboardHelpOpenV56 || KeyboardHelpBlockingV56 || WaveLedgerInputBlockedV56;
            DrawDesignTimerV57(held ? "DURAKLATILDI" : "ÇANTA SÜRESİ", model.TimeRemaining, true, model.TimeRemaining <= 15f);
            Rect needs = BagNeedsBoundsV57;
            DesignPanelV57(needs, model.BagReady ? Sea : Brass);
            DrawDesignBagUnitV57(new Rect(needs.x + 14, needs.y + 10, 142, 56), "water", "SU", model.PackedWater);
            Fill(new Rect(needs.x + 162, needs.y + 14, 1, needs.height - 28), Line);
            DrawDesignBagUnitV57(new Rect(needs.x + 178, needs.y + 10, 136, 56), "food", "GIDA", model.PackedFood);
        }

        void DrawDesignBagUnitV57(Rect rect, string key, string text, int packed)
        {
            bool ready = packed > 0;
            Color accent = ready ? Sea : Brass;
            PanicUiSymbolsV57.Draw(new Rect(rect.x, rect.y + 9, 30, 30), key, accent);
            Label(new Rect(rect.x + 39, rect.y - 2, rect.width - 39, 24), text + " " + Mathf.Min(1, packed) + "/1", uiCaptionV57);
            PanicUiSymbolsV57.Draw(new Rect(rect.x + 39, rect.y + 29, 18, 18), ready ? "check" : "warning", accent);
            Label(new Rect(rect.x + 63, rect.y + 24, rect.width - 63, 26), ready ? "HAZIR" : "EKSİK", ColorText(uiCaptionV57, accent));
        }

        void DrawFinalBagHudDesignV57()
        {
            DrawFinalBagStatusDesignV57();
            if (World.BagCameraTransitionActive)
            {
                Rect entering = new Rect(520, 708, 560, 60);
                DesignPanelV57(entering, Brass);
                PanicUiSymbolsV57.Draw(new Rect(538, 724, 28, 28), "bag", Brass);
                Label(new Rect(582, 717, 482, 42), "AFET ÇANTASINI HAZIRLA", uiActionV57);
                return;
            }
            // The aim point stays in the exact physical centre used by the production ray.
            Fill(new Rect(790, 449, 6, 2), MainText);
            Fill(new Rect(804, 449, 6, 2), MainText);
            Fill(new Rect(799, 440, 2, 6), MainText);
            Fill(new Rect(799, 454, 2, 6), MainText);
            if (string.IsNullOrEmpty(World.FocusLabel)) return;
            const float width = 500f, textWidth = 468f;
            float hintHeight = Mathf.Max(26, uiMessageV57.CalcHeight(new GUIContent(World.ContextHint ?? ""), textWidth));
            Rect focus = new Rect(550, 490, width, 52 + hintHeight);
            DesignPanelV57(focus, Brass);
            FittedLabel(new Rect(focus.x + 16, focus.y + 7, textWidth, 36), World.FocusLabel, uiValueV57, MainText, 24);
            Label(new Rect(focus.x + 16, focus.y + 46, textWidth, hintHeight), World.ContextHint, ColorText(uiMessageV57, SoftText));
        }

        void DrawCompactRequirementsDesignV57()
        {
            EnsureDesignStylesV57();
            var expedition = World.Expedition;
            Rect card = RequirementCardBounds;
            DesignPanelV57(card, Brass);
            PanicUiSymbolsV57.Draw(new Rect(card.x + 12, card.y + 10, 24, 24), "tools", Brass);
            FittedLabel(new Rect(card.x + 46, card.y + 7, card.width - 58, 28), RequirementTitleV414(expedition), uiActionV57, MainText, 22);
            float y = card.y + 40;
            foreach (var requirement in expedition.CurrentRequirements)
            {
                if (requirement.Missing <= 0) continue;
                if (y + 25 > card.yMax - (expedition.RequirementBudgetMissing > 0 ? 30 : 4)) break;
                string name = PanicExpedition.DisplayName(requirement.Id);
                if (PanicUiSymbolsV57.Has(requirement.Id))
                    PanicUiSymbolsV57.Draw(new Rect(card.x + 12, y + 2, 22, 22), requirement.Id, SoftText);
                else DrawIcon(new Rect(card.x + 12, y + 2, 22, 22), requirement.Id);
                FittedLabel(new Rect(card.x + 43, y, 143, 27), name, uiCaptionV57, MainText, 20);
                Label(new Rect(card.x + 190, y, 64, 27), requirement.Owned + "/" + requirement.Required, uiCaptionV57);
                Label(new Rect(card.x + 258, y, 86, 27), "Eksik " + requirement.Missing, ColorText(uiCaptionV57, Brass));
                y += 29;
            }
            if (expedition.RequirementBudgetMissing > 0)
                Label(new Rect(card.x + 12, card.yMax - 30, card.width - 24, 26),
                    "Bütçe eksik: $ " + expedition.RequirementBudgetMissing, ColorText(uiCaptionV57, Brass));
        }

        void DrawCompactControlsDesignV57()
        {
            if (!CompactControlsAllowedV5) return;
            EnsureDesignStylesV57();
            EnsureCompactStylesV5();
            Rect toggle = CompactControlsToggleBoundsV5;
            Frame(toggle, Night, Line, 1);
            Region(toggle);
            DrawCompactKeyV51(new Rect(toggle.x + 6, toggle.y + 4, 37, 24), "F1", 3);
            Label(new Rect(toggle.x + 52, toggle.y + 1, toggle.width - 58, toggle.height - 2), compactControlsExpandedV5 ? "KAPAT" : "TUŞLAR", uiCaptionV57);
            if (MousePressed(toggle)) { ToggleCompactControlsV5(); World.PlayUiSound(); }
        }

        void DrawSelectedTargetDesignV57()
        {
            if (!SelectedTargetHudVisibleV56) return;
            EnsureDesignStylesV57();
            var snapshot = SelectedTargetSnapshotV56;
            if (snapshot == null) return;
            const float x = 1150, width = 426, inner = 390;
            const string roofPrefix = "ÇATI · ";
            string targetName = snapshot.IsRoof && snapshot.Label.StartsWith(roofPrefix, StringComparison.OrdinalIgnoreCase) ?
                snapshot.Label.Substring(roofPrefix.Length).Trim() : snapshot.Label;
            if (selectedTargetTitleStyleV56 == null)
            {
                selectedTargetTitleStyleV56 = new GUIStyle(uiActionV57) { wordWrap = true, alignment = TextAnchor.UpperLeft };
                selectedTargetBodyStyleV56 = new GUIStyle(uiMessageV57) { fontSize = 21, wordWrap = true, alignment = TextAnchor.UpperLeft };
            }
            float titleHeight = Mathf.Max(34, selectedTargetTitleStyleV56.CalcHeight(new GUIContent(targetName), inner));
            float hintHeight = string.IsNullOrWhiteSpace(snapshot.Hint) ? 0 : Mathf.Max(27, selectedTargetBodyStyleV56.CalcHeight(new GUIContent(snapshot.Hint), inner));
            float detailHeight = string.IsNullOrWhiteSpace(snapshot.Detail) ? 0 : Mathf.Max(27, selectedTargetBodyStyleV56.CalcHeight(new GUIContent(snapshot.Detail), inner));
            bool work = !string.IsNullOrEmpty(snapshot.Action), table = work || snapshot.Requirements.Count > 0;
            string footer = work ? World.Model.FreeMaterials || World.Model.InfiniteBudget ?
                "Geliştirici seçenekleri maliyetlere uygulanır." : "İş bitince bir kez harcanır. İptal ücretsiz." : "";
            float footerHeight = footer.Length > 0 ? selectedTargetBodyStyleV56.CalcHeight(new GUIContent(footer), inner) : 0;
            float height = 16 + 30 + titleHeight + (hintHeight > 0 ? hintHeight + 8 : 0) +
                (detailHeight > 0 ? detailHeight + 8 : 0) + (table ? 42 + (snapshot.Requirements.Count + (work ? 1 : 0)) * 36 : 0) +
                (footerHeight > 0 ? footerHeight + 12 : 0) + 16;
            selectedTargetBoundsV56 = new Rect(x, 112, width, height);
            DesignPanelV57(selectedTargetBoundsV56, Brass);
            PanicUiSymbolsV57.Draw(new Rect(x + 17, 127, 24, 24), snapshot.IsRoof ? "home" : snapshot.IsSupply ? "bag" : "tools", Brass);
            Label(new Rect(x + 51, 124, inner - 34, 27), snapshot.IsRoof ? "SEÇİLİ HEDEF · ÇATI" : "SEÇİLİ HEDEF", uiCaptionV57);
            float y = 158;
            Label(new Rect(x + 18, y, inner, titleHeight), targetName, selectedTargetTitleStyleV56); y += titleHeight;
            if (hintHeight > 0) { Label(new Rect(x + 18, y, inner, hintHeight), snapshot.Hint, ColorText(selectedTargetBodyStyleV56, Brass)); y += hintHeight + 8; }
            if (detailHeight > 0) { Label(new Rect(x + 18, y, inner, detailHeight), snapshot.Detail, selectedTargetBodyStyleV56); y += detailHeight + 8; }
            var rendered = new StringBuilder("SEÇİLİ HEDEF\n").Append(targetName).Append('\n').Append(snapshot.Hint).Append('\n').Append(snapshot.Detail).Append('\n');
            if (table)
            {
                Fill(new Rect(x + 18, y + 2, inner, 1), Line); y += 12;
                Label(new Rect(x + 18, y, 177, 26), snapshot.IsSupply ? "ÇANTADAKİ İHTİYAÇ" : "GEREKENLER", uiCaptionV57);
                Label(new Rect(x + 202, y, 57, 26), "VAR", uiCaptionV57);
                Label(new Rect(x + 260, y, 57, 26), "GEREK", uiCaptionV57);
                Label(new Rect(x + 324, y, 66, 26), "EKSİK", ColorText(uiCaptionV57, Brass)); y += 30;
                foreach (var requirement in snapshot.Requirements)
                {
                    DrawSelectedCostDesignV57(y, requirement.Id, PanicExpedition.DisplayName(requirement.Id), requirement.Owned.ToString(), requirement.Required.ToString(), requirement.Missing, rendered);
                    y += 36;
                }
                if (work) { DrawSelectedCostDesignV57(y, "budget", "Bütçe · $", World.Model.InfiniteBudget ? "∞" : snapshot.BudgetOwned.ToString("N0"), snapshot.BudgetCost.ToString("N0"), snapshot.BudgetMissing, rendered); y += 36; }
            }
            if (footerHeight > 0) Label(new Rect(x + 18, y + 8, inner, footerHeight), footer, selectedTargetBodyStyleV56);
            SelectedTargetRenderedTextV56 = rendered.Append(footer).ToString();
            if (Event.current.type == EventType.Repaint && selectedTargetRepaintFrameV56 != Time.frameCount)
            { selectedTargetRepaintFrameV56 = Time.frameCount; SelectedTargetHudDrawFramesV56++; }
        }

        void DrawSelectedCostDesignV57(float y, string key, string name, string owned, string required, int missing, StringBuilder rendered)
        {
            const float x = 1150;
            if (PanicUiSymbolsV57.Has(key)) PanicUiSymbolsV57.Draw(new Rect(x + 18, y + 5, 23, 23), key, SoftText);
            else DrawIcon(new Rect(x + 18, y + 5, 23, 23), key);
            if (key == "budget" && uiCaptionV57.CalcSize(new GUIContent(owned)).x > 57)
                FittedLabel(new Rect(x + 48, y + 1, 211, 31), "Bütçe $ " + owned, uiCaptionV57, MainText, 19);
            else
            {
                FittedLabel(new Rect(x + 48, y + 1, 145, 31), name, uiCaptionV57, MainText, 19);
                FittedLabel(new Rect(x + 202, y + 1, 57, 31), owned, uiCaptionV57, MainText, 19);
            }
            FittedLabel(new Rect(x + 260, y + 1, 57, 31), required, uiCaptionV57, MainText, 19);
            FittedLabel(new Rect(x + 324, y + 1, 66, 31), missing.ToString(), uiCaptionV57, missing > 0 ? Brass : Sea, 19);
            rendered.Append(name).Append(" · Var ").Append(owned).Append(" / Gerek ").Append(required).Append(" / Eksik ").Append(missing).Append('\n');
        }

        void DrawButtonDesignV57(Rect rect, string text, bool selected, Color accent, Action action)
        {
            EnsureDesignStylesV57();
            bool hover = GUI.enabled && rect.Contains(LogicalPointer());
            Color ink = selected ? Night : MainText;
            Frame(rect, selected ? Brass : hover ? Raised : Panel, selected || hover ? Brass : Line, hover || selected ? 2f : 1f);
            if (selected)
            {
                Fill(new Rect(rect.x - 6, rect.y + 8, 4, rect.height - 16), MainText);
                Fill(new Rect(rect.x + 12, rect.yMax - 5, rect.width - 24, 2), Night);
            }
            string symbol = ButtonSymbolV57(text);
            bool hasSymbol = !string.IsNullOrEmpty(symbol) && rect.width >= 140f;
            float inset = hasSymbol ? 53f : 17f;
            if (hasSymbol) PanicUiSymbolsV57.Draw(new Rect(rect.x + 16, rect.center.y - 12, 24, 24), symbol, ink);
            FittedLabel(new Rect(rect.x + inset, rect.y + 2, Mathf.Max(24, rect.width - inset - 34), rect.height - 4),
                text, uiActionV57, ink, rect.height >= 40 ? 22 : 18);
            if (selected || hover)
                PanicUiSymbolsV57.Draw(new Rect(rect.xMax - 29, rect.center.y - 9, 18, 18), "continue", ink);
            Mark(rect);
            if (!GUI.enabled || (!_developerMenuOpen && DeveloperInputBlockedThisFrame && !(WaveLedgerButtonInputV56 || KeyboardHelpButtonInputV56))) return;
            if (MousePressed(rect) && action != null) { World.PlayUiSound(); action(); }
        }

        static string ButtonSymbolV57(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            if (text.IndexOf("ÇIKIŞ", StringComparison.OrdinalIgnoreCase) >= 0) return "exit";
            if (text.IndexOf("ANA MENÜ", StringComparison.OrdinalIgnoreCase) >= 0) return "home";
            if (text.IndexOf("SES", StringComparison.OrdinalIgnoreCase) >= 0) return "sound";
            if (text.IndexOf("DEFTER", StringComparison.OrdinalIgnoreCase) >= 0) return "book";
            if (text.IndexOf("ÇANTA", StringComparison.OrdinalIgnoreCase) >= 0) return "bag";
            if (text.IndexOf("İPTAL", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("GERİ", StringComparison.OrdinalIgnoreCase) >= 0) return "back";
            if (text.IndexOf("DEVAM", StringComparison.OrdinalIgnoreCase) >= 0) return "continue";
            if (text.IndexOf("BAŞLA", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("OYNA", StringComparison.OrdinalIgnoreCase) >= 0) return "play";
            return null;
        }

        void DrawToastDesignV57(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            EnsureDesignStylesV57();
            float height = Mathf.Max(52, uiMessageV57.CalcHeight(new GUIContent(message), 620) + 16);
            Rect toast = new Rect(436, (World.LootVisible ? 590 : 788) - height, 728, height);
            DesignPanelV57(toast, SoftText);
            PanicUiSymbolsV57.Draw(new Rect(toast.x + 16, toast.y + 13, 26, 26), "help", SoftText);
            Label(new Rect(toast.x + 56, toast.y + 8, toast.width - 74, height - 16), message, uiMessageV57);
        }

        void DrawDisasterCueDesignV57()
        {
            EnsureDesignStylesV57();
            // The actual event remains prominent through shape and words. No decorative pulsing edge.
            Rect cue = new Rect(540, 142, 520, 84);
            DesignPanelV57(cue, Ember);
            PanicUiSymbolsV57.Draw(new Rect(cue.x + 18, cue.y + 24, 36, 36), "warning", Ember);
            Label(new Rect(cue.x + 72, cue.y + 8, cue.width - 88, 25), World.Model.ReachedShelterV5 ?
                "AFET BAŞLADI · GÜVENLİ ALANDA KAL" : "AFET BAŞLADI", ColorText(uiCaptionV57, Ember));
            FittedLabel(new Rect(cue.x + 72, cue.y + 36, cue.width - 88, 38),
                World.Model.ActiveDisasterLabelV54, uiValueV57, MainText, 23);
        }
    }
}
