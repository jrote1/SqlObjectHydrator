namespace SqlObjectHydrator.Tests.TestData
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public Contact ContactInfo { get; set; }

        public int? RefId { get; set; }
    }
}