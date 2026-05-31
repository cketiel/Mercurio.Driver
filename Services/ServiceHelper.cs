using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.Services
{
    public static class ServiceHelper
    {
        // This function allows you to obtain any service registered in MauiProgram
        public static T GetService<T>() =>
            IPlatformApplication.Current.Services.GetService<T>();
    }
}
