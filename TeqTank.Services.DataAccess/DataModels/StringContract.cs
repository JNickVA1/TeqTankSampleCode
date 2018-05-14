using System;

namespace TeqTank.Services.DataAccess.DataModels
{
	public class StringContract
    {       
        public string Data { get; set; }
        public string DataHeader { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCd { get; set; }
        public Guid Transaction { get; set; }
    }
}
