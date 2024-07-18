using System.Text.RegularExpressions;

namespace BusDriversBreakApplication
{
    class Program
    {
        static List<(TimeSpan Start, TimeSpan End)> breaks = new List<(TimeSpan, TimeSpan)>();

        static void Main(string[] args)
        {

            if (args.Length == 2 && args[0].ToLower() == "filename")
            {
                string filePath = args[1];

                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    foreach (string line in lines)
                    {
                        AddBreak(line);
                    }
                }
                else
                {
                    Console.WriteLine("File not found: " + filePath);
                }
            }

            DisplayBusiestPeriod();

            // Main loop
            while (true)
            {
                Console.WriteLine("Enter a new time format <start time><end time> (example 13:1514:00) or type 'exit' to quit:");
                string input = Console.ReadLine();
                if (input.ToLower() == "exit")
                {
                    break;
                }
                AddBreak(input);
                DisplayBusiestPeriod();
            }
        }

        static void AddBreak(string timeEntry)
        {
            // Pattern for 14:1516:30 and also 24hrs time, eg 25:3015:60 is invalid time.
            string pattern = @"^(?:[01]\d|2[0-3]):[0-5]\d(?:[01]\d|2[0-3]):[0-5]\d$";
            Regex regex = new Regex(pattern);

            if (!regex.IsMatch(timeEntry))
            {
                Console.WriteLine("Invalid time entry format. Please enter in format <start time><end time> (example 13:1514:00)");
                return;
            }

            if (TimeSpan.TryParse(timeEntry.Substring(0, 5), out TimeSpan startTime) && TimeSpan.TryParse(timeEntry.Substring(5), out TimeSpan endTime))
            {
                if (startTime > endTime) 
                {
                    Console.WriteLine("Invalid time entry. Ending time is earlier than starting time");
                    return;
                }
                breaks.Add((startTime, endTime));
            } else
            {
                Console.WriteLine("Something went wrong with conversion");
                return;
            }

        }

        static void DisplayBusiestPeriod()
        {
            if (!breaks.Any())
            {
                Console.WriteLine("There are no break times currently.");
                return;
            }

            breaks.Sort((a, b) => a.Start.CompareTo(b.Start));
            // Above we check if there are any breaks entered so its safe to initialize it with the first ones here.
            int maxOverlap = 1;
            int currentOverlap = 1;
            TimeSpan overlapStart = breaks[0].Start;
            TimeSpan overlapEnd = breaks[0].End;

            for (int i = 0; i < breaks.Count; i++)
            {
                currentOverlap = 1;
                TimeSpan currentStart = breaks[i].Start;
                TimeSpan currentEnd = breaks[i].End;

                
                for (int j = i + 1; j < breaks.Count; j++)
                {
                    TimeSpan nextStart = breaks[j].Start;
                    TimeSpan nextEnd = breaks[j].End;

                    
                    TimeSpan overlapStartTemp = nextStart > currentStart ? nextStart : currentStart;
                    TimeSpan overlapEndTemp = nextEnd < currentEnd ? nextEnd : currentEnd;

                    // Check if there is overlap including when one driver starts a break at 12:15 and another finishes at 12:15, then both drivers are considered as taking a break at 12:15. 
                    if (overlapStartTemp <= overlapEndTemp)
                    {
                        currentOverlap++;

                        
                        if (currentOverlap > maxOverlap)
                        {
                            maxOverlap = currentOverlap;
                            overlapStart = overlapStartTemp;
                            overlapEnd = overlapEndTemp;
                        }
                    }
                }
            }

            Console.WriteLine($"Busiest time range is {overlapStart:hh\\:mm}-{overlapEnd:hh\\:mm} with {maxOverlap} drivers.");
        }



    }
}
