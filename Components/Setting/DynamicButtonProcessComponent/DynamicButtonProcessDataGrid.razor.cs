

using Microsoft.AspNetCore.Components;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using System.Text.Json.Nodes;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicButtonProcessComponent
{
  public partial class DynamicButtonProcessDataGrid
  {

    #region Parameter
    [Parameter] public string ParentMenuURL { get; set; }
    #endregion
    [Inject] IFINICSClient IFINICSClient { get; set; } = default!;
    [Inject] IFINSYSClient IFINSYSClient { get; set; } = default!;
    DataGrid<JsonObject> dataGrid = new();
    SingleSelectLookup<JsonObject> parentLookup = new();

    private JsonObject filter = new()
    {
      ["ModuleID"] = "all",
      ["ModuleCode"] = "ALL",
      ["ParentMenuID"] = "all",
      ["ParentMenuName"] = "ALL"
    };

    protected override async Task OnInitializedAsync()
    {
      await base.OnInitializedAsync();
      filter["ParentMenuID"] = filter["ParentMenuID"]?.GetValue<string>();
    }

    private async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("DynamicButtonProcess", "GetRows", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
        ParentMenuID = filter["ParentMenuID"]?.GetValue<string>(),
      });
      return res?.Data;
    }
    protected async Task<List<JsonObject>?> LoadParentLookup(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("DynamicButtonProcess", "GetRowsForLookupParent", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
        WithAll = true
      });
      return res?.Data;
    }

    private async void OnSync()
    {
      Loading.Show();
      await IFINICSClient.Post("DynamicButtonProcess", "SyncButtonProcess", null);

      Loading.Close();
      await dataGrid.Reload();
    }

  }
}