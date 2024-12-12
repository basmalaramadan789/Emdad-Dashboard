$(document).ready(function () {
    $('#uploadGeneralDailypdfForm').on('submit', function (e) {
        e.preventDefault();

        var fileInput = $('#pdfFile2')[0].files[0];
        var date = $('#dateValue2').val();
        var shift = $('#shiftValue').val(); // Get the shift value from the dropdown

        if (!fileInput) {
            alert('Please select a file.');
            return;
        }

        if (!date) {
            alert('Please provide a date.');
            return;
        }

        if (!shift) {
            alert('Please select a shift.');
            return;
        }

        var formData = new FormData();
        formData.append('UploadedFile', fileInput);
        formData.append('ReportDate', date);
        formData.append('Shift', shift); // Add the shift to the form data

        $.ajax({
            url: '/Attendance/UploadGeneralDailyReport',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                alert(response.message);
                $('#upload_daily_report').modal('hide');
            },
            error: function (xhr) {
                var errorMessage = xhr.responseJSON?.message || 'Failed to upload the file.';
                alert(errorMessage);
            }
        });
    });

});
