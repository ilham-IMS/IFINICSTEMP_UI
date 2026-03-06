using System.Text.Json.Nodes;


using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.MasterDynamicButtonProcessComponent
{
  public partial class MasterDynamicButtonProcessForm
  {
    #region Variables
    JsonObject masterForm = new();
    List<FormControlsModel> controls = new();
    List<ExtendModel>? extend = new();
    #endregion

    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? ID { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion

    #region Component Field
    #endregion
  
    #region Field
    public JsonObject row = new();
    #endregion

    #region OnParametersSet
    protected override async Task OnParametersSetAsync()
    {

      if (ID != null)
      {
        await GetRow();
      }
      else
      {


      }

      await DynamicFormSetup();
      await base.OnParametersSetAsync();
    }
    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();

      var res = await IFINICSClient.GetRow<JsonObject>("MasterDynamicButtonProcess", "GetRowByID", new { ID });

      if (res?.Data != null)
      {
        row = res.Data;

      }
      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region OnSubmit
    private async void OnSubmit(JsonObject data)
    {
      isLoading = true;
      Loading.Show();

      data = SetAuditInfo(data);
      data = row.Merge(data);

      SetExtensionProperties(data, controls, "Properties");

      #region Insert
      if (ID == null)
      {
        var res = await IFINICSClient.Post("MasterDynamicButtonProcess", "Insert", data);

        if (res?.Data != null)
        {
          NavigationManager.NavigateTo($"{ParentMenuURL}/masterdynamicbuttonprocess/{res.Data["ID"]}");
        }
        isLoading = false;
        StateHasChanged();
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINICSClient.Put("MasterDynamicButtonProcess", "UpdateByID", data);
        if (res != null) isLoading = false;
        await GetRow();
        StateHasChanged();
      }
      #endregion
      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region Back
    private void Back()
    {
      NavigationManager.NavigateTo($"{ParentMenuURL}/masterdynamicbuttonprocess");
    }
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

    #region Render Dynamic Form
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
      NavigationManager.NavigateTo($"setting/dynamicform/{masterFormID}");
    }
    #endregion

    #region DynamicFormSetup
		public async Task DynamicFormSetup()
		{
			masterForm = await LoadMasterForm("MDBP");
			controls = await LoadFormControls(masterForm["ID"]?.GetValue<string>());

			if (!string.IsNullOrEmpty(ID))
			{
				var extRes = await IFINICSClient.GetRows<ExtendModel>("MasterDynamicButtonProcess", "GetRowByExt", new { ID });

				extend = extRes?.Data ?? [];

				AddExtendProperty(controls, extend, row);
			}

			SetInitialValue(row, controls);
		}
		#endregion
  }
}
