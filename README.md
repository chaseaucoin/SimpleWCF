# SimpleWCF
A library dedicated to make working with WCF a pure joy instead of a mind-altering walk through fire-needles.

Also takes care of the WCF channel finalization problem where by you can have open channels hung by not aborting. 

## Example Daemon
```csharp
    static void Main(string[] args)
    {
        var daemon = new WCFDaemon()
                .AddService<IPingService,PingService>(config => 
                    config.AddHttpBinding(9060)                        
                );

        daemon.Start();
    }
```

## Example Client
```csharp
    static void Main(string[] args)
    {
        IPingService service = new WCFProxy<IPingService>().GetHttpProxy("localhost", 9060);

        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine("{0}", service.Ping());
        }
    }
```

# Notes

The daemon can host many services. Just continue to .AddService();

Goals going forward. Make an "easy button" for service discovery. Add additional Bindings. 

## Example Tweaked Host
```csharp
    static void Main(string[] args)
    {
        var daemon = new WCFDaemon()
                .AddService<IPingService,PingService>(config => 
                    config.AddHttpBinding(9060)
                    .TweakHost(host => {
                        var smb = new ServiceMetadataBehavior();
                        smb.HttpGetEnabled = true;
                        smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                        host.Description.Behaviors.Add(smb);
                    })
                );

        daemon.Start();
    }
```

Using TweakHost you have full control over the underlying service host if you have custom behavior you need to add.