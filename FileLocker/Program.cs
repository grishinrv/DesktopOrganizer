var stream = File.OpenWrite("./SampleFile.txt");
Console.WriteLine("Press any key to exit and stop locking file...");
Console.ReadKey();
stream.Dispose();