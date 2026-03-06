
using System.Text.Json;
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicFormControlsComponent
{
  public partial class DynamicFormControlsDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion
    #region parameter
    [Parameter, EditorRequired] public string? MasterFormID { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion
    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    #endregion

    #region Field

    #endregion

    #region OnInitialized

    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("FormControls", "GetRowsForDataTable", new
      {
        MasterFormID
      });

      return res?.Data;
    }
    #endregion

    #region Add
    private void Add()
    {
      NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicform/{MasterFormID}/formcontrol/add");
    }
    #endregion


  }
}