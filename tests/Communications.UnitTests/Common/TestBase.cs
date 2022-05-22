using Communications.Persistence;
using System;

namespace Communications.UnitTests.Common
{
    public class TestBase : IDisposable
    {
        protected readonly CommunicationsDbContext _context;
        public TestBase()
        {
            _context = CommunicationsDbContextFactory.Create();
        }
        public void Dispose()
        {
            CommunicationsDbContextFactory.Destroy(_context);
        }
    }
}