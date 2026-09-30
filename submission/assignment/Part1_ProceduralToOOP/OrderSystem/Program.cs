using System.Globalization;
using System.Text;
using OrderSystem.Helpers;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Display.UsePlainText = args.Contains("--plain");
new ConsoleApplication(new OrderSystem.OrderSystem()).Run();
