

using Microsoft.AspNetCore.Components;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using System.Text.Json.Nodes;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicButtonProcessRoleComponent
{
  public partial class DynamicButtonProcessRoleDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? DynamicButtonProcessID { get; set; }
    [Parameter] public string ParentMenuURL { get; set; }
    #endregion

    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
		MultipleSelectLookup<JsonObject> informationSchemaTableLookup = null!;

    #endregion

    #region Field
    public readonly Dictionary<string, string> roleAccess = new(){
            {"ACCESS","A"},
            {"CREATE/UPDATE/GENERATE/UPLOAD","C"},
            {"MATCHING/VALIDATE/EDITABLE","U"},
            {"DELETE","D"},
            {"POST/PROCEED/APPROVE","O"},
            {"CANCEL/REJECT","R"},
            {"PRINT/DOWNLOAD","P"},
        };
    #endregion

    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("DynamicButtonProcessRole", "GetRows", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
        dynamicButtonProcessID = DynamicButtonProcessID
      });
      return res?.Data;
    }

    private async void Delete()
    {
      if (!dataGrid.selectedData.Any())
      {
        await NoDataSelectedAlert();
        return;
      }

      bool? result = await Confirm();

      if (result == true && dataGrid != null)
      {
        Loading.Show();

        await IFINICSClient.Delete("DynamicButtonProcessRole", "DeleteByID", dataGrid.selectedData.Select(row => row["ID"]).ToArray());

        dataGrid.selectedData.Clear();

        await dataGrid.Reload();

        Loading.Close();

        StateHasChanged();
      }
    }

  }
}