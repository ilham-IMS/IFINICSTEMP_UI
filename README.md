# IFinancing360 UI

Pembuatan Web berfokus pada folder

-   Data\Model : Berisikan model - model sebagai objek data dari/ke API
-   Data\Service : Untuk berkomunikasi dengan API menggunakan HTTP Client
-   Components : Berisikan komponen dari masing masing model
-   Pages : Halaman web, berisikan route dan penggunaan komponen
-   Shared : Berisikan komponen general

Pengembangan Web dapat menggunakan komponen Radzen dan komponen general yang telah dikembangkan Tim RnD (Dapat dilihat di folder Shared\Components).

Dokumentasi Radzen : https://blazor.radzen.com/dashboard

## Model

Model merupakan representasi Object Data yang digunakan saat penerimaan ataupun pengiriman data ke API. Sebaiknya **Model disamakan dengan Model yang ada di API**.
Pastikan Model.

-   Peletakan File Model diletakkan pada _Data\Model_ dengan `Namespace Data.Model`. Model Inherit terhadap `BaseModel`.
-   Penamaan File menggunakan _Suffix_ `Model` agar tidak bentrok dengan Namespace lain.
-   Contoh berikut merupakan contoh model `SysGeneralCodeModel` pada Module `IFINSYS`

```cs
// File: Data/Model/SysGeneralCode.cs

namespace Data.Model
{
	public class SysGeneralCodeModel : BaseModel
	{
		public string? Code { get; set; }
		public string? Description { get; set; }
		public int? IsEditable { get; set; }

	}
}
```

## Service

**Service** merupakan _class_ yang berisikan **method** untuk melakukan **pemanggilan API** oleh karena itu, umumnya **Service** memiliki **5 _method_** yaitu:

-   `GetRows` : Return List Data
-   `GetRowByID` : Return Single Data berdasarkan ID
-   `Insert` : Penambahan Data
-   `UpdateByID` : Ubah Data berdasarkan ID
-   `DeleteByID` : Hapus Data berdasarkan ID (Array)

Tentu _method_ yang ada pada **Service** bergantung pada **ketersediaan API**.

-   `Service` haruslah memiliki **Attribute** `[Service]`.
-   **Service** harus melakukan **\*Inject HTTP Client** yang akan digunakan dengan menambahkan **class** HTTP Client sebagai parameter `Constructor`.
-   Contoh untuk `SysGeneralCodeService` berikut akan menggunakan `IFINSYSClient` yang merupakan HTTP Client untuk API `IFINSYS`.

```cs
// File: Data/Service/SysGeneralCodeService.cs

using Helper.APIClient;

namespace Data.Service
{
	// ServiceAttribute
	[Service]
	public class SysGeneralCodeService
	{
		// IFINSYS HTTP Client Field
		public readonly IFINSYSClient _ifinsysClient;

		// Constructor dengan Inject IFINSYSClient
		public SysGeneralCodeService(IFINSYSClient ifinsysClient)
		{
			_ifinsysClient = ifinsysClient;
		}

		/*
		* METHODS DIBAWAH CONSTRUCTOR
		*/
	}
}
```

### Contoh Method Service

`NOTE` :

-   Parameters bersifat **_Case Sensitive_**, pastikan penulisan parameters sesuai dengan penulisan parameters pada API termasuk besar kecil huruf.
-   Penggunaan **HTTP Client** mengikuti dengan **HTTP Method** yang digunakan pada API.

#### GetRows

```cs
// File: Data/Service/SysGeneralCodeService.cs

using Helper.APIClient;

namespace Data.Service
{
	// ServiceAttribute
	[Service]
	public class SysGeneralCodeService
	{
		// IFINSYS HTTP Client
		public readonly IFINSYSClient _ifinsysClient;

		// API Controller
		public readonly string _apiController = "SysGeneralCode";

		// API Route
		public readonly string _apiRouteGetRows = "GetRows";

		// Constructor
		public SysGeneralCodeService(IFINSYSClient ifinsysClient)
		{
			_ifinsysClient = ifinsysClient;
		}

		public async Task<List<SysGeneralCode>?> GetRows(string keyword, int offset, int limit)
		{
			// Deklarasi parameters
			var parameters = new
			{
				Keyword = keyword,
				Offset = offset,
				Limit = limit
			};

			// Get List Data
			var res = await _ifinsysClient.GetRows<SysGeneralCode>(_apiController, _apiRouteGetRows, parameters);

			return res?.Data;
		}
	}
}
```

### GetRowByID

```cs
// File: Data/Service/SysGeneralCodeService.cs

using Helper.APIClient;

namespace Data.Service
{
	// ServiceAttribute
	[Service]
	public class SysGeneralCodeService
	{
		// IFINSYS HTTP Client
		public readonly IFINSYSClient _ifinsysClient;

		// API Controller
		public readonly string _apiController = "SysGeneralCode";

		// API Route
		public readonly string _apiRouteGetRowByID = "GetRowByID";

		// Constructor
		public SysGeneralCodeService(IFINSYSClient ifinsysClient)
		{
			_ifinsysClient = ifinsysClient;
		}

		public async Task<SysGeneralCode?> GetRowByID(string? id)
		{
			// Deklarasi parameters
			var parameters = new
			{
				ID = id
			};

			// Get Single Data
			var res = await _ifinsysClient.GetRow<SysGeneralCode>(_apiController, _apiRouteGetRow, parameters);

			return res?.Data;
		}
	}
}
```

### Insert

```cs
// File: Data/Service/SysGeneralCodeService.cs

using Helper.APIClient;

namespace Data.Service
{
	// ServiceAttribute
	[Service]
	public class SysGeneralCodeService
	{
		// IFINSYS HTTP Client
		public readonly IFINSYSClient _ifinsysClient;

		// API Controller
		public readonly string _apiController = "SysGeneralCode";

		// API Route
		public readonly string _apiRouteInsert = "Insert";

		// Constructor
		public SysGeneralCodeService(IFINSYSClient ifinsysClient)
		{
			_ifinsysClient = ifinsysClient;
		}

		public async Task<BodyResponse<BaseModel>> Insert(SysGeneralCode model)
		{
			// Mengirim model ke API Insert dengan HTTP Post
			var res = await _ifinsysClient.Post(_apiController, _apiRouteInsert, model);
			return res;
		}
	}
}
```

### UpdateByID

```cs
// File: Data/Service/SysGeneralCodeService.cs

using Helper.APIClient;

namespace Data.Service
{
	// ServiceAttribute
	[Service]
	public class SysGeneralCodeService
	{
		// IFINSYS HTTP Client
		public readonly IFINSYSClient _ifinsysClient;

		// API Controller
		public readonly string _apiController = "SysGeneralCode";

		// API Route
		public readonly string _apiRouteUpdateByID = "UpdateByID";

		// Constructor
		public SysGeneralCodeService(IFINSYSClient ifinsysClient)
		{
			_ifinsysClient = ifinsysClient;
		}

		public async Task<BodyResponse<object>> UpdateByID(SysGeneralCode model)
		{
			// Mengirim model ke API Update dengan HTTP Put
			var res = await _ifinsysClient.Put(_apiController, _apiRouteUpdateByID, model);
			return res;
		}
	}
}
```

### DeleteByID

```cs
// File: Data/Service/SysGeneralCodeService.cs

using Helper.APIClient;

namespace Data.Service
{
	// ServiceAttribute
	[Service]
	public class SysGeneralCodeService
	{
		// IFINSYS HTTP Client
		public readonly IFINSYSClient _ifinsysClient;

		// API Controller
		public readonly string _apiController = "SysGeneralCode";

		// API Route
		public readonly string _apiRouteDeleteByID = "DeleteByID";

		// Constructor
		public SysGeneralCodeService(IFINSYSClient ifinsysClient)
		{
			_ifinsysClient = ifinsysClient;
		}

		public async Task<BodyResponse<object>> DeleteByID(string[] id)
		{
			// Mengirim model ke API Update dengan HTTP Delete
			var res = await _ifinsysClient.Delete(_apiController, _apiRouteDeleteByID, id);
			return res;
		}
	}
}
```

## Component

Umumnya setiap model memiliki 2 Komponen yaitu **DataGrid** (untuk List berbentuk Tabel) dan **Form** (Untuk form insert dan update). Hal ini menyesuaikan kebutuhan.

-   Component diletakkan pada direktori `Components`
-   Standar penamaan Folder dari component ialah {Nama_Model}Component
-   Standar penamaan file component ialah {Nama_Model}{Jenis_Komponen}. Contoh : SysGeneralCodeDataGrid
-   Setiap komponen memiliki file **_(blazor page)_** dengan ekstensi `.razor` dan **_(blazor class)_** dengan ekstensi `.razor.cs`

    -   SysGeneralCodeDataGrid.razor (blazor page): Berisikan HTML
    -   SysGeneralCodeDataGrid.razor.cs (blazor class): Berisikan **_property_** dan **_method_** yang digunakan pada **blazor page**

### Contoh DataGrid komponen

#### SysGeneralCodeDataGrid.razor

```cs
<RadzenStack>
	<!-- #region Toolbar -->
	<RadzenRow Gap="8">
		<RoleAccess Code="">
			<Button ButtonStyle="ButtonStyle.Info" Text="Add" Click="@Add" Icon="add" />
		</RoleAccess>
		<RoleAccess Code="">
			<Button ButtonStyle="ButtonStyle.Danger" Text="Delete" Click="@Delete" Icon="delete" Disabled="@(Loading.IsLoading)" />
		</RoleAccess>
	</RadzenRow>
	<!-- #endregion -->

	<!-- #region List Data -->
	// DataGrid dari iFinancing360.UI.Components.DataGrid
	<DataGrid ID="SysGeneralCodeDataGrid" @ref=@dataGrid TItem="SysGeneralCodeModel" LoadData="LoadData"
		AllowSelected="true">
		<DataGridColumn TItem="SysGeneralCodeModel" Property="Code" Title="Code" Width="20%"
			Link="@(row => $"/systemsetting/generalcode/{row.ID}")" />
		<DataGridColumn TItem="SysGeneralCodeModel" Property="Description" Title="Description" Width="60%" />
		<DataGridColumn TItem="SysGeneralCodeModel" Property="IsEditable" Title="Editable" Width="20%"
			TextAlign="TextAlign.Center" Format="YN" /> // FormatString "YN" untuk menampilkan "YES" atau "NO" untuk properti yang bersifat status
	</DataGrid>
	<!-- #endregion -->
</RadzenStack>
```

#### SysGeneralCodeDataGrid.razor.cs

```cs
using Data.Model;
using Data.Service;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_SYS_UI.Components.SysGeneralCodeComponent
{
	public partial class SysGeneralCodeDataGrid
	{
		#region Service
		[Inject] SysGeneralCodeService SysGeneralCodeService { get; set; } = null!;
		#endregion

		#region Component Field
		DataGrid<SysGeneralCodeModel> dataGrid = null!;
		#endregion

		#region Field

		#endregion

		#region OnInitialized
		protected override async Task OnParametersSetAsync()
		{
			await base.OnParametersSetAsync();
		}
		#endregion

		#region LoadData
		protected async Task<List<SysGeneralCodeModel>?> LoadData(string keyword)
		{
			return await SysGeneralCodeService.GetRows(keyword, 0, 100);
		}
		#endregion

		#region Add
		private void Add()
		{
			NavigationManager.NavigateTo($"/systemsetting/generalcode/add");
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

				await SysGeneralCodeService.DeleteByID(dataGrid.selectedData.Select(row => row.ID).ToArray());

				await dataGrid.Reload();
				dataGrid.selectedData.Clear();

				Loading.Close();

				StateHasChanged();
			}
		}
		#endregion
	}
}
```

### Contoh Form komponen

#### SysGeneralCodeForm.razor

```cs
<!-- #region Form -->
<RadzenTemplateForm TItem="SysGeneralCodeModel" Data="@row" Submit=@OnSubmit>
	<RadzenStack>
		<!-- #region Toolbar -->
		<RadzenRow Gap="8">
			<RoleAccess Code="">
				<Button ButtonType="ButtonType.Submit" ButtonStyle="ButtonStyle.Info" Text="Save" Icon="save"
					Disabled=@(Loading.IsLoading) />
			</RoleAccess>

			@if (ID != null)
			{
				<RoleAccess Code="">
					<Button ButtonStyle=@(select["IsEditable"]?.GetValue<int>() == 1 ? ButtonStyle.Danger : ButtonStyle.Success)
						Text=@(select["IsEditable"]?.GetValue<int>() == 1 ? "Non Editable" : "Editable") Click="@ChangeEditable" />
				</RoleAccess>
			}

			<Button ButtonStyle="ButtonStyle.Danger" Text="Back" Click="Back" Icon="keyboard_backspace" />
		</RadzenRow>
		<!-- #endregion -->

		<RadzenStack>
			<RadzenRow>
				<!-- #region Code -->
				<FormFieldTextBox Label="Code" Name="Code" Value="@row.Code" Max="50" Required Disabled=@(ID != null) />
				<!-- #endregion -->

				<!-- #region Description -->
				<FormFieldTextArea Label="Description" Name="Description" Value="@row.Description" Max="4000"
					Required />
				<!-- #endregion -->

				<!-- #region Is Editable -->
				<FormFieldSwitch Name="IsEditable" Label="Editable" Value="@select["IsEditable"]?.GetValue<int>()" Disabled />
				<!-- #endregion -->
			</RadzenRow>
		</RadzenStack>
	</RadzenStack>
</RadzenTemplateForm>
<!-- #endregion -->
```

#### SysGeneralCodeForm.razor.cs

```cs
using Data.Model;
using Data.Service;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_SYS_UI.Components.SysGeneralCodeComponent
{
	public partial class SysGeneralCodeForm
	{
		#region Service
		[Inject] SysGeneralCodeService SysGeneralCodeService { get; set; } = null!;
		#endregion

		#region Parameter
		[Parameter] public string? ID { get; set; }
		#endregion

		#region Component Field
		#endregion

		#region Field
		public SysGeneralCodeModel row = new();
		#endregion

		#region OnInitialized
		protected override async Task OnParametersSetAsync()
		{
			if (ID != null)
			{
				await GetRow();
			}
			else
			{
				select["IsEditable"]?.GetValue<int>() = 1;
			}
			await base.OnParametersSetAsync();
		}
		#endregion

		#region GetRow
		public async Task GetRow()
		{
			Loading.Show();
			row = await SysGeneralCodeService.GetRowByID(ID) ?? new();
			Loading.Close();
			StateHasChanged();
		}
		#endregion

		#region ChangeEditable
		private async Task ChangeEditable()
		{
			if (ID != null)
			{
				Loading.Show();
				var res = await SysGeneralCodeService.ChangeEditableStatus(row);

				if (res != null)
				{
					await GetRow();
					Loading.Close();
				}

				StateHasChanged();
			}
		}
		#endregion

		#region OnSubmit
		private async void OnSubmit()
		{
			Loading.Show();

			#region Insert
			if (ID == null)
			{
				var res = await SysGeneralCodeService.Insert(row);

				if (res?.Data != null)
				{
					NavigationManager.NavigateTo($"/systemsetting/generalcode/{res.Data.ID}", true);
				}
			}
			#endregion

			#region Update
			else
			{
				await SysGeneralCodeService.UpdateByID(row);
			}

			Loading.Close();
			StateHasChanged();
			#endregion
		}
		#endregion

		#region Back
		private void Back()
		{
			NavigationManager.NavigateTo("/systemsetting/generalcode");
		}
		#endregion

	}
}
```

_NOTE : Jika Model tersebut Merupakan Child dari suatu Parent maka pada Form nya juga mencantumkan Sedikit informasi Parent. Umumnya ialah Code dan Deskripsi atau properti lain yang mudah dimengerti User_

### Contoh Form komponen Child

#### SysGeneralSubcodeForm.razor

```cs
<!-- #region Form -->
<RadzenTemplateForm TItem="SysGeneralSubcodeModel" Data="@row" Submit=@OnSubmit>
	<RadzenStack>
		<!-- #region Toolbar -->
		<RadzenRow Gap="8">
			<Button ButtonType="ButtonType.Submit" ButtonStyle="ButtonStyle.Info" Text="Save" Icon="save" Disabled="@(Loading.IsLoading)" />
			@if (ID != null)
			{
				<Button ButtonStyle=@(row["IsActive"]?.GetValue<string>() == 1 ? ButtonStyle.Danger : ButtonStyle.Success) Text=@(row["IsActive"]?.GetValue<string>() ==
				1 ? "Non Active" : "Active") Click="@ChangeActive" />
			}
			<Button ButtonStyle="ButtonStyle.Danger" Text="Back" Click="Back" Icon="keyboard_backspace" />
		</RadzenRow>
		<!-- #endregion -->

		<RadzenStack>
			// Menampilkan General Code (Parent dari SubCode)
			<!-- #region General Code -->
			<RadzenRow>
				<!-- #region Code -->
				<FormFieldTextBox Label="General Code Code" Name="Code" Value="@rowGeneralCode.Code" Max="50"
					Required Disabled />
				<!-- #endregion -->
				<!-- #region Description -->
				<FormFieldTextBox Label="General Code Description" Name="Description"
					Value="@rowGeneralCode.Description" Max="50" Required Disabled />
				<!-- #endregion -->
			</RadzenRow>
			<!-- #endregion -->

			<!-- #region Sub Code -->
			<RadzenRow>
				<!-- #region Code -->
				<FormFieldTextBox Label="Code" Name="Code" Value="@row.Code" Max="50" Required Disabled=@(ID !=
					null) />
				<!-- #endregion -->

				<!-- #region Description -->
				<FormFieldTextArea Label="Description" Name="Description" Value="@row.Description" Max="4000"
					Required />
				<!-- #endregion -->

				<!-- #region Is Active -->
				<FormFieldSwitch Name="IsActive" Label="Active" Value="@row["IsActive"]?.GetValue<string>()" Disabled />
				<!-- #endregion -->

				<!-- #region OrderKey -->
				<FormFieldNumeric Label="Order Key" Name="OrderKey" Value="@row.OrderKey" Min="0" Required />
				<!-- #endregion -->
			</RadzenRow>
			<!-- #endregion -->
		</RadzenStack>
	</RadzenStack>
</RadzenTemplateForm>
<!-- #endregion -->
```

#### SysGeneralSubcodeForm.razor.cs

```cs
using Data.Model;
using Data.Service;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_SYS_UI.Components.SysGeneralSubcodeComponent
{
	public partial class SysGeneralSubcodeForm
	{
		#region Service
		[Inject] SysGeneralCodeService SysGeneralCodeService { get; set; } = null!;
		[Inject] SysGeneralSubcodeService SysGeneralSubcodeService { get; set; } = null!;
		#endregion

		#region Parameter
		[Parameter, EditorRequired] public string? ID { get; set; }
		[Parameter, EditorRequired] public string? GeneralCodeID { get; set; }
		#endregion

		#region Component Field
		#endregion

		#region Field
		public SysGeneralSubcodeModel row = new();
		public SysGeneralCodeModel rowGeneralCode = new();
		#endregion

		#region OnInitialized
		protected override async Task OnParametersSetAsync()
		{
			if (ID != null)
			{
				await GetRow();
			}
			else
			{
				row["IsActive"]?.GetValue<string>() = 1;
				row.GeneralCodeID = GeneralCodeID;
			}

			await GetRowGeneralCode();
			await base.OnParametersSetAsync();
		}
		#endregion

		#region GetRowGeneralCode
		public async Task GetRowGeneralCode()
		{
			Loading.Show();

			rowGeneralCode = await SysGeneralCodeService.GetRowByID(GeneralCodeID) ?? new();
			StateHasChanged();

			Loading.Close();
		}

		#endregion

		#region GetRow
		public async Task GetRow()
		{
			Loading.Show();

			row = await SysGeneralSubcodeService.GetRowByID(ID) ?? new();
			StateHasChanged();

			Loading.Close();
		}
		#endregion

		#region ChangeActive
		private async Task ChangeActive()
		{
			if (ID != null)
			{
				Loading.Show();
				var res = await SysGeneralSubcodeService.ChangeStatus(row);

				if (res != null)
				{
					await GetRow();
				}

				Loading.Close();
				StateHasChanged();
			}
		}
		#endregion

		#region OnSubmit
		private async void OnSubmit()
		{
			Loading.Show();

			#region Insert
			if (ID == null)
			{
				var res = await SysGeneralSubcodeService.Insert(row);

				if (res?.Data != null)
				{
					NavigationManager.NavigateTo($"/systemsetting/generalcode/{GeneralCodeID}/generalsubcode/{res.Data.ID}", true);
				}
			}
			#endregion

			#region Update
			else
			{
				var res = await SysGeneralSubcodeService.UpdateByID(row);
			}
			#endregion

			Loading.Close();
			StateHasChanged();
		}
		#endregion

		#region Back
		private void Back()
		{
			NavigationManager.NavigateTo($"/systemsetting/generalcode/{GeneralCodeID}");
		}
		#endregion
	}
}

```

## Pages

-   File dibuat pada direktori `Pages` dengan nama direktori menyesuaikan **Menu**
-   Direktori Page memiliki _Suffix_ `Page`. Contoh Direktori : `GeneralCodePage`
-   Penamaan **File** bergantung pada **nama menu** atau **kegunaannya** sebagai contoh:

    -   Halaman yang menampilkan **list** SysGeneralCode : GeneralCodeList.razor
    -   Halaman yang menampilkan **info** SysGeneralCode : GeneralCodeInfo.razor
    -   dst.

-   **_Blazor Page_** memiliki routing (`@page "/namaroute"`) pada bagian atas **_blazor page_**

    ```cs
    @* File : GeneralCodePage\GeneralCodeList.razor *@

    @* Route *@
    @page "/systemsetting/generalcode"

    @*
    * HTML Element
    *@
    ```

### Standar Routing

-   Pemberian route dilakukan dengan menambahkan `@page "url"` pada bagian atas **Razor Page**
    ```html
    <!-- File : Menu\GeneralCodeList.razor -->
    @page "/systemsetting/generalcode"
    ```
-   Route sesuai dengan yang terdaftar pada `IFINSYS`
-   Contoh menu `General Code` (Child menu dari menu **System Setting**) pada Module `IFINSYS`

    -   Halaman List : `'systemsetting/generalcode'`
    -   Halaman Detail (Add) : `'systemsetting/generalcode/add'`
    -   Halaman Detail (Update) : `'systemsetting/generalcode/{ID}'`

-   Jika suatu menu bersarang atau memiliki halaman info lagi untuk childnya (General Code Info yang memiliki General Subcode Info) maka parameter pertama disesuaikan dengan _nama menu sebelumnya_ dan _barulah parameter kedua berupa `'{ID}'`_, Contoh kasus halaman `GeneralCode` yang memiliki detail `GeneralSubcode` :

    -   Halaman Info GeneralSubcode (Add) : `'systemsetting/generalcode/{GeneralCodeID}/generalsubcode/add'`
    -   Halaman Info GeneralSubcode (Update) : `'systemsetting/generalcode/{GeneralCodeID}/generalsubcode/{ID}'`

## Contoh Pages

### List Page

```html
<!-- File: Pages/GeneralCodePage/GeneralCodeList.razor  -->
<!-- Route -->
@page "/systemsetting/generalcode"

<!-- Import Component yang digunakan -->
@using IFinancing360_SYS_UI.Components.SysGeneralCodeComponent

<RoleAccess Code="">
    <Card>
        <title Text="General Code List" />

        <SysGeneralCodeDataGrid />
    </Card>
</RoleAccess>
<!-- #endregion Parent Lookup -->
```

### Info Page

`NOTE` :

-   Route Endpoint Halaman Info untuk insert/add : `/add`
-   Route Endpoint Halaman Info untuk update : `/{ID}`
-   Gunakan atribut `[Parameter]` untuk menangkap `Route Parameter`

```html
<!-- File: Pages/GeneralCodePage/GeneralCodeInfo.razor  -->
<!-- Route -->
@page "/systemsetting/generalcode/add" @page "/systemsetting/generalcode/{ID}"

<!-- Import Component yang digunakan -->
@using IFinancing360_SYS_UI.Components.SysGeneralCodeComponent @using
IFinancing360_SYS_UI.Components.SysGeneralSubcodeComponent

<RoleAccess Code="">
    <Card>
        <title Text="General Code Info" />

        <SysGeneralCodeForm />

        @if (ID != null) {
        <SysGeneralSubcodeDataGrid GeneralCodeID="@ID" />
        }
    </Card>
</RoleAccess>

@code {
<!-- Route Parameter -->
[Parameter] public string? ID { get; set; } }
```

### Info Page (Child)

```html
<!-- File: Pages/GeneralCodePage/GeneralSubcodeInfo.razor  -->
<!-- Route -->
@page "/systemsetting/generalcode/{GeneralCodeID}/generalsubcode/add" @page
"/systemsetting/generalcode/{GeneralCodeID}/generalsubcode/{ID}"

<!-- Import Component yang digunakan -->
@using IFinancing360_SYS_UI.Components.SysGeneralSubcodeComponent

<RoleAccess Code="">
    <Card>
        <SysGeneralSubcodeForm GeneralCodeID="@GeneralCodeID" ID="@ID" />
    </Card>
</RoleAccess>

@code {
<!-- Route Parameter -->
[Parameter] public string? ID { get; set; } [Parameter] public string?
GeneralCodeID { get; set; } }
```

# IFINSVY_UI
