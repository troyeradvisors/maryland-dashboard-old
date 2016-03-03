using System;
namespace Dashboard.Web
{
    public partial class Home
    {
        public string Address { get { return City + ", " + State + " " + ZipCode; } }
    }
}
