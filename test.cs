using System;
using System.Reflection;
using System.Linq;

class Program {
    static void Main() {
        var path = @"/Users/ecemnurozen/.nuget/packages/microsoft.openapi/2.4.1/lib/net8.0/Microsoft.OpenApi.dll";
        var asm = Assembly.LoadFrom(path);
        foreach(var t in asm.GetTypes().Where(t => t.Name.Contains("OpenApiInfo"))) {
            Console.WriteLine(t.FullName);
        }
        foreach(var t in asm.GetTypes().Where(t => t.Name.Contains("OpenApiSecurityScheme"))) {
            Console.WriteLine(t.FullName);
        }
    }
}
