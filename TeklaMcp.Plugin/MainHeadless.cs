using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeklaMcp.Server
{
    public class MainHeadless : IDisposable
    {
        private readonly MainViewModel _vm;
        public MainHeadless()
        {
            _vm = new MainViewModel();
        }

        public MainViewModel TryStart()
        {
            _vm.StartCommand.Execute(null);
            return _vm;
        }

        public void Dispose()
        {
            _vm.Dispose();
        }
    }
}
