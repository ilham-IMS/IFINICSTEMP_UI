using System.Text.Json.Nodes;
using iFinancing360.UI.Config;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.DashboardComponent
{
  public partial class DashboardChartCollection
  {
    [Inject] IFINSYSClient IFINSYSClient { get; set; } = null!;

    List<JsonObject> masterDashboards = [];

    protected override async Task OnInitializedAsync()
    {
      await LoadMasterDashboards();

      await base.OnInitializedAsync();
    }

    protected async Task LoadMasterDashboards()
    {
      Loading.Show();

      var res = await IFINSYSClient.GetRows("SysUserMainDashboard", "GetRowsByModuleCode", new
      {
        UserCode = GetCurrentUser(),
        ModuleCode = AppConfig.MODULE
      });

      masterDashboards = res?.Data?
        .OrderBy(x => x["OrderKey"]?.GetValue<int>() ?? 0)
        .ToList() ?? [];

      Loading.Close();
    }
  }
}