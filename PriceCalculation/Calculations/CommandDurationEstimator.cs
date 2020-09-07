using System;
using System.Collections.Generic;
using System.Linq;

namespace PriceCalculation.Calculations
{
    public static class CommandDurationEstimator
    {
        private static Dictionary<string, Dictionary<string, DurationPeriod>> Data { get; set; } = new Dictionary<string, Dictionary<string, DurationPeriod>>();

        private static Random random = new Random();

        public static string SetCommandStart(string commandName)
        {
            var durationPeriod = new DurationPeriod
            {
                Start = DateTime.Now
            };

            var randString = RandomString(10);
            if (!Data.TryGetValue(commandName, out Dictionary<string, DurationPeriod> cmdPeriods))
            {
                cmdPeriods = new Dictionary<string, DurationPeriod>();
                Data.Add(commandName, cmdPeriods);
            }
            cmdPeriods.Add(randString, durationPeriod);
            return randString;
        }

        public static void SetCommandEnd(string commandName, string durationId)
        {
            var end = DateTime.Now;
            if (!Data.TryGetValue(commandName, out Dictionary<string, DurationPeriod> cmdPeriods))
            {
                throw new NullReferenceException("Command name is not registered on start.");
            }
            if (!cmdPeriods.TryGetValue(durationId, out DurationPeriod durationPeriod))
            {
                throw new NullReferenceException("Duration ID doesn't exist");
            }
            durationPeriod.End = end;
        }

        public static double GetAverageDuration(string commandName, int offset = 0)
        {
            if (!Data.TryGetValue(commandName, out Dictionary<string, DurationPeriod> cmdPeriods))
            {
                throw new NullReferenceException("Command name is not registered");
            }
            try
            {
                var durations = cmdPeriods.Values.Where(d => d.Start.HasValue && d.End.HasValue).Skip(offset).ToList();
                return durations.Select(d => d.Duration).Average();
            }
            catch
            {
                // ignore
                return 0;
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class DurationPeriod
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public double Duration { get
            {
                if (Start.HasValue && End.HasValue)
                {
                    var x = End - Start;
                    return x.Value.TotalMilliseconds;
                }
                throw new NullReferenceException("Start or end date is not defined");
            }}
    }
}
