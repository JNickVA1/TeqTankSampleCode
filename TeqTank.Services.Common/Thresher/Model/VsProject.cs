using System;

namespace TeqTank.Services.Common.Thresher.Model
{
    public class VSProject
    {
        public int CompanyId { get; set; }
        public int ProjectId { get; set; }
        public int ProjectTy { get; set; }
        public string Descr { get; set; }
        public int RevisionId { get; set; }
        public bool CheckedOut { get; set; }
        public string CheckedOutUser { get; set; }
        public string CheckedOutMachine { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ProgName { get; set; }
        public string CheckedOutIP { get; internal set; }
    }
}
