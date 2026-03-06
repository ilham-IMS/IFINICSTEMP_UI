
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.MasterReportUserComponent
{
  public partial class MasterReportUserDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? EmployeeID { get; set; }
    #endregion

    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    #endregion

    #region Component Ref
    MultipleSelectLookup<JsonObject> menuSysReportLookup = null!;
    #endregion

    // #region Field
    // JsonObject filter = new()
    // {
    //   EmployeeID = "all",
    //   ReportID = "all"
    // };
    // JsonObject lookupFilter = new()
    // {
    //   EmployeeID = "all",
    //   ReportID = "all"
    // };
    // #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {
      await base.OnParametersSetAsync();
    }
    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("MasterReportUser", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        EmployeeID = EmployeeID
      });

      return res?.Data;
    }
    #endregion

    #region Add
    private async void Add()
    {
      var data = menuSysReportLookup.GetSelected().Select(x => SetAuditInfo(new JsonObject()
      {
        ["EmployeeID"] = EmployeeID,
        ["ReportID"] = x["ID"]?.GetValue<string>()
      })).ToList();

      await IFINICSClient.Post("MasterReportUser", "Insert", data);

      await dataGrid.Reload();
      await menuSysReportLookup.Reload();
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

        await IFINICSClient.Delete("MasterReportUser", "Delete", id);

        await dataGrid.Reload();
        dataGrid.selectedData.Clear();

        Loading.Close();

        StateHasChanged();
      }
    }
    #endregion
    #region lookup 
    public async Task<List<JsonObject>?> GetRowsForLookup(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("MasterReportUser", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        EmployeeID = EmployeeID
      });

      return res?.Data;
    }
    #endregion
  }
}
