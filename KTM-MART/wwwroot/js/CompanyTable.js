var dataTable;

$(document).ready(function () {

	loadDataTable();

});

function loadDataTable() {
	dataTable = $('#CompanyTblData').DataTable({
		"ajax": {
			"url": "/Admin/Company/GetAll",
		},
		responsive: true,
		autoWidth: false,
		language: {
			search: "",
			searchPlaceholder: "Search",
			sLengthMenu: "_MENU_ items",
		},
		"columns": [

			{ "data": "name" },
			{ "data": "phoneNumber" },
			{ "data": "city" },
			{
				"data": "id",
				"render": function (data) {
					return `
                              <div>
							  <a href="/Admin/Company/Upsert?id=${data}" class="btn btn-icon btn-info btn-icon-style-1"><span class="btn-icon-wrap"><i class="fa fa-edit"></i></span></a>
                              <a onClick=Delete('/Admin/Company/Delete/${data}') class="btn btn-icon btn-danger btn-icon-style-1"><span class="btn-icon-wrap"><i class="fa fa-remove"></i></span></a>
                              </div> 
                           `
				},
			},
		]
	});
}
function Delete(url) {

	Swal.fire({
		title: 'Are you sure?',
		text: "You won't be able to revert this!",
		icon: 'warning',
		showCancelButton: true,
		confirmButtonColor: '#3085d6',
		cancelButtonColor: '#d33',
		confirmButtonText: 'Yes, delete it!'
	}).then((result) => {
		if (result.isConfirmed) {
			$.ajax({
				url: url,
				type: 'DELETE',
				success: function (data) {
					if (data.success) {
						dataTable.ajax.reload();
						toastr.success(data.message);
					}
					else {
						toastr.error(data.message);
					}
				}
			})
		}
	})
}