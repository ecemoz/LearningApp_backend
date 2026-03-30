using System;
using System.Reflection;
using System.Linq;

class Program {
    static void Main() {
        var path = @"/Users/ecemnurozen/.nuget/packages/swashbuckle.aspnetcore.swaggergen/10.1.5/lib/net10.0/Swashbuckle.AspNetCore.SwaggerGen.dll";
        try {
            var asm = Assembly.LoadFrom(path);
            var t = asm.GetTypes().FirstOrDefault(t => t.Name == "SwaggerGenOptions");
            if (t != null) {
                foreach(var m in t.GetMethods().Where(m => m.Name == "AddSecurityRequirement")) {
                    Console.WriteLine($"{m.Name} - {string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))}");
                }
            }
        } catch (ReflectionTypeLoadException ex) {
            foreach(var t in ex.Types.Where(t => t != null && t.Name == "SwaggerGenOptions")) {
                foreach(var m in t.GetMethods().Where(m => m.Name == "AddSecurityRequirement")) {
                    Console.WriteLine($"{m.Name} - {striusing System;
using System.Reflection;(p => p.Parameteusing System.Linq        
cla    }
            }
        }
      }
