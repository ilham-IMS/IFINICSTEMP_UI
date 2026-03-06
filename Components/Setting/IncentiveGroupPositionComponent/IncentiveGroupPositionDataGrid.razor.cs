using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Nodes;

namespace IFinancing360_ICS_UI.Components.Setting.IncentiveGroupPositionComponent
{
  public partial class IncentiveGroupPositionDataGrid
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

    SingleSelectLookup<JsonObject> PositionLookup = null!;

    #endregion

    #region Field
    // controllers
    private string APIController = "IncentiveGroupPosition";
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
      var res = await IFINICSClient.GetRows<JsonObject>("IncentiveGroupPosition", "GetRowsByGroupID", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        GroupID = IncentiveGroupID
      });

      return res?.Data;
    }
    #endregion

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

        List<JsonObject> id = dataGrid.selectedData.Select(row => SetAuditInfo(new JsonObject
        {
          ["IncentiveGroupID"] = IncentiveGroupID,
          ["PositionID"] = row["PositionID"]?.GetValue<string>(),
          ["ID"] = row["ID"]?.GetValue<string>(),
        })).ToList();

        await IFINICSClient.Delete("IncentiveGroupPosition", "DeleteByID", id);

        await GetRowHeader();
        await dataGrid.Reload();
        dataGrid.selectedData.Clear();

        Loading.Close();

        StateHasChanged();
      }
    }
    #endregion

    #region Save
    private async Task Save(JsonObject data)
    {
      Loading.Show();
      List<JsonObject> list = new();

      if (dataGrid.Data != null)
      {
        foreach (var row in dataGrid.Data)
        {
          var id = row["ID"]?.GetValue<string>();
          
          if (string.IsNullOrWhiteSpace(id))
            continue;

          // Cek apakah ada perubahan dari form input (data parameter)
          var hasChanges = data.Any(x => x.Key.Split("_").Last() == id);

          if (hasChanges || row["PositionID"] != null)
          {
            list.Add(SetAuditInfo(new JsonObject()
            {
              ["ID"] = id,
              ["IncentiveGroupID"] = IncentiveGroupID,
              ["PositionID"] = row["PositionID"]?.DeepClone(),
              ["PositionCode"] = row["PositionCode"]?.DeepClone(),
              ["PositionDescription"] = row["PositionDescription"]?.DeepClone(),
              ["PositionRatio"] = row["PositionRatio"]?.DeepClone(),
            }));
          }
        }
      }

      Console.WriteLine($"list to update: {System.Text.Json.JsonSerializer.Serialize(list)}");
      
      if (list.Count > 0) await IFINICSClient.Put("IncentiveGroupPosition", "UpdateByID", list);

      await dataGrid.Reload();
      Loading.Close();
    }
    #endregion

    #region SelectLookupPosition
    public async Task SelectLookupPosition(JsonObject selectedData)
    {
      Loading.Show();

      var updatedRow = dataGrid.Data?.Find(x => x["ID"]?.GetValue<string>() == activeItemID) ?? [];

      updatedRow["PositionID"] = selectedData["ID"]?.GetValue<string>();
      updatedRow["PositionCode"] = selectedData["Code"]?.GetValue<string>();
      updatedRow["PositionDescription"] = selectedData["Description"]?.GetValue<string>();
      
      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region LoadPosition
    public async Task<List<JsonObject>?> LoadPositionFromSys(DataGridLoadArgs args)
    {
      // string[] tempIDs = row.Where(x => x.Key.Contains("ItemID") && x.Value != null).Select(x => x.Value?.GetValue<string>() ?? "").ToArray();

      var res = await IFINSYSClient.GetRows<JsonObject>("SysPosition", "GetRowsForLookup", new
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

    
  }
}