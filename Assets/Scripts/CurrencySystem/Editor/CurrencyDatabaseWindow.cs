using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using CurrencySystem;
using Codice.CM.Common.Matcher;

public class CurrencyDatabaseWindow : EditorWindow
{
    private const int RowHeight = 64;

    // 상태
    private CurrencyDatabaseSO _db;
    private SerializedObject _so;
    private SerializedProperty _itemsProp;

    // UI
    private ToolbarSearchField _searchField;
    private string searchText;
    private Label _dbPathLabel;
    private MultiColumnListView _table;
    private VisualElement _footer;

    // index mapping
    private readonly List<int> _viewToModel = new List<int>();

    [MenuItem("Tools/Currency/Currency Database Window")]
    public static void Open()
    {
        CurrencyDatabaseWindow window = GetWindow<CurrencyDatabaseWindow>("Currency Database");
        window.minSize = new Vector2(900, 520);
    }

    private void OnEnable()
    {
        ConstructUI();
        TryAutoBindDatabase();
        RefreshView();
    }

    private void ConstructUI()
    {
        rootVisualElement.styleSheets.Clear();
        // toolbar
        Toolbar toolbar = new Toolbar();

        ToolbarButton selectBtn = new ToolbarButton(() =>
        {
            string path = EditorUtility.OpenFilePanel("Select CurrencyDatabaseSO", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                CurrencyDatabaseSO asset = AssetDatabase.LoadAssetAtPath<CurrencyDatabaseSO>(relativePath);
                BindDatabase(asset);
            }
        })
        { text = "Select DB" };

        ToolbarButton addBtn = new ToolbarButton(AddRow) { text = "Add" };
        ToolbarButton delBtn = new ToolbarButton(DeleteSelected) { text = "Delete" };

        _searchField = new ToolbarSearchField();
        _searchField.style.minWidth = 240;
        _searchField.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
        {
            searchText = evt.newValue ?? string.Empty;
            RefreshView();
        });

        toolbar.Add(selectBtn);
        toolbar.Add(new Label("  "));
        toolbar.Add(addBtn);
        toolbar.Add(new Label("  "));
        toolbar.Add(delBtn);
        toolbar.Add(new Label("  "));
        toolbar.Add(new Label("Search:"));
        toolbar.Add(_searchField);

        rootVisualElement.Add(toolbar);

        // DB Path label
        _dbPathLabel = new Label("<No Database>");
        _dbPathLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
        _dbPathLabel.style.marginLeft = 8;
        _dbPathLabel.style.marginTop = 4;
        _dbPathLabel.style.marginBottom = 4;
        rootVisualElement.Add(_dbPathLabel);

        // table
        _table = new MultiColumnListView
        {
            selectionType = SelectionType.Single,
            showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly
        };
        _table.style.flexGrow = 1f;
        _table.fixedItemHeight = RowHeight;

        Column cIndex = new Column
        {
            title = " ",
            width = 24,
            minWidth = 24,
            maxWidth = 48,
            makeCell = () =>
            {
                Label lb = new Label();
                lb.style.unityTextAlign = TextAnchor.MiddleRight; // 정렬(우측 추천)
                lb.style.paddingRight = 8;
                return lb;
            },
            bindCell = BindIndexCell
        };

        Column cIcon = new Column
        {
            title = "Icon",
            width = RowHeight,
            minWidth = RowHeight,
            maxWidth = RowHeight,
            makeCell = MakeIconCell,
            bindCell = BindIconCell
        };

        Column cTitle = new Column
        {
            title = "Title",
            width = 200,
            makeCell = () => MakeStretchTextField("title-field", false),
            bindCell = BindTitleCell
        };

        Column cDesc = new Column
        {
            title = "Description",
            width = 360,
            makeCell = () => MakeStretchTextField("desc-field", true),
            bindCell = BindDescriptionCell
        };

        Column cPermanent = new Column
        {
            title = "IsPermanent",
            width = 120,
            makeCell = () =>
            {
                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.justifyContent = Justify.Center;
                row.style.minWidth = 0;
                row.style.height = RowHeight;

                Toggle tg = new Toggle { name = "perm-toggle" };
                tg.style.height = RowHeight;
                row.Add(tg);
                return row;
            },
            bindCell = BindPermanentCell
        };

        _table.columns.Add(cIndex);
        _table.columns.Add(cIcon);
        _table.columns.Add(cTitle);
        _table.columns.Add(cDesc);
        _table.columns.Add(cPermanent);

        // mouse right click
        _table.RegisterCallback<ContextualMenuPopulateEvent>((ContextualMenuPopulateEvent evt) =>
        {
            bool hasSel = _table.selectedIndex >= 0;

            evt.menu.AppendAction("Add", (DropdownMenuAction _) => AddRow());
            evt.menu.AppendAction(
                "Delete",
                (DropdownMenuAction _) => DeleteSelected(),
                (DropdownMenuAction _) => hasSel ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled
            );

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Validate Titles", (DropdownMenuAction _) => ValidateTitles());
        });

        rootVisualElement.Add(_table);

        // footer
        _footer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                paddingLeft = 8, paddingTop = 4, paddingBottom = 4
            }
        };
        Button validateBtn = new Button(ValidateTitles) { text = "Validate Titles" };
        Button pingBtn = new Button(() => { if (_db != null) EditorGUIUtility.PingObject(_db); }) { text = "Find DB Asset" };
        Button saveBtn = new Button(SaveIfDirty) { text = "Save" };

        _footer.Add(validateBtn);
        _footer.Add(new Label("  "));
        _footer.Add(pingBtn);
        _footer.Add(new Label("  "));
        _footer.Add(saveBtn);
        rootVisualElement.Add(_footer);
    }

    private void TryAutoBindDatabase()
    {
        if (Selection.activeObject is CurrencyDatabaseSO dbSel)
        {
            BindDatabase(dbSel);
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:CurrencyDatabaseSO");
        if (guids != null && guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CurrencyDatabaseSO asset = AssetDatabase.LoadAssetAtPath<CurrencyDatabaseSO>(path);
            BindDatabase(asset);
        }
    }

    private void BindDatabase(CurrencyDatabaseSO db)
    {
        _db = db;
        if (_db == null)
        {
            _so = null;
            _itemsProp = null;
            _dbPathLabel.text = "<No Database>";
            RefreshView();
            return;
        }

        _so = new SerializedObject(_db);
        _itemsProp = _so.FindProperty("items");
        _dbPathLabel.text = AssetDatabase.GetAssetPath(_db);
        RefreshView();
    }

    private void RefreshView()
    {
        _viewToModel.Clear();

        if (_db != null && _itemsProp != null && _itemsProp.isArray)
        {
            List<int> indices = Enumerable.Range(0, _db.Items.Count).ToList();

            // search field filter
            if (!string.IsNullOrEmpty(searchText))
            {
                indices = indices.Where(i =>
                {
                    Currency currency = _db.Items[i];
                    string t = currency != null ? (currency.Title ?? string.Empty) : string.Empty;
                    string d = currency != null ? (currency.Description ?? string.Empty) : string.Empty;
                    return t.Contains(searchText) || d.Contains(searchText);
                }).ToList();
            }

            _viewToModel.AddRange(indices);
        }

        _table.itemsSource = _viewToModel;
        _table.Rebuild();
        titleContent.text = $"Currency Database ({_viewToModel.Count})";
    }

    private VisualElement MakeIconCell()
    {
        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.justifyContent = Justify.Center;
        row.style.height = RowHeight;

        ObjectField field = new ObjectField
        {
            name = "icon-field",
            objectType = typeof(Sprite),
            allowSceneObjects = false,
            label = string.Empty
        };
        field.style.width = RowHeight;
        field.style.height = RowHeight;

        row.Add(field);
        return row;
    }

    private VisualElement MakeStretchTextField(string name, bool multiline)
    {
        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.flexGrow = 1;
        row.style.minWidth = 0;
        row.style.height = RowHeight;

        TextField tf = new TextField
        {
            name = name,
            label = string.Empty,
            isDelayed = true,
            multiline = multiline
        };
        tf.style.flexGrow = 1;
        tf.style.minWidth = 0;
        tf.style.width = Length.Percent(100);
        tf.style.height = RowHeight;

        row.Add(tf);
        return row;
    }

    private void BindIndexCell(VisualElement ve, int viewIndex)
    {
        Label lb = ve as Label;
        if (lb == null) return;

        int displayIndex = viewIndex + 1;
        lb.text = displayIndex.ToString();
    }

    private void BindIconCell(VisualElement ve, int viewIndex)
    {
        ObjectField field = ve.Q<ObjectField>("icon-field");
        if (field == null) return;

        if (_db == null || _itemsProp == null || viewIndex < 0 || viewIndex >= _viewToModel.Count)
        {
            field.SetValueWithoutNotify(null);
            return;
        }

        int modelIndex = _viewToModel[viewIndex];
        _so.Update();
        SerializedProperty elementProp = _itemsProp.GetArrayElementAtIndex(modelIndex);
        SerializedProperty iconProp = elementProp.FindPropertyRelative("icon");
        UnityEngine.Object current = iconProp != null ? iconProp.objectReferenceValue : null;
        field.SetValueWithoutNotify(current);

        field.UnregisterValueChangedCallback(OnIconChanged);
        field.userData = modelIndex;
        field.RegisterValueChangedCallback(OnIconChanged);

        void OnIconChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            if (!(field.userData is int idx)) return;

            _so.Update();
            SerializedProperty elem = _itemsProp.GetArrayElementAtIndex(idx);
            SerializedProperty p = elem.FindPropertyRelative("icon");
            if (p != null)
            {
                Undo.RecordObject(_db, "Edit Currency Icon");
                p.objectReferenceValue = evt.newValue;
                _so.ApplyModifiedProperties();
                MarkDirty();
                _table.RefreshItems();
            }
        }
    }

    private void BindTitleCell(VisualElement ve, int viewIndex)
    {
        TextField tf = ve.Q<TextField>("title-field") ?? ve as TextField;
        if (tf == null) return;

        if (_db == null || _itemsProp == null || viewIndex < 0 || viewIndex >= _viewToModel.Count)
        {
            tf.Unbind();
            tf.SetValueWithoutNotify(string.Empty);
            return;
        }

        int modelIndex = _viewToModel[viewIndex];
        _so.UpdateIfRequiredOrScript();
        SerializedProperty elementProp = _itemsProp.GetArrayElementAtIndex(modelIndex);
        SerializedProperty titleProp = elementProp.FindPropertyRelative("title");

        tf.Unbind();                 // 기존 바인딩/콜백 정리
        tf.isDelayed = true;         // Blur/Enter 시에만 커밋
        if (titleProp != null)
            tf.BindProperty(titleProp);
        else
            tf.SetValueWithoutNotify(string.Empty);
    }

    private void BindDescriptionCell(VisualElement ve, int viewIndex)
    {
        TextField tf = ve.Q<TextField>("desc-field") ?? ve as TextField;
        if (tf == null) return;

        if (_db == null || _itemsProp == null || viewIndex < 0 || viewIndex >= _viewToModel.Count)
        {
            tf.Unbind();
            tf.SetValueWithoutNotify(string.Empty);
            return;
        }

        int modelIndex = _viewToModel[viewIndex];
        _so.UpdateIfRequiredOrScript();
        SerializedProperty elementProp = _itemsProp.GetArrayElementAtIndex(modelIndex);
        SerializedProperty descProp = elementProp.FindPropertyRelative("description");

        tf.Unbind();
        tf.isDelayed = true;         // Blur/Enter 시에만 커밋
        if (descProp != null)
            tf.BindProperty(descProp);
        else
            tf.SetValueWithoutNotify(string.Empty);
    }

    private void BindPermanentCell(VisualElement ve, int viewIndex)
    {
        Toggle tg = ve.Q<Toggle>("perm-toggle") ?? ve as Toggle;
        if (tg == null) return;

        if (_db == null || _itemsProp == null || viewIndex < 0 || viewIndex >= _viewToModel.Count)
        {
            tg.SetValueWithoutNotify(false);
            return;
        }

        int modelIndex = _viewToModel[viewIndex];
        _so.Update();
        SerializedProperty elem = _itemsProp.GetArrayElementAtIndex(modelIndex);
        SerializedProperty permProp = elem.FindPropertyRelative("isPermanent");

        bool current = permProp != null ? permProp.boolValue : false;
        tg.SetValueWithoutNotify(current);

        tg.UnregisterValueChangedCallback(OnPermChanged);
        tg.userData = modelIndex;
        tg.RegisterValueChangedCallback(OnPermChanged);

        void OnPermChanged(ChangeEvent<bool> evt)
        {
            if (!(tg.userData is int idx)) return;

            _so.Update();
            SerializedProperty e = _itemsProp.GetArrayElementAtIndex(idx);
            SerializedProperty p = e.FindPropertyRelative("isPermanent");
            if (p != null)
            {
                Undo.RecordObject(_db, "Edit Currency IsPermanent");
                p.boolValue = evt.newValue;
                _so.ApplyModifiedProperties();
                MarkDirty();
                _table.RefreshItems();
            }
        }
    }

    private void AddRow()
    {
        if (_db == null)
        {
            EditorUtility.DisplayDialog("No DB", "Select or create a CurrencyDatabaseSO first.", "OK");
            return;
        }

        Undo.RecordObject(_db, "Add Currency");

        _so.Update();
        SerializedProperty itemsProp = _so.FindProperty("items");
        int newIndex = itemsProp.arraySize;
        itemsProp.arraySize++;
        _so.ApplyModifiedPropertiesWithoutUndo();

        _so.Update();
        SerializedProperty elem = _itemsProp.GetArrayElementAtIndex(newIndex);

        SerializedProperty titleProp = elem.FindPropertyRelative("title");
        SerializedProperty descProp = elem.FindPropertyRelative("description");
        SerializedProperty iconProp = elem.FindPropertyRelative("icon");
        SerializedProperty permProp = elem.FindPropertyRelative("isPermanent");

        if (titleProp != null) titleProp.stringValue = "New Currency";
        if (descProp != null) descProp.stringValue = string.Empty;
        if (iconProp != null) iconProp.objectReferenceValue = null;
        if (permProp != null) permProp.boolValue = false;

        _so.ApplyModifiedProperties();

        MarkDirty();
        RefreshView();
        _table.selectedIndex = _table.itemsSource.Count - 1;
    }

    private void DeleteSelected()
    {
        if (_db == null) return;
        int sel = _table.selectedIndex;
        if (sel < 0) return;

        int modelIndex = _viewToModel[sel];
        Currency cur = _db.Items[modelIndex];
        string titleText = cur != null ? cur.Title : "(unknown)";

        bool ok = EditorUtility.DisplayDialog(
            "Delete",
            $"Delete currency at row {sel}?\n(Title: {titleText})",
            "Delete", "Cancel"
        );
        if (!ok) return;

        Undo.RecordObject(_db, "Delete Currency");

        _so.Update();
        SerializedProperty itemsProp = _so.FindProperty("items");
        itemsProp.DeleteArrayElementAtIndex(modelIndex);
        _so.ApplyModifiedPropertiesWithoutUndo();

        MarkDirty();
        RefreshView();
    }

    private void ValidateTitles()
    {
        if (_db == null) return;

        List<string> dups = _db.Items
            .Where((Currency d) => !string.IsNullOrEmpty(d.Title))
            .GroupBy((Currency d) => d.Title)
            .Where((IGrouping<string, Currency> g) => g.Count() > 1)
            .Select((IGrouping<string, Currency> g) => g.Key)
            .ToList();

        if (dups.Count > 0)
            EditorUtility.DisplayDialog("Duplicate Titles", "Change name \n" + string.Join("\n", dups), "OK");
        else
            EditorUtility.DisplayDialog("Validate", "OK (No duplicate Titles)", "Great");
    }

    private void SaveIfDirty()
    {
        if (_db == null) return;
        EditorUtility.SetDirty(_db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ShowNotification(new GUIContent("Saved"));
    }

    private void MarkDirty()
    {
        if (_db != null)
            EditorUtility.SetDirty(_db);
    }
}