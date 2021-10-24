using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace brightwayInsurrance_Pizza
{
    public class Program
    {
        static readonly HttpClient client = new HttpClient();
        private const string _dataUri = "https://www.brightway.com/CodeTests/pizzas.json";
        private const string _outputFileName = "output.json";
        private static Dictionary<string, Operation> _opreationInputMapping = new Dictionary<string, Operation>
        {
            { "1", Operation.GroupToppings},
            { "2", Operation.OrderByMostOrdered},
            { "3", Operation.DisplayTop20},
        };

        public static async Task Main(string[] args)
        {
            // Read input and validate
            // TOODO: Setup dependency injection
            // TODO: Setup logging

            // TODO: Extract into a standalone class
            string input = ParseUIInput();
            ValidateUIInput(input);
            Operation operation = _opreationInputMapping[input];

            // Download data
            IEnumerable<PizzaDto> downloadedPizzaData = await DownloadPizzaToppingData();


            // Process Data
            IEnumerable<object> groupedBy = ProcessPizzaData(operation, downloadedPizzaData);

            // Output Data
            await File.WriteAllTextAsync(_outputFileName, JsonConvert.SerializeObject(groupedBy, Formatting.Indented));

            Console.WriteLine($"Output generated! Please see: {_outputFileName}");
        }

        private static IEnumerable<object> ProcessPizzaData(Operation operation, IEnumerable<PizzaDto> downloadedPizzaToppings)
        {
            // TODO: Extract into BPR.Business business layer
            IEnumerable<object> groupedBy = null;
            switch (operation)
            {
                case Operation.GroupToppings:
                    groupedBy = GroupPizzaToppings(downloadedPizzaToppings);
                    break;
                case Operation.OrderByMostOrdered:
                    groupedBy = OrderByMostOrdered(downloadedPizzaToppings);
                    break;
                case Operation.DisplayTop20:
                    groupedBy = FilterByTop20(downloadedPizzaToppings);
                    break;
                default:
                    throw new InvalidOperationException("Operation does not exist!");
            }

            return groupedBy;
        }

        private static IEnumerable<ProcessedPizzaDataDto> GroupPizzaToppings(IEnumerable<PizzaDto> pizzas)
        {
            // TODO: Extract into BPR.Business business layer
            var groupBy = pizzas
            .GroupBy(c => string.Join("|", c.Toppings.OrderBy(t => t))) // Flatten the list
            .Select(g => new ProcessedPizzaDataDto{
                Name = string.Join(",", g.First().Toppings.OrderBy(n => n)).ToString(),
                Count = g.Count()
            });

            return groupBy;
        }

        // TODO: Extract into BPR.Business business layer
        private static IEnumerable<ProcessedPizzaDataDto> OrderByMostOrdered(IEnumerable<PizzaDto> pizzas) => GroupPizzaToppings(pizzas)
            .OrderByDescending(c => c.Count);


        // TODO: Extract into BPR.Business business layer
        private static IEnumerable<ProcessedPizzaDataDto> FilterByTop20(IEnumerable<PizzaDto> pizzas) => 
            OrderByMostOrdered(pizzas).Take(20);

        private static string ParseUIInput()
        {
            Console.WriteLine("Welcome to Brightway Pizza reader! Please choose the below options by enter 1, 2, or 3...");
            Console.WriteLine("1: To group the topping combinations with a count of how many times they were used ");
            Console.WriteLine("2: Order the list from the most ordered to the least.");
            Console.WriteLine("3: Display the top 20 most frequently ordered pizza configurations, listing the toppings for each and the number of times that pizza configuration has been ordered.");

            return Console.ReadLine();
        }

        private static void ValidateUIInput(string input)
        {
            // TODO: Extract into a standalone class
            if (!_opreationInputMapping.ContainsKey(input))
                Console.WriteLine("Invalid selection detected. Please choose 1, 2, or 3. Please execute me again. Bye!");
        }

        private static async Task<IEnumerable<PizzaDto>> DownloadPizzaToppingData()
        {
            // TODO: Extract into a standalone class
            try
            {
                string responseBody = await client.GetStringAsync(_dataUri);
                var pizzaToppings = JsonConvert.DeserializeObject<IEnumerable<PizzaDto>>(responseBody);

                Console.WriteLine($"Pizza data has been downloaded! Count = {pizzaToppings.Count()}");
                return pizzaToppings;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                throw;
            }
        }
    }
}
