using System.Text.Json.Nodes;
using iFinancing360.UI.Helper;
using iFinancing360.UI.Helper.APIClient;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;
using SixLabors.ImageSharp.Memory;

namespace IFinancing360_ICS_UI.Components.IncentiveCalculationComponent.Marketing.AgreementMarketingFeeComponent
{
  public partial class AgreementMarketingFeeDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = default!;
    #endregion


    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    [Parameter, EditorRequired] public string? ID { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
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
      BodyResponse<List<JsonObject>>? res = await IFINICSClient.GetRows<JsonObject>("AgreementMarketing", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit
      });
      return res?.Data;
    }
    #endregion

  }
}
