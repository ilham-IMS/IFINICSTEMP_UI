using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace IFinancing360_ICS_UI.Components.Setting.IncentiveGroupCriteriaComponent
{
  public partial class IncentiveGroupCriteriaLabel
  {
    #region Client
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? ID { get; set; }
    #endregion

    #region Component field
    #endregion

    #region Class field
    public JsonObject row = new();

    // Paths
    private const string BASE_PATH = "setting/incentivegroupcriteria";
    public int? ActiveDimensionLookup;

    // Controllers & Routes
    readonly string BaseController = "IncentiveGroup";
    readonly string GetRowByIDRoute = "GetRowByID";
    #endregion

    #region OnInit
    protected override async Task OnParametersSetAsync()
    {
      await Getrow();
      await base.OnParametersSetAsync();
    }
    #endregion

    #region Getrow
    public async Task Getrow()
    {
      Loading.Show();

      var res = await IFINICSClient.GetRow<JsonObject>(BaseController, GetRowByIDRoute, new { ID });

      if (res?.Data != null)
      {
        row = res.Data;
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion
  }
}