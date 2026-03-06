
using System.Text.Json;
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicReportUserComponent
{
	public partial class DynamicReportUserDataGrid
	{
		#region Service
		[Inject] IFINICSClient IFINICSClient { get; set; } = null!;
		[Inject] IFINSYSClient IFINSYSClient { get; set; } = null!;
		#endregion

		#region Parameter
		[Parameter] public string? DynamicReportID { get; set; }
		#endregion

		#region Component Field
		DataGrid<JsonObject> dataGrid = null!;
		MultipleSelectLookup<JsonObject> userMultiLookup = null!;
		#endregion

		#region Field
		public JsonObject rowDynamicReport { get; set; } = [];
		public bool IsPublished
		{
			get
			{
				return rowDynamicReport["IsPublished"]?.GetValue<int>() == 1;
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
			await GetRow();
			await base.OnInitializedAsync();
		}
		#endregion

		#region LoadData
		protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
		{
			var res = await IFINICSClient.GetRows<JsonObject>("DynamicReportUser", "GetRows", new
			{
				args.Keyword,
				args.Offset,
				args.Limit,
				DynamicReportID
			});

			return res?.Data;
		}
		#endregion

		#region GetRow
		public async Task GetRow()
		{
			Loading.Show();
			var res = await IFINICSClient.GetRow<JsonObject>("DynamicReport", "GetRowByID", new
			{
				ID = DynamicReportID
			});

			if (res?.Data != null)
			{
				rowDynamicReport = res.Data;
			}

			Loading.Close();
			StateHasChanged();
		}
		#endregion

		#region LoadLookup
		protected async Task<List<JsonObject>?> LoadUserLookup(DataGridLoadArgs args)
		{
			var res = await IFINSYSClient.GetRows<JsonObject>("SysUserMain", "GetRowsForLookupExcludeByID", new
			{
				args.Keyword,
				args.Offset,
				args.Limit,
				ids = dataGrid.Data?.Select(x => x["UserID"]?.GetValue<string>()).Where(x => x != null).ToArray()
			});

			return res?.Data;
		}
		#endregion

		#region Add
		private void Add()
		{
			userMultiLookup.Open();
		}
		#endregion

		#region AddToList
		private async void AddToList()
		{
			if (!userMultiLookup.GetSelected().Any())
			{
				await NoDataSelectedAlert();
				return;
			}

			Loading.Show();
			var data = userMultiLookup.GetSelected().Select(row => SetAuditInfo(new JsonObject()
			{
				["DynamicReportID"] = DynamicReportID,
				["UserID"] = row["ID"]?.GetValue<string>(),
				["UserCode"] = row["Code"]?.GetValue<string>(),
				["UserName"] = row["EmployeeName"]?.GetValue<string>(),
			})).ToList();

			await IFINICSClient.Post("DynamicReportUser", "Insert", data);

			await dataGrid.Reload();
			await userMultiLookup.Reload();

			Loading.Close();
		}
		#endregion

		#region Save
		private async void Save(JsonObject data)
		{
			Loading.Show();
			List<JsonObject> list = new();

			foreach (var (key, value) in data)
			{
				var id = key.Split("_").Last();
				var objKey = key.Split("_").First();

				if (list.Find(x => x["ID"]?.GetValue<string>() == id) == null)
				{
					var row = dataGrid.Data?.Find(row => row["ID"]?.GetValue<string>() == id);
					list.Add(SetAuditInfo(
						new JsonObject()
						{
							["ID"] = id,
						}
					));
				}

				list.Find(x => x["ID"]?.GetValue<string>() == id)![objKey] = value?.DeepClone();
			}


			await IFINICSClient.Put("DynamicReportUser", "UpdateByID", list);

			await dataGrid.Reload();
			Loading.Close();
		}
		#endregion

		#region Delete
		private async void Delete()
		{
			var selectedData = dataGrid.selectedData;

			if (!selectedData.Any())
			{
				await NoDataSelectedAlert();
				return;
			}

			bool? result = await Confirm();

			if (result == true)
			{
				Loading.Show();

				List<string?> id = dataGrid.selectedData.Select(row => row["ID"]?.GetValue<string>()).ToList();

				await IFINICSClient.Delete("DynamicReportUser", "DeleteByID", id);

				await dataGrid.Reload();
				dataGrid.selectedData.Clear();

				Loading.Close();

				StateHasChanged();
			}
		}
		#endregion


		#region GetRowIndex
		private int GetRowIndex(JsonObject row)
		{
			var data = dataGrid.Data;
			if (data == null) return 0;
			var index = data.FindIndex(x => x["ID"]?.GetValue<string>() == row["ID"]?.GetValue<string>());
			return index;
		}
		#endregion

	}
}