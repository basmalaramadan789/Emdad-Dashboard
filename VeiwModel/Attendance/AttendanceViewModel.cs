using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Emdad_Dashboard.VeiwModel.Attendance
{
    public class AttendanceViewModel
    {
        //[Required(ErrorMessage = "Joining Date is required.")]
        //public DateTime JoiningDate { get; set; }

        [Required(ErrorMessage = "File is required.")]
        public IFormFile UploadedFile { get; set; }
    }
}
