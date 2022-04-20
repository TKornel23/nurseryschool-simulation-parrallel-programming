using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ÓvodaJárvány
{
    internal class Program
    {
        static void Main(string[] args)
        {
            (new Task(() => Óvoda.Upload(), TaskCreationOptions.LongRunning)).Start();
            (new Task(() => Óvoda.Display(), TaskCreationOptions.LongRunning)).Start();
            Óvoda.Alkalmazottak = Enumerable.Range(0,5).Select(x => new Alkalmazott()).ToList();

            Óvoda.Alkalmazottak.ForEach(x => new Task(() => x.DoWork(), TaskCreationOptions.LongRunning).Start());
            Console.ReadLine();
        }
    }

    public static class Óvoda
    {
        public static List<Alkalmazott> Alkalmazottak = new List<Alkalmazott>();
        public static int KitettGyerekek = 6;
        public static ConcurrentQueue<Szülő> Szülők = new ConcurrentQueue<Szülő>();
        public static object _lockObject = new object();
        public static Stopwatch Timer { get; set; }

        public static void Upload()
        {
            for (int i = 0; i < 5; i++)
            {
                lock (_lockObject)
                {
                    Alkalmazottak.Add(new Alkalmazott());
                }
            }
            for (int i = 0; i < 40; i++)
            {
                lock (_lockObject)
                {
                    Szülők.Enqueue(new Szülő());
                }
                Thread.Sleep(Utils.Random.Next(1000, 5001));
            }
        }

        public static void Display()
        {
            Timer = new Stopwatch();
            Timer.Start();
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Alklamazottak:");
                foreach (Alkalmazott alkalmazott in Alkalmazottak)
                {
                    Console.WriteLine(alkalmazott.Státusz);
                }
                if(Óvoda.KitettGyerekek < 0)
                {
                    Console.WriteLine($"Gyerekek száma az óvodában: 0");
                }
                else
                {
                    Console.WriteLine($"Gyerekek száma az óvodában: {Óvoda.KitettGyerekek}");
                }
                TimeSpan timeSpan = Timer.Elapsed;
                if(Timer.ElapsedMilliseconds >= 60000)
                {
                    Console.WriteLine("Idő: {0}óra {1}perc", timeSpan.Minutes, timeSpan.Seconds);
                }
                else
                {
                    Console.WriteLine("Idő: {0}perc", timeSpan.Seconds);
                }
                Thread.Sleep(500);
            }
        }
    }

    public static class Utils
    {
        public static Random Random = new Random();
    }

    public class Alkalmazott
    {
        public int Id { get; set; }
        static int _nextId = 1;
        public object _lockObject = new object();
        public Szülő Szülő { get; set; }
        public string Státusz { get; set; }
        public Alkalmazott()
        {
            this.Id = _nextId++;
            this.Szülő = null;
        }

        public void DoWork()
        {

            while (Óvoda.KitettGyerekek > 0)
            {
                Szülő s = null;
                lock (Óvoda._lockObject)
                {
                    Óvoda.Szülők.TryDequeue(out s);
                }
                if (s != null)
                {
                    if (Utils.Random.Next(0, 101) <= 30)
                    {
                        Státusz = $"|{Id}| Gyerekkel foglalkozik";
                        Thread.Sleep(Utils.Random.Next(1000, 5001));
                    }
                    Szülő = s;
                    s.Alkalmazott = this;
                    Státusz = $"|{Id}| Szülővel foglalkozik ({this.Szülő.Id})";
                    Thread.Sleep(Utils.Random.Next(2000, 8000));
                    Státusz = $"|{Id}| végzett a szülővel ({this.Szülő.Id})";
                    this.Szülő = null;
                    s.Alkalmazott = null;
                    Óvoda.KitettGyerekek--;
                    Thread.Sleep(1500);
                    if(Óvoda.Timer.ElapsedMilliseconds >= 60000)
                    {
                        Státusz = $"|{Id}| Éppen lázat mér";
                        Thread.Sleep(2000);
                        if (Utils.Random.Next(0, 101) <= 2)
                        {
                            lock (Óvoda._lockObject)
                            {
                                Óvoda.Alkalmazottak.Remove(this);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Státusz = $"|{Id}| Szülőre vár";
                }
            }
            Státusz = $"|{Id}| Befejezte mára a munkát";
        }
    }

    public class Szülő
    {
        public int Id { get; set; }
        static int _nextId = 1;
        public Alkalmazott Alkalmazott { get; set; }
        public Szülő()
        {
            this.Id = _nextId++;
            this.Alkalmazott = null;
        }
    }
}
