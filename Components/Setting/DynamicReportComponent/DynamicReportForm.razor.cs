using System.Text.Json;
using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using iFinancing360.UI.Helper.Auth;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicReportComponent
{
	public partial class DynamicReportForm
	{
		#region Variables
		JsonObject masterForm = new();
		List<FormControlsModel> controls = new();
		#endregion
		#region Service
		[Inject] IFINICSClient IFINICSClient { get; set; } = null!;
		#endregion

		#region Parameter
		[Parameter] public string? ID { get; set; }
		[Parameter] public EventCallback<JsonObject> RowChanged { get; set; }
		[Parameter] public string? ParentMenuURL { get; set; }
		#endregion

		#region Component Field
		Modal queryModal = default!;
		Modal printModal = default!;
		#endregion

		#region Field
		public JsonObject row = new();
		public JsonObject Query = [];
		List<ExtendModel>? extend = new();
		RenderFragment Form => builder =>
	   {

		   int seq = 0;
		   foreach (var control in controls)
		   {
			   DynamicRenderForm(builder, ref seq, control);
		   }
	   };
		public bool IsPublished
		{
			get
			{
				return row["IsPublished"]?.GetValue<int>() == 1;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return IsPublished;
			}
		}
		#endregion

		#region OnInitialized
		protected override async Task OnInitializedAsync()
		{
			masterForm = await LoadMasterForm("DR");
      controls = await LoadFormControls(masterForm["ID"]?.GetValue<string>());
			if (ID != null && row["ID"] == null)
			{
				await GetRow();
				var extRes = await IFINICSClient.GetRows<ExtendModel>("DynamicReport", "GetRowByExt", new { ID = ID });
        extend = extRes?.Data;

        var controlNames = controls.Select(control => control.Name).ToHashSet();
        extend = extend?.Where(ext => controlNames.Contains(ext.Keyy)).ToList();

        AddExtendProperty(controls, extend, row);
			}
			else
			{
				row["Properties"] = JsonValue.Create(controls.ToDictionary(x => x.Name, x => x.Value));
			}
			SetInitialValue(row, controls);
			await base.OnInitializedAsync();
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

		#region GetRow
		public async Task GetRow()
		{
			Loading.Show();
			var res = await IFINICSClient.GetRow<JsonObject>("DynamicReport", "GetRowByID", new
			{
				ID = ID
			});

			if (res?.Data != null)
			{
				row = res.Data;
				await RowChanged.InvokeAsync(row);
			}

			Loading.Close();
			StateHasChanged();
		}
		#endregion

		#region OnSubmit
		private async void OnSubmit(JsonObject data)
		{
			Loading.Show();

			data = SetAuditInfo(data);

			data = row.Merge(data);
			SetExtensionProperties(data, controls, "Properties");

			#region Insert
			if (ID == null)
			{
				var res = await IFINICSClient.Post("DynamicReport", "Insert", data);

				if (res?.Data != null)
				{
					NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicreportsetting/{res.Data["ID"]}");
				}
			}
			#endregion

			#region Update
			else
			{
				var res = await IFINICSClient.Put("DynamicReport", "UpdateByID", data);
				await GetRow();
			}

			Loading.Close();
			StateHasChanged();
			#endregion
		}
		#endregion

		#region Back
		private void Back()
		{
			NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicreportsetting");
		}
		#endregion

		#region PrintHandler
		private async void PrintHandler()
		{
			Loading.Show();

			var parameters = await GetParameters();

			if (parameters.Count > 0)
			{
				printModal.Open();
			}
			else
			{
				await Print();
			}

			Loading.Close();
		}
		#endregion

		#region Print
		private async Task Print()
		{
			Loading.Show();
			var res = await IFINICSClient.Post<JsonObject>("DynamicReport", "Print", row);

			if (res == null) return;

			var data = res.Data;

			if (data != null)
			{
				var fileName = data["Name"]?.GetValue<string>();
				var content = data["Content"]?.GetValueAsByteArray();

				PreviewFile(content, fileName);
			}
			Loading.Close();
		}

		#endregion

		#region GetQuery
		private async void GetQuery()
		{
			Loading.Show();

			var result = await IFINICSClient.GetRow<JsonObject>("DynamicReport", "GetQuery", new
			{
				ID = ID
			});

			if (result?.Result > 0)
			{
				Query = result.Data ?? [];
				queryModal.Open();
			}
			else
			{
				await Alert(result?.Message ?? "Something went wrong");
			}



			Loading.Close();
		}
		#endregion

		#region GetParameters
		private async Task<List<JsonObject>> GetParameters()
		{
			var res = await IFINICSClient.GetRows<JsonObject>("DynamicReportParameter", "GetRowsComponentByDynamicReport", new
			{
				DynamicReportID = ID
			});

			return res?.Data ?? [];
		}
		#endregion

		#region ChangePublishStatus
		async void ChangePublishStatus()
		{
			Loading.Show();
			var res = await IFINICSClient.Post<JsonObject>("DynamicReport", "ChangePublishStatus", row);

			await GetRow();

			Loading.Close();
		}
		#endregion
		#region Go to Dynamic Form Setting
		private void GoToSetting()
		{
			string masterFormID = masterForm["ID"]?.GetValue<string>();
			NavigationManager.NavigateTo($"setting/dynamicform/{masterFormID}");
		}
		#endregion

	}
}
