# 'Where Used' - who is using what in .NET Core

Just recently, we published our service project and tried to run it:

```bash
# build + publish
$ dotnet publish -c Release -o out

# run project from publishing directory
$ cd out
$ ./Best.Ever.Service.exe
```

and we got this error:

```text
Unhandled exception. System.IO.FileNotFoundException: Could not load file or assembly 'Microsoft.Extensions.DependencyModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'. The system cannot find the file specified.
File name: 'Microsoft.Extensions.DependencyModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
   at Program.<>c.<<Main>$>b__0_0(HostBuilderContext context, LoggerConfiguration service)
   at Serilog.SerilogHostBuilderExtensions.<>c__DisplayClass2_0.<UseSerilog>b__0(HostBuilderContext hostBuilderContext, IServiceProvider services, LoggerConfiguration loggerConfiguration)
   at Serilog.SerilogHostBuilderExtensions.<>c__DisplayClass3_1.<UseSerilog>b__1(IServiceProvider services)
   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)
   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitRootCache(ServiceCallSite callSite, RuntimeResolverContext context)
   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSite(ServiceCallSite callSite, TArgument argument)
   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.Resolve(ServiceCallSite callSite, ServiceProviderEngineScope scope)
   at Microsoft.Extensions.DependencyInjection.ServiceProvider.CreateServiceAccessor(Type serviceType)
```

OK - it's obvious that _Serilog_ cannot find the correct version of _Microsoft.Extensions.DependencyModel_
as the one in the output directory is _2.1.0.0_  The mystery now is:  which component brought in this older version...

This project is a simple _where-used_ utility which scans a directory for .NET assemblies and prints out
which other assemblies use each assembly.

<details>
  <summary>What was the problem?</summary>

After running this utility on the `out` directory, here are the relevant lines:

```text
Microsoft.Extensions.DependencyModel, Version=2.1.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
  coverlet.core, Version=3.1.0.0, Culture=neutral, PublicKeyToken=31d7fc2a7e877089
Microsoft.Extensions.DependencyModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
  Best.Ever.Service, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
  Serilog.Settings.Configuration, Version=3.3.0.0, Culture=neutral, PublicKeyToken=24c2f752a8e58a10
```

It looks like our unit test code coverage (_coverlet.core_) is bringing in an older version of _Microsoft.Extensions.DependencyModel_

The solution is not to publish the *whole* solution, but only the .NET project which contains our service.

</details>

## Prerequisites
* .NET Core 6

## Getting started
```bash
$ git clone https://github.com/TrevorDArcyEvans/RefDepends.git
$ cd RefDepends
$ dotnet restore
$ dotnet build
$ cd /bin/Debug/net6.0
$ ./RefDepends.exe
System.Collections, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
  RefDepends, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
System.Console, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
  RefDepends, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
System.Linq, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
  RefDepends, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
System.Runtime, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
  RefDepends, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
```

## Further work
* generate a dot file so we can graphically view the where used information
