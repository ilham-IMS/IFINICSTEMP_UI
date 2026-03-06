using System.Text.Json;
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.MasterDashboardUserComponent
{
  public partial class MasterDashboardUserDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter] public string UserID { get; set; } = "";
    #endregion

    #region Component Ref
    // MultipleSelectLookup<MasterDashboardModel> menuDashboardLookup = null!;
    #endregion

    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    #endregion

    #region Field

    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {
      await base.OnParametersSetAsync();
    }
    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("MasterDashboardUser", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit
      });

      return res?.Data;
    }
    #endregion

    #region Field
    JsonObject filter = new()
    {
      ["EmployeeID"] = "all",
      ["DashboardID"] = "all",
      // OrderKey = 1
    };
    JsonObject lookupFilter = new()
    {
      ["EmployeeID"] = "all",
      ["DashboardID"] = "all",
      // OrderKey = 1
    };
    #endregion

    #region Add
    private void Add()
    {
      // var data = menuDashboardLookup.GetSelected().Select(x => new JsonObject
      // {
      //   EmployeeID = EmployeeID,
      //   DashboardID = x.ID
      // }).ToList();

      // Loading.Show();
      // await IFINSVY.Post("MasterDashboardUser", "Insert", data);
      // await dataGrid.Reload();
      // await dashboardLookup.Reload();
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

        await IFINICSClient.Delete("MasterDashboardUser", "Delete", id);

        await dataGrid.Reload();
        dataGrid.selectedData.Clear();

        Loading.Close();

        StateHasChanged();
      }
    }
    #endregion

    #region Update order key
    public void UpdateOrderKey()
    {

      // Loading.Show();

      // await MasterDashboardUserService.UpdateByID(dataGrid.Data);

      // Loading.Close();

      // await dataGrid.Reload();
    }
    #endregion

    // public async Task<List<MasterDashboardModel>?> GetRowsForLookup(string keyword)
    // {
    //   return await MasterDashboardService.GetRowsForLookup(keyword, 0, 100, EmployeeID ?? "");
    // }
  }
}
