using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.IncentiveGroupComponent
{
  public partial class IncentiveGroupForm
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? ID { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion

    #region Component Field
    #region
    private void ChangeIncentiveType(string? value)
    {
      row["IncentiveType"] = value;

    }
    #endregion
    #endregion

    #region Field
    public JsonObject row = new();
    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {
      masterForm = await LoadMasterForm("IG");
      controls = await LoadFormControls(masterForm["ID"]?.GetValue<string>());
      if (ID != null)
      {
        await GetRow();
        var extRes = await IFINICSClient.GetRows<ExtendModel>("IncentiveGroup", "GetRowByExt", new { ID });
        extend = extRes?.Data;
        AddExtendProperty(controls, extend, row);
      }
      else
      {
        row["IsActive"] = 1;
      }
      SetInitialValue(row, controls);
      await base.OnParametersSetAsync();
    }
    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("IncentiveGroup", "GetRowByID", new
      {
        ID = ID
      });

      if (res?.Data != null)
      {
        row = res.Data;
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region OnSubmit
    private async Task OnSubmit(JsonObject data)
    {
      Loading.Show();

      data = SetAuditInfo(data);

      data = row.Merge(data);
      SetExtensionProperties(data, controls, "Properties");

      #region Insert
      if (ID == null)
      {
        var res = await IFINICSClient.Post("IncentiveGroup", "Insert", data);

        if (res?.Data != null)
        {
          NavigationManager.NavigateTo($"{ParentMenuURL}/incentivegroup/{res.Data["ID"]}");
        }
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINICSClient.Put("IncentiveGroup", "UpdateByID", data);
        if (res?.Data != null)
        {
          await GetRow();
        }
      }

      Loading.Close();
      StateHasChanged();
      #endregion
    }
    #endregion

    #region Back
    private void Back()
    {
      NavigationManager.NavigateTo($"{ParentMenuURL}/incentivegroup");
    }
    #endregion

    #region ChangeActive
    private async Task ChangeActive()
    {
      if (ID != null)
      {
        Loading.Show();
        var data = SetAuditInfo(row);
        var res = await IFINICSClient.Put("IncentiveGroup", "ChangeStatus", data);

        if (res != null)
        {
          await GetRow();
          Loading.Close();
        }

        StateHasChanged();
      }
    }
    #endregion

    #region dashboardType
    public readonly Dictionary<string, string> listIncentiveType = new(){
      {"MARKETING","MARKETING"},
      {"COLLECTION","COLLECTION"},
    };
    #endregion

     #region Variables
    JsonObject masterForm = new();
    List<FormControlsModel> controls = new();
    List<ExtendModel>? extend = new();
    #endregion

    #region Load data for Dynamic Form
    private async Task<JsonObject> LoadMasterForm(string code)
    {
      var res = await IFINICSClient.GetRow<JsonObject>("MasterForm", "GetRowByCode", new { Code = code });
      return res?.Data ?? [];
    }

    private async Task<List<FormControlsModel>> LoadFormControls(string formID)
    {
      var res = await IFINICSClient.GetRows<FormControlsModel>("FormControls", "GetRows", new { MasterFormID = formID });
      return res?.Data ?? [];
    }
    #endregion

    #region Dynamic Render
    RenderFragment Form => builder =>
    {
      int seq = 0;

      foreach (var control in controls)
      {
        DynamicRenderForm(builder, ref seq, control);
      }
    };
    #endregion

    #region Go to Dynamic Form Setting
    private void GoToSetting()
    {
      string masterFormID = masterForm["ID"]?.GetValue<string>();
      NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicform/{masterFormID}");
    }
    #endregion
  }
}
