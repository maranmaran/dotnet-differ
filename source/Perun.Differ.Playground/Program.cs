using Differ.DotNet;

var car1 = new Car("Toyota Camry", "Blue", 2022);
var car2 = new Car("Ford Mustang", "Red", 2023);

var carDiff = DifferDotNet.Diff(car1, car2);
carDiff.Output();

record Car(string Model, string Color, int Year);