using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicReportComponent
{
	public partial class DynamicReportPrintForm
	{
		#region Service
		[Inject] IFINICSClient IFINICSClient { get; set; } = null!;
		#endregion

		#region Parameter
		[Parameter] public string? ID { get; set; }
		[Parameter] public bool CanGoBack { get; set; } = true;
		[Parameter] public string? ParentMenuURL { get; set; }
		#endregion

		#region Component Field
		#endregion

		#region Field
		public JsonObject row = new();
		public List<JsonObject> parameters = new();

		RenderFragment Form => builder =>
		{
			int seq = 0;
			foreach (var parameter in parameters)
			{
				RenderParameter(builder, ref seq, parameter);
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
			if (ID != null)
			{
				await GetRow();
				await GetParameters();
			}
			else
			{
			}
			await base.OnInitializedAsync();
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
			}

			Loading.Close();
			StateHasChanged();
		}
		#endregion

		#region GetParameters
		private async Task GetParameters()
		{
			var res = await IFINICSClient.GetRows<JsonObject>("DynamicReportParameter", "GetRowsComponentByDynamicReport", new
			{
				DynamicReportID = ID
			});

			parameters = res?.Data ?? [];
		}
		#endregion

		#region RenderParameter
		private void RenderParameter(RenderTreeBuilder builder, ref int seq, JsonObject parameter)
		{
			builder.OpenComponent(seq++, componentMap[parameter["ComponentName"]?.GetValue<string>() ?? ""]);
			builder.AddAttribute(seq++, "Label", parameter["Label"]?.GetValue<string>());
			builder.AddAttribute(seq++, "Name", "Parameter_" + parameter["Name"]?.GetValue<string>());
			builder.AddAttribute(seq++, "Required", true);

			if (parameter["IsDefaultValue"]?.GetValue<int>() == 1)
			{
				var defaultText = parameter["DefaultValue"]?.GetValue<string>();

				switch (parameter["ComponentName"]?.GetValue<string>())
				{
					case "FormFieldTextBox":
						builder.AddAttribute(seq++, "Value", defaultText);
						break;

					case "FormFieldNumeric":
						if (decimal.TryParse(defaultText, out var num))
							builder.AddAttribute(seq++, "Value", (long)num);
						break;

					case "FormFieldDatePicker":
						if (DateTime.TryParse(defaultText, out var date))
							builder.AddAttribute(seq++, "Value", date);
						break;

					case "FormFieldSwitch":
						if (int.TryParse(defaultText, out var intVal))
						{
							// Force to int? directly (-1 or 1)
							builder.AddAttribute(seq++, "Value", (int?)intVal);
						}
						else
						{
							builder.AddAttribute(seq++, "Value", (int?)null);
						}
						break;
				}
				// 🔹 Hide if IsDefaultValue == 1
				builder.AddAttribute(seq++, "Visible", false);
			}

			builder.CloseComponent();
		}

		#endregion

		#region OnSubmit
		private async void OnSubmit(JsonObject data)
		{
			Loading.Show();

			data = SetAuditInfo(data);

			data = row.Merge(data);



			data["Parameters"] = data.Where(x => x.Key.StartsWith("Parameter_")).ToDictionary(x => x.Key.Replace("Parameter_", ""), x => x.Value).ToJsonNode();

			var res = await IFINICSClient.Post("DynamicReport", "Print", data);

			Loading.Close();
			if (res == null) return;

			var dataResponse = res.Data;

			if (dataResponse != null)
			{
				var fileName = dataResponse["Name"]?.GetValue<string>();
				var content = dataResponse["Content"]?.GetValueAsByteArray();

				PreviewFile(content, fileName);
			}
		}
		#endregion

		#region Back
		private void Back()
		{
			NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicreport");
		}
		#endregion


	}
}
