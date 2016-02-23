using System;
using System.IO;

class Program
{
  static void Main() {
    string vd = "version";
    string sp = Path.DirectorySeparatorChar.ToString();
    if (!Directory.Exists(vd)) {
      Directory.CreateDirectory(vd);
      File.WriteAllText(vd+sp+"xy", "1.4");
      File.WriteAllText(vd+sp+"z", "8");
    }
    string xy = File.ReadAllText(vd+sp+"xy").Trim();
    int z = int.Parse(File.ReadAllText(vd+sp+"z"));
    z += 1;
    File.WriteAllText(vd+sp+"z", z.ToString());
    File.WriteAllText("App.cs", @"namespace nboard
{
  class App
  {
    public static string Version = """ + xy + "." + z;
  }
}");
  }
}
