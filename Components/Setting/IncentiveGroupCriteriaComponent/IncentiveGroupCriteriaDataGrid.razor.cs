using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Nodes;

namespace IFinancing360_ICS_UI.Components.Setting.IncentiveGroupCriteriaComponent
{
  public partial class IncentiveGroupCriteriaDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    [Inject] IFINSYSClient IFINSYSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter] public string? IncentiveGroupID { get; set; }
    #endregion

    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    JsonObject rowHeader = [];

    SingleSelectLookup<JsonObject> CriteriaLookup = null!;
    SingleSelectLookup<JsonObject> CriteriaValueFromLookup = null!;
    SingleSelectLookup<JsonObject> CriteriaValueToLookup = null!;
    JsonObject target = [];
    JsonObject targetValue = [];

    public Dictionary<string, Dictionary<string, string>> operatorDict = [];
    #endregion

    #region Field
    // controllers
    private string APIController = "IncentiveGroupCriteria";
    // routes
    private string APIRouteForGetRowByID = "GetRowByID";
    private string APIRouteForInsertItem = "Insert";
    private string APIRouteForUpdateItem = "UpdateByID";

    string activeItemID = "";
    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {
      await GetRowHeader();

      await base.OnParametersSetAsync();
    }
    #endregion

    #region GetRow header
    public async Task GetRowHeader()
    {
      Loading.Show();

      var res = await IFINICSClient.GetRow<JsonObject>(APIController, APIRouteForGetRowByID, new { ID = IncentiveGroupID });

      if (res?.Data != null) rowHeader = res.Data;

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("IncentiveGroupCriteria", "GetRowsByGroupID", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        GroupID = IncentiveGroupID
      });

      foreach (var item in res?.Data ?? [])
      {
        var ID = item["ID"]?.ToString() ?? "";

        if (!operatorDict.ContainsKey(ID))
        {
          operatorDict[ID] = new() {
            {"EQUAL", "EQUAL"},
            {"MORE THAN", "MORE THAN"},
            {"LESS THAN", "LESS THAN"},
            {"BETWEEN", "BETWEEN"}
          };
        }
      }

      return res?.Data;
    }
    #endregion

    // #region LoadCriteria
    // private async Task<List<JsonObject>?> LoadCriteria(DataGridLoadArgs args)
    // {
    //   string[] tempIDs = rowHeader.Where(x => x.Key.Contains("ItemID") && !string.IsNullOrWhiteSpace(x.Value?.ToString())).Select(x => x.Value?.GetValue<string>() ?? "").ToArray();

    //   var res = await IFINICSClient.GetRows<JsonObject>("MasterScoringItem", "GetrowsForLookupExcludeExistingID", new
    //   {
    //     args.Keyword,
    //     args.Offset,
    //     args.Limit,
    //     existingIDs = tempIDs
    //   });
    //   return res?.Data;
    // }
    // #endregion

    #region Add
    private async Task Add()
    {
      Loading.Show();

      var res = await IFINICSClient.Post(APIController, APIRouteForInsertItem, SetAuditInfo(new JsonObject
      {
        ["IncentiveGroupID"] = IncentiveGroupID
      }));

      if (res?.Data != null) await dataGrid.Reload();

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
        Console.WriteLine("ID to delete: " + string.Join(", ", id));
        await IFINICSClient.Delete("IncentiveGroupCriteria", "DeleteByID", id);

        await GetRowHeader();
        await dataGrid.Reload();
        dataGrid.selectedData.Clear();

        Loading.Close();

        StateHasChanged();
      }
    }
    #endregion

    #region TypeChanged
    public async Task TypeChanged(JsonObject selectedData)
    {
      Loading.Show();

      var payload = new List<JsonObject> { selectedData };
      var res = await IFINICSClient.Put(APIController, APIRouteForUpdateItem, payload);

      if (res?.Result > 0)
      {
        // Hanya reload jika ada perubahan data critical
        StateHasChanged();
      }

      Loading.Close();
    }
    #endregion

    #region SelectLookupCriteria
    public async Task SelectLookupCriteria(JsonObject selectedData)
    {
      Loading.Show();

      var updatedRow = dataGrid.Data?.Find(x => x["ID"]?.GetValue<string>() == activeItemID) ?? [];

      updatedRow["CriteriaID"] = selectedData["ID"]?.GetValue<string>();
      updatedRow["CriteriaCode"] = selectedData["Code"]?.GetValue<string>();
      updatedRow["CriteriaDescription"] = selectedData["Description"]?.GetValue<string>();

      var payload = new List<JsonObject> { updatedRow };
      var res = await IFINICSClient.Put(APIController, APIRouteForUpdateItem, payload);

      if (res?.Result > 0)
      {
        await GetRowHeader();
        await dataGrid.Reload();
      }
      
      await dataGrid.Reload();
      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region UpdateItemValueFrom
    public async Task UpdateItemValueFrom(JsonObject selectedData)
    {
      Loading.Show();
      var updatedRow = dataGrid.Data?.Find(x => x["ID"]?.GetValue<string>() == selectedData["ID"]?.GetValue<string>()) ?? [];
      

      var payload = new List<JsonObject> { updatedRow };
      var res = await IFINICSClient.Put(APIController, APIRouteForUpdateItem, payload);

      if (res?.Result > 0)
      {
        StateHasChanged();
      }

      Loading.Close();
    }
    #endregion

    #region UpdateItemValueTo
    public async Task UpdateItemValueTo(JsonObject selectedData)
    {
      Loading.Show();

      var updatedRow = dataGrid.Data?.Find(x => x["ID"]?.GetValue<string>() == selectedData["ID"]?.GetValue<string>()) ?? [];

      var payload = new List<JsonObject> { updatedRow };
      var res = await IFINICSClient.Put(APIController, APIRouteForUpdateItem, payload);

      if (res?.Result > 0)
      {
        StateHasChanged();
      }

      Loading.Close();
    }
    #endregion

    #region LoadCriteria
    public async Task<List<JsonObject>?> LoadCriteriaFromSys(DataGridLoadArgs args)
    {
      // string[] tempIDs = row.Where(x => x.Key.Contains("ItemID") && x.Value != null).Select(x => x.Value?.GetValue<string>() ?? "").ToArray();

      var res = await IFINSYSClient.GetRows<JsonObject>("SysCriteria", "GetRowsForLookup", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        // IDs = tempIds,
        ModuleCode = "IFINLOS"
      });

      return res?.Data;
    }
    #endregion

    #region LoadCriteriaValue
    public async Task<List<JsonObject>?> LoadCriteriaValue(DataGridLoadArgs args)
    {
      var res = await IFINSYSClient.GetRows<JsonObject>("SysCriteriaValue", "GetRowsForLookupByCriteriaIDExcludeValue", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        criteriaID = targetValue["CriteriaID"],
        ExcludeValue = targetValue["ValueTo"]
      });

      return res?.Data;
    }
    #endregion

    #region LoadCriteriaValueTo
    public async Task<List<JsonObject>?> LoadCriteriaValueTo(DataGridLoadArgs args)
    {
      var res = await IFINSYSClient.GetRows<JsonObject>("SysCriteriaValue", "GetRowsForLookupByCriteriaIDExcludeValue", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        criteriaID = targetValue["CriteriaID"],
        ExcludeValue = targetValue["ValueFrom"]
      });

      return res?.Data;
    }
    #endregion

    #region Select data criteria value from
    private async Task OnSelectDataCriteriaValue(JsonObject source)
    {
      targetValue["ValueFrom"] = source["Value"]?.GetValue<string>();

      await UpdateItemValueFrom(targetValue);
    }
    #endregion

    #region Select data criteria value to
    private async Task OnSelectDataCriteriaValueTo(JsonObject source)
    {
      targetValue["ValueTo"] = source["Value"]?.GetValue<string>();
      await UpdateItemValueTo(targetValue);
    }
    #endregion
  }
}