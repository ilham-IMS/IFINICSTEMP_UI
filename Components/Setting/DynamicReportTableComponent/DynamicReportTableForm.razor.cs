using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicReportTableComponent
{
  public partial class DynamicReportTableForm
  {

    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter] public string? ID { get; set; }
    [Parameter] public string? DynamicReportID { get; set; }
    [Parameter] public EventCallback<JsonObject> RowChanged { get; set; }
    [Parameter] public EventCallback<JsonObject> RowDynamicReportChanged { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion

    #region Component Field
    SingleSelectLookup<JsonObject> tableLookup = null!;
    SingleSelectLookup<JsonObject> referenceReportTableLookup = null!;
    #endregion

    #region Field
    public JsonObject row = new();
    public JsonObject rowDynamicReport = new();
    JsonObject masterForm = new();
		List<ExtendModel>? extend = new();
		List<FormControlsModel> controls = new();
    Dictionary<string, string> joinClauseDict = new Dictionary<string, string>()
    {
      ["INNER"] = "INNER",
      ["LEFT"] = "LEFT",
      ["RIGHT"] = "RIGHT",
    };

    RenderFragment Form => builder =>
    {
      int seq = 0;

      foreach (var control in controls)
      {
        DynamicRenderForm(builder, ref seq, control);
      }
    };

    public bool IsReadOnly
    {
      get
      {
        return rowDynamicReport["IsPublished"]?.GetValue<int>() == 1;
      }
    }
    #endregion

    #region OnParametersSet
    protected override async Task OnParametersSetAsync()
    {
      masterForm = await LoadMasterForm("DRT");
      controls = await LoadFormControls(masterForm["ID"]?.GetValue<string>());
      if (ID != null && row["ID"] == null)
      {
        await GetRow();
        var extRes = await IFINICSClient.GetRows<ExtendModel>("DynamicReportTable", "GetRowByExt", new { ID = ID });
        extend = extRes?.Data;

        var controlNames = controls.Select(control => control.Name).ToHashSet();
        extend = extend?.Where(ext => controlNames.Contains(ext.Keyy)).ToList();

        AddExtendProperty(controls, extend, row);

      }
      else
      {
        row["DynamicReportID"] = DynamicReportID;
        row["Properties"] = JsonValue.Create(controls.ToDictionary(x => x.Name, x => x.Value));
      }

      if (DynamicReportID != null && rowDynamicReport["ID"] == null) await GetDynamicReport();
      SetInitialValue(row, controls);
      await base.OnParametersSetAsync();
    }
    #endregion 

    #region Load Dynamic Form
    private async Task<JsonObject> LoadMasterForm(string code)
    {
      var res = await IFINICSClient.GetRow<JsonObject>("MasterForm", "GetRowByCode", new { code });
      return res?.Data ?? [];
    }
    private async Task<List<FormControlsModel>> LoadFormControls(string formID)
    {
      var res = await IFINICSClient.GetRows<FormControlsModel>("FormControls", "GetRows", new { MasterFormID = formID });
      return res?.Data ?? [];
    }
    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("DynamicReportTable", "GetRowByID", new
      {
        ID = ID
      });

      if (res?.Data != null)
      {
        row = res.Data;
        await RowChanged.InvokeAsync(row);
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region GetDynamicReport
    public async Task GetDynamicReport()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("DynamicReport", "GetRowByID", new
      {
        ID = DynamicReportID
      });

      if (res?.Data != null)
      {
        rowDynamicReport = res.Data;
        await RowDynamicReportChanged.InvokeAsync(rowDynamicReport);
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region Load Lookup
    protected async Task<List<JsonObject>?> LoadDynamicReportTableLookup(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("DynamicReportTable", "GetRows", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
        dynamicReportUserID = DynamicReportID
      });

      return res?.Data?.Where(x => x["ID"]?.ToString() != ID).ToList();
    }
    protected async Task<List<JsonObject>?> LoadMasterDynamicReportTableLookup(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicReportTable", "GetRowsForLookup", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit
      });

      return res?.Data;
    }
    protected async Task<List<JsonObject>?> LoadMasterDynamicReportColumnLookup(DataGridLoadArgs args, string? masterDynamicReportTableID)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicReportColumn", "GetRowsForLookup", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
        masterDynamicReportTableID
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
        DynamicReportTableID = ID
      });

      return res?.Data;
    }
    #endregion

    #region OnSubmit
    private async void OnSubmit(JsonObject data)
    {
      Loading.Show();

      data = SetAuditInfo(data);

      data = row.Merge(data);

      SetExtensionProperties(data, controls, "Properties");

      #region Insert
      if (ID == null)
      {
        var res = await IFINICSClient.Post("DynamicReportTable", "Insert", data);

        if (res?.Data != null)
        {
          // var resReference = await IFINICSClient.GetRows("MasterDynamicReportColumn", "GetRowsForeignReferenceToTable", new
          // {
          //   DynamicReportID = DynamicReportID,
          //   MasterDynamicReportTableID = data["MasterDynamicReportTableID"]?.ToString()
          // });

          // var references = resReference?.Data ?? [];

          // if (references.Count > 0)
          // {
          //   var confirm = await Confirm($"{references.Select(x => x["TableName"]?.GetValue<string>()).Aggregate((x, y) => $"{x}, {y}")} have relation with this table. Do you want to add this table as their foreign reference?");

          //   if (confirm == true)
          //   {
          //     var tableRelations = references.Select(x => SetAuditInfo(
          //       new JsonObject
          //       {
          //         ["MasterDynamicReportColumnID"] = x["ID"]?.DeepClone(),
          //         ["DynamicReportTableID"] = x["ReportTableID"]?.DeepClone(),
          //         ["ReferenceDynamicReportTableID"] = res.Data["ID"]?.DeepClone(),
          //         ["ReferenceMasterDynamicReportColumnID"] = x["ColumnReferenceID"]?.DeepClone(),
          //       }
          //     ));

          //     var resRelation = await IFINICSClient.Post("DynamicReportTableRelation", "Insert", tableRelations);
          //   }
          // }

          NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicreportsetting/{DynamicReportID}/dynamicreporttable/{res.Data["ID"]}");

        }
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINICSClient.Put("DynamicReportTable", "UpdateByID", data);
        await GetRow();
      }

      Loading.Close();
      StateHasChanged();
      #endregion
    }
    #endregion

    #region Back
    private void Back()
    {
      NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicreportsetting/{DynamicReportID}/dynamicreporttable");
    }
    #endregion

    #region IsBaseChanged
    private void IsBaseChanged(int? value)
    {
      row["IsBase"] = value;

      row["ReferenceMasterDynamicReportTableID"] = null;
      row["ReferenceTableName"] = null;

      row["ReferenceMasterDynamicReportColumnID"] = null;
      row["ReferenceColumnName"] = null;

      row["SourceMasterDynamicReportColumnID"] = null;
      row["SourceColumnName"] = null;
    }
    #endregion

    #region Select Table
    private void SelectTable(JsonObject select)
    {
      row["MasterDynamicReportTableID"] = select["ID"]?.DeepClone();
      row["TableName"] = select["Alias"]?.DeepClone();

      row["SourceMasterDynamicReportColumnID"] = null;
      row["SourceColumnName"] = null;

      row["Alias"] = select["Alias"]?.DeepClone();
    }
    #endregion

    #region SelectReferenceTable
    void SelectReferenceTable(JsonObject select)
    {
      row["ReferenceDynamicReportTableID"] = select["ID"]?.DeepClone();
      row["ReferenceTableAlias"] = select["Alias"]?.DeepClone();
    }
    #endregion

    #region Select Table Reference
    private void SelectTableReference(JsonObject select)
    {
      row["ReferenceDynamicReportTableID"] = select["ID"]?.DeepClone();
      row["ReferenceMasterDynamicReportTableID"] = select["MasterDynamicReportTableID"]?.DeepClone();
      row["ReferenceTableName"] = select["Name"]?.DeepClone();

      row["ReferenceMasterDynamicReportColumnID"] = null;
      row["ReferenceColumnName"] = null;
    }
    #endregion

    #region Select Column Reference
    private void SelectColumnReference(JsonObject select)
    {
      row["ReferenceMasterDynamicReportColumnID"] = select["ID"]?.DeepClone();
      row["ReferenceColumnName"] = select["Alias"]?.DeepClone();
    }
    #endregion

    #region Select Column Source
    private void SelectColumnSource(JsonObject select)
    {
      row["SourceMasterDynamicReportColumnID"] = select["ID"]?.DeepClone();
      row["SourceColumnName"] = select["Alias"]?.DeepClone();
    }
    #endregion

    #region Go to Dynamic Form Setting
    private void GoToSetting()
    {
      string masterFormID = masterForm["ID"]?.GetValue<string>();
      NavigationManager.NavigateTo($"setting/dynamicform/{masterFormID}");
    }
    #endregion

    #region ChangeIsTableReference
    private void ChangeIsTableReference(int? value)
    {
      row["IsTableReference"] = value;
      StateHasChanged();
    }
    #endregion
  }
}
