using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.MasterDynamicReportTableComponent
{
	public partial class MasterDynamicReportTableForm
	{
		#region Service
		[Inject] IFINICSClient IFINICSClient { get; set; } = null!;
		#endregion

		#region Parameter
		[Parameter] public string? ID { get; set; }
		[Parameter] public string? ParentMenuURL { get; set; }
		#endregion

		#region Variables
		JsonObject masterForm = new();
		List<FormControlsModel> controls = new();
		List<ExtendModel>? extend = new();
		#endregion

		#region Component Field
		#endregion

		#region Field
		public JsonObject row = new();

		RenderFragment Form => builder =>
		{
			int seq = 0;

			foreach (var control in controls)
			{
				DynamicRenderForm(builder, ref seq, control);
			}
		};
		#endregion

		#region OnInitialized
		protected override async Task OnInitializedAsync()
		{
			if (ID != null)
			{
				await GetRow();
			}
			else
			{
			}
			await DynamicFormSetup();
			await base.OnInitializedAsync();
		}
		#endregion

		#region GetRow
		public async Task GetRow()
		{
			Loading.Show();
			var res = await IFINICSClient.GetRow<JsonObject>("MasterDynamicReportTable", "GetRowByID", new
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
		private async void OnSubmit(JsonObject data)
		{
			Loading.Show();

			data = SetAuditInfo(data);

			data = row.Merge(data);

			SetExtensionProperties(data, controls, "Properties");

			#region Insert
			if (ID == null)
			{
				var res = await IFINICSClient.Post("MasterDynamicReportTable", "Insert", data);

				if (res?.Data != null)
				{
					NavigationManager.NavigateTo($"{ParentMenuURL}/masterdynamicreport/{res.Data["ID"]}");
				}
			}
			#endregion

			#region Update
			else
			{
				var res = await IFINICSClient.Put("MasterDynamicReportTable", "UpdateByID", data);
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
			NavigationManager.NavigateTo($"{ParentMenuURL}/masterdynamicreport");
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
			masterForm = await LoadMasterForm("MDRT");
			controls = await LoadFormControls(masterForm["ID"]?.GetValue<string>());

			if (!string.IsNullOrEmpty(ID))
			{
				var extRes = await IFINICSClient.GetRows<ExtendModel>("MasterDynamicReportTable", "GetRowByExt", new { ID });

				extend = extRes?.Data ?? [];

				AddExtendProperty(controls, extend, row);
			}

			SetInitialValue(row, controls);
		}
		#endregion
	}
}
