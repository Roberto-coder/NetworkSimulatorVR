using System;
using System.Globalization;
using System.Linq;
using Firebase.Auth;
using Systems.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Lobby.UI
{
    public sealed class ProfilePanelPresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text usernameText;
        [SerializeField] private TMP_Text emailText;
        [SerializeField] private TMP_Text activeSlotText;
        [SerializeField] private TMP_Text completedModulesText;
        [SerializeField] private TMP_Text achievementsText;
        [SerializeField] private TMP_Text totalPlayTimeText;
        [SerializeField] private TMP_Text lastSaveText;
        [SerializeField] private TMP_Text syncStatusText;
        [Header("Progreso por módulo")]
        [SerializeField] private Button[] moduleButtons;
        [SerializeField] private TMP_Text moduleNameText;
        [SerializeField] private TMP_Text moduleStatusText;
        [SerializeField] private TMP_Text moduleObjectivesText;
        [SerializeField] private TMP_Text moduleTimeText;
        [SerializeField] private TMP_Text moduleDateText;
        [SerializeField] private Image[] badgeIcons;
        [SerializeField] private TMP_Text[] badgeStateTexts;

        private RectTransform moduleDetailRect;
        private RectTransform progressCardRect;

        private static readonly string[] ModuleIds = { "M1_", "M2_", "M3_" };
        private static readonly string[] ModuleNames =
        {
            "Módulo 1 · Fabricación de cable",
            "Módulo 2 · Rack y switch",
            "Módulo 3 · Diagnóstico de red"
        };
        private static readonly string[] AchievementIds =
        {
            "module_01_completed", "module_02_completed", "module_03_completed"
        };

        private SaveSlot activeSlot;
        private int selectedModuleIndex;

        private void OnEnable()
        {
            ResolveReferences();
            ConfigureModuleButtons();
            Refresh();
        }

        public void Refresh()
        {
            SaveFile saveFile = SaveManager.Instance != null ? SaveManager.Instance.saveFile : null;
            activeSlot = saveFile?.slots?.Find(slot => slot.slotID == saveFile.activeSlotId);

            SetText(usernameText, string.IsNullOrWhiteSpace(SessionContext.Username)
                ? "USUARIO"
                : SessionContext.Username);
            SetText(emailText, SessionContext.IsDebugSession
                ? "Sesión local de prueba"
                : FirebaseAuth.DefaultInstance.CurrentUser?.Email ?? "Correo no disponible");
            SetText(activeSlotText, saveFile != null && saveFile.activeSlotId >= 0
                ? $"SLOT ACTIVO: {saveFile.activeSlotId + 1}"
                : "SIN SLOT ACTIVO");

            SetText(completedModulesText, (activeSlot?.data?.completedModuleIds?.Count ?? 0).ToString());
            SetText(achievementsText, (activeSlot?.data?.achievements?.Count ?? 0).ToString());

            float totalSeconds = saveFile?.slots?.Sum(slot => Mathf.Max(0f, slot.playTime)) ?? 0f;
            TimeSpan playTime = TimeSpan.FromSeconds(totalSeconds);
            SetText(totalPlayTimeText, $"{(int)playTime.TotalHours:00}:{playTime.Minutes:00}:{playTime.Seconds:00}");

            string latestSave = GetLatestSave(saveFile);
            SetText(lastSaveText, string.IsNullOrEmpty(latestSave) ? "Sin guardados" : latestSave);

            bool hasPendingChanges = saveFile?.slots?.Any(slot => slot.needsCloudSync) == true;
            SetText(syncStatusText, SessionContext.IsDebugSession
                ? "DATOS LOCALES"
                : hasPendingChanges ? "SINCRONIZACIÓN PENDIENTE" : "SINCRONIZADO");

            RefreshModuleDetail(selectedModuleIndex);
            RefreshBadges();
            RebuildProfileLayout();
        }

        private void ResolveReferences()
        {
            usernameText ??= FindText("ProfileBody/IdentityCard/UsernameText");
            emailText ??= FindText("ProfileBody/IdentityCard/EmailText");
            activeSlotText ??= FindText("ProfileBody/IdentityCard/ActiveSlotText");
            completedModulesText ??= FindText("ProfileBody/ProgressCard/ModulesRow/Value");
            achievementsText ??= FindText("ProfileBody/ProgressCard/AchievementsRow/Value");
            totalPlayTimeText ??= FindText("ProfileBody/ProgressCard/PlayTimeRow/Value");
            lastSaveText ??= FindText("ProfileBody/ProgressCard/LastSaveRow/Value");
            syncStatusText ??= FindText("Header/SyncStatus/Text (TMP)") ?? FindText("Header/SyncStatus");

            if (moduleButtons == null || moduleButtons.Length != 3) moduleButtons = new Button[3];
            if (badgeIcons == null || badgeIcons.Length != 3) badgeIcons = new Image[3];
            if (badgeStateTexts == null || badgeStateTexts.Length != 3) badgeStateTexts = new TMP_Text[3];
            for (int index = 0; index < 3; index++)
            {
                moduleButtons[index] ??= transform.Find($"ProfileBody/ProgressCard/ModuleTabs/ButtonM{index + 1}")?.GetComponent<Button>();
                badgeIcons[index] ??= transform.Find($"ProfileBody/ProgressCard/BadgesContainer/BadgeM{index + 1}/Icon")?.GetComponent<Image>();
                badgeStateTexts[index] ??= FindText($"ProfileBody/ProgressCard/BadgesContainer/BadgeM{index + 1}/State");
            }

            moduleNameText ??= FindText("ProfileBody/ProgressCard/ModuleDetail/ModuleName");
            moduleStatusText ??= FindText("ProfileBody/ProgressCard/ModuleDetail/Status");
            moduleObjectivesText ??= FindText("ProfileBody/ProgressCard/ModuleDetail/Objectives");
            moduleTimeText ??= FindText("ProfileBody/ProgressCard/ModuleDetail/PlayTime");
            moduleDateText ??= FindText("ProfileBody/ProgressCard/ModuleDetail/CompletionDate");
            moduleDetailRect ??= transform.Find("ProfileBody/ProgressCard/ModuleDetail") as RectTransform;
            progressCardRect ??= transform.Find("ProfileBody/ProgressCard") as RectTransform;
            if (moduleDetailRect != null && moduleDetailRect.TryGetComponent(out LayoutElement detailLayout))
                detailLayout.preferredHeight = 120f;
        }

        private void ConfigureModuleButtons()
        {
            if (moduleButtons == null)
                return;

            for (int index = 0; index < moduleButtons.Length; index++)
            {
                Button button = moduleButtons[index];
                if (button == null) continue;
                TMP_Text label = button.transform.Find("Label")?.GetComponent<TMP_Text>();
                if (label != null)
                {
                    label.text = index == 0 ? "M1" : $"M{index + 1} · BLOQ.";
                    label.fontSize = index == 0 ? 17 : 13;
                }
                int capturedIndex = index;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectModule(capturedIndex));
            }
        }

        private void SelectModule(int index)
        {
            selectedModuleIndex = Mathf.Clamp(index, 0, ModuleIds.Length - 1);
            RefreshModuleDetail(selectedModuleIndex);
        }

        private void RefreshModuleDetail(int index)
        {
            SetText(moduleNameText, ModuleNames[index]);
            ModuleProgress progress = FindModuleProgress(index);
            bool available = index == 0 || progress != null || HasCompletedModule(index) || HasAchievement(index);

            EnsureDetailTextVisible(moduleNameText);
            EnsureDetailTextVisible(moduleStatusText);
            EnsureDetailTextVisible(moduleObjectivesText);
            EnsureDetailTextVisible(moduleTimeText);
            EnsureDetailTextVisible(moduleDateText);

            if (!available)
            {
                SetText(moduleStatusText, "Estado: Módulo bloqueado");
                SetText(moduleObjectivesText, "Objetivos: —");
                SetText(moduleTimeText, "Tiempo: —");
                SetText(moduleDateText, "Finalización: —");
                SetColor(moduleStatusText, new Color32(243, 190, 85, 255));
            }
            else if (progress?.completed == true || HasCompletedModule(index))
            {
                int total = progress != null && progress.totalObjectives > 0 ? progress.totalObjectives : 8;
                int completed = progress != null && progress.completedObjectives > 0 ? progress.completedObjectives : total;
                SetText(moduleStatusText, "Estado: Completado");
                SetText(moduleObjectivesText, $"Objetivos: {completed}/{total}");
                SetText(moduleTimeText, $"Tiempo: {FormatDuration(progress?.playTime ?? activeSlot?.playTime ?? 0f)}");
                SetText(moduleDateText, $"Finalización: {FormatDate(progress?.completedAt ?? FindAchievementDate(index) ?? activeSlot?.lastSave)}");
                SetColor(moduleStatusText, new Color32(98, 214, 155, 255));
            }
            else
            {
                SetText(moduleStatusText, "Estado: Pendiente");
                SetText(moduleObjectivesText, "Objetivos: 0/8");
                SetText(moduleTimeText, $"Tiempo: {FormatDuration(progress?.playTime ?? 0f)}");
                SetText(moduleDateText, "Finalización: —");
                SetColor(moduleStatusText, new Color32(243, 190, 85, 255));
            }

            for (int buttonIndex = 0; moduleButtons != null && buttonIndex < moduleButtons.Length; buttonIndex++)
            {
                Image image = moduleButtons[buttonIndex]?.targetGraphic as Image;
                if (image != null)
                    image.color = buttonIndex == index
                        ? new Color32(52, 200, 255, 255)
                        : new Color32(14, 48, 81, 255);
            }

            RebuildProfileLayout();
        }

        private void RefreshBadges()
        {
            for (int index = 0; index < 3; index++)
            {
                bool unlocked = HasAchievement(index) || HasCompletedModule(index);
                if (badgeIcons != null && index < badgeIcons.Length && badgeIcons[index] != null)
                    badgeIcons[index].color = unlocked ? Color.white : new Color32(82, 98, 115, 140);
                if (badgeStateTexts != null && index < badgeStateTexts.Length)
                    SetText(badgeStateTexts[index], unlocked ? "OBTENIDA" : "BLOQUEADA");
            }
        }

        private ModuleProgress FindModuleProgress(int index) => activeSlot?.data?.modules?.Find(item =>
            string.Equals(item.moduleId, ModuleIds[index], StringComparison.OrdinalIgnoreCase) ||
            NormalizeModuleId(item.moduleId) == index + 1);

        private bool HasCompletedModule(int index) => activeSlot?.data?.completedModuleIds?.Any(id =>
            string.Equals(id, ModuleIds[index], StringComparison.OrdinalIgnoreCase) ||
            NormalizeModuleId(id) == index + 1) == true;

        private bool HasAchievement(int index) => activeSlot?.data?.achievements?.Any(item =>
            string.Equals(item.achievementId, AchievementIds[index], StringComparison.OrdinalIgnoreCase)) == true;

        private string FindAchievementDate(int index) => activeSlot?.data?.achievements?.FirstOrDefault(item =>
            string.Equals(item.achievementId, AchievementIds[index], StringComparison.OrdinalIgnoreCase))?.unlockedAt;

        private static int NormalizeModuleId(string value)
        {
            if (string.IsNullOrEmpty(value)) return -1;
            if (value.IndexOf('1') >= 0) return 1;
            if (value.IndexOf('2') >= 0) return 2;
            if (value.IndexOf('3') >= 0) return 3;
            return -1;
        }

        private static string FormatDuration(float seconds)
        {
            TimeSpan duration = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
            return $"{(int)duration.TotalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
        }

        private static string FormatDate(string value)
        {
            return DateTime.TryParse(value, out DateTime date) ? date.ToString("dd/MM/yyyy HH:mm") : "—";
        }

        private TMP_Text FindText(string path) => transform.Find(path)?.GetComponent<TMP_Text>();

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
                target.text = value;
        }

        private static void SetColor(TMP_Text target, Color color)
        {
            if (target != null)
                target.color = color;
        }

        private static void EnsureDetailTextVisible(TMP_Text target)
        {
            if (target == null) return;
            target.gameObject.SetActive(true);
            target.alpha = 1f;
            target.raycastTarget = false;
            if (target.TryGetComponent(out LayoutElement layout))
                layout.preferredHeight = target.name == "ModuleName" ? 24f : 20f;
        }

        private void RebuildProfileLayout()
        {
            Canvas.ForceUpdateCanvases();
            if (moduleDetailRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(moduleDetailRect);
            if (progressCardRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(progressCardRect);
        }

        private static string GetLatestSave(SaveFile saveFile)
        {
            if (saveFile?.slots == null)
                return null;

            DateTime latest = DateTime.MinValue;
            foreach (SaveSlot slot in saveFile.slots)
            {
                if (DateTime.TryParse(slot.lastSave, CultureInfo.CurrentCulture,
                        DateTimeStyles.AssumeLocal, out DateTime parsed) && parsed > latest)
                    latest = parsed;
            }

            return latest == DateTime.MinValue ? null : latest.ToString("dd/MM/yyyy HH:mm");
        }
    }
}
