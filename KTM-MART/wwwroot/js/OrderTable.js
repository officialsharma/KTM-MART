var dataTable;

$(document).ready(function () {
	var url = window.location.search;
	if (url.includes("inprocess")) {
		loadDataTable("inprocess");
	}
	else
	{
		if (url.includes("completed")) {
			loadDataTable("completed");
		}
		else {
			if (url.includes("pending")) {
				loadDataTable("pending");
			}
			else {
				if (url.includes("approved")) {
					loadDataTable("approved");
				}
				else {
					loadDataTable("all");
				}
			}
		}
	}
});

function loadDataTable(status) {
	dataTable = $('#OrderTblData').DataTable({
		"ajax": {
			
			"url": "/Admin/Order/GetAll?status=" + status

		},
		responsive: true,
		autoWidth: false,
		language: {
			search: "",
			searchPlaceholder: "Search",
			sLengthMenu: "_MENU_ items",
		},
		"columns": [

			{ "data": "id" },
			{ "data": "name" },
			{ "data": "phoneNumber" },
			{
				"data": "applicationUser.email",
				"render": function (data) {
					return data ? data : '';
				}
			},
			{ "data": "orderStatus" },
			{ "data": "orderTotal" },
			/*{ "data": "category.name" },*/
			
			{
				"data": "id",
				"render": function (data) {
					return `
                              <div>
                              <a href="/Admin/Order/Details?orderId=${data}" class="btn btn-warning">View Order</a>
                              </div> 
                           `
				},

			},
		]
	});
}
