using System.Text.Json.Nodes;
using iFinancing360.UI.Helper;
using iFinancing360.UI.Helper.APIClient;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;
using SixLabors.ImageSharp.Memory;

namespace IFinancing360_ICS_UI.Components.IncentiveCalculationComponent.Collection
{
  public partial class IncentiveCollectionDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = default!;
    #endregion


    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    #endregion

    #region Field
    public JsonObject filter = new();
    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {

      filter["ProductOfferingID"] = "ALL";
      filter["ProductOfferingName"] = "ALL";

      await base.OnParametersSetAsync();
    }
    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      BodyResponse<List<JsonObject>>? res = await IFINICSClient.GetRows<JsonObject>("IncentiveCollection", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        FromDate = filter["PeriodeFrom"]?.GetValue<string>(),
        ToDate = filter["PeriodeTo"]?.GetValue<string>(),
      });
      return res?.Data;
    }
    #endregion

    private string GetLink(JsonObject row)
    {
      
      return $"incentivecalculation/collection/{row["ID"]}";
      
    }

  }
}
