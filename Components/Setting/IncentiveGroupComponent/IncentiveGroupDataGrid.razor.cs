using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.IncentiveGroupComponent
{
  public partial class IncentiveGroupDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    #endregion

    #region Field

    #endregion
    #region Parameter
    [Parameter] public string? ParentMenuURL { get; set; }
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
      var res = await IFINICSClient.GetRows<JsonObject>("IncentiveGroup", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit
      });

      return res?.Data;
    }
    #endregion

    #region Add
    private void Add()
    {
      NavigationManager.NavigateTo($"{ParentMenuURL}/incentivegroup/add");
    }
    #endregion

  }
}
