

using Microsoft.AspNetCore.Components;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicButtonProcessRoleComponent
{
  public partial class DynamicButtonProcessRoleForm
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? ID { get; set; }
    [Parameter, EditorRequired] public string? DynamicButtonProcessID { get; set; }
    [Parameter] public string ParentMenuURL { get; set; }
    [Parameter][SupplyParameterFromQuery] public string? From { get; set; }

    #endregion

    #region Field
    JsonObject row = new();
    JsonObject rowMenu = new();
    readonly Dictionary<string, string> roleAccess = new(){
            {"ACCESS","A"},
            {"CREATE/UPDATE/GENERATE/UPLOAD","C"},
            {"MATCHING/VALIDATE/EDITABLE","U"},
            {"DELETE","D"},
            {"POST/PROCEED/APPROVE","O"},
            {"CANCEL/REJECT","R"},
            {"PRINT/DOWNLOAD","P"},
        };

    JsonObject masterForm = new();
    List<FormControlsModel> controls = new();
    List<ExtendModel>? extend = new();
    #endregion

    #region RenderDynamicForm
    RenderFragment Form => builder =>
    {

      int seq = 0;
      foreach (var control in controls)
      {
        DynamicRenderForm(builder, ref seq, control);
      }

    };
    #endregion


    #region OnParametersSetAsync
    protected override async Task OnParametersSetAsync()
    {

      if (ID != null)
      {
        await GetRow();
      }
      else
      {
        row["IsActive"] = -1;
        row["DynamicButtonProcessID"] = DynamicButtonProcessID;
        row["Properties"] = JsonValue.Create(controls.ToDictionary(x => x.Name, x => x.Value));
      }

      await GetMenuRow();
      await base.OnParametersSetAsync();
    }
    #endregion

    #region Load Dynamic Form
    private async Task<JsonObject> LoadMasterForm(string code)
    {
      var res = await IFINICSClient.GetRow<JsonObject>("MasterForm", "GetRowByCode", new { code });
      return res?.Data ?? [];
    }
    private async Task<List<FormControlsModel>> LoadFormControls(string formID)
    {
      var res = await IFINICSClient.GetRows<FormControlsModel>("FormControls", "GetRows", new { MasterFormID = formID });
      return res?.Data ?? [];
    }
    #endregion

    protected async Task GetRow()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("DynamicButtonProcessRole", "GetRowByID", new { ID = ID });

      if (res?.Data != null)
      {
        row = res.Data;
      }
      Loading.Close();
    }



    private async void OnSubmit(JsonObject data)
    {
      Loading.Show();


      data = SetAuditInfo(data);
      data = row.Merge(data);
      SetExtensionProperties(data, controls, "Properties");


      #region Insert
      if (ID == null)
      {
        var res = await IFINICSClient.Post("DynamicButtonProcessRole", "Insert", data);

        if (res?.Data != null)
        {
          NavigationManager.NavigateTo($"dynamicsetting/dynamicbuttonprocessrole/{res?.Data["ID"]}");
        }
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINICSClient.Put("DynamicButtonProcessRole", "UpdateByID", data);
      }
      #endregion

      Loading.Close();
      StateHasChanged();
    }

    private void Back()
    {


      if (From != null)
      {
        NavigationManager.NavigateTo(From);
      }
      else
      {
        NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicbuttonprocess/{DynamicButtonProcessID}");
      }

    }

    #region Go to Dynamic Form Setting
    private void GoToSetting()
    {
      string masterFormID = masterForm["ID"]?.GetValue<string>();
      NavigationManager.NavigateTo($"systemsetting/dynamicform/{masterFormID}");
    }
    #endregion

    #region Change dynamic status
    private async Task ChangeDynamicStatus()
    {
      var res = await IFINICSClient.Put("DynamicButtonProcessRole", "ChangeStatusDynamic", row);
      if (res?.Result > 0)
        GetRow();
    }
    #endregion

    private async Task GetMenuRow()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("DynamicButtonProcess", "GetRowByID", new { ID = DynamicButtonProcessID });

      if (res?.Data != null)
      {
        rowMenu = res.Data;
      }
      Loading.Close();
    }
  }
}