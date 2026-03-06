
using System.Text.Json;
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicReportTableRelationComponent
{
  public partial class DynamicReportTableRelationDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter] public string? ID { get; set; }
    [Parameter] public string? DynamicReportID { get; set; }
    [Parameter] public string? DynamicReportTableID { get; set; }
    [Parameter] public string? MasterDynamicReportColumnID { get; set; }
    [Parameter] public JsonObject DynamicReport { get; set; } = [];
    [Parameter] public JsonObject DynamicReportTable { get; set; } = [];
    [Parameter] public JsonObject MasterDynamicReportColumn { get; set; } = [];
    #endregion

    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    SingleSelectLookup<JsonObject> columnLookup = null!;
    SingleSelectLookup<JsonObject> referenceReportTableLookup = null!;
    SingleSelectLookup<JsonObject> referenceColumnLookup = null!;
    Modal addRelationModal = null!;
    #endregion

    #region Field
    public JsonObject headerRow = new();
    public JsonObject ActiveRow = new();

    Dictionary<string, string> joinClauseDict = new Dictionary<string, string>()
    {
      ["INNER"] = "INNER",
      ["LEFT"] = "LEFT",
      ["RIGHT"] = "RIGHT",
    };
    Dictionary<string, string> operatorDict = new()
    {
      ["Equals"] = "=",
      ["Greater Than"] = ">",
      ["Greater Than or Equals"] = ">=",
      ["Less Than"] = "<",
      ["Less Than or Equals"] = "<=",
      ["Not Equals"] = "!=",
    };

    public bool IsPublished
    {
      get
      {
        return DynamicReport["IsPublished"]?.GetValue<int>() == 1;
      }
    }
    public bool IsReadOnly
    {
      get
      {
        return IsPublished;
      }
    }


    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {
      await GetRow();
      await base.OnParametersSetAsync();
    }
    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("DynamicReportTable", "GetRowByID", new
      {
        ID = DynamicReportTableID
      });

      if (res?.Data != null)
      {
        headerRow = res.Data;
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("DynamicReportTableRelation", "GetRows", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
        DynamicReportTableID
      });

      return res?.Data;
    }
    #endregion

    #region LoadLookup
    protected async Task<List<JsonObject>?> LoadMasterDynamicReportColumnLookup(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicReportColumn", "GetRowsForLookupByDynamicReportTable", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
        DynamicReportTableID
      });

      return res?.Data;
    }
    protected async Task<List<JsonObject>?> LoadReferenceDynamicReportTableLookup(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("DynamicReportTable", "GetRowsExclude", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
        DynamicReportTableID = DynamicReportTableID
      });

      return res?.Data;
    }
    protected async Task<List<JsonObject>?> LoadReferenceMasterDynamicReportColumnLookup(DataGridLoadArgs args)
    {

      var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicReportColumn", "GetRowsForLookupByDynamicReportTable", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
        DynamicReportTableID = headerRow["ReferenceDynamicReportTableID"]?.GetValue<string>()
      });
      return res?.Data;
    }
    #endregion

    #region Add
    private async void Add()
    {
      Loading.Show();

      await IFINICSClient.Post("DynamicReportTableRelation", "Insert", SetAuditInfo(new JsonObject()
      {
        ["ID"] = ID,
        ["DynamicReportTableID"] = DynamicReportTableID,
        ["SourceMasterDynamicReportColumnValue"] = "",
        ["ReferenceMasterDynamicReportColumnValue"] = "",
        ["Operator"] = "="
      }));

      await dataGrid.Reload();

      Loading.Close();
      // NavigationManager.NavigateTo($"setting/dynamicreportsetting/{DynamicReportTableID}/dynamicreporttableRelation/add");
    }
    #endregion
    #region Submit
    private async Task Submit(JsonObject data)
    {
      Loading.Show();

      var list = new List<JsonObject>();
      var dict = new Dictionary<string, JsonObject>(); // key: ID

      foreach (var (key, value) in data)
      {
        // Split key from the last underscore to preserve property names with underscores
        var lastUnderscore = key.LastIndexOf("_");
        if (lastUnderscore < 0) continue; // skip invalid keys

        var objKey = key.Substring(0, lastUnderscore);
        var id = key.Substring(lastUnderscore + 1);

        if (!dict.TryGetValue(id, out var rowObj))
        {
          // Find the row from the DataGrid
          var row = dataGrid.Data?.Find(r => r["ID"]?.GetValue<string>() == id);

          // Create a new JsonObject with audit info
          rowObj = SetAuditInfo(new JsonObject()
          {
            ["ID"] = id,
            ["DynamicReportTableID"] = DynamicReportTableID,
            ["SourceMasterDynamicReportColumnValue"] = row?["SourceMasterDynamicReportColumnValue"]?.GetValue<string>(),
            ["ReferenceMasterDynamicReportColumnValue"] = row?["ReferenceMasterDynamicReportColumnValue"]?.GetValue<string>(),
            ["SourceMasterDynamicReportColumnID"] = row?["SourceMasterDynamicReportColumnID"]?.GetValue<string>(),
            ["ReferenceMasterDynamicReportColumnID"] = row?["ReferenceMasterDynamicReportColumnID"]?.GetValue<string>(),
            ["Operator"] = row?["Operator"]?.GetValue<string>()
          });

          dict[id] = rowObj;
          list.Add(rowObj);
        }

        // Assign the updated value
        rowObj[objKey] = value?.DeepClone();
      }

      try
      {
        // Send updated list to backend
        await IFINICSClient.Put("DynamicReportTableRelation", "UpdateByID", list);

        // Reload DataGrid after successful save
        await dataGrid.Reload();
      }
      finally
      {
        Loading.Close();
        StateHasChanged();
      }
    }
    #endregion

    #region Delete
    private async void Delete()
    {
      var selectedData = dataGrid.selectedData;

      if (!selectedData.Any())
      {
        await NoDataSelectedAlert();
        return;
      }

      bool? result = await Confirm();

      if (result == true)
      {
        Loading.Show();

        List<string?> id = dataGrid.selectedData.Select(row => row["ID"]?.GetValue<string>()).ToList();

        await IFINICSClient.Delete("DynamicReportTableRelation", "DeleteByID", id);

        await dataGrid.Reload();
        dataGrid.selectedData.Clear();

        Loading.Close();

        StateHasChanged();
      }
    }
    #endregion

    #region SelectColumn
    void SelectColumn(JsonObject select)
    {
      ActiveRow["SourceMasterDynamicReportColumnID"] = select["ID"]?.DeepClone();
      ActiveRow["SourceMasterDynamicReportColumnValue"] = select["Alias"]?.DeepClone();
    }
    #endregion

    #region SelectReferenceTable
    void SelectReferenceTable(JsonObject select)
    {
      ActiveRow["ReferenceDynamicReportTableID"] = select["ID"]?.DeepClone();
      ActiveRow["ReferenceTableAlias"] = select["Alias"]?.DeepClone();
    }
    #endregion

    #region SelectReferenceColumn
    void SelectReferenceColumn(JsonObject select)
    {
      ActiveRow["ReferenceMasterDynamicReportColumnID"] = select["ID"]?.DeepClone();
      ActiveRow["ReferenceMasterDynamicReportColumnValue"] = select["Alias"]?.DeepClone();

    }
    #endregion

    #region Reload
    public async Task Reload()
    {
      await dataGrid.Reload();
      StateHasChanged();
    }
    #endregion

    #region CloseModal
    public void CloseModal()
    {
      addRelationModal.Close();
      StateHasChanged();
    }
    #endregion

    #region InputValueChanged from
    private void InputValueChanged(string value, JsonObject row)
    {
      if (row == null) return; // safety check

      row["ReferenceMasterDynamicReportColumnID"] = null;
      row["ReferenceMasterDynamicReportColumnValue"] = JsonValue.Create(value);
    }
    #endregion
  }
}