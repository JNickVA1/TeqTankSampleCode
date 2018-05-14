namespace TeqTank.Services.Common.Configuration.CompanyConfiguration
{
    public class CompanySetting
    {
        public int CompanySettingTypeId { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string Value { get; set; }
        public int DataTypeId { get; set; }

        public override string ToString()
        {
            return $"{CompanySettingTypeId}-{Value}";
        }
    }
}
