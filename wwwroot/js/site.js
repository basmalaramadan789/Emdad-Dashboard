//// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
//// for details on configuring this project to bundle and minify static web assets.

//// Write your JavaScript code.



$(document).ready(function () {
    console.log("App Started");



    $('#uploadForm').on('submit', function (e) {
        e.preventDefault();

        var fileInput = $('#pdfFile2')[0].files[0];
        var date = $('#dateValue').val(); // Get the date from the input field

        if (!fileInput) {
            alert('Please select a file.');
            return;
        }

        if (!date) {
            alert('Please provide a date.');
            return;
        }

        var formData = new FormData();
        formData.append('file', fileInput);
        formData.append('date', date); // Append the date to the form data

        $.ajax({
            url: '/UploadPdf',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.FilePath) {
                    displayPdf(response.FilePath); // Display the PDF
                } else {
                    alert('File uploaded, but no file path returned.');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error uploading file:', error);
                alert('Failed to upload the file.');
            }
        });
    });



    function displayPdf(pdfPath) {
        var container = $('#pdfContainer');
        container.html(`<iframe src="${pdfPath}" width="100%" height="600px"></iframe>`);
    }


    // Handle Date Click
    $(".dateClick").on("click", function () {
        var selectedDate = $(this).data("date"); // Get the date in MM/dd/yyyy format
        var attendanceId = $(this).data("id"); // Get the attendance ID
        $("#oldDate").val(selectedDate); // Set the old date in the modal
        $("#attendanceId").val(attendanceId); // Set the attendance ID in the hidden input
        $("#attendance_info").modal("show"); // Show the modal
        console.log("clicked", selectedDate)
        var request = { date: selectedDate }

        // Fetch attendance statistics
        $.ajax({
            url: "/GetAttendancesStatics",  // Ensure the URL is correct
            type: "POST",
            data: request, // Pass the selected date
            success: function (data) {
                console.log("Total Employees: " + data.totalEmployees);
                console.log("Present Employees: " + data.presentEmployees);
                console.log("Attendance Percentage: " + data.attendancePercentage + "%");
                console.log("Off Percentage: " + data.offPercentage + "%");
                console.log("Absent Percentage: " + data.absentPercentage + "%");
                console.log("Absent With Excuse Percentage: " + data.absentWithExcusePercentage + "%");

                // Percentage Data
                const totalAttendance = document.getElementById('totalAttendance');
                const totalAbsent = document.getElementById('absent');
                const attendancePercentage = document.getElementById('attendancePercentage');
                const offPercentage = document.getElementById('offPercentage');
                const absentExcusePercentage = document.getElementById('absentExcusePercentage');
                const absentPercentage = document.getElementById('absentPercentage');
                const leavePercentage = document.getElementById('leavePercentage');

                // Count Data
                const attendanceCount = document.getElementById('attendanceCount');
                const absentCount = document.getElementById('absentCount');
                const absentWithExcute = document.getElementById('absentWithExcute');
                const offCount = document.getElementById('offCount');
                const leaveCount = document.getElementById('leaveCount');

                // pdf file
                const pdfinput = $("#pdfInput"); 
                const dateValeu = document.getElementById("dateValue");

                // Bind data to button
                const deletebtn = document.getElementById("deletebtn");
                $(deletebtn).data("date", data.date);  // Corrected data setting

                totalAttendance.textContent = data.presentEmployees;

                // Percentage Configuration
                attendancePercentage.style.width = `${data.attendancePercentage}%`;
                attendancePercentage.setAttribute("aria-valuenow", data.attendancePercentage);
                attendancePercentage.textContent = `${data.attendancePercentage}%`;

                offPercentage.style.width = `${data.offPercentage}%`;
                offPercentage.setAttribute("aria-valuenow", data.offPercentage);
                offPercentage.textContent = `${data.offPercentage}%`;

                absentPercentage.style.width = `${data.absentPercentage}%`;
                absentPercentage.setAttribute("aria-valuenow", data.absentPercentage);
                absentPercentage.textContent = `${data.absentPercentage}%`;

                leavePercentage.style.width = `${data.leavePercentage}%`;
                leavePercentage.setAttribute("aria-valuenow", data.leavePercentage);
                leavePercentage.textContent = `${data.leavePercentage}%`;

                absentExcusePercentage.style.width = `${data.absentWithExcusePercentage}%`;
                absentExcusePercentage.setAttribute("aria-valuenow", data.absentWithExcusePercentage);
                absentExcusePercentage.textContent = `${data.absentWithExcusePercentage}%`;

                // Count Configuration
                attendanceCount.innerText = data.presentEmployees;
                absentCount.innerText = data.absentCount;
                totalAbsent.innerText = data.absentCount;
                absentWithExcute.innerText = data.absentWithExcuse;
                offCount.innerText = data.offCount;
                leaveCount.innerText = data.leaveCount;
                pdfinput.data("date", data.date);
                dateValue.value = data.date
                console.log("Data set",pdfinput.data("date"));
            },
            error: function () {
                alert("Failed to fetch attendance statistics.");
            }
        });
    });

    // Form Submission
    $("#updateDateForm").on("submit", function (e) {
        e.preventDefault();
        const oldDate = $("#oldDate").val();
        const newDate = $("#newDate").val();
        const attendanceId = $("#attendanceId").val();

        if (!oldDate || !newDate) {
            alert("Please select both old and new dates.");
            return;
        }

        ajaxCall(
            "/UpdateDate",
            "POST",
            { oldDate, newDate, attendanceId },
            function () {
                alert("Date updated successfully!");
                location.reload();
            }
        );
    });

    // Delete Attendance
    $(document).on("click", ".DeleteBtn", function (e) {
        e.preventDefault();
        const date = $(this).data("date");

        if (!date || !confirm("Are you sure you want to delete this item?")) return;

        ajaxCall(
            `/${date}/DeleteDay`,
            "DELETE",
            null,
            function (response) {
                alert(response.message);
                location.reload();
            }
        );
    });
});

// Assuming ajaxCall is defined somewhere
function ajaxCall(url, method, data, successCallback) {
    $.ajax({
        url: url,
        type: method,
        data: data,
        success: successCallback,
        error: function (error) {
            alert("An error occurred: " + error.statusText);
        }
    });
}






//$(document).ready(function () {
//    console.log("App Started")
//    //chart
//    const ctx = document.getElementById('line-charts').getContext('2d');
//    new Chart(ctx, {
//        type: 'line',
//        data: {
//            labels: ['2020', '2021', '2022', '2023'],
//            datasets: [{
//                label: 'Sales',
//                data: [20, 10, 5, 40],
//                borderColor: 'rgba(75, 192, 192, 1)',
//                borderWidth: 2,
//                fill: false
//            }]
//        },
//        options: {
//            responsive: true,
//            scales: {
//                x: { beginAtZero: true },
//                y: { beginAtZero: true }
//            }
//        }
//    });
    


//    // When a date column is clicked
//    $(document).on("click", ".dateClick", function () {
//        var selectedDate = $(this).data("date"); // Get the date in MM/dd/yyyy format
//        var attendanceId = $(this).data("id"); // Get the attendance ID
//        $("#oldDate").val(selectedDate); // Set the old date in the modal
//        $("#attendanceId").val(attendanceId); // Set the attendance ID in the hidden input
//        $("#attendance_info").modal("show"); // Show the modal
//        console.log("clicked")
//        var request = { date: selectedDate }
//        // Fetch attendance statistics
//        $.ajax({
//            url: "/GetAttendancesStatics",
//            type: "POST",
//            data: request, // Pass the selected date
//            success: function (data) {
//                console.log("Total Employees: " + data.totalEmployees);
//                console.log("Present Employees: " + data.presentEmployees);
//                console.log("Attendance Percentage: " + data.attendancePercentage + "%");
//                console.log("Off Percentage: " + data.offPercentage + "%");
//                console.log("Absent Percentage: " + data.absentPercentage + "%");
//                console.log("Absent With Excuse Percentage: " + data.absentWithExcusePercentage + "%");

//                // Percentage Data
//                const totalAttendance = document.getElementById('totalAttendance');
//                const totalAbsent = document.getElementById('absent');
//                const attendancePercentage = document.getElementById('attendancePercentage');
//                const offPercentage = document.getElementById('offPercentage');
//                const absentExcusePercentage = document.getElementById('absentExcusePercentage');
//                const absentPercentage = document.getElementById('absentPercentage');
//                const leavePercentage = document.getElementById('leavePercentage');

//                // Count Data
//                const attendanceCount = document.getElementById('attendanceCount');
//                const absentCount = document.getElementById('absentCount');
//                const absentWithExcute = document.getElementById('absentWithExcute');
//                const offCount = document.getElementById('offCount');
//                const leaveCount = document.getElementById('leaveCount');


//                // bind data to button
//                const deletebtn = document.getElementById("deletebtn")

//                console.log(deletebtn.data("date"))
//                deletebtn.data("date") = data.date

//                totalAttendance.textContent = data.presentEmployees;

//                // Percentage Configuration
//                attendancePercentage.style.width = `${data.attendancePercentage}%`;
//                attendancePercentage.setAttribute("aria-valuenow", data.attendancePercentage);
//                attendancePercentage.textContent = `${data.attendancePercentage}%`;

//                offPercentage.style.width = `${data.offPercentage}%`;
//                offPercentage.setAttribute("aria-valuenow", data.offPercentage);
//                offPercentage.textContent = `${data.offPercentage}%`;

//                absentPercentage.style.width = `${data.absentPercentage}%`;
//                absentPercentage.setAttribute("aria-valuenow", data.absentPercentage);
//                absentPercentage.textContent = `${data.absentPercentage}%`;

//                leavePercentage.style.width = `${data.leavePercentage}%`;
//                leavePercentage.setAttribute("aria-valuenow", data.leavePercentage);
//                leavePercentage.textContent = `${data.leavePercentage}%`;

//                absentExcusePercentage.style.width = `${data.absentWithExcusePercentage}%`;
//                absentExcusePercentage.setAttribute("aria-valuenow", data.absentWithExcusePercentage);
//                absentExcusePercentage.textContent = `${data.absentWithExcusePercentage}%`;

//                // Count Configuration
//                attendanceCount.innerText = data.presentEmployees
//                absentCount.innerText = data.absentCount
//                totalAbsent.innerText = data.absentCount
//                absentWithExcute.innerText = data.absentWithExcuse
//                offCount.innerText = data.offCount
//                leaveCount.innerText = data.leaveCount
//            },
//            error: function () {
//                alert("Failed to fetch attendance statistics.");
//            }
//        });

//    });



//    // Handle the form submission to update the date
//    $("#updateDateForm").on("submit", function (e) {
//        e.preventDefault(); // Prevent the default form submission

//        const oldDate = $("#oldDate").val(); // Get the old date in MM/dd/yyyy format
//        const newDate = $("#newDate").val(); // Get the new date from the input
//        const attendanceId = $("#attendanceId").val(); // Get the attendance ID from the hidden input

//        if (!oldDate || !newDate) {
//            alert("Please select both old and new dates.");
//            return;
//        }

//        // Send AJAX request to update the date in the database
//        $.ajax({
//            url: "/UpdateDate", // Your endpoint for updating the date
//            type: "POST",
//            contentType: "application/json",
//            data: JSON.stringify({
//                oldDate: oldDate,
//                newDate: newDate,
//                attendanceId: attendanceId // Pass the ID for specificity
//            }),
//            success: function () {
//                alert("Date updated successfully!");
//                location.reload(); // Reload the page to show updated data
//            },
//            error: function () {
//                alert("An error occurred while updating the date.");
//            }
//        });
//    });

//    // search input
//    $('#search').on('keyup', function (e) {
//        if (e.key === "Enter" || e.keyCode === 13) {
//            const employeeName = $(this).val();
//            // Make an AJAX request to the endpoint
//            window.location.href = '/Search?employeeName=' + employeeName;

//        }
//    });


//    // delete attendance day
//    $("DeleteBtn").on("click", function (e) {
//        e.preventDefault();
//        console.log($(this).data("date"));

//        // Get the date from the button's data-date attribute
//        var date = $(this).data("date");
//        if (!date) {
//            alert("No date specified.");
//            return;
//        }

//        // Confirm deletion
//        if (confirm("Are you sure you want to delete this item?")) {
//            // Send DELETE request to the API
//            $.ajax({
//                url: `/${date.toString()}/DeleteDay`,
//                type: "DELETE",
//                success: function (response) {
//                    alert(response.message); // Display success message
//                    // Optionally, reload or update the UI
//                    location.reload();
//                },
//                error: function (xhr) {
//                    alert("An error occurred: " + xhr.responseText);
//                }
//            });
//        }
//    });
//    //$("#deletebtn").on("click", function (e) {
//    //    e.preventDefault();

//    //    // Get the ID from the button's data-id attribute
//    //    var date = $(this).data("date");

//    //    // Confirm deletion
//    //    if (confirm("Are you sure you want to delete this item?")) {
//    //        // Send DELETE request to the API
//    //        $.ajax({
//    //            url: `/YourControllerName/${date}/DeleteDay`,
//    //            type: "DELETE",
//    //            success: function (response) {
//    //                alert(response.message); // Display success message
//    //                // Optionally, reload or update the UI
//    //                location.reload();
//    //            },
//    //            error: function (xhr) {
//    //                alert("An error occurred: " + xhr.responseText);
//    //            }
//    //        });
//    //})
//});



////$(document).on("click", ".DeleteBtn", function (e) {
////    e.preventDefault();
////    console.log($(this).data("date"));

////    // Get the date from the button's data-date attribute
////    var date = $(this).data("date");
////    if (!date) {
////        alert("No date specified.");
////        return;
////    }

////    // Confirm deletion
////    if (confirm("Are you sure you want to delete this item?")) {
////        // Send DELETE request to the API
////        $.ajax({
////            url: `/${date.toString()}/DeleteDay`,
////            type: "DELETE",
////            success: function (response) {
////                alert(response.message); // Display success message
////                // Optionally, reload or update the UI
////                location.reload();
////            },
////            error: function (xhr) {
////                alert("An error occurred: " + xhr.responseText);
////            }
////        });
////    }
////});
