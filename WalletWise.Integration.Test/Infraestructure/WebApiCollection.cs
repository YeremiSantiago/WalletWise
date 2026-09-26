using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWise.Integration.Test.Infraestructure
{
    [CollectionDefinition("WebApi")]
    public class WebApiCollection : ICollectionFixture<CustomWebApplicationFactory>
    {

    }
}
