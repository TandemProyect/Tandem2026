using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Desing.Models
{
    public class EmployeeViewMySpaceModel
    {
		public string FullName { get; set; }
		public string UserName { get; set; }
		public string AttPhoto { get; set; }
        public string AttPhotoMenu { get; set; }
		public bool EmailConfirmed { get; set; }
		public DateTime AccountCreationDate { get; set; }
		public string CompanyName { get; set; }
		public int TotalDesigns { get; set; }
	}


    public class EmployeeViewModel
    {
        public long SysObjectID { get; set; }
        [Required(ErrorMessage = "Nombre del Empleado es obligatorio")]
        [StringLength(500)]
        [DisplayName("Nombre del Empleado")]
        public string AttName { get; set; }
        [Required(ErrorMessage = "El apellido del Empleado es obligatorio")]
        [StringLength(500)]
        [DisplayName("Apellido del Empleado")]
        public string AttSurname { get; set; }

        [DisplayName("Avatar")]
        public string AttPhoto { get; set; }
        [DisplayName("Avatar Menu")]
        public string AttPhotoMenu { get; set; }

        [DisplayName("Empresa")]
        [Required(ErrorMessage = "insertar nombre de la empresa")]
        public long LinCompany { get; set; }
        public long LinBusiness { get; set; } = 1;
        public string LinAspNetUsert { get; set; }
        public string AttPassAspNetUsert { get; set; }
        public string userSystem { get; set; }

        public string AddLeter { get; set; }
        public string AddCompany { get; set; }
        public DateTime AttCreated { get; set; }
        public bool EmailConfirmed { get; set; }
        public string userId { get; set; }
        public int TotalDesing { get; set; }
        [DisplayName("Usuario")]
        public string UserName { get; set; }
    }
    public class EmployeeUserModel
    {
        public long SysObjectID { get; set; }
        public string AttName { get; set; }
        public string AttSurname { get; set; }
        public string AttPhoto { get; set; }
        public string AttPhotoMenu { get; set; }

    }
}
