using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.UISystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalFlux.SaveSystem
{
    public class SaveListPanelUI : MonoBehaviour
    {
        public static SaveListPanelUI Instance { get; private set; }

        [Header("Panel")]
        public GameObject root;
        public TextMeshProUGUI titleText;

        [Header("Tabs")]
        public Button buildsTab;
        public Button runsTab;
        public Image buildsTabHighlight;
        public Image runsTabHighlight;

        [Header("Search")]
        public TMP_InputField searchField;
        [Tooltip("Builds a default search field at runtime when none is assigned")]
        public bool createSearchFieldIfMissing = true;
        public Vector2 searchFieldSize = new(360f, 44f);
        public Vector2 searchFieldOffset = new(0f, -30f);
        public string searchPlaceholder = "Search saves (name, attack, mode...)";

        [Header("Sort")]
        public TMP_Dropdown sortDropdown;

        [Header("List")]
        public Transform listContent;
        public SaveRowUI rowPrefab;
        public TextMeshProUGUI emptyText;

        [Header("Actions")]
        public Button loadButton;
        public Button renameButton;
        public Button deleteButton;
        public Button closeButton;

        private static readonly string[] SortLabels = { "Date", "Wave", "Highest DPS", "Name" };

        private readonly List<SaveRowUI> rows = new();
        private SaveKind current = SaveKind.Build;
        private SaveFile selected;
        private bool pushedPause;
        private int shownCount;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            Instance = this;
            if (root == null) root = gameObject;

            SetupSearchField();
            SetupSort();
            SetupListLayout();

            if (buildsTab != null) buildsTab.onClick.AddListener(ShowBuilds);
            if (runsTab != null) runsTab.onClick.AddListener(ShowRuns);
            if (loadButton != null) loadButton.onClick.AddListener(OnLoad);
            if (renameButton != null) renameButton.onClick.AddListener(OnRename);
            if (deleteButton != null) deleteButton.onClick.AddListener(OnDelete);
            if (closeButton != null) closeButton.onClick.AddListener(Close);

            root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (buildsTab != null) buildsTab.onClick.RemoveListener(ShowBuilds);
            if (runsTab != null) runsTab.onClick.RemoveListener(ShowRuns);
            if (loadButton != null) loadButton.onClick.RemoveListener(OnLoad);
            if (renameButton != null) renameButton.onClick.RemoveListener(OnRename);
            if (deleteButton != null) deleteButton.onClick.RemoveListener(OnDelete);
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (searchField != null) searchField.onValueChanged.RemoveListener(OnSearchChanged);
            if (sortDropdown != null) sortDropdown.onValueChanged.RemoveListener(OnSortChanged);

            ReleasePause();
            if (ReferenceEquals(Instance, this)) Instance = null;
        }

        private void SetupSearchField()
        {
            if (searchField == null && createSearchFieldIfMissing)
            {
                GameObject go = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
                go.name = "SaveSearch";
                go.transform.SetParent(root.transform, false);

                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = searchFieldSize;
                rt.anchoredPosition = searchFieldOffset;

                if (go.TryGetComponent<Image>(out var bg)) bg.color = new Color(0.08f, 0.08f, 0.1f, 0.9f);

                searchField = go.GetComponent<TMP_InputField>();
                if (searchField.textComponent != null) searchField.textComponent.color = Color.white;
                if (searchField.placeholder is TMP_Text ph)
                {
                    ph.text = searchPlaceholder;
                    ph.color = new Color(1f, 1f, 1f, 0.4f);
                }
            }

            if (searchField == null) return;

            searchField.restoreOriginalTextOnEscape = false;
            searchField.onValueChanged.AddListener(OnSearchChanged);
        }

        private void SetupListLayout()
        {
            if (listContent == null) return;

            if (!listContent.TryGetComponent<VerticalLayoutGroup>(out var group))
                group = listContent.gameObject.AddComponent<VerticalLayoutGroup>();

            group.spacing = 6f;
            group.padding = new RectOffset(6, 6, 6, 6);
            group.childAlignment = TextAnchor.UpperCenter;
            group.childControlWidth = true;
            group.childControlHeight = false;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            if (!listContent.TryGetComponent<ContentSizeFitter>(out var fitter))
                fitter = listContent.gameObject.AddComponent<ContentSizeFitter>();

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void SetupSort()
        {
            if (sortDropdown == null) return;

            sortDropdown.ClearOptions();
            sortDropdown.AddOptions(new List<string>(SortLabels));
            sortDropdown.SetValueWithoutNotify(0);
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
        }

        public static bool IsTyping => Instance != null && Instance.IsOpen && Instance.searchField != null && Instance.searchField.isFocused;

        public void Open(SaveKind tab)
        {
            current = tab;
            selected = null;

            root.SetActive(true);
            root.transform.SetAsLastSibling();

            if (!pushedPause)
            {
                RunSession.PushPause?.Invoke();
                pushedPause = true;
            }

            ApplyButtonTooltips();
            Refresh();
        }

        private void ApplyButtonTooltips()
        {
            SetTooltip(loadButton, "Load", "Load the selected save. Loading a run removes its save.");
            SetTooltip(renameButton, "Rename", "Rename the selected save.");
            SetTooltip(deleteButton, "Delete", "Permanently delete the selected save.");
            SetTooltip(closeButton, "Close", "Close this window.");
            SetTooltip(buildsTab, "Builds", "Show saved builds.");
            SetTooltip(runsTab, "Runs", "Show saved runs.");
        }

        private static void SetTooltip(Button b, string title, string description)
        {
            if (b == null) return;
            if (!b.TryGetComponent<ITooltipDisplay>(out var tt)) tt = b.gameObject.AddComponent<TooltipTrigger>();
            tt.ShowTooltip(title, description);
        }

        public void Close()
        {
            if (!IsOpen) return;

            root.SetActive(false);
            ReleasePause();
        }

        private void ReleasePause()
        {
            if (!pushedPause) return;

            pushedPause = false;
            RunSession.PopPause?.Invoke();
        }

        private void ShowBuilds() => SwitchTab(SaveKind.Build);
        private void ShowRuns() => SwitchTab(SaveKind.Run);

        private void SwitchTab(SaveKind kind)
        {
            if (current == kind) return;

            current = kind;
            selected = null;
            Refresh();
        }

        private void OnSearchChanged(string _) => Refresh();
        private void OnSortChanged(int _) => Refresh();

        private SaveSort Sort => sortDropdown != null ? (SaveSort)Mathf.Clamp(sortDropdown.value, 0, SortLabels.Length - 1) : SaveSort.Date;

        public void Refresh()
        {
            if (!IsOpen) return;

            SaveAssetCatalog cat = SaveAssetCatalog.Instance;
            List<SaveFile> shown = SaveQuery.Apply(SaveStore.List(current), searchField != null ? searchField.text : "", Sort);

            if (selected != null && !shown.Exists(f => f.meta.id == selected.meta.id)) selected = null;

            EnsureRows(shown.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                bool on = i < shown.Count;
                rows[i].gameObject.SetActive(on);
                if (!on) continue;

                SaveFile f = shown[i];
                rows[i].Bind(f, SaveAssetCatalog.CompatibilityIssue(f.meta, cat), selected != null && selected.meta.id == f.meta.id, OnRowClicked);
            }

            if (titleText != null) titleText.text = current == SaveKind.Build ? "Builds" : "Runs";
            shownCount = shown.Count;
            UpdateStatusText();
            if (buildsTabHighlight != null) buildsTabHighlight.enabled = current == SaveKind.Build;
            if (runsTabHighlight != null) runsTabHighlight.enabled = current == SaveKind.Run;

            UpdateButtons();
        }

        private void UpdateStatusText()
        {
            if (emptyText == null) return;

            string current = $"Current: {(selected != null ? selected.meta.name : "none")}";
            emptyText.text = shownCount == 0 ? $"List is Empty\n{current}" : current;
            emptyText.gameObject.SetActive(true);
        }

        private void EnsureRows(int count)
        {
            if (rowPrefab == null || listContent == null) return;

            while (rows.Count < count)
            {
                SaveRowUI row = Instantiate(rowPrefab, listContent);
                rows.Add(row);
            }
        }

        private void OnRowClicked(SaveRowUI row)
        {
            selected = row.File;
            foreach (SaveRowUI r in rows)
                if (r.gameObject.activeSelf) r.SetSelected(r.File != null && r.File.meta.id == selected.meta.id);

            UpdateButtons();
            UpdateStatusText();
        }

        private SaveRowUI SelectedRow()
        {
            if (selected == null) return null;

            foreach (SaveRowUI r in rows)
                if (r.gameObject.activeSelf && r.File != null && r.File.meta.id == selected.meta.id) return r;

            return null;
        }

        private void UpdateButtons()
        {
            SaveRowUI row = SelectedRow();
            bool has = row != null;

            if (loadButton != null) loadButton.interactable = has && row.Issue == null && !row.File.meta.readOnly && RunSaveController.Instance != null;
            if (renameButton != null) renameButton.interactable = has;
            if (deleteButton != null) deleteButton.interactable = has;
        }

        private void OnLoad()
        {
            SaveRowUI row = SelectedRow();
            if (row == null || row.Issue != null || row.File.meta.readOnly || RunSaveController.Instance == null) return;

            SaveFile f = row.File;
            if (f.meta.kind == SaveKind.Run)
            {
                if (ConfirmDialogUI.Instance == null) return;

                ConfirmDialogUI.Instance.Show($"Load '{f.meta.name}'?\nThe save is removed once loaded.", () =>
                {
                    Close();
                    RunSaveController.Instance.QueueLoad(f);
                });
                return;
            }

            Close();
            RunSaveController.Instance.QueueLoad(f);
        }

        private void OnRename()
        {
            SaveRowUI row = SelectedRow();
            if (row == null || SaveNamePromptUI.Instance == null) return;

            SaveFile f = row.File;
            SaveNamePromptUI.Instance.Show("Rename", f.meta.name, f.meta.kind, f.meta.id, (name, overwrite) =>
            {
                if (overwrite)
                {
                    SaveFile other = SaveStore.FindByName(f.meta.kind, name);
                    if (other != null && other.meta.id != f.meta.id) SaveStore.Delete(other.meta.kind, other.meta.id);
                }

                bool ok = SaveStore.Rename(f.meta.kind, f.meta.id, name);
                Refresh();
                return ok;
            });
        }

        private void OnDelete()
        {
            SaveRowUI row = SelectedRow();
            if (row == null || ConfirmDialogUI.Instance == null) return;

            SaveFile f = row.File;
            ConfirmDialogUI.Instance.Show($"Delete '{f.meta.name}'?", () =>
            {
                SaveStore.Delete(f.meta.kind, f.meta.id);
                selected = null;
                Refresh();
            });
        }
    }
}
