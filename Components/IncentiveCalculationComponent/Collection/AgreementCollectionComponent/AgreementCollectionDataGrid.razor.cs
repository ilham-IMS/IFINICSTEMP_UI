using System.Text.Json.Nodes;
using iFinancing360.UI.Helper;
using iFinancing360.UI.Helper.APIClient;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;
using SixLabors.ImageSharp.Memory;

namespace IFinancing360_ICS_UI.Components.IncentiveCalculationComponent.Collection.AgreementCollectionComponent
{
  public partial class AgreementCollectionDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = default!;
    #endregion


    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    [Parameter, EditorRequired] public string? ID { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    IEnumerable<JsonObject> data;

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
      BodyResponse<List<JsonObject>>? res = await IFINICSClient.GetRows<JsonObject>("AgreementCollection", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit
      });
      data = res?.Data ?? new List<JsonObject>();
      return res?.Data;
    }
    #endregion

    private string GetLink(JsonObject row)
    {
      
      return $"incentivecalculation/collection/{ID}/agreement/{row["ID"]}";
      
    }

  }
}
