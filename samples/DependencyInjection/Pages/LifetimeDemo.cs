using System;

namespace DependencyInjection.Pages
{
    public class MyService
    {
        public MyService(string name) { this.Name = name; }
        public string Name { get; }
    }

    public class MySingletonService : MyService { public MySingletonService() : base("Singleton") { } }
    public class MyTransientService : MyService { public MyTransientService() : base("Transient") { } }
    public class MyScopedService : MyService { public MyScopedService() : base("Scoped") { } }
}
