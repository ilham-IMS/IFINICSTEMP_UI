
using System.Text.Json;
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicReportColumnComponent
{
  public partial class DynamicReportColumnDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter] public string? DynamicReportID { get; set; }
    #endregion

    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    MultipleSelectLookup<JsonObject> columnMultiLookup = null!;
    Modal addFormulaModal = null!;
    Modal guideModal = null!;
    #endregion

    #region Field
    public JsonObject rowDynamicReport { get; set; } = [];
    public JsonObject ActiveRow = new();
    Dictionary<string, string?> groupFunctionDict = new()
    {
      [""] = null,
      ["AVERAGE"] = "AVG",
      ["SUM"] = "SUM",
      ["MAX"] = "MAX",
      ["MIN"] = "MIN",
      ["COUNT"] = "COUNT",
    };

    Dictionary<string, string?> orderByDict = new()
    {
      ["-"] = null,
      ["ASCENDING"] = "ASC",
      ["DESCENDING"] = "DESC",
    };

    public bool IsPublished
    {
      get
      {
        return rowDynamicReport["IsPublished"]?.GetValue<int>() == 1;
      }
    }
    public bool IsReadOnly
    {
      get
      {
        return IsPublished;
      }
    }

    JsonObject row { get; set; } = [];
    #endregion

    #region OnInitialized
    protected override async Task OnInitializedAsync()
    {
      await GetRowrowDynamicReport();
      await base.OnInitializedAsync();
    }
    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("DynamicReportColumn", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        DynamicReportID
      });

      return res?.Data;
    }
    #endregion

    #region GetRowrowDynamicReport
    public async Task GetRowrowDynamicReport()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("DynamicReport", "GetRowByID", new
      {
        ID = DynamicReportID
      });

      if (res?.Data != null)
      {
        rowDynamicReport = res.Data;
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region LoadLookup
    protected async Task<List<JsonObject>?> LoadMasterDynamicReportColumnLookup(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicReportColumn", "GetRowsForLookupExcludeByDynamicReport", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        DynamicReportID
      });

      return res?.Data;
    }
    #endregion

    #region Add
    private void Add()
    {
      columnMultiLookup.Open();

    }
    #endregion

    #region AddFormula
    private void AddFormula()
    {
      row = [];
      addFormulaModal.Open();
    }
    #endregion

    #region AddToList
    private async void AddToList()
    {
      var selectedData = columnMultiLookup.GetSelected();

      if (!selectedData.Any())
      {
        await NoDataSelectedAlert();
        return;
      }
      Loading.Show();
      var data = columnMultiLookup.GetSelected().Select(row => SetAuditInfo(new JsonObject()
      {
        ["DynamicReportID"] = DynamicReportID,
        ["MasterDynamicReportColumnID"] = row["ID"]?.GetValue<string>(),
        ["HeaderTitle"] = row["Alias"]?.GetValue<string>(),
        ["DynamicReportTableID"] = row["ReportTableID"]?.GetValue<string>(),
      })).ToList();

      await IFINICSClient.Post("DynamicReportColumn", "Insert", data);

      await dataGrid.Reload();
      await columnMultiLookup.Reload();

      Loading.Close();
    }
    #endregion

    #region Save
    private async void Save(JsonObject data)
    {
      Loading.Show();
      List<JsonObject> list = new();

      foreach (var (key, value) in data)
      {
        var id = key.Split("_").Last();
        var objKey = key.Split("_").First();

        if (list.Find(x => x["ID"]?.GetValue<string>() == id) == null)
        {
          var row = dataGrid.Data?.Find(row => row["ID"]?.GetValue<string>() == id);
          list.Add(SetAuditInfo(
            new JsonObject()
            {
              ["MasterDynamicReportColumnID"] = row?["MasterDynamicReportColumnID"]?.GetValue<string>(),
              ["ID"] = id,
              ["Formula"] = row?["Formula"]?.GetValue<string>(),
            }
          ));
        }

        list.Find(x => x["ID"]?.GetValue<string>() == id)![objKey] = value?.DeepClone();
      }


      await IFINICSClient.Put("DynamicReportColumn", "UpdateByID", list);

      await dataGrid.Reload();
      Loading.Close();
    }
    #endregion
    #region SaveFormula
    private async void SaveFormula(JsonObject data)
    {
      Loading.Show();

      data["DynamicReportID"] = DynamicReportID;
      data["HeaderTitle"] = data["Formula"]?.DeepClone();
      data = SetAuditInfo(data);

      await IFINICSClient.Post("DynamicReportColumn", "Insert", new List<JsonObject>() { data });

      await dataGrid.Reload();
      Loading.Close();
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

        await IFINICSClient.Delete("DynamicReportColumn", "DeleteByID", id);

        await dataGrid.Reload();
        dataGrid.selectedData.Clear();

        Loading.Close();

        StateHasChanged();
      }
    }
    #endregion

    #region Select Column
    private void SelectColumn(JsonObject select)
    {
      var row = dataGrid.Rows?.Find(r => r["ID"]?.GetValue<string>() == ActiveRow["ID"]?.GetValue<string>());

      if (row != null)
      {
        ActiveRow["MasterDynamicReportColumnID"] = select["ID"]?.DeepClone();
        ActiveRow["ColumnName"] = select["Alias"]?.DeepClone();
      }

      if (string.IsNullOrWhiteSpace(ActiveRow["HeaderTitle"]?.GetValue<string>()))
        ActiveRow["HeaderTitle"] = ActiveRow["ColumnName"]?.GetValue<string>().ToUpper().Replace("_", " ");
    }
    #endregion

    #region GetRowIndex
    private int GetRowIndex(JsonObject row)
    {
      var data = dataGrid.Data;
      if (data == null) return 0;
      var index = data.FindIndex(x => x["ID"]?.GetValue<string>() == row["ID"]?.GetValue<string>());
      return index;
    }
    #endregion

    #region Order
    async void OrderUp(JsonObject data)
    {
      await IFINICSClient.Put("DynamicReportColumn", "OrderUp", SetAuditInfo(data));

      await dataGrid.Reload();
    }
    async void OrderDown(JsonObject data)
    {
      await IFINICSClient.Put("DynamicReportColumn", "OrderDown", SetAuditInfo(data));
      await dataGrid.Reload();
    }
    #endregion

    #region ShowGuide
    void ShowGuide()
    {
      guideModal.Open();
    }
    #endregion

    #region FormatColumnName
    public string? FormatColumnName(JsonObject row)
    {
      string? column = row["Formula"]?.GetValue<string>() ?? row["ColumnName"]?.GetValue<string>();
      if (row["GroupFunction"] != null)
      {
        return $"{row["GroupFunction"]}({column})";
      }
      else
      {
        return column;
      }
    }
    #endregion
  }
}