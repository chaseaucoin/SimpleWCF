# SimpleWCF
A library dedicated to make working with WCF a pure joy instead of a mind-altering walk through fire-needles.

Also takes care of the WCF channel finalization problem where by you can have open channels hung by not aborting. 

[![cupcakefactory MyGet Build Status](https://www.myget.org/BuildSource/Badge/cupcakefactory?identifier=d7a7f3d4-e8b9-4f5d-8fee-1fa2b7ee5900)](https://www.myget.org/)

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

## Example Hosting With TopShelf
```csharp
    static void Main(string[] args)
    {
        HostFactory.Run(x =>
        {
            x.Service<WCFDaemon>(s =>
            {
                s.ConstructUsing(name => new WCFDaemon()
                    .AddService<IPingService, PingService>(config =>
                    config.AddHttpBinding(9060)
                ));     
                s.WhenStarted(tc => tc.Start());
                s.WhenStopped(tc => tc.Stop());

            });

            x.RunAsLocalSystem();
            x.StartAutomatically();
            x.SetStartTimeout(TimeSpan.FromSeconds(120));

            x.SetDescription("PingService");
            x.SetDisplayName("PingService");
            x.SetServiceName("PingService");
        });
    }
```
# Secure Messaging

Currently for security SimpleWCF uses message security. So the message itself is encrypted before traveling over the wire.
This will give you a high level of security between your clients. I will add Transport level security as well later. 
Setting up https certs is not easy or simple, and so doesn't align with the goals of SimpleWCF.

Soon I'll be adding NetTCP support which will easily allow for transport level security easily.
But, NetTCP has scale concerns.

## Example Secure Daemon
```csharp
    static void Main(string[] args)
    {
        var daemon = new WCFDaemon()
                .AddService<IPingService,PingService>(config => 
                    config.AddSecureHttpBinding(2029, "Name of Certificate")
                );

        daemon.Start();
    }
```

## Example Secure Client
```csharp
    static void Main(string[] args)
    {
        IPingService service = new WCFProxy<IPingService>()
            .GetSecureHttpProxy("localhost", 2029, "Name of Certificate");

        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine("{0}", service.Ping());
        }
    }
```
